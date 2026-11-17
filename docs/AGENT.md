# Agent repair workflow

Each scenario is a **real defect in `Domain/`** (not a throw in the scenario runner). Runners only orchestrate and log.

## What to use

1. **Logs** (`logs/crash-*.jsonl`): `scenarioId`, `stackTrace`, `fixFile`, `fixMember`, `agentHint`.
2. **Source**: open `src/CrashLogSimulator/{fixFile}` and edit `{fixMember}` (stack trace should match).
3. **Verify**: re-run the same scenario id; expect exit code **0** and no `Critical` log.

```
dotnet run --project src/CrashLogSimulator -- <scenario-id>
```

## Fix style

- Smallest change that matches production quality (guards, correct keys, await async, etc.).
- Do **not** disable scenarios or catch-all swallow exceptions unless that scenario is about error handling (`aggregate` may use coordinated handling after individual client bugs are fixed).

## Scenario → primary file

| Id | File |
|----|------|
| nullref | Domain/OrderService.cs |
| index | Domain/PriceSyncService.cs |
| divide | Domain/MetricsService.cs |
| cast | Domain/WebhookSettlementService.cs |
| filenotfound | Domain/FeatureFlagLoader.cs |
| task | Domain/EmailBatchDispatcher.cs |
| stackoverflow | Domain/OrgChartWalker.cs |
| connection | Domain/DatabaseSettings.cs |
| logic | Domain/InventoryService.cs |
| dispose | Domain/AuditExportWriter.cs |
| keynotfound | Domain/TenantSettingsResolver.cs |
| format | Domain/PayrollImportService.cs |
| json | Domain/PartnerWebhookHandler.cs |
| aggregate | Domain/DownstreamFanOutCoordinator.cs (+ client stubs in same file) |
| linq | Domain/UserElevationService.cs |
| overflow | Domain/EdiQuantityParser.cs |
| argument | Domain/ProductAliasService.cs |
| timeout | Domain/PaymentGatewayClient.cs |
| unauthorized | Domain/ComplianceExportService.cs |
| io | Domain/FileLockCoordinator.cs |
| notimpl | Domain/LedgerRouter.cs |
| crypto | Domain/TokenDecryptionService.cs |