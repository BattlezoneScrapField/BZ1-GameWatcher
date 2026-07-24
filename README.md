# BZ1 Game Watcher

Live lobby list for **Battlezone 98 Redux**, running at [bz98gamewatcher.com](https://bz98gamewatcher.com).

The site shows the games currently open, who is in them, and lets you jump straight into a lobby
through Steam.

## How it works

```
Rebellion lobby server ──websocket──▶ API (ASP.NET Core 8) ──REST──▶ Web (Angular 18) ──▶ nginx
```

- **API** (`API/`) holds one long-lived websocket connection to the Battlezone lobby server, keeps
  an in-memory snapshot of the lobby list, enriches players with their Steam avatars, and serves it
  at `GET /api/BZ98Lobby`.
- **Web** (`Web/`) polls that endpoint every few seconds and renders the game list.
- **nginx** (`nginx/`) terminates TLS, serves the built Angular bundle, and proxies `/api/` to the
  API container. The API is not published outside the Docker network.

## Requirements

- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- [Node.js 22](https://nodejs.org/) (LTS)
- Docker and Docker Compose, for running the full stack

## Configuration

The API reads its settings from `API/appsettings.json`, environment variables, and user secrets.

| Setting | Environment variable | Description |
| --- | --- | --- |
| `Steam:ApiKey` | `Steam__ApiKey` | Steam Web API key, from <https://steamcommunity.com/dev/apikey>. Without it the site still works, but players show without avatars. |
| `Cors:AllowedOrigins` | `Cors__AllowedOrigins__0` | Origins allowed to call the API directly. Not needed in production, where nginx makes the API same-origin. |
| `Battlezone:LobbyServerUrl` | `Battlezone__LobbyServerUrl` | Websocket endpoint of the lobby server. |
| `Battlezone:FlaggedSteamIds` | `Battlezone__FlaggedSteamIds__0` | Steam IDs marked with `isDangerous` in the API response. Empty by default. |

Keep the key out of source control. For local development, use user secrets:

```bash
cd API
dotnet user-secrets set "Steam:ApiKey" "<your key>"
```

For Docker Compose, put it in a `.env` file next to `docker-compose.yml` (git-ignored):

```
STEAM_API_KEY=<your key>
```

## Running locally

API — <http://localhost:5283>, with Swagger UI at the root:

```bash
cd API
dotnet run
```

Web — <http://localhost:4200>:

```bash
cd Web
npm ci
npm start
```

The dev server calls `/api/` on its own origin, so point it at the API with a proxy or run the
full stack under Docker Compose.

## Tests

```bash
cd Web
npm run test:ci     # headless, used by CI
npm test            # watch mode
```

## Running the full stack

```bash
docker compose up --build
```

This starts nginx (ports 80/443), the API (internal only), and a certbot container that renews the
TLS certificate every 12 hours. Certificates live in `certbot/conf/`.

## Deployment

Pushing to `main` builds and tests both projects, then publishes images to GHCR tagged `latest` and
with the commit SHA:

- `ghcr.io/jj173/battlezone-api-ghcr`
- `ghcr.io/jj173/battlezone-web-ghcr`

Pull requests build and test without publishing.
