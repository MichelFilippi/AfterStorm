# AfterStorm

AfterStorm is a community match-analysis backend for Heroes of the Storm.

## Current milestone

This initial skeleton provides:

- .NET 10 Web API
- Domain/Application/Infrastructure separation
- PostgreSQL via EF Core + Npgsql
- `.StormReplay` upload endpoint
- SHA-256 replay deduplication
- local replay storage abstraction
- domain models for matches, players and talent selections
- placeholder `StormReplayParser` ready for the next milestone
- Docker + Docker Compose

## Structure

```text
AfterStorm/
  src/
    AfterStorm.Api/
    AfterStorm.Application/
    AfterStorm.Domain/
    AfterStorm.Infrastructure/
```

## Local startup

Requirements:

- .NET 10 SDK
- Docker Desktop (recommended for PostgreSQL)

Start only PostgreSQL:

```bash
docker compose up -d postgres
```

Create the EF Core migration after installing the EF tool:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate \
  --project src/AfterStorm.Infrastructure \
  --startup-project src/AfterStorm.Api

dotnet ef database update \
  --project src/AfterStorm.Infrastructure \
  --startup-project src/AfterStorm.Api
```

Run the API:

```bash
dotnet run --project src/AfterStorm.Api
```

Health endpoint:

```text
GET http://localhost:5180/health
```

Replay upload:

```text
POST /api/replays
Content-Type: multipart/form-data
field: replay
```

Example with curl:

```bash
curl -F "replay=@C:/Replays/game.StormReplay" http://localhost:5180/api/replays
```

At the current milestone the upload will intentionally return HTTP 501 after saving and hashing the replay because the real replay parser is the next component to be implemented.

## Next milestone

Connect Blizzard `heroprotocol` through a small parser process that outputs normalized JSON:

```text
.StormReplay
   -> heroprotocol
   -> details / tracker events / game events
   -> normalized JSON
   -> StormReplayParser (C#)
   -> PostgreSQL
```

The first events to normalize will be:

- match metadata
- players and heroes
- winner/team
- score statistics
- `TalentChosen`
- talent timestamp based on game loop
