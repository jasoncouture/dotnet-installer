using Microsoft.Extensions.Logging;

namespace StrangeSoft.DotNetInstaller.Core.Logging;

internal class NakedConsoleOutputLoggingProvider : ILoggerProvider
{
    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new NakedConsoleOutputLogger(categoryName);
    }
}