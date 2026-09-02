namespace Vladify.Infrastructure.YouTube;

/// <summary>Configuration for the yt-dlp-backed YouTube search (appsettings.json "YouTube" section).</summary>
public sealed class YouTubeOptions
{
    public const string SectionName = "YouTube";

    /// <summary>Path or command name for the yt-dlp executable. Defaults to "yt-dlp" (must be on PATH).</summary>
    public string YtDlpPath { get; set; } = "yt-dlp";

    /// <summary>Per-search timeout, to bound how long a slow/hung yt-dlp process can block a request.</summary>
    public int TimeoutSeconds { get; set; } = 30;
}
