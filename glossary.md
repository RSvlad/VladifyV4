# Glossary — Vladify v4

Ubiquitous language for the domain. Confirmed terms are agreed via Phase 1 discussion per problem.

## Confirmed terms (Problem 1: Spotify playlist import)

- **Playlist** — Entity. A Spotify playlist mirrored into the local Library. Holds spotifyId, name, ownerName, trackIds (references), importedAt, lastRefreshedAt.
- **Track** — Entity with stable identity (spotifyId). Holds title, artist(s), album, durationMs. Shared across playlists — stored once in Library, referenced by any number of Playlists (deduplicated by spotifyId).
- **Library** — the user's client-side (IndexedDB) collection of imported Playlists and Tracks. No server-side persistence of user data.
- **Import** — the act of fetching a Spotify playlist's metadata (via Client Credentials, public playlists only) and storing it in the Library for the first time. Metadata only — does NOT trigger download/track-resolution.
- **Refresh** — re-fetching an already-imported Playlist to reconcile Track additions/removals against the Library. If the Spotify-side playlist is unavailable (private/deleted), Refresh fails silently and keeps the existing local version — no error surfaced to the user.
- **PlaylistImported** — Domain Event. Fired when a Playlist is imported for the first time.
- **PlaylistRefreshed** — Domain Event. Fired on a successful Refresh; carries the diff (Tracks added/removed).

## Candidate terms (still unconfirmed — deferred to later problems)

- **Sync** — broader reconciliation concept; Refresh (above) covers the Problem 1 scope, but a fuller multi-playlist Sync concept may still be introduced later.
- **Offline mode** — app usability state where the Library is playable without network access. Not addressed yet.

These remain placeholders until their own Phase 1 discussion.

## Confirmed terms (Problem 2: Track Resolution)

- **Track Resolution** — the bounded context and the act of matching a Track (already in the Library, from Spotify) to a YouTube video. Produces a YouTube match (video id, title, channel, confidence score) attached to the existing Track record — does not create a new entity and does not fetch/download audio.
- **Resolve** — the use case that performs Track Resolution for a single Track: search YouTube (via yt-dlp), score candidates, apply the best if it's confident enough.
- **Resolve (batch)** — the same, run over a list of Tracks (e.g. a whole Playlist's unresolved Tracks). Never stops early on an individual failure; always returns every Track's outcome as a resolved/failed summary.
- **Match confidence** — a 0–1 score produced by the matching heuristic (title/artist similarity + duration proximity). Below the confidence threshold, the Track is left unresolved rather than attached to a low-quality guess.
- **Resolved / Unresolved** — a Track's state with respect to Track Resolution. Resolved means it has a YouTube video match; Unresolved means it doesn't (either never attempted, or attempted and no confident match/no candidates found).
- **TrackResolved** — Domain Event. Fired when a Track is successfully resolved to a YouTube video.

## Candidate terms (Problem 2, deferred)

- **Download job** — the process of fetching audio for an already-resolved Track via yt-dlp. Deferred to a future problem (Download/Acquisition) — Track Resolution stops at finding the video, not fetching it.
