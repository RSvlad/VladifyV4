using NSubstitute;
using Vladify.Application.Tracks;
using Vladify.Domain.Tracks;
using Xunit;

namespace Vladify.Application.Tests.Tracks;

public class ResolveTracksBatchUseCaseTests
{
    private static Track NewTrack(string id, string title) =>
        new(new SpotifyTrackId(id), title, new[] { "Artist" }, "Album", 200_000);

    [Fact]
    public async Task ExecuteAsync_MixedResults_DoesNotStopEarly_ReturnsFullSummary()
    {
        var searcher = Substitute.For<IYouTubeTrackSearcher>();
        // Track "a" and "c" get a matching candidate; track "b" gets nothing.
        searcher.SearchAsync(Arg.Is<string>(q => q.Contains("Song A")), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<YouTubeSearchCandidate> { new("v1", "Artist - Song A", "Ch", 200_000) });
        searcher.SearchAsync(Arg.Is<string>(q => q.Contains("Song B")), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<YouTubeSearchCandidate>());
        searcher.SearchAsync(Arg.Is<string>(q => q.Contains("Song C")), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<YouTubeSearchCandidate> { new("v3", "Artist - Song C", "Ch", 200_000) });

        var tracks = new[] { NewTrack("a", "Song A"), NewTrack("b", "Song B"), NewTrack("c", "Song C") };
        var useCase = new ResolveTracksBatchUseCase(new ResolveTrackUseCase(searcher));

        var result = await useCase.ExecuteAsync(tracks, CancellationToken.None);

        Assert.Equal(2, result.Resolved.Count);
        Assert.Single(result.Failed);
        Assert.Equal(new SpotifyTrackId("b"), result.Failed[0].SpotifyTrackId);
    }

    [Fact]
    public async Task ExecuteAsync_AllFail_ReturnsEmptyResolvedAndFullFailedList()
    {
        var searcher = Substitute.For<IYouTubeTrackSearcher>();
        searcher.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<YouTubeSearchCandidate>());

        var tracks = new[] { NewTrack("a", "Song A"), NewTrack("b", "Song B") };
        var useCase = new ResolveTracksBatchUseCase(new ResolveTrackUseCase(searcher));

        var result = await useCase.ExecuteAsync(tracks, CancellationToken.None);

        Assert.Empty(result.Resolved);
        Assert.Equal(2, result.Failed.Count);
    }
}
