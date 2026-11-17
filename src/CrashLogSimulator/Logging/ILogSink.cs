namespace CrashLogSimulator.Logging;

/// <summary>
/// Plug-and-play log destination. Implement for Elasticsearch, Seq, cloud storage, etc.
/// </summary>
public interface ILogSink
{
    string SinkId { get; }

    Task WriteAsync(LogRecord record, CancellationToken cancellationToken = default);

    Task FlushAsync(CancellationToken cancellationToken = default);
}