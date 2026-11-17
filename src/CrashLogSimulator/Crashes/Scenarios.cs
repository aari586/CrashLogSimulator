using CrashLogSimulator.Domain;
using CrashLogSimulator.Logging;

namespace CrashLogSimulator.Crashes;

internal sealed class NullReferenceScenario : ICrashScenario
{
    public string Id => "nullref";
    public string Name => "Null reference in order total";
    public string Description => "Order line map never loaded from repository.";
    public string HintForAgent => "Initialize linesBySku from LoadLines; null-check or TryGetValue.";
    public string FixFile => "Domain/OrderService.cs";
    public string FixMember => "GetLineQuantity";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Loading order ORD-8842", Id).ConfigureAwait(false);
        var service = new OrderService();
        _ = service.GetLineQuantity("ORD-8842", "WIDGET-A");
    }
}

internal sealed class IndexOutOfRangeScenario : ICrashScenario
{
    public string Id => "index";
    public string Name => "Index out of range in batch processor";
    public string Description => "Price sync loop uses inclusive upper bound.";
    public string HintForAgent => "Loop while i < Math.Min(skus.Count, prices.Count).";
    public string FixFile => "Domain/PriceSyncService.cs";
    public string FixMember => "ApplyNightlyPrices";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Starting nightly price sync", Id).ConfigureAwait(false);
        var skus = new List<string> { "A", "B", "C" };
        var prices = new List<decimal> { 10m, 20m };
        new PriceSyncService().ApplyNightlyPrices(skus, prices, (s, p) =>
        {
            log.DebugStepAsync($"Updating {s} -> {p}", Id).GetAwaiter().GetResult();
        });
    }
}

internal sealed class DivideByZeroScenario : ICrashScenario
{
    public string Id => "divide";
    public string Name => "Divide by zero in metrics";
    public string Description => "Throughput divides by duration without zero guard.";
    public string HintForAgent => "Return 0 when totalDurationSeconds is 0.";
    public string FixFile => "Domain/MetricsService.cs";
    public string FixMember => "JobsPerSecond";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Computing hourly throughput", Id).ConfigureAwait(false);
        _ = new MetricsService().JobsPerSecond(completedJobs: 0, totalDurationSeconds: 0);
    }
}

internal sealed class InvalidCastScenario : ICrashScenario
{
    public string Id => "cast";
    public string Name => "Invalid cast from JSON payload";
    public string Description => "Settlement amount cast directly from object.";
    public string HintForAgent => "Use decimal.TryParse or convert from string.";
    public string FixFile => "Domain/WebhookSettlementService.cs";
    public string FixMember => "ReadSettlementAmount";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Applying webhook settlement", Id).ConfigureAwait(false);
        var fields = new Dictionary<string, object> { ["amount"] = "124.50" };
        _ = new WebhookSettlementService().ReadSettlementAmount(fields);
    }
}

internal sealed class FileNotFoundScenario : ICrashScenario
{
    public string Id => "filenotfound";
    public string Name => "Missing config file";
    public string Description => "Feature flags path points at wrong folder.";
    public string HintForAgent => "Use config/features.json (copied to output).";
    public string FixFile => "Domain/FeatureFlagLoader.cs";
    public string FixMember => "LoadJson";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Loading feature flags", Id).ConfigureAwait(false);
        _ = new FeatureFlagLoader().LoadJson();
    }
}

internal sealed class UnobservedTaskExceptionScenario : ICrashScenario
{
    public string Id => "task";
    public string Name => "Fire-and-forget task failure";
    public string Description => "Email batch dispatched without awaiting.";
    public string HintForAgent => "Await SendInternalAsync in DispatchBatch.";
    public string FixFile => "Domain/EmailBatchDispatcher.cs";
    public string FixMember => "DispatchBatch";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Dispatching async email batch", Id).ConfigureAwait(false);
        new EmailBatchDispatcher().DispatchBatch(["ops@example.com"]);
        await Task.Delay(500, cancellationToken).ConfigureAwait(false);
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}

internal sealed class StackOverflowScenario : ICrashScenario
{
    public string Id => "stackoverflow";
    public string Name => "Stack overflow (unbounded recursion)";
    public string Description => "Org walk recursion has no base case.";
    public string HintForAgent => "Stop recursion at max depth or use iteration.";
    public string FixFile => "Domain/OrgChartWalker.cs";
    public string FixMember => "Walk";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Walking org chart for CEO report", Id).ConfigureAwait(false);
        new OrgChartWalker().BuildReport(rootEmployeeId: 1);
    }
}

internal sealed class WrongConnectionStringScenario : ICrashScenario
{
    public string Id => "connection";
    public string Name => "Invalid connection string merge";
    public string Description => "Config keys do not match appsettings names.";
    public string HintForAgent => "Map Server, Database, User keys in FromConfiguration.";
    public string FixFile => "Domain/DatabaseSettings.cs";
    public string FixMember => "FromConfiguration";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Opening database connection", Id).ConfigureAwait(false);
        var config = new Dictionary<string, string>
        {
            ["Server"] = "sql.local",
            ["Database"] = "Billing",
            ["User"] = "app_user",
        };
        _ = new DatabaseSettings().FromConfiguration(config);
    }
}

internal sealed class OffByOneInventoryScenario : ICrashScenario
{
    public string Id => "logic";
    public string Name => "Logic bug: negative stock";
    public string Description => "Shipment allowed when quantity exceeds on-hand.";
    public string HintForAgent => "Reject shipment when shipped > onHand before subtracting.";
    public string FixFile => "Domain/InventoryService.cs";
    public string FixMember => "Ship";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Processing shipment SHP-991", Id).ConfigureAwait(false);
        var result = new InventoryService().Ship(onHand: 3, shippedQuantity: 5);
        if (result.OnHandAfter < 0)
        {
            await log.CriticalAsync(
                "Inventory went negative — data corruption risk",
                new InvalidOperationException($"onHand={result.OnHandAfter} after invalid shipment (IsValid={result.IsValid})"),
                Id,
                new Dictionary<string, object?> { ["onHandAfter"] = result.OnHandAfter, ["isValid"] = result.IsValid }).ConfigureAwait(false);
            Environment.ExitCode = 1;
        }
    }
}

internal sealed class DoubleDisposeScenario : ICrashScenario
{
    public string Id => "dispose";
    public string Name => "ObjectDisposed on shared stream";
    public string Description => "Audit writer disposes shared stream.";
    public string HintForAgent => "Do not dispose stream owned by caller.";
    public string FixFile => "Domain/AuditExportWriter.cs";
    public string FixMember => "WriteAndMeasure";

    public async Task RunAsync(CompositeLogDispatcher log, CancellationToken cancellationToken = default)
    {
        await log.InfoAsync("Writing audit export", Id).ConfigureAwait(false);
        _ = new AuditExportWriter().WriteAndMeasure(new MemoryStream());
    }
}

file static class LogExtensions
{
    public static Task DebugStepAsync(this CompositeLogDispatcher log, string message, string scenarioId) =>
        log.WriteAsync("Debug", message, scenarioId);
}