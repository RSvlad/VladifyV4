using Vladify.Application.Tracks;
using Vladify.Domain.Tracks;

namespace Vladify.Api.Tracks;

/// <summary>Wire-shape DTOs for the Track Resolution endpoints. The server is stateless —
/// the client sends its own Track state and receives the resolved version back to
/// persist into its Library, same pattern as the Playlist endpoints.</summary>

public sealed record TrackInputDto(
    string SpotifyId,
    string Title,
    IReadOnlyList<string> Artists,
    string Album,
    int DurationMs);

public sealed record ResolvedTrackDto(
    string SpotifyId,
    string YouTubeVideoId,
    string YouTubeTitle,
    string YouTubeChannelName,
    double MatchConfidence,
    DateTimeOffset ResolvedAt);

public sealed record UnresolvedTrackDto(string SpotifyId, string Reason);

public sealed record ResolveTrackRequest(TrackInputDto Track);

public sealed record ResolveTrackResponseDto(ResolvedTrackDto? Resolved, UnresolvedTrackDto? Unresolved);

public sealed record ResolveTracksBatchRequest(IReadOnlyList<TrackInputDto> Tracks);

public sealed record ResolveTracksBatchResponseDto(
    IReadOnlyList<ResolvedTrackDto> Resolved,
    IReadOnlyList<UnresolvedTrackDto> Failed);

internal static class TrackResolutionDtoMapper
{
    public static Track ToDomain(TrackInputDto dto) =>
        new(new SpotifyTrackId(dto.SpotifyId), dto.Title, dto.Artists, dto.Album, dto.DurationMs);

    public static ResolvedTrackDto ToResolvedDto(Track track) => new(
        track.SpotifyId.Value,
        track.YouTubeVideoId!.Value.Value,
        track.YouTubeTitle!,
        track.YouTubeChannelName!,
        track.MatchConfidence!.Value,
        track.ResolvedAt!.Value);

    public static UnresolvedTrackDto ToUnresolvedDto(ResolveTrackResult result) => new(
        result.SpotifyTrackId.Value,
        ReasonText(result.FailureReason!.Value));

    private static string ReasonText(TrackResolutionFailureReason reason) => reason switch
    {
        TrackResolutionFailureReason.NoCandidatesFound => "No YouTube results found for this track.",
        TrackResolutionFailureReason.NoConfidentMatch => "No confident match among the results found.",
        _ => "Could not resolve this track.",
    };
}
