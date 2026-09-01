namespace Vladify.Domain.Tracks;

/// <summary>
/// Stable identity for a Track, sourced from Spotify. Used as the deduplication key
/// across the Library (Glossary: "Track ... stored once in Library, referenced by
/// any number of Playlists, deduplicated by spotifyId").
/// </summary>
public readonly record struct SpotifyTrackId
{
    public string Value { get; }

    public SpotifyTrackId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Spotify track id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value;
}
