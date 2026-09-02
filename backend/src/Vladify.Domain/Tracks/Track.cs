namespace Vladify.Domain.Tracks;

/// <summary>
/// Entity with stable identity (SpotifyTrackId). Holds Spotify-sourced metadata plus
/// an optional resolved YouTube match (Bounded Context: Track Resolution). Resolution
/// is an attribute of the Track, not a separate entity — a Track is either resolved
/// or it isn't, and the client's Library stores it as one record per glossary.md.
/// </summary>
public sealed class Track
{
    public SpotifyTrackId SpotifyId { get; }
    public string Title { get; }
    public IReadOnlyList<string> Artists { get; }
    public string Album { get; }
    public int DurationMs { get; }

    public YouTubeVideoId? YouTubeVideoId { get; private set; }
    public string? YouTubeTitle { get; private set; }
    public string? YouTubeChannelName { get; private set; }
    public double? MatchConfidence { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }

    public bool IsResolved => YouTubeVideoId is not null;

    public Track(SpotifyTrackId spotifyId, string title, IReadOnlyList<string> artists, string album, int durationMs)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Track title cannot be empty.", nameof(title));
        }

        if (artists is null || artists.Count == 0)
        {
            throw new ArgumentException("Track must have at least one artist.", nameof(artists));
        }

        if (durationMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMs), "Track duration must be positive.");
        }

        SpotifyId = spotifyId;
        Title = title;
        Artists = artists;
        Album = album;
        DurationMs = durationMs;
    }

    /// <summary>
    /// Applies a successful Track Resolution match, raising TrackResolved. Overwrites
    /// any previous match — re-resolving a Track is allowed (e.g. retry after a
    /// low-confidence result) and always replaces rather than accumulates state.
    /// </summary>
    public TrackResolved ResolveTo(
        YouTubeVideoId youTubeVideoId,
        string youTubeTitle,
        string youTubeChannelName,
        double matchConfidence,
        DateTimeOffset resolvedAt)
    {
        if (string.IsNullOrWhiteSpace(youTubeTitle))
        {
            throw new ArgumentException("YouTube title cannot be empty.", nameof(youTubeTitle));
        }

        if (matchConfidence is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(matchConfidence), "Match confidence must be between 0 and 1.");
        }

        YouTubeVideoId = youTubeVideoId;
        YouTubeTitle = youTubeTitle;
        YouTubeChannelName = youTubeChannelName;
        MatchConfidence = matchConfidence;
        ResolvedAt = resolvedAt;

        return new TrackResolved(SpotifyId, youTubeVideoId, matchConfidence, resolvedAt);
    }
}
