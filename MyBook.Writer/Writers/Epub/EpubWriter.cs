using Microsoft.Extensions.Logging;
using MyBook.Writer.Core;
using System.IO.Compression;
using System.Xml.Linq;

namespace MyBook.Writer.Writers.Epub
{
    /// <summary>
    /// EPUB writer implementation that saves books in EPUB format
    /// </summary>
    public class EpubWriter : BaseWriter
    {
        private readonly Dictionary<string, MemoryStream> _contentStreams;
        private readonly Dictionary<string, XDocument> _xmlDocuments;

        /// <summary>
        /// Create a new EPUB writer
        /// </summary>
        /// <param name="contentStreams">Dictionary of content streams</param>
        /// <param name="xmlDocuments">Dictionary of XML documents</param>
        /// <param name="logger">Logger for recording operations</param>
        /// <param name="bookTitle">Title of the book</param>
        public EpubWriter(
            Dictionary<string, MemoryStream> contentStreams,
            Dictionary<string, XDocument> xmlDocuments,
            ILogger<EpubWriter> logger,
            string bookTitle = "")
            : base(BookFormat.Epub, logger)
        {
            _contentStreams = contentStreams;
            _xmlDocuments = xmlDocuments;
            BookTitle = bookTitle;
        }

        /// <summary>
        /// Save the EPUB book to a memory stream
        /// </summary>
        /// <returns>MemoryStream containing the EPUB data</returns>
        public override async Task<Stream> SaveAsStreamAsync()
        {
            _logger.LogInformation("Starting to save EPUB to memory stream");
            var outputStream = new MemoryStream();

            try
            {
                using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
                {
                    // Add mimetype first and uncompressed
                    _logger.LogDebug("Adding mimetype file (uncompressed)");
                    var mimeEntry = archive.CreateEntry("mimetype", CompressionLevel.NoCompression);
                    using (var entryStream = mimeEntry.Open())
                    {
                        var mimeStream = _contentStreams["mimetype"];
                        mimeStream.Position = 0;
                        await mimeStream.CopyToAsync(entryStream);
                    }

                    // Add all other content
                    _logger.LogDebug("Adding {ContentCount} content files", _contentStreams.Count - 1);
                    foreach (var content in _contentStreams.Where(c => c.Key != "mimetype"))
                    {
                        _logger.LogTrace("Adding content file: {FileName}", content.Key);
                        var entry = archive.CreateEntry(content.Key, CompressionLevel.Optimal);
                        using var entryStream = entry.Open();
                        content.Value.Position = 0;
                        await content.Value.CopyToAsync(entryStream);
                    }

                    // Add all XML documents
                    _logger.LogDebug("Adding {XmlCount} XML documents", _xmlDocuments.Count);
                    foreach (var doc in _xmlDocuments)
                    {
                        _logger.LogTrace("Adding XML document: {FileName}", doc.Key);
                        var entry = archive.CreateEntry(doc.Key, CompressionLevel.Optimal);
                        using var entryStream = entry.Open();
                        await doc.Value.SaveAsync(entryStream, SaveOptions.None, CancellationToken.None);
                    }
                }

                outputStream.Position = 0;
                _logger.LogInformation("EPUB saved to memory stream successfully. Size: {Size} bytes", outputStream.Length);
                return outputStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving EPUB to memory stream");
                throw;
            }
        }
    }
}