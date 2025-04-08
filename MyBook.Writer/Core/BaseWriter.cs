using Microsoft.Extensions.Logging;
using MyBook.Writer.Core.Interfaces;

namespace MyBook.Writer.Core
{
    /// <summary>
    /// Base class for book writers providing common functionality
    /// </summary>
    public abstract class BaseWriter : IBookWriter
    {
        protected readonly ILogger _logger;

        /// <summary>
        /// Book format for this writer
        /// </summary>
        protected BookFormat Format { get; }

        /// <summary>
        /// Book title
        /// </summary>
        protected string BookTitle { get; set; } = string.Empty;

        /// <summary>
        /// Constructor for the base writer
        /// </summary>
        /// <param name="format">Book format</param>
        /// <param name="logger">Logger for recording operations</param>
        protected BaseWriter(BookFormat format, ILogger logger)
        {
            Format = format;
            _logger = logger;
        }

        /// <summary>
        /// Set the book title
        /// </summary>
        /// <param name="title">Book title</param>
        public void SetBookTitle(string title)
        {
            BookTitle = title;
        }

        /// <summary>
        /// Get the file extension for the current book format
        /// </summary>
        /// <returns>File extension including the dot</returns>
        protected string GetFileExtension()
        {
            return Format switch
            {
                BookFormat.Epub => ".epub",
                BookFormat.FB2 => ".fb2",
                _ => throw new NotSupportedException($"Unsupported book format: {Format}")
            };
        }

        /// <summary>
        /// Save the book to a memory stream
        /// </summary>
        /// <returns>Stream containing the book data</returns>
        public abstract Task<Stream> SaveAsStreamAsync();

        /// <summary>
        /// Save the book to a file
        /// </summary>
        /// <param name="outputPath">File path to save the book</param>
        public virtual async Task SaveToFileAsync(string outputPath)
        {
            _logger.LogInformation("Saving {Format} to file: {FilePath}", Format, outputPath);

            try
            {
                // Check if outputPath is a directory
                if (Directory.Exists(outputPath))
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string sanitizedTitle = SanitizeFileName(BookTitle);
                    string fileName = $"{timestamp}_{sanitizedTitle}{GetFileExtension()}";
                    outputPath = Path.Combine(outputPath, sanitizedTitle, fileName);
                }
                else
                {
                    // Ensure the file has the correct extension
                    string currentExtension = Path.GetExtension(outputPath);
                    string requiredExtension = GetFileExtension();

                    if (string.IsNullOrEmpty(currentExtension))
                    {
                        // No extension provided, append the correct one
                        outputPath += requiredExtension;
                    }
                    else if (!currentExtension.Equals(requiredExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        // Replace incorrect extension with the correct one
                        outputPath = Path.ChangeExtension(outputPath, requiredExtension.TrimStart('.'));
                    }
                }

                // Create directory if it doesn't exist
                string directoryPath = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    _logger.LogDebug("Creating directory: {DirectoryPath}", directoryPath);
                    Directory.CreateDirectory(directoryPath);
                }

                // Save to file
                using var stream = await SaveAsStreamAsync();
                using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(fileStream);

                _logger.LogInformation("{Format} file saved successfully: {FilePath}", Format, outputPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving {Format} to file: {FilePath}", Format, outputPath);
                throw;
            }
        }

        /// <summary>
        /// Sanitize a file name to remove invalid characters
        /// </summary>
        /// <param name="fileName">File name to sanitize</param>
        /// <returns>Sanitized file name</returns>
        private static string SanitizeFileName(string fileName)
        {
            // Remove characters that are invalid in file names
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries))
                .TrimEnd('.');

            // If the sanitized name is empty, provide a default
            return string.IsNullOrWhiteSpace(sanitized) ? "Book" : sanitized;
        }
    }
}