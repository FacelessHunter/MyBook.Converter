using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MyBook.Writer.Builders;
using MyBook.Writer.Core.Interfaces;

namespace MyBook.Writer
{
    /// <summary>
    /// Supported e-book formats
    /// </summary>
    public enum BookFormat
    {
        /// <summary>
        /// Electronic Publication format
        /// </summary>
        Epub,

        /// <summary>
        /// FictionBook format
        /// </summary>
        FB2
    }

    /// <summary>
    /// Factory for creating book builders of different formats
    /// </summary>
    public static class BookFactory
    {
        private static ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

        /// <summary>
        /// Set the logger factory for creating loggers
        /// </summary>
        /// <param name="loggerFactory">Logger factory to use</param>
        public static void SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }

        /// <summary>
        /// Create a book builder for a specific format
        /// </summary>
        /// <param name="format">Book format to create</param>
        /// <param name="title">Book title</param>
        /// <param name="language">Book language</param>
        /// <returns>Builder for the specified format</returns>
        public static IBookBuilder CreateBuilder(BookFormat format, string title, string language)
        {
            var logger = _loggerFactory.CreateLogger("BookFactory");

            logger.LogInformation("Creating book builder. Format: {Format}, Title: {Title}, Language: {Language}",
                format, title, language);

            return format switch
            {
                BookFormat.Epub => EpubBuilder.Create(
                    title,
                    language,
                    _loggerFactory.CreateLogger<EpubBuilder>(),
                    _loggerFactory),

                BookFormat.FB2 => FB2Builder.Create(
                    title,
                    language,
                    _loggerFactory.CreateLogger<FB2Builder>(),
                    _loggerFactory),

                _ => throw new ArgumentException($"Unsupported book format: {format}", nameof(format))
            };
        }
    }
}