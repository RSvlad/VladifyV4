# Vladify v4 — Context Index

## What this is
Self-hosted web app, spiritual successor to the old Vladify-archive MAUI project (Spotify → YouTube downloader). Stack: TypeScript/React frontend + C# (.NET) backend. Spotify API for playlists, youtube-dl/yt-dlp for downloading. Adds: personal music library sync with offline mode.

## Status
**Problem 1 (Spotify playlist import) — COMPLETE.** All three phases done: Phase 1 (concept), Phase 2 (backend + frontend implementation, verified via `dotnet build`/`npm run build`), Phase 3 (test scenarios proposed and implemented — 11 backend xUnit tests + 5 frontend Vitest tests, all passing via `dotnet test`/`npm test`). Frontend `npm audit` is clean (0 vulnerabilities) after bumping `vitest` to `^4.1.9`. The one open UCD question (network/server errors on Refresh: silent vs. visible signal) is resolved — visible signal (ADR-008), UI side (ErrorToast) built and tested.

**Ready to start Problem 2** whenever the user picks the next piece of functionality (e.g. track-to-YouTube resolution/download, per the README's stated scope).

## Predecessor project note
The old "Vladify-archive" MAUI project was referenced as prior art but was **not found** in D:\Documents\Projects during setup (checked Archive/ and searched *ladify*). If it exists elsewhere, point Claude to its path so it can be inspected for reusable domain logic, naming, or lessons learned before Phase 1 begins on any problem.

## Active task
None — Problem 1 is closed. Awaiting the user's choice of the next problem to design/build.


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
2026-09-02 — **Problem 1 (Spotify playlist import) marked COMPLETE.** All tests passing (backend `dotnet test`, frontend `npm test`), `npm audit` clean after the `vitest` bump. See file-registry.md for the full file list and review-log.md for the session history.
