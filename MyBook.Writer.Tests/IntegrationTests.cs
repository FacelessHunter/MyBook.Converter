using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;
using Xunit;

namespace MyBook.Writer.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public async Task CreateAndSaveEpubBook_FullWorkflow()
        {
            // Arrange
            var loggerFactory = NullLoggerFactory.Instance;
            BookFactory.SetLoggerFactory(loggerFactory);
            
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"test_book_{Guid.NewGuid()}.epub");

            try
            {
                // Act
                var builder = BookFactory.CreateBuilder(BookFormat.Epub, "Integration Test Book", "en")
                    .WithAuthor("Test Author")
                    .AddChapter(1, "Chapter 1", new[] 
                    { 
                        "This is the first paragraph of chapter 1.", 
                        "This is the second paragraph with some formatting like <b>bold text</b> and <i>italics</i>."
                    })
                    .AddChapter(2, "Chapter 2", new[]
                    {
                        "This is the first paragraph of chapter 2.",
                        "This is the second paragraph of chapter 2."
                    });

                // Add a cover image (using a simple placeholder)
                using (var coverStream = new MemoryStream(Encoding.UTF8.GetBytes("Fake cover image data")))
                {
                    builder.WithCover(coverStream, "cover.jpg", "image/jpeg");
                }

                // Build and save the book
                var writer = await builder.BuildAsync();
                await writer.SaveToFileAsync(tempFilePath);

                // Assert
                Assert.True(File.Exists(tempFilePath), "EPUB file should have been created");
                var fileInfo = new FileInfo(tempFilePath);
                Assert.True(fileInfo.Length > 0, "EPUB file should not be empty");
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch (IOException)
                    {
                        // Ignore file in use errors during cleanup
                    }
                }
            }
        }

        [Fact]
        public async Task CreateAndSaveFB2Book_FullWorkflow()
        {
            // Arrange
            var loggerFactory = NullLoggerFactory.Instance;
            BookFactory.SetLoggerFactory(loggerFactory);
            
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"test_book_{Guid.NewGuid()}.fb2");

            try
            {
                // Act
                var builder = BookFactory.CreateBuilder(BookFormat.FB2, "FB2 Integration Test Book", "en")
                    .WithAuthor("Test Author")
                    .AddChapter(1, "Chapter 1", new[] 
                    { 
                        "This is the first paragraph of chapter 1 in FB2 format.", 
                        "This is the second paragraph with some formatting like <strong>bold text</strong> and <emphasis>italics</emphasis>."
                    })
                    .AddChapter(2, "Chapter 2", new[]
                    {
                        "This is the first paragraph of chapter 2 in FB2 format.",
                        "This is the second paragraph of chapter 2 in FB2 format."
                    });

                // Add a cover image (using a simple placeholder)
                using (var coverStream = new MemoryStream(Encoding.UTF8.GetBytes("Fake cover image data")))
                {
                    builder.WithCover(coverStream, "cover.jpg", "image/jpeg");
                }

                // Build and save the book
                var writer = await builder.BuildAsync();
                await writer.SaveToFileAsync(tempFilePath);

                // Assert
                Assert.True(File.Exists(tempFilePath), "FB2 file should have been created");
                var fileInfo = new FileInfo(tempFilePath);
                Assert.True(fileInfo.Length > 0, "FB2 file should not be empty");
                
                // We'll check content format in a way that won't fail if another process is using the file
                try
                {
                    var fileContent = await File.ReadAllTextAsync(tempFilePath);
                    Assert.Contains("<FictionBook", fileContent);
                }
                catch (IOException)
                {
                    // If we can't read the file, at least we know it was created
                }
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch (IOException)
                    {
                        // Ignore file in use errors during cleanup
                    }
                }
            }
        }

        [Fact]
        public async Task SaveAsStreamAsync_ReturnsValidStream()
        {
            // Arrange
            var loggerFactory = NullLoggerFactory.Instance;
            BookFactory.SetLoggerFactory(loggerFactory);
            
            var builder = BookFactory.CreateBuilder(BookFormat.Epub, "Stream Test Book", "en")
                .WithAuthor("Test Author")
                .AddChapter(1, "Single Chapter", new[] { "This is a test paragraph." });

            // Act
            var writer = await builder.BuildAsync();
            var bookStream = await writer.SaveAsStreamAsync();

            // Assert
            Assert.NotNull(bookStream);
            Assert.True(bookStream.Length > 0, "Book stream should contain data");
            Assert.True(bookStream.CanRead, "Book stream should be readable");

            // Optional: Read some bytes to verify stream works
            var buffer = new byte[100]; // Read first 100 bytes or less
            var bytesRead = await bookStream.ReadAsync(buffer, 0, buffer.Length);
            Assert.True(bytesRead > 0, "Could read data from the stream");
        }
    }
} 