using Vladify.Domain.Playlists;
using Vladify.Domain.Tracks;

namespace Vladify.Application.Playlists;

/// <summary>
/// Result of a successful Import use case: the new Playlist plus the domain event,
/// shaped for the client to persist into its own IndexedDB Library (server is stateless).
/// </summary>
public sealed record ImportPlaylistResult(Playlist Playlist, IReadOnlyList<Track> Tracks, PlaylistImported Event);

/// <summary>
/// Import use case (Glossary: "Import" — fetch a Spotify playlist's metadata via
/// Client Credentials, public playlists only, and store it in the Library for the
/// first time). The server does not persist the result; it returns the imported
/// Playlist for the client to store client-side.
/// </summary>
public sealed class ImportPlaylistUseCase
{
    private readonly ISpotifyPlaylistReader _spotifyReader;

    public ImportPlaylistUseCase(ISpotifyPlaylistReader spotifyReader)
    {
        _spotifyReader = spotifyReader;
    }

    /// <summary>Returns null if the playlist could not be fetched from Spotify.</summary>
    public async Task<ImportPlaylistResult?> ExecuteAsync(SpotifyPlaylistId spotifyId, CancellationToken cancellationToken)
    {
        var snapshot = await _spotifyReader.FetchPlaylistAsync(spotifyId, cancellationToken);
        if (snapshot is null)
        {
            return null;
        }

        var tracks = snapshot.Tracks
            .Select(t => new Track(t.SpotifyId, t.Title, t.Artists, t.Album, t.DurationMs))
            .ToList();
        var trackIds = tracks.Select(t => t.SpotifyId).ToList();

        var (playlist, @event) = Playlist.Import(
            snapshot.SpotifyId,
            snapshot.Name,
            snapshot.OwnerName,
            trackIds,
            DateTimeOffset.UtcNow);

        return new ImportPlaylistResult(playlist, tracks, @event);
    }
}
