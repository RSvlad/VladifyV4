using Vladify.Domain.Tracks;

namespace Vladify.Application.Tracks;

/// <summary>
/// Scores YouTube search candidates against a Spotify Track and picks the best one,
/// per the Phase 1 heuristic: weighted title/artist similarity + duration proximity,
/// no ML. Pure function — no I/O, easy to unit test in isolation from the searcher.
/// </summary>
public static class TrackMatchScorer
{
    /// <summary>
    /// Below this score, a candidate is not considered a confident match — the Track
    /// is left unresolved rather than attached to a low-quality guess (Phase 1: "no
    /// confident match" is a valid outcome, not an error to hide).
    /// </summary>
    public const double ConfidenceThreshold = 0.5;

    private const int DurationToleranceMs = 5_000;
    private const double TitleArtistWeight = 0.7;
    private const double DurationWeight = 0.3;

    /// <summary>Returns the best-scoring candidate and its score, or null if there are no candidates.</summary>
    public static (YouTubeSearchCandidate Candidate, double Score)? PickBest(
        Track track,
        IReadOnlyList<YouTubeSearchCandidate> candidates)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates
            .Select(c => (Candidate: c, Score: Score(track, c)))
            .OrderByDescending(x => x.Score)
            .First();
    }

    private static double Score(Track track, YouTubeSearchCandidate candidate)
    {
        var textScore = TitleArtistSimilarity(track, candidate.Title);
        var durationScore = DurationProximity(track.DurationMs, candidate.DurationMs);
        return (textScore * TitleArtistWeight) + (durationScore * DurationWeight);
    }

    /// <summary>
    /// Token-overlap similarity between "{artists} {title}" and the candidate title —
    /// simple, dependency-free, good enough given the duration signal carries real weight too.
    /// </summary>
    private static double TitleArtistSimilarity(Track track, string candidateTitle)
    {
        var expectedTokens = Tokenize($"{string.Join(' ', track.Artists)} {track.Title}");
        var candidateTokens = Tokenize(candidateTitle);

        if (expectedTokens.Count == 0 || candidateTokens.Count == 0)
        {
            return 0;
        }

        var overlap = expectedTokens.Intersect(candidateTokens).Count();
        return (double)overlap / expectedTokens.Count;
    }

    private static double DurationProximity(int expectedMs, int? candidateMs)
    {
        // No duration signal from the candidate (e.g. yt-dlp couldn't determine it) —
        // neutral score rather than penalizing what we don't know.
        if (candidateMs is null)
        {
            return 0.5;
        }

        var diff = Math.Abs(expectedMs - candidateMs.Value);
        if (diff <= DurationToleranceMs)
        {
            return 1.0;
        }

        // Linear falloff beyond tolerance, floored at 0 once the gap exceeds ~30s more.
        var falloffRange = 30_000;
        var overshoot = diff - DurationToleranceMs;
        return Math.Max(0, 1.0 - ((double)overshoot / falloffRange));
    }

    private static HashSet<string> Tokenize(string text) =>
        text
            .ToLowerInvariant()
            .Split([' ', '-', '_', '(', ')', '[', ']', ',', '.', '\''], StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();
}
