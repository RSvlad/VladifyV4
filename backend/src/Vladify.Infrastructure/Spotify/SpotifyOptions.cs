namespace Vladify.Infrastructure.Spotify;

/// <summary>
/// Configuration for the Spotify Client Credentials flow (public playlists only,
/// per Glossary "Import" — no user auth, no refresh tokens).
/// </summary>
public sealed class SpotifyOptions
{
    public const string SectionName = "Spotify";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
