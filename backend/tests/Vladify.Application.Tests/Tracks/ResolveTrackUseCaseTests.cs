using NSubstitute;
using Vladify.Application.Tracks;
using Vladify.Domain.Tracks;
using Xunit;

namespace Vladify.Application.Tests.Tracks;

public class ResolveTrackUseCaseTests
{
    private static Track NewTrack() =>
        new(new SpotifyTrackId("s1"), "Bohemian Rhapsody", new[] { "Queen" }, "A Night at the Opera", 355_000);

    [Fact]
    public async Task ExecuteAsync_ConfidentMatch_ResolvesTrackAndReturnsSuccess()
    {
        var searcher = Substitute.For<IYouTubeTrackSearcher>();
        searcher.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<YouTubeSearchCandidate>
            {
                new("v1", "Queen - Bohemian Rhapsody (Official Video)", "Queen Official", 355_000),
            });

        var track = NewTrack();
        var useCase = new ResolveTrackUseCase(searcher);
        var result = await useCase.ExecuteAsync(track, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(track.IsResolved);
        Assert.NotNull(result.Event);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public async Task ExecuteAsync_NoConfidentCandidate_LeavesTrackUnresolved()
    {
        var searcher = Substitute.For<IYouTubeTrackSearcher>();
        searcher.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<YouTubeSearchCandidate>
            {
                new("v1", "Some Totally Unrelated Video", "Random Channel", 40_000),
            });

        var track = NewTrack();
        var useCase = new ResolveTrackUseCase(searcher);
        var result = await useCase.ExecuteAsync(track, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.False(track.IsResolved);
        Assert.Equal(TrackResolutionFailureReason.NoConfidentMatch, result.FailureReason);
    }

    [Fact]
    public async Task ExecuteAsync_NoCandidatesFound_ReturnsNoCandidatesFailure()
    {
        var searcher = Substitute.For<IYouTubeTrackSearcher>();
        searcher.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<YouTubeSearchCandidate>());

        var track = NewTrack();
        var useCase = new ResolveTrackUseCase(searcher);
        var result = await useCase.ExecuteAsync(track, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(TrackResolutionFailureReason.NoCandidatesFound, result.FailureReason);
    }
}
