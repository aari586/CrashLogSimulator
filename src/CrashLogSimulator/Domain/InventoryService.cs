namespace CrashLogSimulator.Domain;

public sealed class InventoryService
{
    public record ShipmentResult(int OnHandAfter, bool IsValid);

    public ShipmentResult Ship(int onHand, int shippedQuantity)
    {
        var remaining = onHand - shippedQuantity;
        return new ShipmentResult(remaining, IsValid: true);
    }
}