namespace CrashLogSimulator.Logging;

public sealed class FileSystemLogSinkOptions
{
    public const string SectionName = "LogSinks:FileSystem";

    public bool Enabled { get; set; } = true;

  /// <summary>Relative to app base directory unless absolute.</summary>
    public string Directory { get; set; } = "logs";

    /// <summary>Tokens: {date} = yyyy-MM-dd, {time} = HHmmss.</summary>
    public string FileNamePattern { get; set; } = "crash-{date}.jsonl";
}