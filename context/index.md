# Vladify v4 — Context Index

## What this is
Self-hosted web app, spiritual successor to the old Vladify-archive MAUI project (Spotify → YouTube downloader). Stack: TypeScript/React frontend + C# (.NET) backend. Spotify API for playlists, youtube-dl/yt-dlp for downloading. Adds: personal music library sync with offline mode.

## Status
**Problem 1 (Spotify playlist import) — Phase 2 in progress.** Backend (Domain/Application/Infrastructure/API layers) and a minimal frontend skeleton (Vite/React/TS, IndexedDB Library, API client, import/refresh hook) are written. The one open UCD question (network/server errors on Refresh: silent vs. visible signal) is **resolved — visible signal** (ADR-008). Remaining for Phase 2: no root frontend entry point or UI component exists yet (input to trigger import, playlist view, error/toast surface).

## Predecessor project note
The old "Vladify-archive" MAUI project was referenced as prior art but was **not found** in D:\Documents\Projects during setup (checked Archive/ and searched *ladify*). If it exists elsewhere, point Claude to its path so it can be inspected for reusable domain logic, naming, or lessons learned before Phase 1 begins on any problem.

## Active task
**Problem 1: Spotify playlist import — Phase 2 (implementation), paused.**
Done so far:
- Backend solution `backend/Vladify.sln` with 4 projects: Vladify.Domain, Vladify.Application, Vladify.Infrastructure, Vladify.Api (layered per user's Phase-2 setup choice).
- Domain: `Track`, `Playlist` (Entities), `SpotifyTrackId`/`SpotifyPlaylistId` (identity VOs), `PlaylistImported`/`PlaylistRefreshed` (Domain Events). `Playlist.Refresh` computes the added/removed diff.
- Application: `ISpotifyPlaylistReader` port, `ImportPlaylistUseCase`, `RefreshPlaylistUseCase` (both return null on Spotify fetch failure — silent-fail policy lives here).
- Infrastructure: `SpotifyTokenProvider` (Client Credentials, in-memory cached token), `SpotifyPlaylistReader` (HTTP calls to Spotify Web API, maps 404/403/401 and any non-success to null).
- API: `POST /api/playlists/import`, `POST /api/playlists/refresh` minimal-API endpoints. Import failure -> 404 with a user-facing message (visible, since there's nothing to fall back to). Refresh failure -> 200 with the client's own unchanged state and empty tracks/diff (fully silent, no signal field — user's explicit choice, see ADR-007).
- Both responses now carry full `Tracks` (TrackDto[]) alongside the Playlist, so the client can persist Track data into its Library (user correction during Phase 2 — DTOs originally only carried trackIds).
- Frontend skeleton: `frontend/` (Vite + React + TS + idb). `playlists/types.ts` (mirrors Domain naming), `playlists/api.ts` (fetch wrapper), `library/db.ts` (IndexedDB Library — playlists + tracks object stores, keyed by spotifyId), `playlists/usePlaylistImport.ts` (hook wiring API -> Library).

**Next step:** build the minimal UI (root entry point + component(s) using `usePlaylistImport`, including a visible surface for the hook's `error` state per ADR-008), then Phase 3 (test scenarios).

## Known constraints / decisions so far
- Two stacks combined for the first time as a full web app: C# backend, TS/React frontend.
- Self-hosted (not a SaaS/multi-tenant product) — **confirmed single-user, no auth needed.**
- **Library persists client-side (IndexedDB).** Server (C#) used for stateless API proxying (e.g. Spotify Client Credentials calls), not for persisting user data.
- Offline mode is a stated goal — implies some form of local storage/cache strategy on the client side, still TBD in detail.

## Open questions (high-level, pre-Phase 1)
- ~~Is this single-user only, or multi-user (auth needed)?~~ **Resolved: single-user, no auth.**
- ~~Where does the "library" persist?~~ **Resolved: client-side (IndexedDB).**
- Legal/ToS posture on youtube-dl downloading — out of scope for Claude to advise on, but worth the user's own awareness.
- Deployment target (Docker? bare metal? NAS?) — affects backend architecture decisions later.

## Last updated
2026-09-01 — Phase 2 (Problem 1) in progress: backend layered solution + frontend skeleton written; paused on Refresh network-error UX question. See adr-log.md ADR-007 (pending) and open-issues.md.
