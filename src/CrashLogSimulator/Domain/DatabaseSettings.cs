namespace CrashLogSimulator.Domain;

public sealed class DatabaseSettings
{
    public string FromConfiguration(IReadOnlyDictionary<string, string> config)
    {
        var server = config.GetValueOrDefault("DataSource", string.Empty);
        var database = config.GetValueOrDefault("Catalog", string.Empty);
        var user = config.GetValueOrDefault("Uid", string.Empty);
        var merged = $"Server={server};Database={database};User={user}";

        if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException($"Unable to parse connection string after config merge: '{merged}'");
        }

        return merged;
    }
}