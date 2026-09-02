using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vladify.Application.Tracks;

namespace Vladify.Infrastructure.YouTube;

/// <summary>
/// Searches YouTube via the yt-dlp CLI (Phase 1 decision: no YouTube Data API quota,
/// accepting the fragility of shelling out to an external tool). Runs
/// "yt-dlp ytsearchN:&lt;query&gt; --dump-json --flat-playlist --no-warnings" and parses
/// one JSON object per line from stdout. Any failure (missing binary, non-zero exit,
/// timeout, malformed output) yields an empty result list rather than throwing — per
/// IYouTubeTrackSearcher's contract, "search failed" and "no candidates" are the same
/// thing to the Application layer.
/// </summary>
public sealed class YtDlpTrackSearcher : IYouTubeTrackSearcher
{
    private readonly YouTubeOptions _options;
    private readonly ILogger<YtDlpTrackSearcher> _logger;

    public YtDlpTrackSearcher(IOptions<YouTubeOptions> options, ILogger<YtDlpTrackSearcher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<YouTubeSearchCandidate>> SearchAsync(string query, int maxResults, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _options.YtDlpPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add($"ytsearch{maxResults}:{query}");
        startInfo.ArgumentList.Add("--dump-json");
        startInfo.ArgumentList.Add("--flat-playlist");
        startInfo.ArgumentList.Add("--no-warnings");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                _logger.LogWarning("yt-dlp process failed to start for query {Query}.", query);
                return [];
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync(timeoutCts.Token);
            var stderrTask = process.StandardError.ReadToEndAsync(timeoutCts.Token);
            await process.WaitForExitAsync(timeoutCts.Token);
            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                _logger.LogWarning("yt-dlp exited {ExitCode} for query {Query}: {Stderr}", process.ExitCode, query, stderr);
                return [];
            }

            return ParseCandidates(stdout);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("yt-dlp timed out after {TimeoutSeconds}s for query {Query}.", _options.TimeoutSeconds, query);
            return [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Covers missing binary (Win32Exception) and any other unexpected process/IO failure.
            _logger.LogWarning(ex, "yt-dlp search failed for query {Query}.", query);
            return [];
        }
    }

    private List<YouTubeSearchCandidate> ParseCandidates(string stdout)
    {
        var candidates = new List<YouTubeSearchCandidate>();

        foreach (var line in stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            YtDlpEntry? entry;
            try
            {
                entry = JsonSerializer.Deserialize<YtDlpEntry>(line);
            }
            catch (JsonException)
            {
                continue; // Skip malformed lines rather than failing the whole search.
            }

            if (entry is null || string.IsNullOrWhiteSpace(entry.Id) || string.IsNullOrWhiteSpace(entry.Title))
            {
                continue;
            }

            candidates.Add(new YouTubeSearchCandidate(
                entry.Id,
                entry.Title,
                entry.Channel ?? entry.Uploader ?? "Unknown",
                entry.Duration is null ? null : (int)(entry.Duration.Value * 1000)));
        }

        return candidates;
    }

    // Minimal subset of yt-dlp's --dump-json output used for matching.
    private sealed record YtDlpEntry(
        string Id,
        string Title,
        string? Channel,
        string? Uploader,
        [property: JsonPropertyName("duration")] double? Duration);
}
