using Vladify.Domain.Tracks;
using Xunit;

namespace Vladify.Domain.Tests.Tracks;

public class TrackResolutionTests
{
    private static Track NewTrack() =>
        new(new SpotifyTrackId("s1"), "Song Title", new[] { "Artist" }, "Album", 200_000);

    [Fact]
    public void ResolveTo_SetsAllFieldsAndReturnsEvent()
    {
        var track = NewTrack();
        var videoId = new YouTubeVideoId("v1");
        var resolvedAt = DateTimeOffset.UtcNow;

        var @event = track.ResolveTo(videoId, "Song Title (Official)", "Channel", 0.85, resolvedAt);

        Assert.True(track.IsResolved);
        Assert.Equal(videoId, track.YouTubeVideoId);
        Assert.Equal("Song Title (Official)", track.YouTubeTitle);
        Assert.Equal("Channel", track.YouTubeChannelName);
        Assert.Equal(0.85, track.MatchConfidence);
        Assert.Equal(resolvedAt, track.ResolvedAt);

        Assert.Equal(track.SpotifyId, @event.SpotifyTrackId);
        Assert.Equal(videoId, @event.YouTubeVideoId);
        Assert.Equal(0.85, @event.MatchConfidence);
    }

    [Fact]
    public void ResolveTo_CalledTwice_OverwritesPreviousMatch()
    {
        var track = NewTrack();
        track.ResolveTo(new YouTubeVideoId("v1"), "First Match", "Channel A", 0.6, DateTimeOffset.UtcNow);

        var secondResolvedAt = DateTimeOffset.UtcNow.AddMinutes(1);
        track.ResolveTo(new YouTubeVideoId("v2"), "Second Match", "Channel B", 0.9, secondResolvedAt);

        Assert.Equal(new YouTubeVideoId("v2"), track.YouTubeVideoId);
        Assert.Equal("Second Match", track.YouTubeTitle);
        Assert.Equal("Channel B", track.YouTubeChannelName);
        Assert.Equal(0.9, track.MatchConfidence);
        Assert.Equal(secondResolvedAt, track.ResolvedAt);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void ResolveTo_ConfidenceOutOfRange_Throws(double confidence)
    {
        var track = NewTrack();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            track.ResolveTo(new YouTubeVideoId("v1"), "Title", "Channel", confidence, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void NewTrack_IsNotResolved()
    {
        var track = NewTrack();

        Assert.False(track.IsResolved);
        Assert.Null(track.YouTubeVideoId);
    }
}
