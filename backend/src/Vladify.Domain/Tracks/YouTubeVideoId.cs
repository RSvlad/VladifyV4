namespace Vladify.Domain.Tracks;

/// <summary>
/// Identity of a YouTube video, as resolved for a Track (Bounded Context: Track
/// Resolution). Distinct type from SpotifyTrackId — the two identity spaces are
/// unrelated and must never be confused at compile time.
/// </summary>
public readonly record struct YouTubeVideoId
{
    public string Value { get; }

    public YouTubeVideoId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("YouTube video id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value;
}
