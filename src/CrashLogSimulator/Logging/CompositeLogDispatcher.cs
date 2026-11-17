namespace CrashLogSimulator.Logging;

public sealed class CompositeLogDispatcher
{
    private readonly IReadOnlyList<ILogSink> _sinks;
    private readonly string _correlationId;

    public CompositeLogDispatcher(IEnumerable<ILogSink> sinks, string correlationId)
    {
        _sinks = sinks.ToList();
        _correlationId = correlationId;
    }

    public string CorrelationId => _correlationId;

    public Task InfoAsync(string message, string? scenarioId = null, IReadOnlyDictionary<string, object?>? properties = null) =>
        WriteAsync("Information", message, scenarioId, null, properties);

    public Task WarningAsync(string message, string? scenarioId = null, IReadOnlyDictionary<string, object?>? properties = null) =>
        WriteAsync("Warning", message, scenarioId, null, properties);

    public Task ErrorAsync(string message, Exception? ex = null, string? scenarioId = null, IReadOnlyDictionary<string, object?>? properties = null) =>
        WriteAsync("Error", message, scenarioId, ex, properties);

    public Task CriticalAsync(string message, Exception ex, string? scenarioId = null, IReadOnlyDictionary<string, object?>? properties = null) =>
        WriteAsync("Critical", message, scenarioId, ex, properties);

    public async Task WriteAsync(
        string level,
        string message,
        string? scenarioId = null,
        Exception? exception = null,
        IReadOnlyDictionary<string, object?>? properties = null)
    {
        var record = new LogRecord
        {
            TimestampUtc = DateTimeOffset.UtcNow,
            Level = level,
            Message = message,
            CorrelationId = _correlationId,
            ScenarioId = scenarioId,
            ExceptionType = exception?.GetType().FullName,
            ExceptionMessage = exception?.Message,
            StackTrace = exception?.StackTrace,
            Properties = properties,
        };

        foreach (var sink in _sinks)
        {
            await sink.WriteAsync(record).ConfigureAwait(false);
        }
    }

    public async Task FlushAllAsync()
    {
        foreach (var sink in _sinks)
        {
            await sink.FlushAsync().ConfigureAwait(false);
        }
    }
}