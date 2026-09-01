# Architecture Decision Log — Vladify v4

## ADR-000 — Stack choice (pre-existing, from initial idea)
- **Decision:** TypeScript/React frontend + C# (.NET) backend, self-hosted.
- **Status:** Given by user as the starting premise, not yet formally debated in a Phase 1 session.
- **Rationale (as stated):** combines the user's two main stacks in a full web app for the first time.
- **Date:** 2026-09-01

## ADR-001 — Single-user, no auth
- **Decision:** App is single-user only; no authentication/identity system.
- **Status:** Confirmed by user during Problem 1 (Spotify playlist import) Phase 1.
- **Rationale:** Self-hosted personal tool, matches stated deployment scope; simplifies every future context (no per-user data partitioning needed).
- **Date:** 2026-09-01

## ADR-002 — Library persists client-side (IndexedDB)
- **Decision:** User's Library (Playlists, Tracks) is stored client-side via IndexedDB, not in a server-side database.
- **Status:** Confirmed by user during Problem 1 Phase 1.
- **Rationale:** Consistent with single-user/self-hosted posture and the stated offline-mode goal; server stays stateless re: user data.
- **Date:** 2026-09-01

## ADR-003 — Spotify auth via Client Credentials (no user OAuth login)
- **Decision:** Spotify Integration context uses the Client Credentials flow (app-level auth), limited to public playlists. No per-user Spotify account login/OAuth.
- **Status:** Confirmed by user during Problem 1 Phase 1.
- **Rationale:** Matches single-user/no-auth posture; avoids building a login flow for a personal tool. Trade-off: private playlists cannot be imported.
- **Date:** 2026-09-01

## ADR-004 — Playlist import is metadata-only; download is a separate future problem
- **Decision:** The "import" operation (Problem 1) fetches and stores Playlist/Track metadata only. It does not trigger YouTube resolution or audio download.
- **Status:** Confirmed by user during Problem 1 Phase 1.
- **Rationale:** Keeps bounded contexts clean (Spotify Integration vs. future Track Resolution/Download); one problem at a time per the ddd-ucd-dev workflow.
- **Date:** 2026-09-01

## ADR-005 — Track is a shared, deduplicated Entity across Playlists
- **Decision:** Track has stable identity (spotifyId) and is stored once in the Library; multiple Playlists reference the same Track via trackIds rather than each holding its own copy.
- **Status:** Confirmed by user during Problem 1 Phase 1.
- **Rationale:** Avoids duplicate storage for tracks appearing in multiple imported playlists; keeps Track ready for later YouTube-resolution reuse across playlists.
- **Date:** 2026-09-01

## ADR-006 — Refresh fails silently on unavailable playlist
- **Decision:** If a Refresh cannot reach the Spotify-side playlist (private/deleted), the operation fails silently: old local data is kept, no PlaylistRefreshed event fires, no error is surfaced to the user.
- **Status:** Confirmed by user during Problem 1 Phase 1.
- **Rationale:** User's explicit choice. Trade-off noted in open-issues.md: user has no visible signal that a refresh silently did nothing.
- **Date:** 2026-09-01

## ADR-007 — Refresh API contract: no explicit success/failure signal field
- **Decision:** `POST /api/playlists/refresh` always returns HTTP 200. On a Spotify-side silent fail (ADR-006), the response body echoes the caller's own unchanged state with empty `tracks`/`tracksAdded`/`tracksRemoved` — no `refreshSucceeded` or similar flag.
- **Status:** Confirmed by user during Problem 1 Phase 2. User was explicitly offered the alternative (a `refreshSucceeded: false` debug field) and declined it.
- **Rationale:** Keeps the silent-fail policy (ADR-006) genuinely silent end-to-end, including at the wire level — no back door for the client to detect and react to it.
- **Open follow-up (unresolved):** this ADR covers only the Spotify-side-unavailable case. Whether *network/server* errors on Refresh (backend unreachable, 5xx, etc.) should also be silent, or should surface a visible signal (e.g. a toast), was raised during Phase 2 and is **not yet decided** — see open-issues.md item 8. Do not assume either answer.
- **Date:** 2026-09-01
