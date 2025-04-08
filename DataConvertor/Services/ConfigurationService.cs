using Microsoft.Extensions.Logging;
using MyBook.Writer;

namespace DataConvertor.Services
{
    /// <summary>
    /// Service for managing application configuration
    /// </summary>
    public static class ConfigurationService
    {
        /// <summary>
        /// Default configuration settings
        /// </summary>
        public static class Defaults
        {
            /// <summary>
            /// Default log level
            /// </summary>
            public const LogLevel LogLevel = Microsoft.Extensions.Logging.LogLevel.Information;

            /// <summary>
            /// Default output format
            /// </summary>
            public const BookFormat OutputFormat = BookFormat.FB2;

            /// <summary>
            /// Default base location for book files
            /// </summary>
            public const string BaseLocation = "%USERPROFILE%/ScrapedBooks";

            /// <summary>
            /// Default language for output books
            /// </summary>
            public const string DefaultLanguage = "ua";
        }

        /// <summary>
        /// Get the expanded base location for book files
        /// </summary>
        /// <param name="baseLocation">Base location with environment variables</param>
        /// <returns>Expanded path</returns>
        public static string GetExpandedBaseLocation(string baseLocation = Defaults.BaseLocation)
        {
            return Environment.ExpandEnvironmentVariables(baseLocation);
        }

        /// <summary>
        /// Get output file path for a given input file
        /// </summary>
        /// <param name="inputFilePath">Input file path</param>
        /// <param name="format">Output format</param>
        /// <param name="baseLocation">Optional custom base location</param>
        /// <returns>Output file path</returns>
        public static string GetOutputFilePath(string inputFilePath, BookFormat format, string? baseLocation = null)
        {
            var expandedBaseLocation = baseLocation != null 
                ? GetExpandedBaseLocation(baseLocation) 
                : GetExpandedBaseLocation();
            
            var filename = Path.GetFileNameWithoutExtension(inputFilePath);
            var extension = format.ToString().ToLowerInvariant();
            
            return Path.Combine(expandedBaseLocation, $"{filename}.{extension}");
        }
    }
} 