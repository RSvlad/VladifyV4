using Vladify.Application.Playlists;
using Vladify.Domain.Playlists;

namespace Vladify.Api.Playlists;

/// <summary>
/// Minimal API endpoints for Problem 1 (Spotify playlist import). Server is stateless:
/// every response hands the client the full Playlist state to persist in its own
/// IndexedDB Library (Glossary: "Library ... No server-side persistence of user data").
/// </summary>
public static class PlaylistEndpoints
{
    public static IEndpointRouteBuilder MapPlaylistEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/playlists").WithTags("Playlists");

        group.MapPost("/import", ImportAsync);
        group.MapPost("/refresh", RefreshAsync);

        return app;
    }

    private static async Task<IResult> ImportAsync(
        ImportPlaylistRequest request,
        ImportPlaylistUseCase useCase,
        CancellationToken cancellationToken)
    {
        var spotifyId = new SpotifyPlaylistId(request.SpotifyPlaylistId);
        var result = await useCase.ExecuteAsync(spotifyId, cancellationToken);

        if (result is null)
        {
            // Unlike Refresh, Import has nothing to fall back to — the user is actively
            // waiting on this playlist appearing, so the failure must be visible (UCD).
            return Results.NotFound(new
            {
                message = "Couldn't find that playlist on Spotify. Double-check the link and make sure the playlist is public.",
            });
        }

        var playlistDto = PlaylistDtoMapper.ToDto(result.Playlist);
        var trackDtos = result.Tracks.Select(PlaylistDtoMapper.ToDto).ToList();

        return Results.Ok(new ImportPlaylistResponseDto(playlistDto, trackDtos));
    }

    private static async Task<IResult> RefreshAsync(
        RefreshPlaylistRequest request,
        RefreshPlaylistUseCase useCase,
        CancellationToken cancellationToken)
    {
        var existing = new ExistingPlaylistState(
            new SpotifyPlaylistId(request.SpotifyId),
            request.Name,
            request.OwnerName,
            PlaylistDtoMapper.ToTrackIds(request.TrackIds),
            request.ImportedAt);

        var result = await useCase.ExecuteAsync(existing, cancellationToken);

        if (result is null)
        {
            // Silent fail per Glossary "Refresh": client keeps its existing local
            // version. We still return 200 with the client's own unchanged state so
            // the client doesn't need special-case error handling for this path.
            var unchanged = new PlaylistDto(
                request.SpotifyId,
                request.Name,
                request.OwnerName,
                request.TrackIds,
                request.ImportedAt,
                LastRefreshedAt: null);

            // No fresh Track data was fetched, so Tracks is empty — the client already
            // has these Tracks in its Library and doesn't need them resent.
            return Results.Ok(new RefreshPlaylistResponseDto(unchanged, Tracks: [], TracksAdded: [], TracksRemoved: []));
        }

        var dto = PlaylistDtoMapper.ToDto(result.Playlist);
        var trackDtos = result.Tracks.Select(PlaylistDtoMapper.ToDto).ToList();
        var tracksAdded = result.Event.TracksAdded.Select(id => id.Value).ToList();
        var tracksRemoved = result.Event.TracksRemoved.Select(id => id.Value).ToList();

        return Results.Ok(new RefreshPlaylistResponseDto(dto, trackDtos, tracksAdded, tracksRemoved));
    }
}
