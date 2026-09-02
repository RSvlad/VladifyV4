# Vladify v4 — Context Index

## What this is
Self-hosted web app, spiritual successor to the old Vladify-archive MAUI project (Spotify → YouTube downloader). Stack: TypeScript/React frontend + C# (.NET) backend. Spotify API for playlists, youtube-dl/yt-dlp for downloading. Adds: personal music library sync with offline mode.

## Status
**Problem 1 (Spotify playlist import) — COMPLETE.** All three phases done: Phase 1 (concept), Phase 2 (backend + frontend implementation, verified via `dotnet build`/`npm run build`), Phase 3 (test scenarios proposed and implemented — 11 backend xUnit tests + 5 frontend Vitest tests, all passing via `dotnet test`/`npm test`). Frontend `npm audit` is clean (0 vulnerabilities) after bumping `vitest` to `^4.1.9`. The one open UCD question (network/server errors on Refresh: silent vs. visible signal) is resolved — visible signal (ADR-008), UI side (ErrorToast) built and tested.

**Problem 2 (Track Resolution: Spotify track → YouTube video) — COMPLETE.** All three phases done. Phase 1: new bounded context confirmed, resolution modeled as fields on the existing Track entity (not a separate entity), manual per-track + batch triggers, automatic best-match heuristic (no manual candidate picking), backend-hosted via yt-dlp search (no YouTube Data API quota), batch never stops early — always returns a full resolved/failed summary. Phase 2: Domain (`Track.ResolveTo`, `YouTubeVideoId`, `TrackResolved`), Application (`TrackMatchScorer` — weighted title/artist token-overlap + duration-proximity heuristic, `ResolveTrackUseCase`, `ResolveTracksBatchUseCase`), Infrastructure (`YtDlpTrackSearcher` — shells out to the yt-dlp CLI, never throws, empty list on any failure), API (`POST /api/tracks/resolve`, `POST /api/tracks/resolve-batch`), Frontend (`useTrackResolution` hook, `TrackRow`/`BatchResolutionSummaryToast` components, `PlaylistCard` extended to show/hide its track list). Phase 2 also fixed a pre-existing gap from Problem 1: `frontend/src/vite-env.d.ts` was missing, so `npm run build`'s `tsc -b` failed on `import.meta.env` even though `npm test` had been passing — added the standard Vite triple-slash reference. Phase 3: 10 new backend xUnit tests (Domain: Track resolution invariants; Application: TrackMatchScorer heuristic scenarios, ResolveTrackUseCase, ResolveTracksBatchUseCase incl. partial-failure) + 5 new frontend Vitest tests (useTrackResolution: resolve success/no-match/network-failure, batch summary accuracy, independent clearError/clearBatchSummary). All passing via `dotnet test`/`npm test`.

**Ready to start Problem 3** whenever the user picks the next piece of functionality (e.g. Download/Acquisition — actually fetching audio for a resolved Track via yt-dlp, per the README's stated scope and context-map.md's remaining candidate contexts).

## Predecessor project note
The old "Vladify-archive" MAUI project was referenced as prior art but was **not found** in D:\Documents\Projects during setup (checked Archive/ and searched *ladify*). If it exists elsewhere, point Claude to its path so it can be inspected for reusable domain logic, naming, or lessons learned before Phase 1 begins on any problem.

## Active task
None — Problem 2 is closed. Awaiting the user's choice of the next problem to design/build.


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
2026-09-02 — **Problem 2 (Track Resolution) marked COMPLETE.** All tests passing (backend `dotnet test` — 21 total incl. Problem 1's, frontend `npm test` — 10 total incl. Problem 1's). Also fixed a Problem 1 gap: `frontend/src/vite-env.d.ts` was missing, causing `npm run build` to fail on `import.meta.env` typing (this had gone unnoticed since Phase 3 verification only ran `npm test`, not `npm run build`). See file-registry.md for the full file list and review-log.md for the session history.
