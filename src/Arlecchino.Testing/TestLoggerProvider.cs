using System;
using Arlecchino.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Arlecchino.Testing;

/// <summary>
/// Puts logging straight into the buffer the overlay draws from. An application reaches the overlay the
/// long way round, through a provider that writes to the console and is caught there.
/// </summary>
internal sealed class TestLoggerProvider : ILoggerProvider
{
    private readonly LogBuffer _buffer;
    private readonly TimeProvider _time;

    /// <summary>Creates the provider.</summary>
    /// <param name="buffer">Where the lines are kept.</param>
    /// <param name="time">Where the timestamps come from, so a test can hold the clock still.</param>
    public TestLoggerProvider(LogBuffer buffer, TimeProvider time)
    {
        _buffer = buffer;
        _time = time;
    }

    /// <summary>Creates a logger for one category.</summary>
    /// <param name="categoryName">Full category name; only its last part is kept.</param>
    /// <returns>The logger.</returns>
    public ILogger CreateLogger(string categoryName) => new BufferLogger(_buffer, ShortName(categoryName), _time);

    /// <summary>Nothing is held open, so there is nothing to release.</summary>
    public void Dispose() { }

    private static string ShortName(string categoryName)
    {
        var lastDot = categoryName.LastIndexOf('.');
        return lastDot < 0 ? categoryName : categoryName[(lastDot + 1)..];
    }

    private sealed class BufferLogger : ILogger
    {
        private readonly LogBuffer _buffer;
        private readonly string _category;
        private readonly TimeProvider _time;

        public BufferLogger(LogBuffer buffer, string category, TimeProvider time)
        {
            _buffer = buffer;
            _category = category;
            _time = time;
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

            _buffer.Add(new(_time.GetLocalNow(), logLevel, _category, message));
        }
    }
}
