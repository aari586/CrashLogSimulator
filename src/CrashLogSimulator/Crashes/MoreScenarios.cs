using CrashLogSimulator.Domain;
using CrashLogSimulator.Logging;

namespace CrashLogSimulator.Crashes;

internal sealed class KeyNotFoundScenario : ICrashScenario
{
    public string Id => "keynotfound";
    public string Name => "Missing dictionary key";
    public string Description => "Tenant settings indexer used for optional key.";
    public string HintForAgent => "Use TryGetValue and default cadence.";
    public string FixFile => "Domain/TenantSettingsResolver.cs";
    public string FixMember => "GetBillingCadence";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Resolving billing cadence for tenant T-12004", Id).ConfigureAwait(false);
        var settings = new Dictionary<string, string> { ["currency"] = "AUD" };
        _ = new TenantSettingsResolver().GetBillingCadence(settings);
    }
}

internal sealed class FormatExceptionScenario : ICrashScenario
{
    public string Id => "format";
    public string Name => "Bad date parse";
    public string Description => "AU date parsed with invariant culture.";
    public string HintForAgent => "Use dd/MM/yyyy for en-AU ParseExact format.";
    public string FixFile => "Domain/PayrollImportService.cs";
    public string FixMember => "ParsePeriodEnd";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Importing payroll period from CSV", Id).ConfigureAwait(false);
        _ = new PayrollImportService().ParsePeriodEnd("15/03/2025", useAustralianFormat: true);
    }
}

internal sealed class JsonExceptionScenario : ICrashScenario
{
    public string Id => "json";
    public string Name => "Malformed JSON body";
    public string Description => "Truncated JSON passed to deserializer.";
    public string HintForAgent => "Validate body length/schema before deserialize.";
    public string FixFile => "Domain/PartnerWebhookHandler.cs";
    public string FixMember => "Parse";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Processing partner webhook", Id).ConfigureAwait(false);
        const string body = "{\"invoiceId\":8842,\"lines\":[";
        _ = new PartnerWebhookHandler().Parse(body);
    }
}

internal sealed class AggregateExceptionScenario : ICrashScenario
{
    public string Id => "aggregate";
    public string Name => "Parallel downstream failures";
    public string Description => "Fan-out has wrong region, pricing URL, and stub client.";
    public string HintForAgent => "Fix region code, default pricing URL, and InventoryClient stub.";
    public string FixFile => "Domain/DownstreamFanOutCoordinator.cs";
    public string FixMember => "SyncAllAsync";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Fan-out to 3 downstream APIs", Id).ConfigureAwait(false);
        await new DownstreamFanOutCoordinator().SyncAllAsync(cancellationToken).ConfigureAwait(false);
    }
}

internal sealed class LinqSingleScenario : ICrashScenario
{
    public string Id => "linq";
    public string Name => "LINQ Single() on empty";
    public string Description => "Break-glass expects Admin but list has none.";
    public string HintForAgent => "Use SingleOrDefault or filter active admins.";
    public string FixFile => "Domain/UserElevationService.cs";
    public string FixMember => "FindBreakGlassAdmin";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Elevating permissions for break-glass account", Id).ConfigureAwait(false);
        var users = new[] { new UserRecord("u1", "Member"), new UserRecord("u2", "Member") };
        _ = new UserElevationService().FindBreakGlassAdmin(users);
    }
}

internal sealed class OverflowScenario : ICrashScenario
{
    public string Id => "overflow";
    public string Name => "Integer overflow on parse";
    public string Description => "EDI quantity parsed as int without range check.";
    public string HintForAgent => "Use long.TryParse or bounded int parse.";
    public string FixFile => "Domain/EdiQuantityParser.cs";
    public string FixMember => "ParseQuantity";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Applying EDI quantity adjustment", Id).ConfigureAwait(false);
        _ = new EdiQuantityParser().ParseQuantity("99999999999999999999");
    }
}

internal sealed class ArgumentScenario : ICrashScenario
{
    public string Id => "argument";
    public string Name => "ArgumentException on validation";
    public string Description => "Import row maps wrong column into SKU.";
    public string HintForAgent => "Read Sku not SkuCode from row dictionary.";
    public string FixFile => "Domain/ProductAliasService.cs";
    public string FixMember => "CreateAliasFromImportRow";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Creating product alias", Id).ConfigureAwait(false);
        var row = new Dictionary<string, string> { ["Sku"] = "WIDGET-A", ["Alias"] = "Widget Pro" };
        new ProductAliasService().CreateAliasFromImportRow(row);
    }
}

internal sealed class TimeoutScenario : ICrashScenario
{
    public string Id => "timeout";
    public string Name => "Payment gateway timeout";
    public string Description => "Wait timeout uses milliseconds where seconds intended.";
    public string HintForAgent => "Use TimeSpan.FromSeconds for gateway timeout.";
    public string FixFile => "Domain/PaymentGatewayClient.cs";
    public string FixMember => "ChargeAsync";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Charging invoice", Id).ConfigureAwait(false);
        await new PaymentGatewayClient().ChargeAsync(42.50m, cancellationToken).ConfigureAwait(false);
    }
}

internal sealed class UnauthorizedScenario : ICrashScenario
{
    public string Id => "unauthorized";
    public string Name => "Unauthorized file write";
    public string Description => "Export directory resolved under System32.";
    public string HintForAgent => "Write under LocalApplicationData or configured folder.";
    public string FixFile => "Domain/ComplianceExportService.cs";
    public string FixMember => "ResolveExportDirectory";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Writing compliance export", Id).ConfigureAwait(false);
        await new ComplianceExportService().WriteExportAsync("export", cancellationToken).ConfigureAwait(false);
    }
}

internal sealed class IOExceptionLockScenario : ICrashScenario
{
    public string Id => "io";
    public string Name => "File locked (sharing violation)";
    public string Description => "Exclusive lock opened twice on same path.";
    public string HintForAgent => "Use one stream or FileShare.ReadWrite.";
    public string FixFile => "Domain/FileLockCoordinator.cs";
    public string FixMember => "WriteWithExclusiveLock";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(Path.GetTempPath(), $"crashlogsim-{Guid.NewGuid():N}.lock");
        await log.InfoAsync($"Acquiring lock on {path}", Id).ConfigureAwait(false);
        new FileLockCoordinator().WriteWithExclusiveLock(path, "payload");
    }
}

internal sealed class NotImplementedScenario : ICrashScenario
{
    public string Id => "notimpl";
    public string Name => "Not implemented code path";
    public string Description => "Ledger v2 selected but still throws.";
    public string HintForAgent => "Implement PostV2 or route v2 to v1 until ready.";
    public string FixFile => "Domain/LedgerRouter.cs";
    public string FixMember => "PostJournalEntry";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Feature flag NEW_LEDGER enabled — routing to v2 handler", Id).ConfigureAwait(false);
        new LedgerRouter().PostJournalEntry("v2", 120m);
    }
}

internal sealed class CryptographicScenario : ICrashScenario
{
    public string Id => "crypto";
    public string Name => "Bad padding on decrypt";
    public string Description => "Wrong key material for ciphertext version.";
    public string HintForAgent => "ResolveKey(2) must return KeyV2, not KeyV1.";
    public string FixFile => "Domain/TokenDecryptionService.cs";
    public string FixMember => "ResolveKey";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Decrypting stored API token", Id).ConfigureAwait(false);
        _ = new TokenDecryptionService().DecryptSampleToken(keyVersion: 2);
    }
}