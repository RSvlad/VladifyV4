using Vladify.Application.Tracks;

namespace Vladify.Api.Tracks;

/// <summary>
/// Minimal API endpoints for Track Resolution (matching Spotify Tracks to YouTube
/// videos via yt-dlp search + heuristic scoring). Stateless, same pattern as the
/// Playlist endpoints: client sends its own Track state, server returns the
/// resolved/unresolved result for the client to persist into its Library.
/// </summary>
public static class TrackResolutionEndpoints
{
    public static IEndpointRouteBuilder MapTrackResolutionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tracks").WithTags("Tracks");

        group.MapPost("/resolve", ResolveAsync);
        group.MapPost("/resolve-batch", ResolveBatchAsync);

        return app;
    }

    private static async Task<IResult> ResolveAsync(
        ResolveTrackRequest request,
        ResolveTrackUseCase useCase,
        CancellationToken cancellationToken)
    {
        var track = TrackResolutionDtoMapper.ToDomain(request.Track);
        var result = await useCase.ExecuteAsync(track, cancellationToken);

        return Results.Ok(result.Succeeded
            ? new ResolveTrackResponseDto(TrackResolutionDtoMapper.ToResolvedDto(result.Resolved!), null)
            : new ResolveTrackResponseDto(null, TrackResolutionDtoMapper.ToUnresolvedDto(result)));
    }

    private static async Task<IResult> ResolveBatchAsync(
        ResolveTracksBatchRequest request,
        ResolveTracksBatchUseCase useCase,
        CancellationToken cancellationToken)
    {
        var tracks = request.Tracks.Select(TrackResolutionDtoMapper.ToDomain).ToList();
        var result = await useCase.ExecuteAsync(tracks, cancellationToken);

        var resolvedDtos = result.Resolved.Select(r => TrackResolutionDtoMapper.ToResolvedDto(r.Resolved!)).ToList();
        var failedDtos = result.Failed.Select(TrackResolutionDtoMapper.ToUnresolvedDto).ToList();

        return Results.Ok(new ResolveTracksBatchResponseDto(resolvedDtos, failedDtos));
    }
}
