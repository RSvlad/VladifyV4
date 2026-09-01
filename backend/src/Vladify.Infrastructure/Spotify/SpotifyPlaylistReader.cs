using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Vladify.Application.Playlists;
using Vladify.Domain.Playlists;
using Vladify.Domain.Tracks;

namespace Vladify.Infrastructure.Spotify;

/// <summary>
/// Reads public Spotify playlists via the Client Credentials flow. Returns null on
/// any fetch failure (private/deleted/unreachable) — the silent-fail policy lives in
/// the Application layer's use cases; this class just reports "could not fetch".
/// </summary>
public sealed class SpotifyPlaylistReader : ISpotifyPlaylistReader
{
    private const string ApiBaseUrl = "https://api.spotify.com/v1";

    private readonly HttpClient _httpClient;
    private readonly SpotifyTokenProvider _tokenProvider;

    public SpotifyPlaylistReader(HttpClient httpClient, SpotifyTokenProvider tokenProvider)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
    }

    public async Task<SpotifyPlaylistSnapshot?> FetchPlaylistAsync(SpotifyPlaylistId spotifyId, CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBaseUrl}/playlists/{spotifyId.Value}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        // Private, deleted, or otherwise unreachable playlist — silent fail per Glossary "Refresh".
        if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<SpotifyPlaylistResponse>(cancellationToken: cancellationToken);
        if (payload is null)
        {
            return null;
        }

        return MapToSnapshot(payload);
    }

    private static SpotifyPlaylistSnapshot MapToSnapshot(SpotifyPlaylistResponse payload)
    {
        var tracks = payload.Tracks.Items
            .Where(item => item.Track is not null)
            .Select(item => new SpotifyTrackSnapshot(
                new SpotifyTrackId(item.Track!.Id),
                item.Track.Name,
                item.Track.Artists.Select(a => a.Name).ToList(),
                item.Track.Album.Name,
                item.Track.DurationMs))
            .ToList();

        return new SpotifyPlaylistSnapshot(
            new SpotifyPlaylistId(payload.Id),
            payload.Name,
            payload.Owner.DisplayName,
            tracks);
    }

    // --- Spotify Web API response shapes (minimal subset used by Problem 1) ---

    private sealed record SpotifyPlaylistResponse(
        string Id,
        string Name,
        SpotifyOwner Owner,
        SpotifyPlaylistTracks Tracks);

    private sealed record SpotifyOwner(
        [property: System.Text.Json.Serialization.JsonPropertyName("display_name")] string DisplayName);

    private sealed record SpotifyPlaylistTracks(List<SpotifyPlaylistItem> Items);

    private sealed record SpotifyPlaylistItem(SpotifyTrack? Track);

    private sealed record SpotifyTrack(
        string Id,
        string Name,
        List<SpotifyArtist> Artists,
        SpotifyAlbum Album,
        [property: System.Text.Json.Serialization.JsonPropertyName("duration_ms")] int DurationMs);

    private sealed record SpotifyArtist(string Name);

    private sealed record SpotifyAlbum(string Name);
}
