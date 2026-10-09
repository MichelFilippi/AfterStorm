# AfterStorm - integração do hots-parser

Esta versão conecta o upload de `.StormReplay` ao `hotsdata/hots-parser`.

## Estrutura esperada

```text
AfterStorm/
├─ hots-parser/
│  ├─ main.py
│  └─ .venv/Scripts/python.exe
└─ src/
   └─ AfterStorm.Api/
```

## Fluxo

1. `POST /api/replays` recebe o `.StormReplay`.
2. O replay é salvo em `replays/`.
3. `StormReplayParser` executa `hots-parser/main.py --dump-payloads`.
4. Os JSONs são gerados em uma pasta temporária.
5. A API lê `replayinfo.json`, `players.json`, `generalstats.json`, `teamgeneralstats.json` e `timeline.json`.
6. Os arquivos são interligados por `playerId`, `team` e replay id.
7. O resultado normalizado é salvo no PostgreSQL.
8. A pasta JSON temporária é removida.
9. `GET /api/matches/{id}` devolve a partida já pronta para frontend/mobile.

## Banco

Como as entidades mudaram, remova a migration antiga de desenvolvimento e gere uma nova se ainda não houver dados importantes:

```powershell
dotnet ef database drop --force `
  --project src/AfterStorm.Infrastructure `
  --startup-project src/AfterStorm.Api

Remove-Item -Recurse -Force .\src\AfterStorm.Infrastructure\Migrations -ErrorAction SilentlyContinue

dotnet ef migrations add InitialAfterStormReplayModel `
  --project src/AfterStorm.Infrastructure `
  --startup-project src/AfterStorm.Api

dotnet ef database update `
  --project src/AfterStorm.Infrastructure `
  --startup-project src/AfterStorm.Api
```

Se seu PostgreSQL Docker está publicado em `5433`, altere a porta em `src/AfterStorm.Api/appsettings.json`.

## Executar

```powershell
dotnet run --project src/AfterStorm.Api
```

No Swagger, envie um `.StormReplay` em `POST /api/replays`.

O retorno terá o `matchId`. Abra:

```text
GET /api/matches/{matchId}
```

Você verá jogadores, estatísticas, talentos com timestamps, mortes, times, XP e timeline.
