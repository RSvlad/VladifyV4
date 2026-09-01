using Vladify.Domain.Tracks;

namespace Vladify.Domain.Playlists;

/// <summary>
/// Domain Event: raised on a successful re-fetch of an existing Playlist.
/// Carries the diff of Tracks added/removed. Per Glossary, this is NOT raised
/// when the refresh fails (silent fail — old data kept, no event, no user-facing error).
/// </summary>
public sealed record PlaylistRefreshed(
    SpotifyPlaylistId PlaylistId,
    DateTimeOffset RefreshedAt,
    IReadOnlyList<SpotifyTrackId> TracksAdded,
    IReadOnlyList<SpotifyTrackId> TracksRemoved);
