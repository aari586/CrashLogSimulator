namespace CrashLogSimulator.Domain;

public sealed class ComplianceExportService
{
    public async Task WriteExportAsync(string content, CancellationToken cancellationToken = default)
    {
        var directory = ResolveExportDirectory();
        var path = Path.Combine(directory, "crashlogsim.export");
        await File.WriteAllTextAsync(path, content, cancellationToken).ConfigureAwait(false);
    }

    private static string ResolveExportDirectory() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "drivers", "etc");
}