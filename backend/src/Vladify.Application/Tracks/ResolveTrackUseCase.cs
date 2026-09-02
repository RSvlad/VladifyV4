using Vladify.Domain.Tracks;

namespace Vladify.Application.Tracks;

/// <summary>
/// Outcome of attempting to resolve one Track. Exactly one of Resolved/Reason is set.
/// Modeled as a result rather than a nullable, since "no confident match" is an
/// expected outcome the caller must handle distinctly from other failures.
/// </summary>
public enum TrackResolutionFailureReason
{
    NoCandidatesFound,
    NoConfidentMatch,
}

public sealed record ResolveTrackResult(
    SpotifyTrackId SpotifyTrackId,
    Track? Resolved,
    TrackResolved? Event,
    TrackResolutionFailureReason? FailureReason)
{
    public bool Succeeded => Resolved is not null;

    public static ResolveTrackResult Success(Track track, TrackResolved @event) =>
        new(track.SpotifyId, track, @event, null);

    public static ResolveTrackResult Failure(SpotifyTrackId spotifyTrackId, TrackResolutionFailureReason reason) =>
        new(spotifyTrackId, null, null, reason);
}

/// <summary>
/// Resolves a single Track to its best-matching YouTube video (Bounded Context: Track
/// Resolution). The caller supplies the Track itself (client sends its own Library
/// state — server is stateless per the project's persistence model, same pattern as
/// Playlist import/refresh).
/// </summary>
public sealed class ResolveTrackUseCase
{
    private const int MaxCandidates = 5;

    private readonly IYouTubeTrackSearcher _searcher;

    public ResolveTrackUseCase(IYouTubeTrackSearcher searcher)
    {
        _searcher = searcher;
    }

    public async Task<ResolveTrackResult> ExecuteAsync(Track track, CancellationToken cancellationToken)
    {
        var query = $"{string.Join(' ', track.Artists)} {track.Title}";
        var candidates = await _searcher.SearchAsync(query, MaxCandidates, cancellationToken);

        var best = TrackMatchScorer.PickBest(track, candidates);
        if (best is null)
        {
            return ResolveTrackResult.Failure(track.SpotifyId, TrackResolutionFailureReason.NoCandidatesFound);
        }

        var (candidate, score) = best.Value;
        if (score < TrackMatchScorer.ConfidenceThreshold)
        {
            return ResolveTrackResult.Failure(track.SpotifyId, TrackResolutionFailureReason.NoConfidentMatch);
        }

        var @event = track.ResolveTo(
            new YouTubeVideoId(candidate.VideoId),
            candidate.Title,
            candidate.ChannelName,
            score,
            DateTimeOffset.UtcNow);

        return ResolveTrackResult.Success(track, @event);
    }
}
