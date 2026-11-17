namespace CrashLogSimulator.Domain;

public sealed record OrderLine(string Sku, int Quantity);

public sealed record WebhookEnvelope(int InvoiceId, IReadOnlyList<WebhookLine>? Lines);

public sealed record WebhookLine(string Sku, decimal Amount);

public sealed record UserRecord(string Id, string Role);