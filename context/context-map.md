# Context Map — Vladify v4

## Confirmed bounded contexts

- **Spotify Integration** — CONFIRMED (Problem 1). Scope: fetch playlist/track metadata via Spotify Web API, Client Credentials flow (public playlists only, no user OAuth login), server-side (C#) to keep secret off the client. Produces Playlist/Track metadata for the Library.
- **Library** — CONFIRMED (Problem 1, partial). Scope: client-side (IndexedDB) persistence of imported Playlists and Tracks, deduplication of Tracks by spotifyId. Full sync/offline behavior still TBD in later problems.
- **Track Resolution** — CONFIRMED (Problem 2). Scope: matching a Spotify Track to a YouTube video via yt-dlp search + a weighted heuristic (title/artist similarity + duration proximity), server-side (C#), same statelessness pattern as Spotify Integration. Produces YouTube match metadata (video id, title, channel, confidence) that the client persists as fields on the existing Track record in its Library. Explicitly does not fetch/download audio — that remains Download/Acquisition's job.

## Candidate contexts (unconfirmed, deferred)

- **Download/Acquisition** — running youtube-dl/yt-dlp jobs to fetch and store audio for a Track already resolved to a YouTube video, file handling. Deferred.
- **Playback / Offline** — client-side playback and offline availability. Deferred.

Each candidate context gets its own Phase 1 discussion before design decisions are made.
