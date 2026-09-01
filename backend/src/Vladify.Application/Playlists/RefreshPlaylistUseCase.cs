using Vladify.Domain.Playlists;
using Vladify.Domain.Tracks;

namespace Vladify.Application.Playlists;

/// <summary>Current client-side state of a Playlist, sent by the client to request a Refresh.</summary>
public sealed record ExistingPlaylistState(
    SpotifyPlaylistId SpotifyId,
    string Name,
    string OwnerName,
    IReadOnlyList<SpotifyTrackId> TrackIds,
    DateTimeOffset ImportedAt);

public sealed record RefreshPlaylistResult(Playlist Playlist, IReadOnlyList<Track> Tracks, PlaylistRefreshed Event);

/// <summary>
/// Refresh use case (Glossary: "Refresh" — re-fetch an already-imported Playlist to
/// reconcile Track additions/removals). Per Glossary and PlaylistRefreshed docs: if
/// the Spotify-side playlist is unavailable, this fails silently — returns null,
/// raises no event, caller keeps its existing local version untouched.
/// </summary>
public sealed class RefreshPlaylistUseCase
{
    private readonly ISpotifyPlaylistReader _spotifyReader;

    public RefreshPlaylistUseCase(ISpotifyPlaylistReader spotifyReader)
    {
        _spotifyReader = spotifyReader;
    }

    /// <summary>Returns null on fetch failure — per Glossary, this is a silent fail, not an error.</summary>
    public async Task<RefreshPlaylistResult?> ExecuteAsync(ExistingPlaylistState existing, CancellationToken cancellationToken)
    {
        var snapshot = await _spotifyReader.FetchPlaylistAsync(existing.SpotifyId, cancellationToken);
        if (snapshot is null)
        {
            return null;
        }

        var playlist = ReconstructFrom(existing);
        var tracks = snapshot.Tracks
            .Select(t => new Track(t.SpotifyId, t.Title, t.Artists, t.Album, t.DurationMs))
            .ToList();
        var freshTrackIds = tracks.Select(t => t.SpotifyId).ToList();
        var @event = playlist.Refresh(snapshot.Name, snapshot.OwnerName, freshTrackIds, DateTimeOffset.UtcNow);

        return new RefreshPlaylistResult(playlist, tracks, @event);
    }

    private static Playlist ReconstructFrom(ExistingPlaylistState existing)
    {
        // Server is stateless (Glossary: "Library ... No server-side persistence of
        // user data"), so the Playlist aggregate is rebuilt from client-supplied state
        // for the duration of this request, via the same Import factory.
        var (playlist, _) = Playlist.Import(
            existing.SpotifyId,
            existing.Name,
            existing.OwnerName,
            existing.TrackIds,
            existing.ImportedAt);

        return playlist;
    }
}
