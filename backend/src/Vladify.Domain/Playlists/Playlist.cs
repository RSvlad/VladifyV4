using Vladify.Domain.Tracks;

namespace Vladify.Domain.Playlists;

/// <summary>
/// Entity: a Spotify playlist mirrored into the local Library. Holds references
/// (SpotifyTrackId) to Tracks rather than owning Track instances directly —
/// Tracks are deduplicated and shared across Playlists in the Library.
/// </summary>
public sealed class Playlist
{
    private readonly List<SpotifyTrackId> _trackIds;

    public SpotifyPlaylistId SpotifyId { get; }
    public string Name { get; private set; }
    public string OwnerName { get; private set; }
    public DateTimeOffset ImportedAt { get; }
    public DateTimeOffset? LastRefreshedAt { get; private set; }
    public IReadOnlyList<SpotifyTrackId> TrackIds => _trackIds;

    private Playlist(
        SpotifyPlaylistId spotifyId,
        string name,
        string ownerName,
        IEnumerable<SpotifyTrackId> trackIds,
        DateTimeOffset importedAt)
    {
        SpotifyId = spotifyId;
        Name = name;
        OwnerName = ownerName;
        _trackIds = trackIds.ToList();
        ImportedAt = importedAt;
    }

    /// <summary>
    /// Imports a Spotify playlist for the first time. Raises PlaylistImported.
    /// Metadata only — does not trigger download/track-resolution (Glossary: "Import").
    /// </summary>
    public static (Playlist Playlist, PlaylistImported Event) Import(
        SpotifyPlaylistId spotifyId,
        string name,
        string ownerName,
        IEnumerable<SpotifyTrackId> trackIds,
        DateTimeOffset importedAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Playlist name cannot be empty.", nameof(name));
        }

        var playlist = new Playlist(spotifyId, name, ownerName, trackIds, importedAt);
        var @event = new PlaylistImported(spotifyId, importedAt);
        return (playlist, @event);
    }

    /// <summary>
    /// Reconciles the current track set against a freshly-fetched track set from Spotify.
    /// On success, updates state and returns the PlaylistRefreshed event with the diff.
    /// Fetch failures are NOT modeled here — the caller (Infrastructure) only invokes
    /// this method when a fetch succeeded. Per Glossary, a failed fetch keeps the
    /// existing local version untouched and raises no event; that path never reaches
    /// this method (silent fail happens at the Infrastructure/Application boundary).
    /// </summary>
    public PlaylistRefreshed Refresh(
        string name,
        string ownerName,
        IReadOnlyList<SpotifyTrackId> freshTrackIds,
        DateTimeOffset refreshedAt)
    {
        var previousTrackIds = new HashSet<SpotifyTrackId>(_trackIds);
        var freshTrackIdSet = new HashSet<SpotifyTrackId>(freshTrackIds);

        var added = freshTrackIds.Where(id => !previousTrackIds.Contains(id)).ToList();
        var removed = _trackIds.Where(id => !freshTrackIdSet.Contains(id)).ToList();

        Name = name;
        OwnerName = ownerName;
        _trackIds.Clear();
        _trackIds.AddRange(freshTrackIds);
        LastRefreshedAt = refreshedAt;

        return new PlaylistRefreshed(SpotifyId, refreshedAt, added, removed);
    }
}
