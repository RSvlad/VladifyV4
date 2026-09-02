using NSubstitute;
using Vladify.Application.Playlists;
using Vladify.Domain.Playlists;
using Vladify.Domain.Tracks;
using Xunit;

namespace Vladify.Application.Tests.Playlists;

public class ImportPlaylistUseCaseTests
{
    private static SpotifyTrackSnapshot TrackSnapshot(string id) =>
        new(new SpotifyTrackId(id), "Title " + id, new[] { "Artist" }, "Album", 200_000);

    [Fact]
    public async Task ExecuteAsync_SuccessfulFetch_ReturnsPlaylistAndTracks()
    {
        var reader = Substitute.For<ISpotifyPlaylistReader>();
        var spotifyId = new SpotifyPlaylistId("p1");
        var snapshot = new SpotifyPlaylistSnapshot(spotifyId, "My Playlist", "owner", new[] { TrackSnapshot("t1"), TrackSnapshot("t2") });
        reader.FetchPlaylistAsync(spotifyId, Arg.Any<CancellationToken>()).Returns(snapshot);

        var useCase = new ImportPlaylistUseCase(reader);
        var result = await useCase.ExecuteAsync(spotifyId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("My Playlist", result!.Playlist.Name);
        Assert.Equal(2, result.Tracks.Count);
        Assert.Equal(spotifyId, result.Event.PlaylistId);
    }

    [Fact]
    public async Task ExecuteAsync_FetchFails_ReturnsNull()
    {
        var reader = Substitute.For<ISpotifyPlaylistReader>();
        var spotifyId = new SpotifyPlaylistId("p1");
        reader.FetchPlaylistAsync(spotifyId, Arg.Any<CancellationToken>()).Returns((SpotifyPlaylistSnapshot?)null);

        var useCase = new ImportPlaylistUseCase(reader);
        var result = await useCase.ExecuteAsync(spotifyId, CancellationToken.None);

        Assert.Null(result);
    }
}
