namespace Vladify.Domain.Playlists;

/// <summary>Domain Event: raised on first successful import of a Playlist.</summary>
public sealed record PlaylistImported(SpotifyPlaylistId PlaylistId, DateTimeOffset ImportedAt);
