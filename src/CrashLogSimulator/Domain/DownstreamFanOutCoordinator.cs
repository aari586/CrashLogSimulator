namespace CrashLogSimulator.Domain;

public sealed class DownstreamFanOutCoordinator
{
    public async Task SyncAllAsync(CancellationToken cancellationToken = default)
    {
        var regionCode = "A";
        var pricing = PricingClient.FetchAsync(cancellationToken);
        var tax = TaxClient.FetchAsync(regionCode, cancellationToken);
        var inventory = InventoryClient.FetchAsync(cancellationToken);

        await Task.WhenAll(pricing, tax, inventory).ConfigureAwait(false);
    }
}

file static class PricingClient
{
    internal static Task FetchAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var baseUrl = Environment.GetEnvironmentVariable("PRICING_API_URL");
        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new HttpRequestException("503 from pricing API");
        }

        return Task.CompletedTask;
    }
}

file static class TaxClient
{
    internal static Task FetchAsync(string regionCode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (regionCode.Length != 2)
        {
            throw new ArgumentException("Invalid region code");
        }

        return Task.CompletedTask;
    }
}

file static class InventoryClient
{
    internal static Task FetchAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw new IOException("Socket reset on tax service");
    }
}