namespace CrashLogSimulator.Domain;

public sealed class FeatureFlagLoader
{
    public string LoadJson()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "configuration", "features.json");
        return File.ReadAllText(path);
    }
}