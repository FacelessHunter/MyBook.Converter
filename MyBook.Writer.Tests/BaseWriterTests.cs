using Microsoft.Extensions.Logging;
using MyBook.Writer.Core;
using MyBook.Writer.Core.Interfaces;
using MyBook.Writer.Tests.TestHelpers;
using System.Text;
using Xunit;

namespace MyBook.Writer.Tests
{
    public class BaseWriterTests
    {
        // Concrete implementation of BaseWriter for testing
        private class TestBookWriter : BaseWriter
        {
            private readonly MemoryStream _contentStream;

            public TestBookWriter(BookFormat format, ILogger logger, string content = "Test book content")
                : base(format, logger)
            {
                _contentStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            }

            public override Task<Stream> SaveAsStreamAsync()
            {
                // Reset position in case the stream was read
                _contentStream.Position = 0;
                return Task.FromResult<Stream>(_contentStream);
            }

            // Expose protected method for testing
            public string TestGetFileExtension() => GetFileExtension();
        }

        [Theory]
        [InlineData(BookFormat.Epub, ".epub")]
        [InlineData(BookFormat.FB2, ".fb2")]
        public void GetFileExtension_ReturnsCorrectExtension(BookFormat format, string expectedExtension)
        {
            // Arrange
            var logger = new TestLogger("TestBookWriter");
            var writer = new TestBookWriter(format, logger);

            // Act
            var extension = writer.TestGetFileExtension();

            // Assert
            Assert.Equal(expectedExtension, extension);
        }

        [Fact]
        public void GetFileExtension_WithInvalidFormat_ThrowsNotSupportedException()
        {
            // Arrange
            var logger = new TestLogger("TestBookWriter");
            var writer = new TestBookWriter((BookFormat)999, logger);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => writer.TestGetFileExtension());
        }

        [Fact]
        public void SetBookTitle_UpdatesTitle()
        {
            // Arrange
            var logger = new TestLogger("TestBookWriter");
            var writer = new TestBookWriter(BookFormat.Epub, logger);
            var testTitle = "New Book Title";

            // Act
            writer.SetBookTitle(testTitle);

            // Assert - verify the title was set via logs 
            // (We can't directly access the protected property)
            Assert.Contains(logger.LogMessages, msg => msg.Contains(testTitle));
        }

        [Fact]
        public async Task SaveToFileAsync_CreatesDirectory_IfNotExists()
        {
            // Arrange
            var logger = new TestLogger("TestBookWriter");
            var writer = new TestBookWriter(BookFormat.Epub, logger);
            writer.SetBookTitle("Test Book");

            var tempDir = Path.Combine(Path.GetTempPath(), $"MyBookWriterTests_{Guid.NewGuid()}");
            var filePath = Path.Combine(tempDir, "test_book.epub");

            try
            {
                // Act
                await writer.SaveToFileAsync(filePath);

                // Assert
                Assert.True(Directory.Exists(tempDir), "Directory should have been created");
                Assert.True(File.Exists(filePath), "File should have been created");
                Assert.Contains(logger.LogMessages, msg => msg.Contains("Creating directory"));
            }
            finally
            {
                // Cleanup
                if (File.Exists(filePath))
                {
                    try { File.Delete(filePath); } catch { }
                }
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        [Fact]
        public async Task SaveToFileAsync_AppendsExtension_IfMissing()
        {
            // Arrange
            var logger = new TestLogger("TestBookWriter");
            var writer = new TestBookWriter(BookFormat.Epub, logger);
            writer.SetBookTitle("Test Book");

            var tempPath = Path.Combine(Path.GetTempPath(), $"test_book_{Guid.NewGuid()}");
            var expectedPath = tempPath + ".epub";

            try
            {
                // Act
                await writer.SaveToFileAsync(tempPath);

                // Assert
                Assert.False(File.Exists(tempPath), "File without extension should not exist");
                Assert.True(File.Exists(expectedPath), "File with extension should exist");
            }
            finally
            {
                // Cleanup
                if (File.Exists(expectedPath))
                {
                    try { File.Delete(expectedPath); } catch { }
                }
            }
        }

        [Fact]
        public async Task SaveToFileAsync_WithDirectoryPath_CreatesFolderAndFile()
        {
            // Arrange
            var logger = new TestLogger("TestBookWriter");
            var writer = new TestBookWriter(BookFormat.Epub, logger);
            var title = "Special Book";
            writer.SetBookTitle(title);

            var tempDir = Path.Combine(Path.GetTempPath(), $"MyBookWriterTests_{Guid.NewGuid()}");
            try
            {
                Directory.CreateDirectory(tempDir);

                // Act
                await writer.SaveToFileAsync(tempDir);

                // Assert
                // Format should be: tempDir/Special Book/yyyyMMdd_HHmmss_Special Book.epub
                var bookDir = Path.Combine(tempDir, "Special_Book");
                Assert.True(Directory.Exists(bookDir), "Book directory should have been created");

                // Check if any file was created
                var files = Directory.GetFiles(bookDir);
                Assert.NotEmpty(files);
                Assert.Contains(files, f => Path.GetExtension(f).Equals(".epub", StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        [Fact]
        public async Task SaveToFileAsync_WithInvalidFilename_SanitizesName()
        {
            // Arrange
            var logger = new TestLogger("TestBookWriter");
            var writer = new TestBookWriter(BookFormat.Epub, logger);
            var invalidTitle = "Invalid:File*Name?";
            writer.SetBookTitle(invalidTitle);

            var tempDir = Path.Combine(Path.GetTempPath(), $"MyBookWriterTests_{Guid.NewGuid()}");
            try
            {
                Directory.CreateDirectory(tempDir);

                // Act
                await writer.SaveToFileAsync(tempDir);

                // Assert
                // Should sanitize to "Invalid_File_Name_"
                var sanitizedDir = Path.Combine(tempDir, "Invalid_File_Name_");
                Assert.True(Directory.Exists(sanitizedDir), "Sanitized directory should exist");
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        [Fact]
        public async Task SaveToFileAsync_WithWrongExtension_CorrectExtension()
        {
            // Arrange
            var logger = new TestLogger("TestBookWriter");
            var writer = new TestBookWriter(BookFormat.Epub, logger);
            writer.SetBookTitle("Test Book");

            var tempPath = Path.Combine(Path.GetTempPath(), $"test_book_{Guid.NewGuid()}.pdf");
            var expectedPath = Path.ChangeExtension(tempPath, ".epub");

            try
            {
                // Act
                await writer.SaveToFileAsync(tempPath);

                // Assert
                Assert.False(File.Exists(tempPath), "File with wrong extension should not exist");
                Assert.True(File.Exists(expectedPath), "File with correct extension should exist");
            }
            finally
            {
                // Cleanup
                if (File.Exists(expectedPath))
                {
                    try { File.Delete(expectedPath); } catch { }
                }
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }
    }
} 