namespace Vladify.Domain.Playlists;

/// <summary>Stable identity for a Playlist, sourced from Spotify.</summary>
public readonly record struct SpotifyPlaylistId
{
    public string Value { get; }

    public SpotifyPlaylistId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Spotify playlist id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value;
}
