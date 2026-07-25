using System;
using Microsoft.Extensions.Logging;

namespace Arlecchino.Diagnostics;

/// <summary>
/// Sends logging into the overlay's buffer instead of the console. Registered by default, because a
/// provider that writes to standard output would draw straight over the frame.
/// </summary>
public sealed class ArlecchinoLoggerProvider : ILoggerProvider
{
    private readonly LogBuffer _buffer;

    /// <summary>Creates the provider.</summary>
    /// <param name="buffer">Where the lines are kept.</param>
    public ArlecchinoLoggerProvider(LogBuffer buffer)
    {
        _buffer = buffer;
    }

    /// <summary>Creates a logger for one category.</summary>
    /// <param name="categoryName">Full category name; only its last part is shown.</param>
    /// <returns>The logger.</returns>
    public ILogger CreateLogger(string categoryName) => new BufferLogger(_buffer, ShortName(categoryName));

    /// <summary>Nothing is held open, so there is nothing to release.</summary>
    public void Dispose()
    {
    }

    private static string ShortName(string categoryName)
    {
        var lastDot = categoryName.LastIndexOf('.');
        return lastDot < 0 ? categoryName : categoryName[(lastDot + 1)..];
    }

    private sealed class BufferLogger : ILogger
    {
        private readonly LogBuffer _buffer;
        private readonly string _category;

        public BufferLogger(LogBuffer buffer, string category)
        {
            _buffer = buffer;
            _category = category;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

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

            var message = formatter(state, exception);
            if (exception is not null)
            {
                message = $"{message} — {exception.Message}";
            }

            _buffer.Add(new(DateTimeOffset.Now, logLevel, _category, message));
        }
    }
}
