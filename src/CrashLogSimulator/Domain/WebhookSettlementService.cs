namespace CrashLogSimulator.Domain;

public sealed class WebhookSettlementService
{
    public decimal ReadSettlementAmount(IReadOnlyDictionary<string, object> fields)
    {
        var amount = fields["amount"];
        return (decimal)amount;
    }
}