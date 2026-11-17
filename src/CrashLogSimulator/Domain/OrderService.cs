namespace CrashLogSimulator.Domain;

public sealed class OrderService
{
    public int GetLineQuantity(string orderId, string sku)
    {
        Dictionary<string, OrderLine>? linesBySku = LoadLines(orderId);
        return linesBySku![sku].Quantity;
    }

    private static Dictionary<string, OrderLine>? LoadLines(string orderId)
    {
        _ = orderId;
        return null;
    }
}