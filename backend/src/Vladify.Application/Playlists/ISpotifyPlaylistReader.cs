using Vladify.Domain.Playlists;
using Vladify.Domain.Tracks;

namespace Vladify.Application.Playlists;

/// <summary>Result of fetching a playlist's current state from Spotify.</summary>
public sealed record SpotifyPlaylistSnapshot(
    SpotifyPlaylistId SpotifyId,
    string Name,
    string OwnerName,
    IReadOnlyList<SpotifyTrackSnapshot> Tracks);

public sealed record SpotifyTrackSnapshot(
    SpotifyTrackId SpotifyId,
    string Title,
    IReadOnlyList<string> Artists,
    string Album,
    int DurationMs);

/// <summary>
/// Port to the Spotify integration. Implemented in Infrastructure (Client Credentials
/// flow, public playlists only — per Glossary "Import"). Returns null on any fetch
/// failure (playlist private/deleted/unreachable) so the Application layer can apply
/// the silent-fail Refresh policy without knowing HTTP/Spotify specifics.
/// </summary>
public interface ISpotifyPlaylistReader
{
    Task<SpotifyPlaylistSnapshot?> FetchPlaylistAsync(SpotifyPlaylistId spotifyId, CancellationToken cancellationToken);
}
