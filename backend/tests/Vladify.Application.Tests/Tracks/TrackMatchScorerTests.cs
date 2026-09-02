using Vladify.Application.Tracks;
using Vladify.Domain.Tracks;
using Xunit;

namespace Vladify.Application.Tests.Tracks;

public class TrackMatchScorerTests
{
    private static Track NewTrack(string title = "Bohemian Rhapsody", string artist = "Queen", int durationMs = 355_000) =>
        new(new SpotifyTrackId("s1"), title, new[] { artist }, "A Night at the Opera", durationMs);

    private static YouTubeSearchCandidate Candidate(string title, string channel = "Some Channel", int? durationMs = null) =>
        new("v1", title, channel, durationMs);

    [Fact]
    public void PickBest_ExactTitleAndCloseDuration_ScoresHigh()
    {
        var track = NewTrack();
        var candidates = new[] { Candidate("Queen - Bohemian Rhapsody", durationMs: 356_000) };

        var best = TrackMatchScorer.PickBest(track, candidates);

        Assert.NotNull(best);
        Assert.True(best!.Value.Score >= TrackMatchScorer.ConfidenceThreshold);
    }

    [Fact]
    public void PickBest_UnrelatedTitle_ScoresBelowThreshold()
    {
        var track = NewTrack();
        var candidates = new[] { Candidate("Completely Different Song Name", durationMs: 356_000) };

        var best = TrackMatchScorer.PickBest(track, candidates);

        Assert.NotNull(best);
        Assert.True(best!.Value.Score < TrackMatchScorer.ConfidenceThreshold);
    }

    [Fact]
    public void PickBest_NullCandidateDuration_UsesNeutralScore_NotZero()
    {
        var track = NewTrack();
        var withKnownDuration = Candidate("Queen - Bohemian Rhapsody", durationMs: 900_000); // way off
        var withUnknownDuration = Candidate("Queen - Bohemian Rhapsody", durationMs: null);

        var scoreWithFarDuration = TrackMatchScorer.PickBest(track, new[] { withKnownDuration })!.Value.Score;
        var scoreWithUnknownDuration = TrackMatchScorer.PickBest(track, new[] { withUnknownDuration })!.Value.Score;

        // Unknown duration (neutral 0.5) should score better than a duration that's wildly off (falls toward 0).
        Assert.True(scoreWithUnknownDuration > scoreWithFarDuration);
    }

    [Fact]
    public void PickBest_MultipleCandidates_PicksHighestScoring()
    {
        var track = NewTrack();
        var poorMatch = Candidate("Some Unrelated Video", durationMs: 100_000);
        var goodMatch = Candidate("Queen - Bohemian Rhapsody (Official Video)", durationMs: 355_000);
        var candidates = new[] { poorMatch, goodMatch };

        var best = TrackMatchScorer.PickBest(track, candidates);

        Assert.Equal(goodMatch, best!.Value.Candidate);
    }

    [Fact]
    public void PickBest_EmptyCandidateList_ReturnsNull()
    {
        var track = NewTrack();

        var best = TrackMatchScorer.PickBest(track, Array.Empty<YouTubeSearchCandidate>());

        Assert.Null(best);
    }

    [Fact]
    public void PickBest_DurationFarBeyondTolerance_FallsToZeroDurationScore()
    {
        var track = NewTrack(durationMs: 200_000);
        // 60s off — well past the 5s tolerance + 30s falloff range, so duration score floors at 0.
        var candidate = Candidate("Bohemian Rhapsody Queen", durationMs: 260_000);

        var best = TrackMatchScorer.PickBest(track, new[] { candidate });

        // With strong title match (0.7 weight) but zero duration score, total is capped below the title-only max.
        Assert.NotNull(best);
        Assert.True(best!.Value.Score <= 0.7);
    }
}
