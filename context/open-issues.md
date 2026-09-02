# Open Issues — Vladify v4

1. **Predecessor project location unknown** — old Vladify-archive MAUI project not found in D:\Documents\Projects (re-checked during Problem 1 setup, still not found). Needs user-supplied path if it should inform design.
2. ~~Single-user vs multi-user~~ — **RESOLVED: single-user, no auth.**
3. ~~Library persistence location~~ — **RESOLVED: client-side (IndexedDB). Server does not persist user data.**
4. **Deployment target** — not decided (Docker/bare metal/NAS), affects backend architecture.
5. **youtube-dl/yt-dlp ToS considerations** — flagged for user's own awareness, not a Claude design concern. Now directly relevant since Problem 2 (Track Resolution) actively shells out to yt-dlp for search.
6. **Playlist unavailable on refresh (private/deleted)** — RESOLVED for Problem 1: fails silently, keeps old local data, no user-facing error. Revisit if this proves confusing in practice (user has no way to know a Refresh silently did nothing).
7. **Predecessor project note relevance** — since predecessor still not found, Problem 1 design proceeds without it; flag again if it surfaces later.
8. ~~Refresh: network/server error UX~~ — **RESOLVED (ADR-008): surfaces a visible signal** (distinct from the silent Spotify-side-unavailable case in ADR-006/007). Implemented in `usePlaylistImport.ts`'s `doRefresh` catch block via the hook's `error` state.
9. ~~Track Resolution trigger~~ — **RESOLVED (Problem 2 Phase 1): manual per-track AND batch**, no automatic-on-import resolution.
10. ~~Track Resolution candidate selection~~ — **RESOLVED (Problem 2 Phase 1): fully automatic heuristic**, no user-facing candidate picker. Revisit only if the heuristic proves unreliable in practice (see domain-model.md Problem 2 "deferred" note).
11. ~~Track Resolution: where the YouTube call lives~~ — **RESOLVED (Problem 2 Phase 1): backend (C#)**, same statelessness pattern as Spotify Integration.
12. ~~Track Resolution: entity shape~~ — **RESOLVED (Problem 2 Phase 1): fields on the existing Track**, not a separate `ResolvedTrack` entity.
13. ~~Track Resolution: YouTube search source~~ — **RESOLVED (Problem 2 Phase 1): yt-dlp search**, not the YouTube Data API (no quota, accepted fragility).
14. ~~Track Resolution: batch partial failure~~ — **RESOLVED (Problem 2 Phase 1): never stop early, always return a full resolved/failed summary.**
15. **Track Resolution match confidence threshold (0.5) and heuristic weights (0.7 title/artist, 0.3 duration)** — set during Phase 2 implementation as a reasonable starting point, not validated against real yt-dlp search results (Application-layer unit tests use synthetic candidates). May need tuning once used against live YouTube search data — revisit if resolution quality proves poor in practice (too many false negatives at the threshold, or confident matches that are actually wrong).
16. **yt-dlp availability/installation** — `YtDlpTrackSearcher` assumes `yt-dlp` is on PATH (configurable via `appsettings.json` `YouTube:YtDlpPath`) but the project has no check/setup step ensuring it's actually installed on the host. Currently fails soft (empty candidate list, Track stays unresolved) rather than erroring loudly — acceptable per the searcher's contract, but the user should ensure yt-dlp is installed for Track Resolution to work at all.
