using Vladify.Domain.Tracks;

namespace Vladify.Application.Tracks;

/// <summary>
/// Result of a batch resolution run: every Track attempted, split into resolved and
/// failed. Per Phase 1 decision, a batch never stops early on an individual failure —
/// it always processes every Track and returns a full summary.
/// </summary>
public sealed record ResolveTracksBatchResult(
    IReadOnlyList<ResolveTrackResult> Resolved,
    IReadOnlyList<ResolveTrackResult> Failed);

/// <summary>
/// Resolves a batch of Tracks (e.g. an entire Playlist) to YouTube videos. Runs each
/// Track through ResolveTrackUseCase independently — one Track's failure never aborts
/// the rest (Glossary/Phase 1: batch always returns a full resolved/failed summary).
/// </summary>
public sealed class ResolveTracksBatchUseCase
{
    private readonly ResolveTrackUseCase _resolveTrack;

    public ResolveTracksBatchUseCase(ResolveTrackUseCase resolveTrack)
    {
        _resolveTrack = resolveTrack;
    }

    public async Task<ResolveTracksBatchResult> ExecuteAsync(IReadOnlyList<Track> tracks, CancellationToken cancellationToken)
    {
        var resolved = new List<ResolveTrackResult>();
        var failed = new List<ResolveTrackResult>();

        foreach (var track in tracks)
        {
            var result = await _resolveTrack.ExecuteAsync(track, cancellationToken);
            (result.Succeeded ? resolved : failed).Add(result);
        }

        return new ResolveTracksBatchResult(resolved, failed);
    }
}
