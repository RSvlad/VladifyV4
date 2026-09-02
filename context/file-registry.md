# File Registry — Vladify v4

Review status per file touched. All written 2026-09-01 during Problem 1 (Spotify playlist import) Phase 2 — status: **new, not yet reviewed by user**.

## Backend

### Vladify.Domain
- `backend/src/Vladify.Domain/Vladify.Domain.csproj` — new
- `backend/src/Vladify.Domain/Tracks/SpotifyTrackId.cs` — new
- `backend/src/Vladify.Domain/Tracks/Track.cs` — new
- `backend/src/Vladify.Domain/Playlists/SpotifyPlaylistId.cs` — new
- `backend/src/Vladify.Domain/Playlists/PlaylistImported.cs` — new
- `backend/src/Vladify.Domain/Playlists/PlaylistRefreshed.cs` — new
- `backend/src/Vladify.Domain/Playlists/Playlist.cs` — new

### Vladify.Application
- `backend/src/Vladify.Application/Vladify.Application.csproj` — new
- `backend/src/Vladify.Application/Playlists/ISpotifyPlaylistReader.cs` — new
- `backend/src/Vladify.Application/Playlists/ImportPlaylistUseCase.cs` — new (revised once: added `Tracks` to result, per user correction)
- `backend/src/Vladify.Application/Playlists/RefreshPlaylistUseCase.cs` — new (revised once: added `Tracks` to result, per user correction)

### Vladify.Infrastructure
- `backend/src/Vladify.Infrastructure/Vladify.Infrastructure.csproj` — new
- `backend/src/Vladify.Infrastructure/Spotify/SpotifyOptions.cs` — new
- `backend/src/Vladify.Infrastructure/Spotify/SpotifyTokenProvider.cs` — new
- `backend/src/Vladify.Infrastructure/Spotify/SpotifyPlaylistReader.cs` — new
- `backend/src/Vladify.Infrastructure/Spotify/SpotifyServiceCollectionExtensions.cs` — new

### Vladify.Api
- `backend/src/Vladify.Api/Vladify.Api.csproj` — new
- `backend/src/Vladify.Api/Program.cs` — new
- `backend/src/Vladify.Api/appsettings.json` — new (Spotify ClientId/ClientSecret left empty — user must fill in)
- `backend/src/Vladify.Api/Playlists/PlaylistDtos.cs` — new (revised once: added `Tracks`/`TrackDto` to response DTOs, per user correction)
- `backend/src/Vladify.Api/Playlists/PlaylistEndpoints.cs` — new (revised once: wire up new Tracks field)

### Solution
- `backend/Vladify.sln` — new

## Frontend
- `frontend/package.json` — new
- `frontend/tsconfig.json` — new
- `frontend/vite.config.ts` — new
- `frontend/src/playlists/types.ts` — new
- `frontend/src/playlists/api.ts` — new (revised once: `ImportPlaylistResponse`/`RefreshPlaylistResponse` now carry `tracks`)
- `frontend/src/library/db.ts` — new
- `frontend/src/playlists/usePlaylistImport.ts` — new, revised once: `doRefresh`'s catch block now calls `setError(...)` on network/server failure (visible signal, per ADR-008), distinct from the silent Spotify-side-unavailable case. Local playlist still returned unchanged.

## Frontend UI (added 2026-09-02)
- `frontend/index.html` — new
- `frontend/src/main.tsx` — new
- `frontend/src/App.tsx` — new (loads Library on mount via `getAllPlaylists`, wires `usePlaylistImport`)
- `frontend/src/App.css` — new
- `frontend/src/index.css` — new
- `frontend/src/components/ImportForm.tsx` — new (accepts raw ID, playlist URL, or spotify: URI)
- `frontend/src/components/PlaylistCard.tsx` — new (renders a Playlist + Refresh button)
- `frontend/src/components/ErrorToast.tsx` — new (visible surface for the hook's `error` state, ADR-008)
- `frontend/src/playlists/usePlaylistImport.ts` — revised: added `clearError` to the returned API so the UI can dismiss the toast without a page reload

Status: **verified 2026-09-02 — `npm install`/`npm run build` (frontend) and `dotnet build` (backend) all pass.**

## Tests (Phase 3, added 2026-09-02)
- `backend/tests/Vladify.Domain.Tests/Vladify.Domain.Tests.csproj` — new
- `backend/tests/Vladify.Domain.Tests/Playlists/PlaylistTests.cs` — new (6 tests: Refresh diff logic, Import validation)
- `backend/tests/Vladify.Application.Tests/Vladify.Application.Tests.csproj` — new
- `backend/tests/Vladify.Application.Tests/Playlists/ImportPlaylistUseCaseTests.cs` — new (2 tests)
- `backend/tests/Vladify.Application.Tests/Playlists/RefreshPlaylistUseCaseTests.cs` — new (3 tests)
- `backend/Vladify.sln` — revised: added both test projects
- `frontend/package.json` — revised: added vitest/jsdom/fake-indexeddb/@testing-library/react devDeps + `test` script
- `frontend/vite.config.ts` — revised: added `test` block (jsdom env, setup file)
- `frontend/src/test/setup.ts` — new (imports `fake-indexeddb/auto`)
- `frontend/src/playlists/usePlaylistImport.test.tsx` — new (5 tests: doImport success/not-found, doRefresh network-failure/success, clearError); revised once — added a `beforeEach` that replaces `globalThis.indexedDB` with a fresh `IDBFactory` and calls `__resetDbForTests()`, fixing cross-test state leakage (fake-indexeddb tests share one DB instance by default, and `library/db.ts` caches its handle at module scope)
- `frontend/src/library/db.ts` — revised: added `__resetDbForTests()` test-only export to drop the cached DB handle

Status: **backend all passing (`dotnet test`). Frontend: all 5 passing after the IndexedDB-reset fix.**

### Security fix (2026-09-02)
`npm install` flagged 5 vulnerabilities (3 moderate, 1 high, 1 critical), transitively from `esbuild@0.21.5` bundled by `vite@8.2.2`/`vitest@2.1.4` (dev-server request-exposure advisories — build/test-tooling only, no production runtime exposure since the frontend ships a static build). Fixed by bumping `vitest` from `^2.1.4` to `^4.1.9` in `frontend/package.json` (skips the v3 line; pulls a newer esbuild transitively that resolves the flagged advisories). Vitest 4's breaking changes (pool/workspace/browser-mode config, `vi.restoreAllMocks()` semantics) don't affect this project's config or `usePlaylistImport.test.tsx` (no `poolOptions`/`workspace`/browser mode used; `vi.spyOn` mocks are unaffected by the `vi.restoreAllMocks()` semantics change). **User needs to run `npm install` again and re-run `npm test` to confirm the bump is clean and `npm audit` is 0.**

## Not yet written
- No `.gitignore` review, Docker/deployment files.
- No loading state for the initial Library read in `App.tsx` (loads synchronously via useEffect; acceptable for IndexedDB's speed but not explicitly discussed).
- No API-layer (`Vladify.Api`) or Infrastructure-layer (`SpotifyPlaylistReader`, `SpotifyTokenProvider`) tests — Phase 3 scope was Application + Domain + the frontend hook; endpoint/HTTP-integration and live-Spotify-call tests were not part of the agreed 11 scenarios.

---

## Problem 2: Track Resolution (files touched 2026-09-02)

### Backend — Vladify.Domain
- `backend/src/Vladify.Domain/Tracks/Track.cs` — revised: added optional resolution fields (`YouTubeVideoId`, `YouTubeTitle`, `YouTubeChannelName`, `MatchConfidence`, `ResolvedAt`, `IsResolved`) and the `ResolveTo(...)` method + doc comment update (no longer says resolution is out of scope).
- `backend/src/Vladify.Domain/Tracks/YouTubeVideoId.cs` — new.
- `backend/src/Vladify.Domain/Tracks/TrackResolved.cs` — new (Domain Event).

### Backend — Vladify.Application
- `backend/src/Vladify.Application/Tracks/IYouTubeTrackSearcher.cs` — new (port + `YouTubeSearchCandidate`).
- `backend/src/Vladify.Application/Tracks/TrackMatchScorer.cs` — new (pure heuristic function).
- `backend/src/Vladify.Application/Tracks/ResolveTrackUseCase.cs` — new (`ResolveTrackResult`, `TrackResolutionFailureReason`).
- `backend/src/Vladify.Application/Tracks/ResolveTracksBatchUseCase.cs` — new (`ResolveTracksBatchResult`).

### Backend — Vladify.Infrastructure
- `backend/src/Vladify.Infrastructure/YouTube/YouTubeOptions.cs` — new.
- `backend/src/Vladify.Infrastructure/YouTube/YtDlpTrackSearcher.cs` — new (shells out to `yt-dlp --dump-json --flat-playlist`; never throws, returns `[]` on any failure).
- `backend/src/Vladify.Infrastructure/YouTube/YouTubeServiceCollectionExtensions.cs` — new.

### Backend — Vladify.Api
- `backend/src/Vladify.Api/Tracks/TrackResolutionDtos.cs` — new.
- `backend/src/Vladify.Api/Tracks/TrackResolutionEndpoints.cs` — new (`POST /api/tracks/resolve`, `POST /api/tracks/resolve-batch`).
- `backend/src/Vladify.Api/Program.cs` — revised: registered YouTube integration + new use cases, mapped the new endpoints.
- `backend/src/Vladify.Api/appsettings.json` — revised: added `YouTube` section (`YtDlpPath`, `TimeoutSeconds`).

### Frontend
- `frontend/src/playlists/types.ts` — revised: `Track` interface extended with optional `youTubeVideoId`/`youTubeTitle`/`youTubeChannelName`/`matchConfidence`/`resolvedAt`.
- `frontend/src/tracks/api.ts` — new (`resolveTrack`, `resolveTracksBatch`).
- `frontend/src/tracks/useTrackResolution.ts` — new (`doResolve`, `doResolveBatch`, `lastBatchSummary`, `clearError`, `clearBatchSummary`).
- `frontend/src/components/TrackRow.tsx` — new (per-Track resolved/unresolved display + "Find on YouTube").
- `frontend/src/components/BatchResolutionSummaryToast.tsx` — new.
- `frontend/src/components/PlaylistCard.tsx` — revised: now accepts `tracks`, shows/hides a track list, adds a batch-resolve button for unresolved tracks. Props changed (`isLoading` → `isRefreshing`, new resolution-related props) — a breaking change to this component's interface, applied because it's only consumed by `App.tsx` in this codebase.
- `frontend/src/App.tsx` — revised: loads Tracks per Playlist via `getTracksByIds`, wires `useTrackResolution`, merges error/summary toasts from both hooks.
- `frontend/src/App.css` — revised: added `.track-list`/`.track-row*`/`.batch-summary-toast`/`.playlist-card__actions` styles; renamed the `.playlist-card button` selector to `.playlist-card__actions button` to match the new markup.
- `frontend/src/vite-env.d.ts` — new. **Fixes a pre-existing Problem 1 gap**, not part of Problem 2's scope: this file (the standard Vite-generated `/// <reference types="vite/client" />`) was missing, so `import.meta.env` had no type and `npm run build`'s `tsc -b` step failed. Undetected during Problem 1 because Phase 3 verification only ran `npm test` (Vitest doesn't need this reference), not `npm run build`.

## Tests (Phase 3, added 2026-09-02)
- `backend/tests/Vladify.Domain.Tests/Tracks/TrackResolutionTests.cs` — new (4 tests: `ResolveTo` field-setting, event, overwrite-on-retry, confidence-range validation, unresolved-by-default).
- `backend/tests/Vladify.Application.Tests/Tracks/TrackMatchScorerTests.cs` — new (6 tests: exact match scores high, unrelated title scores low, unknown duration is neutral not zero, best-of-multiple selection, empty list returns null, far-duration falloff to zero).
- `backend/tests/Vladify.Application.Tests/Tracks/ResolveTrackUseCaseTests.cs` — new (3 tests: confident match resolves, low-confidence leaves unresolved, no candidates found).
- `backend/tests/Vladify.Application.Tests/Tracks/ResolveTracksBatchUseCaseTests.cs` — new (2 tests: mixed results don't stop early, all-fail returns full failed list).
- `frontend/src/tracks/useTrackResolution.test.tsx` — new (5 tests: resolve success persists to Library, no-confident-match sets informational error and writes nothing, network failure sets generic error, batch summary accuracy + selective persistence, clearError/clearBatchSummary are independent). One iteration during Phase 3: the first draft of the "independent reset" test asserted state in the wrong order — `doResolveBatch`'s `setError(null)` on entry cleared the earlier `doResolve` error before the assertion ran. Fixed by reordering the calls/assertions to check each hook action's effect before the next one overwrites shared-but-unrelated state.

Status: **backend all passing (`dotnet test`, 21 total including Problem 1's 11). Frontend all passing (`npm test`, 10 total including Problem 1's 5).**
