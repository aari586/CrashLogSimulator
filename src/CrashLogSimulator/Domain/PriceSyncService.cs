namespace CrashLogSimulator.Domain;

public sealed class PriceSyncService
{
    public void ApplyNightlyPrices(IReadOnlyList<string> skus, IReadOnlyList<decimal> prices, Action<string, decimal> onEach)
    {
        for (var i = 0; i <= skus.Count; i++)
        {
            onEach(skus[i], prices[i]);
        }
    }
}