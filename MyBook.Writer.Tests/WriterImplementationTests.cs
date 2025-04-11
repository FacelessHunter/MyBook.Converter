using Microsoft.Extensions.Logging;
using MyBook.Writer.Core;
using MyBook.Writer.Core.Interfaces;
using MyBook.Writer.Core.Models;
using MyBook.Writer.Tests.TestHelpers;
using MyBook.Writer.Writers.Epub;
using MyBook.Writer.Writers.FB2;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace MyBook.Writer.Tests
{
    public class WriterImplementationTests
    {
        [Fact]
        public async Task EpubWriter_SaveAsStreamAsync_CreatesValidEpubStream()
        {
            // Arrange
            var logger = new TestLogger<EpubWriter>();
            var contentStreams = new Dictionary<string, MemoryStream> 
            {
                { "mimetype", new MemoryStream(Encoding.UTF8.GetBytes("application/epub+zip")) },
                { "OEBPS/chapter1.xhtml", new MemoryStream(Encoding.UTF8.GetBytes("<html><body><p>Test content</p></body></html>")) }
            };
            var xmlDocuments = new Dictionary<string, XDocument>
            {
                { "META-INF/container.xml", new XDocument(new XElement("container")) },
                { "OEBPS/content.opf", new XDocument(new XElement("package")) }
            };
            
            var writer = new EpubWriter(contentStreams, xmlDocuments, logger, "Test Book");

            // Act
            var stream = await writer.SaveAsStreamAsync();

            // Assert
            Assert.NotNull(stream);
            Assert.True(stream.Length > 0, "Stream should contain data");
            Assert.True(stream.Position == 0, "Stream position should be at the beginning");

            // Validate the EPUB structure by trying to open it as a ZIP archive
            using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, true);
            Assert.NotEmpty(zipArchive.Entries);
            
            // The EPUB should contain mimetype, container.xml and content.opf at minimum
            Assert.Contains(zipArchive.Entries, e => e.FullName == "mimetype");
            Assert.Contains(zipArchive.Entries, e => e.FullName == "META-INF/container.xml");
            Assert.Contains(zipArchive.Entries, e => e.FullName == "OEBPS/content.opf");
            
            // Check for mimetype content
            using var mimetypeStream = zipArchive.GetEntry("mimetype")?.Open();
            if (mimetypeStream != null)
            {
                using var reader = new StreamReader(mimetypeStream);
                var content = await reader.ReadToEndAsync();
                Assert.Equal("application/epub+zip", content);
            }
        }

        [Fact]
        public async Task FB2Writer_SaveAsStreamAsync_CreatesValidFB2Stream()
        {
            // Arrange
            var logger = new TestLogger<FB2Writer>();
            var xmlDocument = new XDocument(
                new XElement("FictionBook",
                    new XElement("description",
                        new XElement("title-info",
                            new XElement("book-title", "Test Book")
                        )
                    ),
                    new XElement("body",
                        new XElement("section",
                            new XElement("p", "Test paragraph")
                        )
                    )
                )
            );
            
            var writer = new FB2Writer(xmlDocument, logger, "Test Book");

            // Act
            var stream = await writer.SaveAsStreamAsync();

            // Assert
            Assert.NotNull(stream);
            Assert.True(stream.Length > 0, "Stream should contain data");
            Assert.True(stream.Position == 0, "Stream position should be at the beginning");

            // Validate XML structure 
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            Assert.Contains("<FictionBook", content);
            Assert.Contains("<book-title>Test Book</book-title>", content);
            Assert.Contains("<p>Test paragraph</p>", content);
        }

        [Fact]
        public async Task EpubWriter_SaveToFileAsync_CreatesValidEpubFile()
        {
            // Arrange
            var logger = new TestLogger<EpubWriter>();
            var contentStreams = new Dictionary<string, MemoryStream> 
            {
                { "mimetype", new MemoryStream(Encoding.UTF8.GetBytes("application/epub+zip")) },
                { "OEBPS/style.css", new MemoryStream(Encoding.UTF8.GetBytes("body { font-family: serif; }")) }
            };
            var xmlDocuments = new Dictionary<string, XDocument>
            {
                { "META-INF/container.xml", new XDocument(new XElement("container")) },
                { "OEBPS/content.opf", new XDocument(new XElement("package")) }
            };
            
            var writer = new EpubWriter(contentStreams, xmlDocuments, logger, "Test Book");
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"test_epub_{Guid.NewGuid()}.epub");

            try
            {
                // Act
                await writer.SaveToFileAsync(tempFilePath);

                // Assert
                Assert.True(File.Exists(tempFilePath), "EPUB file should have been created");
                var fileInfo = new FileInfo(tempFilePath);
                Assert.True(fileInfo.Length > 0, "File should not be empty");

                // Validate by opening as ZIP
                using var zipArchive = ZipFile.OpenRead(tempFilePath);
                Assert.NotEmpty(zipArchive.Entries);
                Assert.Contains(zipArchive.Entries, e => e.FullName == "mimetype");
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); } catch { }
                }
            }
        }

        [Fact]
        public async Task FB2Writer_SaveToFileAsync_CreatesValidFB2File()
        {
            // Arrange
            var logger = new TestLogger<FB2Writer>();
            var xmlDocument = new XDocument(
                new XElement("FictionBook",
                    new XElement("description",
                        new XElement("title-info",
                            new XElement("book-title", "Test Book")
                        )
                    ),
                    new XElement("body",
                        new XElement("section",
                            new XElement("p", "Test paragraph")
                        )
                    )
                )
            );
            
            var writer = new FB2Writer(xmlDocument, logger, "Test Book");
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"test_fb2_{Guid.NewGuid()}.fb2");

            try
            {
                // Act
                await writer.SaveToFileAsync(tempFilePath);

                // Assert
                Assert.True(File.Exists(tempFilePath), "FB2 file should have been created");
                var fileInfo = new FileInfo(tempFilePath);
                Assert.True(fileInfo.Length > 0, "File should not be empty");

                // Validate content
                var content = await File.ReadAllTextAsync(tempFilePath);
                Assert.Contains("<FictionBook", content);
                Assert.Contains("<book-title>Test Book</book-title>", content);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); } catch { }
                }
            }
        }

        [Fact]
        public async Task FB2Writer_LogsErrorOnSaveFailure()
        {
            // Arrange - Create a test logger to capture error messages
            var logger = new TestLogger<FB2Writer>();
            
            // Create a document with null content that would fail to save
            XDocument nullDocument = null!;
            
            // Create the writer with null document - this will compile but fail at runtime
            var writer = new FB2Writer(nullDocument, logger, "Test Book");
            
            // Act - Expect exception when trying to save
            await Assert.ThrowsAnyAsync<Exception>(() => writer.SaveAsStreamAsync());
            
            // Assert - Check that error was logged
            Assert.Contains(logger.LogMessages, msg => msg.Contains("Error saving FB2"));
        }

        [Fact]
        public async Task EpubWriter_LogsErrorOnSaveFailure()
        {
            // Arrange - Create a test logger to capture error messages
            var logger = new TestLogger<EpubWriter>();
            
            // Create a dictionary with null stream that would fail when accessed
            var contentStreams = new Dictionary<string, MemoryStream>
            {
                { "mimetype", null! }
            };
            
            var xmlDocuments = new Dictionary<string, XDocument>
            {
                { "META-INF/container.xml", new XDocument(new XElement("container")) }
            };
            
            var writer = new EpubWriter(contentStreams, xmlDocuments, logger, "Test Book");
            
            // Act - Expect exception when trying to save
            await Assert.ThrowsAnyAsync<Exception>(() => writer.SaveAsStreamAsync());
            
            // Assert - Check that error was logged
            Assert.Contains(logger.LogMessages, msg => msg.Contains("Error saving EPUB"));
        }
    }
}