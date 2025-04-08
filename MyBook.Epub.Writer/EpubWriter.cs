using MyBook.Epub.Writer.Models;
using MyBook.Epub.Writer.Models.ChapterModels;
using SharedModels;
using System;
using System.IO.Compression;
using System.Xml.Linq;

namespace MyBook.Epub.Writer
{
    public class EpubBuilder
    {
        private readonly EpubBook _epubBook;
        private readonly Dictionary<string, MemoryStream> _contentStreams = new();
        private readonly Dictionary<string, XDocument> _xmlDocuments = new();

        private static readonly XNamespace xhtml = "http://www.w3.org/1999/xhtml";
        private static readonly XNamespace epub = "http://www.idpf.org/2007/ops";
        private static readonly XNamespace opf = "http://www.idpf.org/2007/opf";
        private static readonly XNamespace dc = "http://purl.org/dc/elements/1.1/";
        private static readonly XNamespace container = "urn:oasis:names:tc:opendocument:xmlns:container";

        public EpubBuilder(EpubBook epubBook)
        {
            _epubBook = epubBook;
        }

        public static EpubBuilder Create(string title, string language)
        {
            var epubBook = new EpubBook
            {
                Metadata = new EpubBookMetaData
                {
                    Title = title,
                    Language = language,
                    Identifier = Guid.NewGuid().ToString()
                },
                Content = new EpubBookContent()
            };

            return new EpubBuilder(epubBook);
        }

        public EpubBuilder WithCover(Stream coverImageStream, string fileName, string contentType)
        {
            _epubBook.CoverPage = new FileModel
            {
                Stream = coverImageStream as MemoryStream ?? new MemoryStream(),
                FileName = fileName,
                ContentType = contentType
            };

            return this;
        }

        public EpubBuilder AddChapter(int chapterId, string title, List<BaseRowModel> content)
        {
            var chapter = new EpubChapter
            {
                Title = title
            };

            foreach (var baseRow in content)
            {
                chapter.Content.Add(baseRow.Id, baseRow);
            }

            _epubBook.Content.Chapters.Add(chapterId, chapter);

            return this;
        }

        public EpubBuilder AddChapterWithParagraphs(int chapterId, string title, List<string> paragraphs)
        {
            var chapterBuilder = new EpubChapterBuilder();
            chapterBuilder.SetTitle(title);

            foreach (var text in paragraphs)
            {
                chapterBuilder.AddTextParagraph(text);
            }

            var chapter = chapterBuilder.Build();
            _epubBook.Content.Chapters.Add(chapterId, chapter);

            return this;
        }

        public async Task<EpubBuilder> BuildAsync()
        {
            await CreateMimetypeAsync();
            await CreateStyleCssAsync();
            await CreateCoverPageAsync();
            CreateContainerXml();
            CreateContentOpf();
            CreateToc();
            CreateChapters();

            return this;
        }

        public async Task<MemoryStream> SaveAsStreamAsync()
        {
            var outputStream = new MemoryStream();

            using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
            {
                // Add mimetype first and uncompressed
                var mimeEntry = archive.CreateEntry("mimetype", CompressionLevel.NoCompression);
                using (var entryStream = mimeEntry.Open())
                {
                    var mimeStream = _contentStreams["mimetype"];
                    mimeStream.Position = 0;
                    await mimeStream.CopyToAsync(entryStream);
                }

                // Add all other content
                foreach (var content in _contentStreams.Where(c => c.Key != "mimetype"))
                {
                    var entry = archive.CreateEntry(content.Key, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    content.Value.Position = 0;
                    await content.Value.CopyToAsync(entryStream);
                }

                // Add all XML documents
                foreach (var doc in _xmlDocuments)
                {
                    var entry = archive.CreateEntry(doc.Key, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    await doc.Value.SaveAsync(entryStream, SaveOptions.None, CancellationToken.None);
                }
            }

            outputStream.Position = 0;
            return outputStream;
        }

        public async Task SaveToFileAsync(string outputPath)
        {
            using var stream = await SaveAsStreamAsync();
            using var fileStream = new FileStream(outputPath, FileMode.Create);
            await stream.CopyToAsync(fileStream);
        }

        private async Task CreateMimetypeAsync()
        {
            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, leaveOpen: true);
            await writer.WriteAsync("application/epub+zip");
            await writer.FlushAsync();
            stream.Position = 0;
            _contentStreams["mimetype"] = stream;
        }

        private async Task CreateStyleCssAsync()
        {
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
                """;

            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, leaveOpen: true);
            await writer.WriteAsync(css);
            await writer.FlushAsync();
            stream.Position = 0;
            _contentStreams["OEBPS/style.css"] = stream;
        }

        private async Task CreateCoverPageAsync()
        {
            if (_epubBook.CoverPage is null)
                return;

            var coverStream = new MemoryStream();
            _epubBook.CoverPage.Stream.Position = 0;
            await _epubBook.CoverPage.Stream.CopyToAsync(coverStream);
            coverStream.Position = 0;
            _contentStreams[$"OEBPS/{_epubBook.CoverPage.FileName}"] = coverStream;

            var coverXhtml = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(xhtml + "html",
                    new XAttribute("xmlns", xhtml),
                    new XAttribute("lang", _epubBook.Metadata.Language),
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
                                new XAttribute("src", _epubBook.CoverPage.FileName),
                                new XAttribute("alt", "Cover Image")
                            )
                        )
                    )
                )
            );

            _xmlDocuments["OEBPS/cover.xhtml"] = coverXhtml;
        }

        private void CreateContainerXml()
        {
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
            var chapters = _epubBook.Content.Chapters;

            var metadataElement =
                new XElement(opf + "metadata",
                    new XAttribute(XNamespace.Xmlns + "dc", dc),
                    new XElement(dc + "identifier",
                        new XAttribute("id", "book-id"),
                        "urn:uuid:" + _epubBook.Metadata.Identifier
                    ),
                    new XElement(dc + "title", _epubBook.Metadata.Title),
                    new XElement(dc + "language", _epubBook.Metadata.Language),
                    new XElement(opf + "meta",
                        new XAttribute("property", "dcterms:modified"),
                        DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    ),
                    new XElement(opf + "meta",
                        new XAttribute("property", "rendition:layout"),
                        "reflowable"
                    )
                );

            var chapterManifestReferences = chapters.Select(i =>
                new XElement(opf + "item",
                        new XAttribute("id", $"{FormatChapterId(i.Key)}"),
                        new XAttribute("href", $"{FormatChapterId(i.Key)}.xhtml"),
                        new XAttribute("media-type", "application/xhtml+xml")
                ));

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

            if (_epubBook.CoverPage != null)
            {
                manifestItems.Add(
                    new XElement(opf + "item",
                        new XAttribute("id", "cover-image"),
                        new XAttribute("href", _epubBook.CoverPage.FileName),
                        new XAttribute("media-type", _epubBook.CoverPage.ContentType),
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
            
            if (_epubBook.CoverPage != null)
            {
                spineItems.Add(
                    new XElement(opf + "itemref",
                        new XAttribute("idref", "cover-page")
                    )
                );
            }
            
            // Add chapter spine items
            spineItems.AddRange(spineSequenceElements);
            
            var spineElement = new XElement(opf + "spine", spineItems);

            var doc = new XDocument(
                new XElement(opf + "package",
                    new XAttribute("version", "3.3"),
                    new XAttribute("unique-identifier", "book-id"),
                    metadataElement,
                    manifestElement,
                    spineElement
                )
            );
            
            _xmlDocuments["OEBPS/content.opf"] = doc;
        }

        private void CreateToc()
        {
            var chapters = _epubBook.Content.Chapters;

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
                        new XElement(xhtml + "title", "Table of Contents")
                    ),
                    new XElement(xhtml + "body",
                        new XElement(xhtml + "nav",
                            new XAttribute(epub + "type", "toc"),
                            new XAttribute("id", "toc"),
                            new XElement(xhtml + "h1", "Contents"),
                            new XElement(xhtml + "ol",
                                tableOfContent
                            )
                        )
                    )
                )
            );
            
            _xmlDocuments["OEBPS/toc.xhtml"] = doc;
        }

        private void CreateChapters()
        {
            foreach (var chapter in _epubBook.Content.Chapters)
            {
                var doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement(xhtml + "html",
                        new XAttribute("xmlns", xhtml),
                        new XAttribute("lang", _epubBook.Metadata.Language),
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
                                chapter.Value.Content.Select(i => GenerateElementBasedOnRowType(i.Value))
                            )
                        )
                    )
                );

                _xmlDocuments[$"OEBPS/{FormatChapterId(chapter.Key)}.xhtml"] = doc;
            }
        }

        private string FormatChapterId(int id)
        {
            return $"chapter_{id}";
        }

        private XElement GenerateElementBasedOnRowType(BaseRowModel rowModel)
        {
            return rowModel switch
            {
                Paragraph paragraph => new XElement(xhtml + "p", paragraph.Text),
                _ => throw new NotImplementedException()
            };
        }
    }
}