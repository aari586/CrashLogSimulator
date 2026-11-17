namespace CrashLogSimulator.Domain;

public sealed class TenantSettingsResolver
{
    public string GetBillingCadence(IReadOnlyDictionary<string, string> settings) =>
        settings["billingCadence"];
}