namespace CrashLogSimulator.Domain;

public sealed class FileLockCoordinator
{
    public void WriteWithExclusiveLock(string path, string content)
    {
        using var first = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        using var second = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        using var writer = new StreamWriter(second);
        writer.Write(content);
    }
}