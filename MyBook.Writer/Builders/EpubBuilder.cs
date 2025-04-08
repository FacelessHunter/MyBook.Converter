using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MyBook.Writer.Core.Interfaces;
using MyBook.Writer.Core.Models;
using MyBook.Writer.Writers.Epub;
using System.Xml.Linq;

namespace MyBook.Writer.Builders
{
    /// <summary>
    /// Builder for creating EPUB books with a fluent API
    /// </summary>
    public class EpubBuilder : IBookBuilder
    {
        private readonly Book _book;
        private readonly Dictionary<string, MemoryStream> _contentStreams = new();
        private readonly Dictionary<string, XDocument> _xmlDocuments = new();
        private readonly ILogger<EpubBuilder> _logger;
        private readonly ILoggerFactory _loggerFactory;

        // XML namespaces
        private static readonly XNamespace xhtml = "http://www.w3.org/1999/xhtml";

        private static readonly XNamespace epub = "http://www.idpf.org/2007/ops";
        private static readonly XNamespace opf = "http://www.idpf.org/2007/opf";
        private static readonly XNamespace dc = "http://purl.org/dc/elements/1.1/";
        private static readonly XNamespace container = "urn:oasis:names:tc:opendocument:xmlns:container";

        /// <summary>
        /// Create a new EPUB builder
        /// </summary>
        public EpubBuilder() : this(NullLogger<EpubBuilder>.Instance)
        {
        }

        /// <summary>
        /// Create a new EPUB builder with a logger
        /// </summary>
        /// <param name="logger">Logger for recording operations</param>
        public EpubBuilder(ILogger<EpubBuilder> logger)
        {
            _book = new Book();
            _logger = logger;
            _loggerFactory = NullLoggerFactory.Instance;
        }

        /// <summary>
        /// Create a new EPUB builder with a logger factory
        /// </summary>
        /// <param name="logger">Logger for recording operations</param>
        /// <param name="loggerFactory">Factory for creating loggers</param>
        public EpubBuilder(ILogger<EpubBuilder> logger, ILoggerFactory loggerFactory)
        {
            _book = new Book();
            _logger = logger;
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }

        /// <summary>
        /// Create a new EPUB builder with a title and language
        /// </summary>
        /// <param name="title">Book title</param>
        /// <param name="language">Book language code</param>
        /// <returns>EpubBuilder instance</returns>
        public static EpubBuilder Create(string title, string language)
        {
            return new EpubBuilder()
                .WithTitle(title)
                .WithLanguage(language);
        }

        /// <summary>
        /// Create a new EPUB builder with a title, language, and logger
        /// </summary>
        /// <param name="title">Book title</param>
        /// <param name="language">Book language code</param>
        /// <param name="logger">Logger for recording operations</param>
        /// <returns>EpubBuilder instance</returns>
        public static EpubBuilder Create(string title, string language, ILogger<EpubBuilder> logger)
        {
            return new EpubBuilder(logger)
                .WithTitle(title)
                .WithLanguage(language);
        }

        /// <summary>
        /// Create a new EPUB builder with a title, language, and logger factory
        /// </summary>
        /// <param name="title">Book title</param>
        /// <param name="language">Book language code</param>
        /// <param name="logger">Logger for recording operations</param>
        /// <param name="loggerFactory">Factory for creating loggers</param>
        /// <returns>EpubBuilder instance</returns>
        public static EpubBuilder Create(string title, string language, ILogger<EpubBuilder> logger, ILoggerFactory loggerFactory)
        {
            return new EpubBuilder(logger, loggerFactory)
                .WithTitle(title)
                .WithLanguage(language);
        }

        /// <summary>
        /// Set the book title
        /// </summary>
        /// <param name="title">Book title</param>
        /// <returns>Builder instance for method chaining</returns>
        public EpubBuilder WithTitle(string title)
        {
            _logger.LogDebug("Setting book title: {Title}", title);
            _book.Metadata.Title = title;
            return this;
        }

        /// <summary>
        /// Set the book language
        /// </summary>
        /// <param name="language">Language code (e.g., "en")</param>
        /// <returns>Builder instance for method chaining</returns>
        public EpubBuilder WithLanguage(string language)
        {
            _logger.LogDebug("Setting book language: {Language}", language);
            _book.Metadata.Language = language;
            return this;
        }

        /// <summary>
        /// Set the book author
        /// </summary>
        /// <param name="author">Author name</param>
        /// <returns>Builder instance for method chaining</returns>
        public EpubBuilder WithAuthor(string author)
        {
            _logger.LogDebug("Setting book author: {Author}", author);
            _book.Metadata.Author = author;
            return this;
        }

        /// <summary>
        /// Add a cover image
        /// </summary>
        /// <param name="imageStream">Image data stream</param>
        /// <param name="fileName">Image file name</param>
        /// <param name="contentType">Image content type (MIME type)</param>
        /// <returns>Builder instance for method chaining</returns>
        public EpubBuilder WithCover(Stream imageStream, string fileName, string contentType)
        {
            _logger.LogInformation("Adding cover image: {FileName}, Content-Type: {ContentType}", fileName, contentType);
            _book.Cover = CoverImage.FromStreamAsync(imageStream, fileName, contentType).Result;
            return this;
        }

        /// <summary>
        /// Add a chapter to the book
        /// </summary>
        /// <param name="chapterId">Chapter ID/sequence number</param>
        /// <param name="title">Chapter title</param>
        /// <param name="paragraphs">List of paragraph texts</param>
        /// <returns>Builder instance for method chaining</returns>
        public EpubBuilder AddChapter(int chapterId, string title, IEnumerable<string> paragraphs)
        {
            _logger.LogInformation("Adding chapter {ChapterId}: {Title} with {ParagraphCount} paragraphs",
                chapterId, title, paragraphs.Count());

            var chapter = new Chapter
            {
                Title = title
            };
            chapter.AddParagraphs(paragraphs);
            _book.AddChapter(chapterId, chapter);

            return this;
        }

        /// <summary>
        /// Build the EPUB book and prepare it for saving
        /// </summary>
        /// <returns>IBookWriter implementation for saving the book</returns>
        public async Task<IBookWriter> BuildAsync()
        {
            _logger.LogInformation("Building EPUB book: {Title}", _book.Metadata.Title);

            try
            {
                _logger.LogDebug("Creating mimetype file");
                await CreateMimetypeAsync();

                _logger.LogDebug("Creating CSS stylesheet");
                await CreateStyleCssAsync();

                _logger.LogDebug("Creating cover page");
                await CreateCoverPageAsync();

                _logger.LogDebug("Creating container.xml");
                CreateContainerXml();

                _logger.LogDebug("Creating content.opf with metadata and manifest");
                CreateContentOpf();

                _logger.LogDebug("Creating table of contents");
                CreateToc();

                _logger.LogDebug("Creating {ChapterCount} chapters", _book.Chapters.Count);
                CreateChapters();

                _logger.LogInformation("EPUB book built successfully: {ContentFiles} content files, {XmlFiles} XML files",
                    _contentStreams.Count, _xmlDocuments.Count);

                return new EpubWriter(
                    _contentStreams,
                    _xmlDocuments,
                    _loggerFactory.CreateLogger<EpubWriter>(),
                    _book.Metadata.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building EPUB book: {Title}", _book.Metadata.Title);
                throw;
            }
        }

        // Implement IBookBuilder interface
        IBookBuilder IBookBuilder.WithTitle(string title) => WithTitle(title);

        IBookBuilder IBookBuilder.WithLanguage(string language) => WithLanguage(language);

        IBookBuilder IBookBuilder.WithAuthor(string author) => WithAuthor(author);

        IBookBuilder IBookBuilder.WithCover(Stream imageStream, string fileName, string contentType) => WithCover(imageStream, fileName, contentType);

        IBookBuilder IBookBuilder.AddChapter(int chapterId, string title, IEnumerable<string> paragraphs) => AddChapter(chapterId, title, paragraphs);

        #region Private implementation methods

        private async Task CreateMimetypeAsync()
        {
            _logger.LogTrace("Creating mimetype file with content: application/epub+zip");
            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, leaveOpen: true);
            await writer.WriteAsync("application/epub+zip");
            await writer.FlushAsync();
            stream.Position = 0;
            _contentStreams["mimetype"] = stream;
        }

        private async Task CreateStyleCssAsync()
        {
            _logger.LogTrace("Creating style.css for EPUB styling");
            string css = """
                body {
                    font-family: serif;
                    line-height: 1.2;
                    margin: 1em;
                }

                p {
                    text-indent: 1.5em;
                    margin-top: 0;
                    margin-bottom: 1em;
                }

                h1.chapter-title {
                    text-align: center;
                }

                /* Table of Contents Styling */
                .toc-container {
                    margin: 2em 1em;
                }

                nav#toc ol {
                    margin-top: 1em;
                }

                nav#toc li {
                    margin-bottom: 0.5em;
                }

                nav#toc a {
                    color: #000;
                    text-decoration: none;
                    display: block;
                    padding: 0.3em 0;
                }

                nav#toc a:hover {
                    color: #444;
                    text-decoration: underline;
                }
                """;

            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, leaveOpen: true);
            await writer.WriteAsync(css);
            await writer.FlushAsync();
            stream.Position = 0;
            _contentStreams["OEBPS/style.css"] = stream;
            _logger.LogTrace("Style.css created with {Length} bytes", stream.Length);
        }

        private async Task CreateCoverPageAsync()
        {
            if (_book.Cover is null)
            {
                _logger.LogDebug("No cover image provided, skipping cover page creation");
                return;
            }

            _logger.LogDebug("Creating cover page with image: {FileName}", _book.Cover.FileName);

            // Save cover image
            var coverStream = _book.Cover.GetDataCopy();
            _contentStreams[$"OEBPS/{_book.Cover.FileName}"] = coverStream;
            _logger.LogTrace("Cover image saved: {FileName}, {Size} bytes", _book.Cover.FileName, coverStream.Length);

            // Create cover XHTML
            var coverXhtml = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(xhtml + "html",
                    new XAttribute("xmlns", xhtml),
                    new XAttribute("lang", _book.Metadata.Language),
                    new XAttribute(XNamespace.Xmlns + "epub", epub),
                    new XElement(xhtml + "head",
                        new XElement(xhtml + "title", "Cover"),
                        new XElement(xhtml + "meta", new XAttribute("charset", "utf-8"))
                    ),
                    new XElement(xhtml + "body",
                        new XElement(xhtml + "section",
                            new XAttribute(epub + "type", "cover"),
                            new XAttribute("role", "doc-cover"),
                            new XElement(xhtml + "img",
                                new XAttribute("src", _book.Cover.FileName),
                                new XAttribute("alt", "Cover Image")
                            )
                        )
                    )
                )
            );

            _xmlDocuments["OEBPS/cover.xhtml"] = coverXhtml;
            _logger.LogTrace("Cover XHTML created: cover.xhtml");
        }

        private void CreateContainerXml()
        {
            _logger.LogTrace("Creating container.xml with rootfile path: OEBPS/content.opf");
            var doc = new XDocument(
                new XElement(container + "container",
                    new XAttribute("version", "1.0"),
                    new XElement(container + "rootfiles",
                        new XElement(container + "rootfile",
                            new XAttribute("full-path", "OEBPS/content.opf"),
                            new XAttribute("media-type", "application/oebps-package+xml")
                        )
                    )
                )
            );

            _xmlDocuments["META-INF/container.xml"] = doc;
        }

        private void CreateContentOpf()
        {
            _logger.LogDebug("Creating content.opf with {ChapterCount} chapters", _book.Chapters.Count);
            var chapters = _book.Chapters;

            var metadataElement =
                new XElement(opf + "metadata",
                    new XAttribute(XNamespace.Xmlns + "dc", dc),
                    new XElement(dc + "identifier",
                        new XAttribute("id", "book-id"),
                        "urn:uuid:" + _book.Metadata.Identifier
                    ),
                    new XElement(dc + "title", _book.Metadata.Title),
                    new XElement(dc + "language", _book.Metadata.Language)
                );

            // Add author if present
            if (!string.IsNullOrEmpty(_book.Metadata.Author))
            {
                _logger.LogTrace("Adding author to metadata: {Author}", _book.Metadata.Author);
                metadataElement.Add(
                    new XElement(dc + "creator",
                        new XAttribute(opf + "role", "aut"),
                        _book.Metadata.Author)
                );
            }

            // Add publication date if present
            if (_book.Metadata.PublicationDate.HasValue)
            {
                _logger.LogTrace("Adding publication date to metadata: {Date}",
                    _book.Metadata.PublicationDate.Value.ToString("yyyy-MM-dd"));
                metadataElement.Add(
                    new XElement(dc + "date", _book.Metadata.PublicationDate.Value.ToString("yyyy-MM-dd"))
                );
            }

            // Add other metadata
            metadataElement.Add(
                new XElement(opf + "meta",
                    new XAttribute("property", "dcterms:modified"),
                    DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                ),
                new XElement(opf + "meta",
                    new XAttribute("property", "rendition:layout"),
                    "reflowable"
                )
            );

            _logger.LogTrace("Creating manifest items for chapters");
            var chapterManifestReferences = chapters.Select(i =>
                new XElement(opf + "item",
                        new XAttribute("id", $"{FormatChapterId(i.Key)}"),
                        new XAttribute("href", $"{FormatChapterId(i.Key)}.xhtml"),
                        new XAttribute("media-type", "application/xhtml+xml")
                ));

            _logger.LogTrace("Creating spine items for chapters");
            var spineSequenceElements = chapters.Select(i =>
                new XElement(opf + "itemref",
                        new XAttribute("idref", $"{FormatChapterId(i.Key)}")
                ));

            var manifestItems = new List<XElement>
            {
                new XElement(opf + "item",
                    new XAttribute("id", "toc"),
                    new XAttribute("href", "toc.xhtml"),
                    new XAttribute("media-type", "application/xhtml+xml"),
                    new XAttribute("properties", "nav")
                ),
                new XElement(opf + "item",
                    new XAttribute("id", "style"),
                    new XAttribute("href", "style.css"),
                    new XAttribute("media-type", "text/css")
                )
            };

            if (_book.Cover != null)
            {
                _logger.LogTrace("Adding cover image to manifest");
                manifestItems.Add(
                    new XElement(opf + "item",
                        new XAttribute("id", "cover-image"),
                        new XAttribute("href", _book.Cover.FileName),
                        new XAttribute("media-type", _book.Cover.ContentType),
                        new XAttribute("properties", "cover-image")
                    )
                );

                manifestItems.Add(
                    new XElement(opf + "item",
                        new XAttribute("id", "cover-page"),
                        new XAttribute("href", "cover.xhtml"),
                        new XAttribute("media-type", "application/xhtml+xml")
                    )
                );
            }

            // Add chapter manifest items
            manifestItems.AddRange(chapterManifestReferences);

            var manifestElement = new XElement(opf + "manifest", manifestItems);

            var spineItems = new List<XElement>();

            if (_book.Cover != null)
            {
                _logger.LogTrace("Adding cover page to spine");
                spineItems.Add(
                    new XElement(opf + "itemref",
                        new XAttribute("idref", "cover-page")
                    )
                );
            }

            // Add ToC as the second page (after cover or as first page if no cover)
            spineItems.Add(
                new XElement(opf + "itemref",
                    new XAttribute("idref", "toc")
                )
            );

            // Add chapter spine items
            spineItems.AddRange(spineSequenceElements);

            var spineElement = new XElement(opf + "spine", spineItems);

            var doc = new XDocument(
                new XElement(opf + "package",
                    new XAttribute("version", "3.0"),
                    new XAttribute("unique-identifier", "book-id"),
                    metadataElement,
                    manifestElement,
                    spineElement
                )
            );

            _xmlDocuments["OEBPS/content.opf"] = doc;
            _logger.LogDebug("Content.opf created with {ManifestCount} manifest items and {SpineCount} spine items",
                manifestItems.Count, spineItems.Count);
        }

        private void CreateToc()
        {
            _logger.LogDebug("Creating table of contents with {ChapterCount} chapters", _book.Chapters.Count);
            var chapters = _book.Chapters;

            var tableOfContent = chapters.Select(i =>
                new XElement(xhtml + "li",
                    new XElement(xhtml + "a",
                        new XAttribute("href", $"{FormatChapterId(i.Key)}.xhtml"),
                        i.Value.Title
                        )
                    )
            );

            var doc = new XDocument(
                new XElement(xhtml + "html",
                    new XAttribute("xmlns", xhtml),
                    new XAttribute(XNamespace.Xmlns + "epub", epub),
                    new XElement(xhtml + "head",
                        new XElement(xhtml + "title", "Table of Contents"),
                        new XElement(xhtml + "meta", new XAttribute("charset", "utf-8")),
                        new XElement(xhtml + "link",
                            new XAttribute("href", "style.css"),
                            new XAttribute("rel", "stylesheet"),
                            new XAttribute("type", "text/css")
                        )
                    ),
                    new XElement(xhtml + "body",
                        new XElement(xhtml + "div",
                            new XAttribute("class", "toc-container"),
                            new XElement(xhtml + "h1",
                                new XAttribute("style", "text-align: center; margin-bottom: 1.5em;"),
                                "Table of Contents"
                            ),
                            new XElement(xhtml + "nav",
                                new XAttribute(epub + "type", "toc"),
                                new XAttribute("id", "toc"),
                                new XElement(xhtml + "ol",
                                    new XAttribute("style", "list-style-type: none; padding-left: 0;"),
                                    tableOfContent
                                )
                            )
                        )
                    )
                )
            );

            _xmlDocuments["OEBPS/toc.xhtml"] = doc;
            _logger.LogTrace("Table of contents created: toc.xhtml");
        }

        private void CreateChapters()
        {
            _logger.LogDebug("Creating {ChapterCount} chapter XHTML files", _book.Chapters.Count);
            foreach (var chapter in _book.Chapters)
            {
                _logger.LogTrace("Creating chapter file: {ChapterId} - {Title}",
                    FormatChapterId(chapter.Key), chapter.Value.Title);

                var doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement(xhtml + "html",
                        new XAttribute("xmlns", xhtml),
                        new XAttribute("lang", _book.Metadata.Language),
                        new XAttribute(XNamespace.Xmlns + "epub", epub),
                        new XElement(xhtml + "head",
                            new XElement(xhtml + "title", chapter.Value.Title),
                            new XElement(xhtml + "link",
                                new XAttribute("href", "style.css"),
                                new XAttribute("rel", "stylesheet"),
                                new XAttribute("type", "text/css")
                            ),
                            new XElement(xhtml + "meta", new XAttribute("charset", "utf-8"))
                        ),
                        new XElement(xhtml + "body",
                            new XElement(xhtml + "section",
                                new XAttribute(epub + "type", "chapter"),
                                new XAttribute("role", "doc-chapter"),
                                new XElement(xhtml + "h1",
                                    new XAttribute("class", "chapter-title"),
                                    chapter.Value.Title
                                ),
                                chapter.Value.Paragraphs.Select(p =>
                                    new XElement(xhtml + "p", p.Text)
                                )
                            )
                        )
                    )
                );

                var chapterFileName = $"OEBPS/{FormatChapterId(chapter.Key)}.xhtml";
                _xmlDocuments[chapterFileName] = doc;
                _logger.LogTrace("Chapter file created: {FileName} with {ParagraphCount} paragraphs",
                    chapterFileName, chapter.Value.Paragraphs.Count());
            }
        }

        private string FormatChapterId(int id)
        {
            return $"chapter_{id}";
        }

        #endregion Private implementation methods
    }
}