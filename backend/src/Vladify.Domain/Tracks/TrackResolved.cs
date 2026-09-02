namespace Vladify.Domain.Tracks;

/// <summary>Domain Event: raised when a Track is successfully matched to a YouTube video.</summary>
public sealed record TrackResolved(
    SpotifyTrackId SpotifyTrackId,
    YouTubeVideoId YouTubeVideoId,
    double MatchConfidence,
    DateTimeOffset ResolvedAt);
