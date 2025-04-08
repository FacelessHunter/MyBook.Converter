using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MyBook.Writer.Core.Interfaces;
using MyBook.Writer.Core.Models;
using MyBook.Writer.Writers.FB2;
using System.Xml.Linq;

namespace MyBook.Writer.Builders
{
    /// <summary>
    /// Builder for creating FB2 format books with a fluent API
    /// </summary>
    public class FB2Builder : IBookBuilder
    {
        private readonly Book _book;
        private readonly XDocument _document;
        private readonly ILogger<FB2Builder> _logger;
        private readonly ILoggerFactory _loggerFactory;

        private static readonly XNamespace ns = "http://www.gribuser.ru/xml/fictionbook/2.0";
        private static readonly XNamespace xlinkNs = "http://www.w3.org/1999/xlink";

        /// <summary>
        /// Create a new FB2 book builder
        /// </summary>
        public FB2Builder() : this(NullLogger<FB2Builder>.Instance)
        {
        }

        /// <summary>
        /// Create a new FB2 book builder with a logger
        /// </summary>
        /// <param name="logger">Logger for recording operations</param>
        public FB2Builder(ILogger<FB2Builder> logger)
        {
            _book = new Book();
            _document = InitializeDocument();
            _logger = logger;
            _loggerFactory = NullLoggerFactory.Instance;
        }

        /// <summary>
        /// Create a new FB2 book builder with a logger and logger factory
        /// </summary>
        /// <param name="logger">Logger for recording operations</param>
        /// <param name="loggerFactory">Factory for creating loggers</param>
        public FB2Builder(ILogger<FB2Builder> logger, ILoggerFactory loggerFactory)
        {
            _book = new Book();
            _document = InitializeDocument();
            _logger = logger;
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }

        /// <summary>
        /// Create a new FB2 builder with a title and language
        /// </summary>
        /// <param name="title">Book title</param>
        /// <param name="language">Book language code</param>
        /// <returns>Builder instance</returns>
        public static FB2Builder Create(string title, string language)
        {
            return new FB2Builder()
                .WithTitle(title)
                .WithLanguage(language);
        }

        /// <summary>
        /// Create a new FB2 builder with a title, language, and logger
        /// </summary>
        /// <param name="title">Book title</param>
        /// <param name="language">Book language code</param>
        /// <param name="logger">Logger for recording operations</param>
        /// <returns>Builder instance</returns>
        public static FB2Builder Create(string title, string language, ILogger<FB2Builder> logger)
        {
            return new FB2Builder(logger)
                .WithTitle(title)
                .WithLanguage(language);
        }

        /// <summary>
        /// Create a new FB2 builder with a title, language, logger, and logger factory
        /// </summary>
        /// <param name="title">Book title</param>
        /// <param name="language">Book language code</param>
        /// <param name="logger">Logger for recording operations</param>
        /// <param name="loggerFactory">Factory for creating loggers</param>
        /// <returns>Builder instance</returns>
        public static FB2Builder Create(string title, string language, ILogger<FB2Builder> logger, ILoggerFactory loggerFactory)
        {
            return new FB2Builder(logger, loggerFactory)
                .WithTitle(title)
                .WithLanguage(language);
        }

        /// <summary>
        /// Set the book title
        /// </summary>
        /// <param name="title">Book title</param>
        /// <returns>Builder instance for method chaining</returns>
        public FB2Builder WithTitle(string title)
        {
            _logger.LogDebug("Setting book title: {Title}", title);
            _book.Metadata.Title = title;

            // Add title to the FB2 document
            _document
                .Element(FormatName("FictionBook"))?
                .Element(FormatName("description"))?
                .Element(FormatName("title-info"))?
                .Add(new XElement(FormatName("book-title"), title));

            return this;
        }

        /// <summary>
        /// Set the book language
        /// </summary>
        /// <param name="language">Language code (e.g., "en")</param>
        /// <returns>Builder instance for method chaining</returns>
        public FB2Builder WithLanguage(string language)
        {
            _logger.LogDebug("Setting book language: {Language}", language);
            _book.Metadata.Language = language;

            // Add language to the FB2 document
            _document
                .Element(FormatName("FictionBook"))?
                .Element(FormatName("description"))?
                .Element(FormatName("title-info"))?
                .Add(new XElement(FormatName("lang"), language));

            return this;
        }

        /// <summary>
        /// Set the book author
        /// </summary>
        /// <param name="author">Author name</param>
        /// <returns>Builder instance for method chaining</returns>
        public FB2Builder WithAuthor(string author)
        {
            _logger.LogDebug("Setting book author: {Author}", author);
            _book.Metadata.Author = author;

            // Create author element - assuming simple first/last name
            var parts = author.Split(' ');
            var firstName = parts.Length > 0 ? parts[0] : string.Empty;
            var lastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;

            var authorElement = new XElement(FormatName("author"),
                new XElement(FormatName("first-name"), firstName),
                new XElement(FormatName("last-name"), lastName)
            );

            _document
                .Element(FormatName("FictionBook"))?
                .Element(FormatName("description"))?
                .Element(FormatName("title-info"))?
                .Add(authorElement);

            return this;
        }

        /// <summary>
        /// Add a cover image to the book
        /// </summary>
        /// <param name="imageStream">Image data stream</param>
        /// <param name="fileName">Image file name</param>
        /// <param name="contentType">Image content type (MIME type)</param>
        /// <returns>Builder instance for method chaining</returns>
        public FB2Builder WithCover(Stream imageStream, string fileName, string contentType)
        {
            _logger.LogInformation("Adding cover image: {FileName}, Content-Type: {ContentType}", fileName, contentType);

            // First, save to the book model
            _book.Cover = CoverImage.FromStreamAsync(imageStream, fileName, contentType).Result;

            // Convert to FB2 binary and add to document
            var ms = new MemoryStream();
            imageStream.Position = 0;
            imageStream.CopyTo(ms);
            string coverId = "cover-image";

            var encodedCoverpage = Convert.ToBase64String(ms.ToArray());
            var binary = new XElement(FormatName("binary"),
                new XAttribute("id", coverId),
                new XAttribute("content-type", contentType),
                encodedCoverpage);

            _document?
                .Element(FormatName("FictionBook"))?
                .Add(binary);

            _document?
                .Element(FormatName("FictionBook"))?
                .Element(FormatName("description"))?
                .Element(FormatName("title-info"))?
                .Add(
                    new XElement(FormatName("coverpage"),
                        new XElement(FormatName("image"),
                            new XAttribute(XNamespace.Xmlns + "l", xlinkNs),
                            new XAttribute(xlinkNs + "href", "#" + coverId)
                        )
                    )
                );

            return this;
        }

        /// <summary>
        /// Add a chapter to the book
        /// </summary>
        /// <param name="chapterId">Chapter ID/sequence number</param>
        /// <param name="title">Chapter title</param>
        /// <param name="paragraphs">List of paragraph texts</param>
        /// <returns>Builder instance for method chaining</returns>
        public FB2Builder AddChapter(int chapterId, string title, IEnumerable<string> paragraphs)
        {
            _logger.LogInformation("Adding chapter {ChapterId}: {Title} with {ParagraphCount} paragraphs",
                chapterId, title, paragraphs.Count());

            // Add to book model
            var chapter = new Chapter
            {
                Title = title
            };
            chapter.AddParagraphs(paragraphs);
            _book.AddChapter(chapterId, chapter);

            // Add to FB2 document
            var section = new XElement(FormatName("section"),
                new XElement(FormatName("title"),
                new XElement(FormatName("p"), title)));

            section.Add(paragraphs.Select(line =>
                new XElement(FormatName("p"), line)));

            _document?
                .Element(FormatName("FictionBook"))?
                .Element(FormatName("body"))?
                .Add(section);

            return this;
        }

        /// <summary>
        /// Build the FB2 book and prepare for saving
        /// </summary>
        /// <returns>IBookWriter implementation for saving</returns>
        public async Task<IBookWriter> BuildAsync()
        {
            _logger.LogInformation("Building FB2 book: {Title}", _book.Metadata.Title);

            try
            {
                _logger.LogInformation("FB2 book built successfully");
                return new FB2Writer(_document, _loggerFactory.CreateLogger<FB2Writer>(), _book.Metadata.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building FB2 book: {Title}", _book.Metadata.Title);
                throw;
            }
        }

        // Implement IBookBuilder interface
        IBookBuilder IBookBuilder.WithTitle(string title) => WithTitle(title);

        IBookBuilder IBookBuilder.WithLanguage(string language) => WithLanguage(language);

        IBookBuilder IBookBuilder.WithAuthor(string author) => WithAuthor(author);

        IBookBuilder IBookBuilder.WithCover(Stream imageStream, string fileName, string contentType) => WithCover(imageStream, fileName, contentType);

        IBookBuilder IBookBuilder.AddChapter(int chapterId, string title, IEnumerable<string> paragraphs) => AddChapter(chapterId, title, paragraphs);

        /// <summary>
        /// Format XML element names with the FB2 namespace
        /// </summary>
        /// <param name="name">Element name</param>
        /// <returns>Qualified XML name</returns>
        private static XName FormatName(string name) => ns + name;

        /// <summary>
        /// Initialize a new FB2 document structure
        /// </summary>
        /// <returns>Basic FB2 XML document</returns>
        private static XDocument InitializeDocument()
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(ns + "FictionBook",
                    new XAttribute(XNamespace.Xmlns + "xlink", xlinkNs),
                    new XElement(ns + "description",
                        new XElement(ns + "title-info"),
                        new XElement(ns + "document-info",
                            new XElement(ns + "program-used", "MyBook.Writer")
                        )
                    ),
                    new XElement(ns + "body")
                )
            );

            return doc;
        }
    }
}