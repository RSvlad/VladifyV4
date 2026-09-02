using NSubstitute;
using Vladify.Application.Playlists;
using Vladify.Domain.Playlists;
using Vladify.Domain.Tracks;
using Xunit;

namespace Vladify.Application.Tests.Playlists;

public class RefreshPlaylistUseCaseTests
{
    private static SpotifyTrackSnapshot TrackSnapshot(string id) =>
        new(new SpotifyTrackId(id), "Title " + id, new[] { "Artist" }, "Album", 200_000);

    private static ExistingPlaylistState ExistingState(SpotifyPlaylistId spotifyId, params string[] trackIds) =>
        new(spotifyId, "My Playlist", "owner", trackIds.Select(id => new SpotifyTrackId(id)).ToList(), DateTimeOffset.UtcNow.AddDays(-1));

    [Fact]
    public async Task ExecuteAsync_SuccessfulFetch_NoChanges_ReturnsEmptyDiff()
    {
        var reader = Substitute.For<ISpotifyPlaylistReader>();
        var spotifyId = new SpotifyPlaylistId("p1");
        var existing = ExistingState(spotifyId, "t1", "t2");
        var snapshot = new SpotifyPlaylistSnapshot(spotifyId, "My Playlist", "owner", new[] { TrackSnapshot("t1"), TrackSnapshot("t2") });
        reader.FetchPlaylistAsync(spotifyId, Arg.Any<CancellationToken>()).Returns(snapshot);

        var useCase = new RefreshPlaylistUseCase(reader);
        var result = await useCase.ExecuteAsync(existing, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result!.Event.TracksAdded);
        Assert.Empty(result.Event.TracksRemoved);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulFetch_TracksAddedAndRemoved_DiffReflectsChanges()
    {
        var reader = Substitute.For<ISpotifyPlaylistReader>();
        var spotifyId = new SpotifyPlaylistId("p1");
        var existing = ExistingState(spotifyId, "t1", "t2");
        var snapshot = new SpotifyPlaylistSnapshot(spotifyId, "My Playlist", "owner", new[] { TrackSnapshot("t2"), TrackSnapshot("t3") });
        reader.FetchPlaylistAsync(spotifyId, Arg.Any<CancellationToken>()).Returns(snapshot);

        var useCase = new RefreshPlaylistUseCase(reader);
        var result = await useCase.ExecuteAsync(existing, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(new[] { new SpotifyTrackId("t3") }, result!.Event.TracksAdded);
        Assert.Equal(new[] { new SpotifyTrackId("t1") }, result.Event.TracksRemoved);
        Assert.Equal(2, result.Tracks.Count);
    }

    [Fact]
    public async Task ExecuteAsync_FetchFails_ReturnsNull_SilentFail()
    {
        var reader = Substitute.For<ISpotifyPlaylistReader>();
        var spotifyId = new SpotifyPlaylistId("p1");
        var existing = ExistingState(spotifyId, "t1");
        reader.FetchPlaylistAsync(spotifyId, Arg.Any<CancellationToken>()).Returns((SpotifyPlaylistSnapshot?)null);

        var useCase = new RefreshPlaylistUseCase(reader);
        var result = await useCase.ExecuteAsync(existing, CancellationToken.None);

        Assert.Null(result);
    }
}
