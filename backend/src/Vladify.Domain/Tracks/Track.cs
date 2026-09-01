namespace Vladify.Domain.Tracks;

/// <summary>
/// Entity with stable identity (SpotifyTrackId). Holds metadata only —
/// track-to-YouTube resolution and download are out of scope (deferred bounded contexts).
/// </summary>
public sealed class Track
{
    public SpotifyTrackId SpotifyId { get; }
    public string Title { get; }
    public IReadOnlyList<string> Artists { get; }
    public string Album { get; }
    public int DurationMs { get; }

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
}
