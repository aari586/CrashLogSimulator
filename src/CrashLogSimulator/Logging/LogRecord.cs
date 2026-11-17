namespace CrashLogSimulator.Logging;

public sealed record LogRecord
{
    public required DateTimeOffset TimestampUtc { get; init; }
    public required string Level { get; init; }
    public required string Message { get; init; }
    public required string CorrelationId { get; init; }
    public string? ScenarioId { get; init; }
    public string? ExceptionType { get; init; }
    public string? ExceptionMessage { get; init; }
    public string? StackTrace { get; init; }
    public IReadOnlyDictionary<string, object?>? Properties { get; init; }
}