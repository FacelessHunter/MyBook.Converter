using Microsoft.Extensions.Logging;

namespace MyBook.Writer.Core.Services
{
    /// <summary>
    /// Extension methods for configuring logging in MyBook.Writer
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Configure MyBook.Writer to use the specified logger factory
        /// </summary>
        /// <param name="loggerFactory">Logger factory to use</param>
        /// <returns>The logger factory for method chaining</returns>
        public static ILoggerFactory ConfigureBookWriter(this ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            BookFactory.SetLoggerFactory(loggerFactory);
            return loggerFactory;
        }
    }
}