using Microsoft.Extensions.Logging;
using MyBook.Writer.Core.Services;

namespace MyBook.Writer.Examples
{
    /// <summary>
    /// Example demonstrating how to use logging with MyBook.Writer
    /// </summary>
    public static class LoggingExample
    {
        /// <summary>
        /// Run the logging example
        /// </summary>
        public static async Task RunAsync()
        {
            // Configure logging
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("MyBook.Writer", LogLevel.Debug) // Set minimum level for our library
                    .AddConsole() // Log to console
                    .AddDebug();  // Log to debug output
            });

            // Configure the book writer library to use our logger factory
            loggerFactory.ConfigureBookWriter();

            // Create a logger for this example
            var logger = loggerFactory.CreateLogger("LoggingExample");

            logger.LogInformation("Starting logging example");

            // Create a simple book
            var builder = BookFactory.CreateBuilder(BookFormat.Epub, "Sample Book with Logging", "en");

            // Add metadata
            builder
                .WithAuthor("John Doe")
                .WithTitle("Sample Book with Logging")
                .WithLanguage("en");

            logger.LogInformation("Added book metadata");

            // Add some chapters
            builder.AddChapter(1, "Chapter 1", new List<string>
            {
                "This is the first paragraph of the first chapter.",
                "This is the second paragraph of the first chapter."
            });

            builder.AddChapter(2, "Chapter 2", new List<string>
            {
                "This is the first paragraph of the second chapter.",
                "This is the second paragraph of the second chapter."
            });

            logger.LogInformation("Added book chapters");

            // Build the book
            var writer = await builder.BuildAsync();
            logger.LogInformation("Book built successfully");

            // Save the book to a file
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "sample-book.epub");
            await writer.SaveToFileAsync(outputPath);

            logger.LogInformation("Book saved to {FilePath}", outputPath);
            logger.LogInformation("Logging example completed");
        }
    }
}