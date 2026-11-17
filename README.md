# CrashLogSimulator

A .NET 8 console app that **intentionally fails** in controlled ways and writes **structured JSONL crash logs** to plug-and-play sinks (default: **file system**). Use it to generate realistic failure data for demos where an AI agent reads logs, patches `Domain/` code, and verifies the fix.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (e.g. `dotnet --version` → `8.0.x`)

## Quick start

```powershell
git clone https://github.com/aari586/CrashLogSimulator.git
cd CrashLogSimulator

dotnet build
dotnet run --project src\CrashLogSimulator -- --list
dotnet run --project src\CrashLogSimulator -- nullref
dotnet run --project src\CrashLogSimulator -- random
```

**Logs** (default): `src\CrashLogSimulator\bin\Debug\net8.0\logs\crash-YYYY-MM-DD.jsonl`  
(Configurable in `src/CrashLogSimulator/appsettings.json` → `LogSinks:FileSystem`.)

You can run from the **repo root**; the host uses the app base directory for `appsettings.json`.

## How it works

```text
CLI scenario id  →  Crashes/* (runner)  →  Domain/* (buggy code)  →  exception / exit
                           ↓
                    Logging/ILogSink  →  logs/*.jsonl
```

| Piece | Role |
|--------|------|
| `Crashes/` | Scenario catalog, breadcrumbs, invokes domain code |
| `Domain/` | **Intentional defects** — stack traces point here |
| `Logging/` | `ILogSink`, `FileSystemLogSink`, JSONL records |
| `Program.cs` | Host, unhandled handlers, CLI (`--list`, `--help`, `random`) |

Each startup log line includes **`fixFile`**, **`fixMember`**, and **`agentHint`** in `properties`. Fault logs add `exceptionType`, `exceptionMessage`, and `stackTrace`.

**Agent workflow:** [docs/AGENT.md](docs/AGENT.md)

## Scenario ids (22 + `random`)

| Id | Exception / outcome | Primary fix location |
|----|---------------------|----------------------|
| `nullref` | NullReferenceException | `Domain/OrderService.cs` |
| `index` | IndexOutOfRangeException | `Domain/PriceSyncService.cs` |
| `divide` | DivideByZeroException | `Domain/MetricsService.cs` |
| `cast` | InvalidCastException | `Domain/WebhookSettlementService.cs` |
| `filenotfound` | FileNotFoundException | `Domain/FeatureFlagLoader.cs` |
| `keynotfound` | KeyNotFoundException | `Domain/TenantSettingsResolver.cs` |
| `format` | FormatException | `Domain/PayrollImportService.cs` |
| `json` | JsonException | `Domain/PartnerWebhookHandler.cs` |
| `aggregate` | AggregateException | `Domain/DownstreamFanOutCoordinator.cs` |
| `linq` | InvalidOperationException (`Single`) | `Domain/UserElevationService.cs` |
| `overflow` | OverflowException | `Domain/EdiQuantityParser.cs` |
| `argument` | ArgumentException | `Domain/ProductAliasService.cs` |
| `timeout` | TimeoutException | `Domain/PaymentGatewayClient.cs` |
| `unauthorized` | UnauthorizedAccessException | `Domain/ComplianceExportService.cs` |
| `io` | IOException (file lock) | `Domain/FileLockCoordinator.cs` |
| `notimpl` | NotImplementedException | `Domain/LedgerRouter.cs` |
| `crypto` | CryptographicException | `Domain/TokenDecryptionService.cs` |
| `task` | Fire-and-forget async (may exit 0 on .NET 8) | `Domain/EmailBatchDispatcher.cs` |
| `stackoverflow` | StackOverflowException — **use sparingly** | `Domain/OrgChartWalker.cs` |
| `connection` | InvalidOperationException | `Domain/DatabaseSettings.cs` |
| `logic` | No throw — **Critical** log + exit **1** | `Domain/InventoryService.cs` |
| `dispose` | ObjectDisposedException | `Domain/AuditExportWriter.cs` |
| `random` | Picks from safe pool (excludes `stackoverflow`, `task`) | (varies) |

### Generate many log samples

```powershell
$scenarios = @(
  'nullref','index','divide','json','aggregate','linq','overflow',
  'timeout','unauthorized','crypto','logic','dispose','connection'
)
foreach ($id in $scenarios) {
  dotnet run --project src/CrashLogSimulator -- $id
}
```

Each run gets a new `correlationId`.

### Verify an agent fix

```powershell
dotnet run --project src/CrashLogSimulator -- nullref
echo $LASTEXITCODE   # expect 1 before fix, 0 after
```

## Log record shape

One JSON object per line (JSONL):

| Field | Purpose |
|--------|---------|
| `timestampUtc` | Event time (UTC) |
| `level` | `Information`, `Warning`, `Critical`, … |
| `message` | Human-readable event |
| `correlationId` | All lines for one run |
| `scenarioId` | Scenario id |
| `exceptionType` / `exceptionMessage` / `stackTrace` | On failure |
| `properties` | `agentHint`, `fixFile`, `fixMember`, machine/os, etc. |

## Configuration

`src/CrashLogSimulator/appsettings.json`:

```json
"LogSinks": {
  "FileSystem": {
    "Enabled": true,
    "Directory": "logs",
    "FileNamePattern": "crash-{date}.jsonl"
  }
}
```

`config/features.json` is copied to the output folder (used by the `filenotfound` scenario when the path is corrected).

## Extending log sinks

1. Implement `ILogSink` in `Logging/`.
2. Register in `Program.cs`: `services.AddSingleton<ILogSink, YourSink>();`
3. Add config under `LogSinks` if needed.

## Project layout

```text
CrashLogSimulator.sln
src/CrashLogSimulator/
  Program.cs
  appsettings.json
  config/features.json
  Crashes/          # scenario runners
  Domain/           # fixable application logic
  Logging/          # sinks and dispatch
docs/
  AGENT.md          # id → file map and repair steps
```

## Security notes

- For **local simulation only** — do not deploy as a production service.
- Logs may contain stack traces and file paths; treat as operational (non-secret) data and restrict log directory permissions.
- No real secrets or production connection strings; configure credentials in a secret manager if you add remote sinks.