using Vladify.Domain.Playlists;
using Vladify.Domain.Tracks;
using Xunit;

namespace Vladify.Domain.Tests.Playlists;

public class PlaylistTests
{
    private static SpotifyTrackId Id(string value) => new(value);

    private static Playlist ImportWith(params string[] trackIds)
    {
        var (playlist, _) = Playlist.Import(
            new SpotifyPlaylistId("playlist-1"),
            "My Playlist",
            "owner",
            trackIds.Select(Id),
            DateTimeOffset.UtcNow);
        return playlist;
    }

    [Fact]
    public void Refresh_EmptyToFull_AllTracksAdded_NoneRemoved()
    {
        var playlist = ImportWith();

        var @event = playlist.Refresh("My Playlist", "owner", new[] { Id("a"), Id("b") }, DateTimeOffset.UtcNow);

        Assert.Equal(new[] { Id("a"), Id("b") }, @event.TracksAdded);
        Assert.Empty(@event.TracksRemoved);
        Assert.Equal(new[] { Id("a"), Id("b") }, playlist.TrackIds);
    }

    [Fact]
    public void Refresh_FullToEmpty_AllTracksRemoved_NoneAdded()
    {
        var playlist = ImportWith("a", "b");

        var @event = playlist.Refresh("My Playlist", "owner", Array.Empty<SpotifyTrackId>(), DateTimeOffset.UtcNow);

        Assert.Empty(@event.TracksAdded);
        Assert.Equal(new[] { Id("a"), Id("b") }, @event.TracksRemoved);
        Assert.Empty(playlist.TrackIds);
    }

    [Fact]
    public void Refresh_PartialOverlap_ComputesCorrectDiff()
    {
        var playlist = ImportWith("a", "b", "c");

        var @event = playlist.Refresh("My Playlist", "owner", new[] { Id("b"), Id("c"), Id("d") }, DateTimeOffset.UtcNow);

        Assert.Equal(new[] { Id("d") }, @event.TracksAdded);
        Assert.Equal(new[] { Id("a") }, @event.TracksRemoved);
        Assert.Equal(new[] { Id("b"), Id("c"), Id("d") }, playlist.TrackIds);
    }

    [Fact]
    public void Refresh_IdenticalTrackSet_NoDiffButUpdatesTimestamp()
    {
        var playlist = ImportWith("a", "b");

        var @event = playlist.Refresh("My Playlist", "owner", new[] { Id("a"), Id("b") }, DateTimeOffset.UtcNow);

        Assert.Empty(@event.TracksAdded);
        Assert.Empty(@event.TracksRemoved);
        Assert.NotNull(playlist.LastRefreshedAt);
    }

    [Fact]
    public void Refresh_UpdatesNameAndOwnerName()
    {
        var playlist = ImportWith("a");

        playlist.Refresh("Renamed Playlist", "new-owner", new[] { Id("a") }, DateTimeOffset.UtcNow);

        Assert.Equal("Renamed Playlist", playlist.Name);
        Assert.Equal("new-owner", playlist.OwnerName);
    }

    [Fact]
    public void Import_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Playlist.Import(new SpotifyPlaylistId("p1"), "", "owner", Array.Empty<SpotifyTrackId>(), DateTimeOffset.UtcNow));
    }
}
