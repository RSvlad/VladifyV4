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
- **Download job** — the process of resolving a Track to a YouTube source and fetching audio via youtube-dl/yt-dlp. Deferred to a future problem (Track Resolution / Download).
- **Offline mode** — app usability state where the Library is playable without network access. Not addressed in Problem 1.

These remain placeholders until their own Phase 1 discussion.
