using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace CrashLogSimulator.Logging;

public sealed class FileSystemLogSink : ILogSink, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    private readonly FileSystemLogSinkOptions _options;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private StreamWriter? _writer;
    private string? _resolvedPath;

    public FileSystemLogSink(IOptions<FileSystemLogSinkOptions> options)
    {
        _options = options.Value;
    }

    public string SinkId => "filesystem";

    public async Task WriteAsync(LogRecord record, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var writer = await EnsureWriterAsync(cancellationToken).ConfigureAwait(false);
            var line = JsonSerializer.Serialize(record, JsonOptions);
            await writer.WriteLineAsync(line.AsMemory(), cancellationToken).ConfigureAwait(false);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public Task FlushAsync(CancellationToken cancellationToken = default)
    {
        if (_writer is null)
        {
            return Task.CompletedTask;
        }

        return _writeLock.WaitAsync(cancellationToken).ContinueWith(
            static (_, state) =>
            {
                var sink = (FileSystemLogSink)state!;
                try
                {
                    return sink._writer!.FlushAsync();
                }
                finally
                {
                    sink._writeLock.Release();
                }
            },
            this,
            cancellationToken,
            TaskContinuationOptions.None,
            TaskScheduler.Default).Unwrap();
    }

    private async Task<StreamWriter> EnsureWriterAsync(CancellationToken cancellationToken)
    {
        if (_writer is not null)
        {
            return _writer;
        }

        var dir = Path.IsPathRooted(_options.Directory)
            ? _options.Directory
            : Path.Combine(AppContext.BaseDirectory, _options.Directory);

        Directory.CreateDirectory(dir);

        var fileName = _options.FileNamePattern
            .Replace("{date}", DateTime.UtcNow.ToString("yyyy-MM-dd"), StringComparison.Ordinal)
            .Replace("{time}", DateTime.UtcNow.ToString("HHmmss"), StringComparison.Ordinal);

        _resolvedPath = Path.Combine(dir, fileName);
        var stream = new FileStream(
            _resolvedPath,
            FileMode.Append,
            FileAccess.Write,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        _writer = new StreamWriter(stream) { AutoFlush = true };
        return _writer;
    }

    public async ValueTask DisposeAsync()
    {
        await _writeLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_writer is not null)
            {
                await _writer.DisposeAsync().ConfigureAwait(false);
                _writer = null;
            }
        }
        finally
        {
            _writeLock.Release();
            _writeLock.Dispose();
        }
    }
}