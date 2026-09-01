# Vladify

Самостално хостована веб-апликација за увоз Spotify плејлиста и (у каснијим фазама) преузимање нумера преко YouTube-а, уз локалну библиотеку доступну и офлајн.

## Историја пројекта

- **v1 (2022)** — прва верзија, писана у Xamarinu. Ово ми је уједно био и први Android пројекат.
- **v2** — портовано на .NET MAUI, задржавајући исти основни концепт.
- **v3** — визуелно реконструисана верзија MAUI апликације, са проширеним функцијама у односу на v2.
- **v4 (текућа)** — потпуно нови приступ: уместо мобилне апликације, Vladify постаје самостално хостована веб-апликација. Духовни наследник претходних верзија, али одвојен стек — TypeScript/React фронтенд и C#/.NET бекенд, дизајниран доменски (DDD приступ) и документован кроз `context/`.

## Стек

- **Бекенд:** C# / .NET, слојевита архитектура (`Vladify.Domain`, `Vladify.Application`, `Vladify.Infrastructure`, `Vladify.Api`)
- **Фронтенд:** TypeScript, React, Vite, IndexedDB (преко `idb`)
- **Интеграције:** Spotify Web API (Client Credentials flow, само јавне плејлисте), youtube-dl/yt-dlp (планирано, за преузимање нумера)

## Архитектура

Бекенд је организован по слојевима:

- `Vladify.Domain` — ентитети (`Playlist`, `Track`), value objecti (`SpotifyPlaylistId`, `SpotifyTrackId`) и domain eventi (`PlaylistImported`, `PlaylistRefreshed`)
- `Vladify.Application` — use case-ови (`ImportPlaylistUseCase`, `RefreshPlaylistUseCase`) и портови (`ISpotifyPlaylistReader`)
- `Vladify.Infrastructure` — имплементација комуникације са Spotify Web API-jem
- `Vladify.Api` — минимални API endpoint-и (`/api/playlists/import`, `/api/playlists/refresh`)

Библиотека корисника (увезене плејлисте и нумере) чува се искључиво клијентски, у IndexedDB — бекенд не персистира корисничке податке, само проксира позиве ка Spotify-ju.

## Статус

Пројекат је у активном развоју. Тренутно у раду: **Problem 1 — увоз Spotify плејлиста**, бекенд и почетни фронтенд скелет су написани; преостаје UI за окидање увоза и тестирање.

Детаљна документација домена, одлука (ADR) и отворених питања налази се у `context/` фасцикли (`index.md`, `domain-model.md`, `context-map.md`, `adr-log.md`, `open-issues.md`).

## Покретање

### Бекенд

```bash
cd backend
dotnet build
dotnet run --project src/Vladify.Api
```

### Фронтенд

```bash
cd frontend
npm install
npm run dev
```
