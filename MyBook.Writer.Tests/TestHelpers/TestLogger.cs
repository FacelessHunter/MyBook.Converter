using Microsoft.Extensions.Logging;

namespace MyBook.Writer.Tests.TestHelpers
{
    public class TestLogger<T> : ILogger<T>
    {
        private readonly string _name;
        public List<string> LogMessages { get; } = new();

        public TestLogger()
        {
            _name = typeof(T).Name;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string message = formatter(state, exception);
            LogMessages.Add($"{logLevel}: {message}");
        }
    }

    public class TestLogger : ILogger
    {
        private readonly string _name;
        public List<string> LogMessages { get; } = new();

        public TestLogger(string name)
        {
            _name = name;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string message = formatter(state, exception);
            LogMessages.Add($"{logLevel}: {message}");
        }
    }
} 