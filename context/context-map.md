# Context Map — Vladify v4

## Confirmed bounded contexts

- **Spotify Integration** — CONFIRMED (Problem 1). Scope: fetch playlist/track metadata via Spotify Web API, Client Credentials flow (public playlists only, no user OAuth login), server-side (C#) to keep secret off the client. Produces Playlist/Track metadata for the Library.
- **Library** — CONFIRMED (Problem 1, partial). Scope: client-side (IndexedDB) persistence of imported Playlists and Tracks, deduplication of Tracks by spotifyId. Full sync/offline behavior still TBD in later problems.

## Candidate contexts (unconfirmed, deferred)

- **Track Resolution** — mapping a Spotify track to a YouTube source. Explicitly deferred from Problem 1 (import = metadata only).
- **Download/Acquisition** — running youtube-dl/yt-dlp jobs, file handling. Deferred.
- **Playback / Offline** — client-side playback and offline availability. Deferred.

Each candidate context gets its own Phase 1 discussion before design decisions are made.
