using Microsoft.Extensions.Logging;

namespace MyBook.Writer.Tests.TestHelpers
{
    public class TestLoggerFactory : ILoggerFactory
    {
        private readonly Dictionary<string, TestLogger> _loggers = new();

        public TestLogger GetTestLogger(string categoryName)
        {
            if (_loggers.TryGetValue(categoryName, out var logger))
            {
                return logger;
            }
            
            return new TestLogger("Unused Logger");
        }

        public void Dispose() { }

        public ILogger CreateLogger(string categoryName)
        {
            if (!_loggers.TryGetValue(categoryName, out var logger))
            {
                logger = new TestLogger(categoryName);
                _loggers[categoryName] = logger;
            }
            
            return logger;
        }

        public void AddProvider(ILoggerProvider provider) { /* Not needed for tests */ }
    }
} 