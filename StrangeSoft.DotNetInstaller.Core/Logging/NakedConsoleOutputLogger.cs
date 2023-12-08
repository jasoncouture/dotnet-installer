using Microsoft.Extensions.Logging;

namespace StrangeSoft.DotNetInstaller.Core.Logging;

internal class NakedConsoleOutputLogger : ILogger
{
    public NakedConsoleOutputLogger(string categoryName)
    {
        CategoryName = categoryName;
    }

    private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    public string CategoryName { get; }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "TRCE",
            LogLevel.Debug => " DBG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => " ERR",
            LogLevel.Critical => "CRIT",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
    }
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;
        
        var color = logLevel switch
        {
            LogLevel.Trace or LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Information => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.None => throw new InvalidOperationException("Invalid log level: None"),
            _ => ConsoleColor.Red
        };
        
        _semaphore.Wait();
        try
        {
            var originalColor = Console.ForegroundColor;
            Console.Write('[');
            Console.ForegroundColor = color;
            Console.Write(GetLogLevelString(logLevel));
            Console.ForegroundColor = originalColor;
            Console.Write("] ");
            Console.WriteLine(formatter.Invoke(state, exception));
        }
        finally
        {
            _semaphore.Release();
        }

    }

    public bool IsEnabled(LogLevel logLevel)
    {
#if DEBUG
        return true;
#else
        return logLevel != LogLevel.Debug && logLevel != LogLevel.Trace;
#endif
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}