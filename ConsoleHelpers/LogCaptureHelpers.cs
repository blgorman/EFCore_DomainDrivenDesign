using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ConsoleHelpers;

// Demos need to show what a handler logged, inside a boxed screen.
// The console logger provider writes on a background thread, so its lines
// arrive late or not at all in a short demo run, and they land outside the box.
// This provider records matching log messages into a list the demo can read
// back synchronously and print wherever it wants.
public static class LogCaptureHelpers
{
    public static ILoggerProvider CaptureFor(string categoryContains, List<string> messages)
    {
        return new CapturingLoggerProvider(categoryContains, messages);
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly string _categoryContains;
        private readonly List<string> _messages;

        public CapturingLoggerProvider(string categoryContains, List<string> messages)
        {
            _categoryContains = categoryContains;
            _messages = messages;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return categoryName.Contains(_categoryContains, StringComparison.Ordinal)
                ? new CapturingLogger(_messages)
                : NullLogger.Instance;
        }

        public void Dispose()
        {
        }

        private sealed class CapturingLogger : ILogger
        {
            private readonly List<string> _messages;

            public CapturingLogger(List<string> messages)
            {
                _messages = messages;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                _messages.Add(formatter(state, exception));
            }
        }
    }
}
