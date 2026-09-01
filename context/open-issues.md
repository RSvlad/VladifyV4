# Open Issues — Vladify v4

1. **Predecessor project location unknown** — old Vladify-archive MAUI project not found in D:\Documents\Projects (re-checked during Problem 1 setup, still not found). Needs user-supplied path if it should inform design.
2. ~~Single-user vs multi-user~~ — **RESOLVED: single-user, no auth.**
3. ~~Library persistence location~~ — **RESOLVED: client-side (IndexedDB). Server does not persist user data.**
4. **Deployment target** — not decided (Docker/bare metal/NAS), affects backend architecture.
5. **youtube-dl/yt-dlp ToS considerations** — flagged for user's own awareness, not a Claude design concern.
6. **Playlist unavailable on refresh (private/deleted)** — RESOLVED for Problem 1: fails silently, keeps old local data, no user-facing error. Revisit if this proves confusing in practice (user has no way to know a Refresh silently did nothing).
7. **Predecessor project note relevance** — since predecessor still not found, Problem 1 design proceeds without it; flag again if it surfaces later.
8. ~~Refresh: network/server error UX~~ — **RESOLVED (ADR-008): surfaces a visible signal** (distinct from the silent Spotify-side-unavailable case in ADR-006/007). Implemented in `usePlaylistImport.ts`'s `doRefresh` catch block via the hook's `error` state.
