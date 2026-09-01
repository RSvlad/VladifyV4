using Vladify.Domain.Playlists;
using Vladify.Domain.Tracks;

namespace Vladify.Api.Playlists;

/// <summary>Wire-shape DTOs for the Playlist endpoints. Kept separate from Domain types
/// so the API contract can evolve independently of the domain model.</summary>

public sealed record TrackDto(string SpotifyId, string Title, IReadOnlyList<string> Artists, string Album, int DurationMs);

public sealed record PlaylistDto(
    string SpotifyId,
    string Name,
    string OwnerName,
    IReadOnlyList<string> TrackIds,
    DateTimeOffset ImportedAt,
    DateTimeOffset? LastRefreshedAt);

public sealed record ImportPlaylistRequest(string SpotifyPlaylistId);

public sealed record RefreshPlaylistRequest(
    string SpotifyId,
    string Name,
    string OwnerName,
    IReadOnlyList<string> TrackIds,
    DateTimeOffset ImportedAt);

public sealed record ImportPlaylistResponseDto(PlaylistDto Playlist, IReadOnlyList<TrackDto> Tracks);

public sealed record RefreshPlaylistResponseDto(
    PlaylistDto Playlist,
    IReadOnlyList<TrackDto> Tracks,
    IReadOnlyList<string> TracksAdded,
    IReadOnlyList<string> TracksRemoved);

internal static class PlaylistDtoMapper
{
    public static PlaylistDto ToDto(Playlist playlist) => new(
        playlist.SpotifyId.Value,
        playlist.Name,
        playlist.OwnerName,
        playlist.TrackIds.Select(id => id.Value).ToList(),
        playlist.ImportedAt,
        playlist.LastRefreshedAt);

    public static TrackDto ToDto(Vladify.Domain.Tracks.Track track) => new(
        track.SpotifyId.Value,
        track.Title,
        track.Artists,
        track.Album,
        track.DurationMs);

    public static IReadOnlyList<SpotifyTrackId> ToTrackIds(IReadOnlyList<string> raw) =>
        raw.Select(id => new SpotifyTrackId(id)).ToList();
}
