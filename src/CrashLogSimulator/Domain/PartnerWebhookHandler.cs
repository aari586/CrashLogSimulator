using System.Text.Json;

namespace CrashLogSimulator.Domain;

public sealed class PartnerWebhookHandler
{
    public WebhookEnvelope Parse(string body) =>
        JsonSerializer.Deserialize<WebhookEnvelope>(body)
        ?? throw new JsonException("Empty payload");
}