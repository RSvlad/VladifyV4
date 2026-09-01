# Domain Model — Vladify v4

## Problem 1: Spotify playlist import (Phase 1 agreed, awaiting Phase 2)

### Entities
- **Playlist** — spotifyId, name, ownerName, trackIds (references to Track), importedAt, lastRefreshedAt.
- **Track** — spotifyId (stable identity), title, artist(s), album, durationMs. Deduplicated in Library by spotifyId; referenced by any number of Playlists (many-to-many via Playlist.trackIds).

### Domain Events
- **PlaylistImported** — raised on first successful import of a Playlist.
- **PlaylistRefreshed** — raised on successful re-fetch of an existing Playlist; carries diff (Tracks added/removed). Not raised if the refresh fails (silent fail, per UCD decision — old data kept, no event, no user-facing error).

### Not yet modeled (deferred to future problems)
- Track-to-YouTube resolution (separate bounded context: Track Resolution).
- Download/Acquisition (yt-dlp jobs).
- Offline playback state.

No other Aggregates/Entities/VOs agreed yet for other problems — populated per-problem after each Phase 1.
