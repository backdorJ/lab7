namespace TaskManager.Api.Logging;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _path;
    private readonly object _gate = new();

    public FileLoggerProvider(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _path = path;
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, _path, _gate);

    public void Dispose()
    {
    }
}

public sealed class FileLogger : ILogger
{
    private readonly string _category;
    private readonly string _path;
    private readonly object _gate;

    public FileLogger(string category, string path, object gate)
    {
        _category = category;
        _path = path;
        _gate = gate;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) =>
        logLevel >= LogLevel.Information &&
        _category.StartsWith("TaskManager", StringComparison.Ordinal);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_category}: {formatter(state, exception)}";
        if (exception is not null)
        {
            line += Environment.NewLine + exception;
        }

        lock (_gate)
        {
            File.AppendAllText(_path, line + Environment.NewLine);
        }
    }
}
