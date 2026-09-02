# Domain Model — Vladify v4

## Problem 1: Spotify playlist import (COMPLETE — all 3 phases done)

### Entities
- **Playlist** — spotifyId, name, ownerName, trackIds (references to Track), importedAt, lastRefreshedAt.
- **Track** — spotifyId (stable identity), title, artist(s), album, durationMs. Deduplicated in Library by spotifyId; referenced by any number of Playlists (many-to-many via Playlist.trackIds).

### Domain Events
- **PlaylistImported** — raised on first successful import of a Playlist.
- **PlaylistRefreshed** — raised on successful re-fetch of an existing Playlist; carries diff (Tracks added/removed). Not raised if the refresh fails (silent fail, per UCD decision — old data kept, no event, no user-facing error).

### Not yet modeled (deferred to future problems)
- Download/Acquisition (yt-dlp jobs to actually fetch audio for a resolved Track).
- Offline playback state.

No other Aggregates/Entities/VOs agreed yet for other problems — populated per-problem after each Phase 1.

## Problem 2: Track Resolution (COMPLETE — all 3 phases done)

### Entities (extended)
- **Track** — extended with optional resolution fields: `YouTubeVideoId?`, `YouTubeTitle?`, `YouTubeChannelName?`, `MatchConfidence?` (0–1), `ResolvedAt?`, plus a derived `IsResolved` flag. Resolution is an attribute of the existing Track, not a separate entity — a Track is either resolved or it isn't. `Track.ResolveTo(...)` applies a match and overwrites any previous one (re-resolving/retrying always replaces, never accumulates).

### Value Objects
- **YouTubeVideoId** — stable identity for a YouTube video, distinct type from SpotifyTrackId so the two identity spaces can't be confused at compile time.

### Domain Events
- **TrackResolved** — raised by `Track.ResolveTo` on a successful match; carries SpotifyTrackId, YouTubeVideoId, MatchConfidence, ResolvedAt.
- No event for the failure case — "no confident match" and "no candidates found" are represented as a `TrackResolutionFailureReason` result value (Application layer), not a domain event, since nothing changed on the Track.

### Application-layer heuristic
- **TrackMatchScorer** (pure function, no I/O) — weighted score: 0.7 × title/artist token-overlap similarity + 0.3 × duration proximity (±5s tolerance, linear falloff over the next 30s, floor 0; unknown candidate duration scores a neutral 0.5 rather than being penalized). `ConfidenceThreshold = 0.5` — below this, the Track is left unresolved rather than attached to a low-quality guess.

### Use cases
- **ResolveTrackUseCase** — one Track: query yt-dlp search, score candidates, apply the best if it clears the threshold, else return a typed failure reason (`NoCandidatesFound` / `NoConfidentMatch`).
- **ResolveTracksBatchUseCase** — many Tracks: runs each independently, never stops early, always returns a full `{ Resolved, Failed }` summary.

### Not yet modeled (deferred to future problems)
- Download/Acquisition (yt-dlp jobs to actually fetch audio for a resolved Track) — Track Resolution only finds the YouTube video, does not fetch it.
- Offline playback state.
- Manual candidate selection (Phase 1 decision: fully automatic heuristic, no user-facing candidate list) — revisit only if the heuristic proves unreliable in practice.
