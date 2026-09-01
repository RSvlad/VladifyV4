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

## Not yet written
- No `main.tsx`/`App.tsx`/root frontend entry point or any actual UI component (input field, import button, playlist view) — only the data/hook layer exists so far.
- No backend or frontend automated tests (Phase 3, not started).
- No `.gitignore`, README, or Docker/deployment files.
