namespace Vladify.Application.Tracks;

/// <summary>One YouTube search result candidate, prior to match scoring.</summary>
public sealed record YouTubeSearchCandidate(
    string VideoId,
    string Title,
    string ChannelName,
    int? DurationMs);

/// <summary>
/// Port to the YouTube search integration. Implemented in Infrastructure via yt-dlp
/// search (Phase 1 decision: no YouTube Data API quota, accepting yt-dlp's relative
/// fragility). Returns an empty list if the search itself fails or yields nothing —
/// the Application layer treats "no candidates" and "search failed" identically,
/// since neither can produce a resolution.
/// </summary>
public interface IYouTubeTrackSearcher
{
    Task<IReadOnlyList<YouTubeSearchCandidate>> SearchAsync(string query, int maxResults, CancellationToken cancellationToken);
}
