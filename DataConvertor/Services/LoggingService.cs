using Microsoft.Extensions.Logging;
using MyBook.Writer.Core.Services;

namespace DataConvertor.Services
{
    /// <summary>
    /// Service for setting up and managing logging in the DataConvertor
    /// </summary>
    public static class LoggingService
    {
        /// <summary>
        /// Configure logging for the application
        /// </summary>
        /// <param name="logLevel">Minimum log level to record</param>
        /// <returns>Configured ILoggerFactory</returns>
        public static ILoggerFactory ConfigureLogging(LogLevel logLevel = LogLevel.Information)
        {
            // Create and configure logger factory
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("DataConvertor", logLevel)
                    .AddFilter("MyBook.Writer", logLevel)
                    .AddConsole()
                    .AddDebug();
            });

            // Configure MyBook.Writer to use our logger factory
            loggerFactory.ConfigureBookWriter();

            return loggerFactory;
        }
    }
} 