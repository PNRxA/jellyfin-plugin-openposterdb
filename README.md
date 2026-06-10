# Jellyfin Plugin — OpenPosterDB

A Jellyfin remote image provider for [OpenPosterDB](https://github.com/PNRxA/openposterdb). It fetches
posters, backdrops, logos and episode stills — with rating badges overlaid — from your OpenPosterDB
instance and offers them as artwork for items in your library.

The instance Base URL and API key are configurable, so the plugin points at whichever OpenPosterDB
instance you run.

For each library item, it asks Jellyfin to fetch artwork from:

```
{BaseUrl}/{ApiKey}/{id_type}/{poster|backdrop|logo|episode}-default/{id_value}.{jpg|png}
```

…resolving `id_type`/`id_value` from the item's IMDb / TMDB / TVDB ids.

## Features

- **Movies** — poster (Primary), backdrop, logo
- **Series** — poster (Primary), backdrop, logo
- **Episodes** — per-episode still with episode-level rating badges
- Configurable **Base URL** + **API key**, validated against `/{ApiKey}/isValid`
- Per-image-type toggles
- Optional **extra query** passthrough for OpenPosterDB styling params
  (`lang`, `imageSize`, `badge_size`, `ratings_limit`, …)

## Install (plugin repository)

1. In Jellyfin: **Dashboard → Plugins → Repositories → +**
2. Add this repository manifest URL:
   ```
   https://raw.githubusercontent.com/PNRxA/jellyfin-plugin-openposterdb/main/manifest.json
   ```
3. **Dashboard → Plugins → Catalog → OpenPosterDB → Install**, then restart Jellyfin.
4. **Dashboard → Plugins → OpenPosterDB** and set:
   - **Base URL** — e.g. `https://openposterdb.com` or `http://your-host:3000`
   - **API key** — your 64-character OpenPosterDB key
   - Click **Test connection** to validate the key.
5. For each **Movies** / **Shows** library: **Manage Library → Edit → Image Fetchers**, drag
   **OpenPosterDB** to the top so it wins.
6. To backfill existing items: **Scan Library** with **“Replace existing images”** enabled.

> Jellyfin downloads the chosen image **server-side**, so your OpenPosterDB instance only needs to be
> reachable from the Jellyfin server (the “Test connection” button, however, runs in your browser).

## How IDs are mapped

| Item | Preferred id | Example OpenPosterDB path |
|------|--------------|---------------------------|
| Movie | IMDb → TMDB → TVDB | `/imdb/poster-default/tt0111161.jpg`, `/tmdb/poster-default/movie-550.jpg` |
| Series | IMDb → TMDB → TVDB | `/tmdb/poster-default/series-1396.jpg` |
| Episode | parent series id + S/E | `/imdb/episode-default/episode-tt0903747-S1E1.jpg` |

IMDb is preferred because it is the most reliably populated id and OpenPosterDB cross-resolves it via
TMDB. Logos are requested as `.png`; everything else as `.jpg`.

## Build from source

Requires the .NET 8 SDK.

```bash
dotnet build --configuration Release
# -> Jellyfin.Plugin.OpenPosterDB/bin/Release/net8.0/Jellyfin.Plugin.OpenPosterDB.dll
```

To produce an installable plugin zip (matching the manifest), use
[`jprm`](https://github.com/oddstr13/jellyfin-plugin-repository-manager):

```bash
pip install jprm
jprm plugin build .
```

### Releasing

`git tag v1.0.0.0 && git push origin v1.0.0.0` triggers `.github/workflows/release.yml`, which builds
the zip and attaches it to a GitHub Release. Then update `manifest.json`:

- `versions[].sourceUrl` → the uploaded asset URL
- `versions[].checksum` → the MD5 printed in the release job logs
- `versions[].timestamp` → the release time (ISO-8601)

## Jellyfin version / ABI

Targets **Jellyfin 10.11.x** (net9.0, `Jellyfin.Controller` 10.11.11, `targetAbi` 10.11.0.0).

The published `manifest.json` also keeps a **10.10.x** build (`1.0.0.1`, net8.0, `targetAbi`
10.10.3.0); Jellyfin automatically installs the newest version whose `targetAbi` your server
satisfies, so 10.10 servers get `1.0.0.1` and 10.11 servers get `1.1.0.0`.

To re-target another Jellyfin line: set `<TargetFramework>` (net9.0 for 10.11, net8.0 for 10.10) and
the `Jellyfin.Controller` version in the `.csproj`, and update `targetAbi` / `framework` in
`build.yaml` and `manifest.json` to match.

## Troubleshooting

- **No OpenPosterDB images appear** — raise OpenPosterDB in the library's *Image Fetchers* order and
  re-scan with *Replace existing images*. Jellyfin auto-applies the first enabled fetcher for
  single-value image types (Primary/Logo).
- **“Test connection” fails but the key is valid** — the test runs in your *browser*; make sure the
  Base URL is reachable from where you opened the dashboard. The actual image fetch happens on the
  *server* and only needs server→instance reachability.
- **Episode stills missing** — OpenPosterDB needs the series id + season/episode numbers; items with
  no season/episode metadata are skipped. IMDb episode ratings additionally require an OMDb key on the
  OpenPosterDB side.

## License

[MIT](LICENSE) — not affiliated with the Jellyfin project.
