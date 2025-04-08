using Microsoft.Extensions.Logging;
using MyBook.Writer.Core;
using System.Xml.Linq;

namespace MyBook.Writer.Writers.FB2
{
    /// <summary>
    /// Writer for saving books in FB2 format
    /// </summary>
    public class FB2Writer : BaseWriter
    {
        private readonly XDocument _document;

        /// <summary>
        /// Create a new FB2 writer
        /// </summary>
        /// <param name="document">XML document containing FB2 data</param>
        /// <param name="logger">Logger for recording operations</param>
        /// <param name="bookTitle">Title of the book</param>
        public FB2Writer(XDocument document, ILogger<FB2Writer> logger, string bookTitle = "")
            : base(BookFormat.FB2, logger)
        {
            _document = document;
            BookTitle = bookTitle;
        }

        /// <summary>
        /// Save the FB2 book to a memory stream
        /// </summary>
        /// <returns>Stream containing the FB2 XML data</returns>
        public override async Task<Stream> SaveAsStreamAsync()
        {
            _logger.LogInformation("Starting to save FB2 to memory stream");

            try
            {
                var outputStream = new MemoryStream();
                await _document.SaveAsync(outputStream, SaveOptions.None, CancellationToken.None);
                outputStream.Position = 0;

                _logger.LogInformation("FB2 saved to memory stream successfully. Size: {Size} bytes", outputStream.Length);
                return outputStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving FB2 to memory stream");
                throw;
            }
        }
    }
}