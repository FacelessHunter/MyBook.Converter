using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MyBook.Writer.Builders;
using MyBook.Writer.Core.Interfaces;
using MyBook.Writer.Tests.TestHelpers;
using System.Text;
using Xunit;

namespace MyBook.Writer.Tests
{
    public class InputValidationTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void BookFactory_WithEmptyTitle_StillCreatesBook(string title)
        {
            // Arrange
            var format = BookFormat.Epub;
            var language = "en";
            BookFactory.SetLoggerFactory(NullLoggerFactory.Instance);

            // Act - Should not throw an exception
            var builder = BookFactory.CreateBuilder(format, title!, language);

            // Assert
            Assert.NotNull(builder);
            Assert.IsType<EpubBuilder>(builder);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void BookFactory_WithEmptyLanguage_StillCreatesBook(string language)
        {
            // Arrange
            var format = BookFormat.Epub;
            var title = "Test Book";
            BookFactory.SetLoggerFactory(NullLoggerFactory.Instance);

            // Act - Should not throw an exception
            var builder = BookFactory.CreateBuilder(format, title, language!);

            // Assert
            Assert.NotNull(builder);
            Assert.IsType<EpubBuilder>(builder);
        }

        [Fact]
        public async Task WithCover_NullStream_ThrowsArgumentNullException()
        {
            // Arrange
            var logger = new TestLogger<EpubBuilder>();
            var loggerFactory = new TestLoggerFactory();
            var builder = EpubBuilder.Create("Test Book", "en", logger, loggerFactory);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                builder.WithCover(null!, "cover.jpg", "image/jpeg"));
        }

        [Fact]
        public async Task WithCover_EmptyStream_CreatesEmptyCover()
        {
            // Arrange
            var logger = new TestLogger<EpubBuilder>();
            var loggerFactory = new TestLoggerFactory();
            var builder = EpubBuilder.Create("Test Book", "en", logger, loggerFactory);
            
            using var emptyStream = new MemoryStream();

            // Act - should handle empty stream without exception
            var result = builder.WithCover(emptyStream, "cover.jpg", "image/jpeg");

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public async Task AddChapter_EmptyParagraphs_StillAddsChapter()
        {
            // Arrange
            var logger = new TestLogger<EpubBuilder>();
            var loggerFactory = new TestLoggerFactory();
            var builder = EpubBuilder.Create("Test Book", "en", logger, loggerFactory);
            
            // Act
            var result = builder.AddChapter(1, "Empty Chapter", Array.Empty<string>());

            // Assert
            Assert.Same(builder, result);
            Assert.Contains(logger.LogMessages, msg => 
                msg.Contains("Adding chapter") && msg.Contains("Empty Chapter"));
        }

        [Fact]
        public async Task SaveToFileAsync_InvalidPath_ThrowsException()
        {
            // Arrange
            var logger = new TestLogger<EpubBuilder>();
            var loggerFactory = new TestLoggerFactory();
            var builder = EpubBuilder.Create("Test Book", "en", logger, loggerFactory)
                .AddChapter(1, "Chapter 1", new[] { "Test paragraph" });
            
            var writer = await builder.BuildAsync();
            
            // Use an invalid path with characters not allowed in file paths
            var invalidPath = "invalid:path*with?illegal|chars";

            // Act & Assert
            await Assert.ThrowsAsync<IOException>(() => 
                writer.SaveToFileAsync(invalidPath));
        }

        [Fact]
        public async Task SaveToFileAsync_WithInvalidPermissions_HandlesException()
        {
            // This test is OS-specific and might not always work reliably
            // Try to create a file path that would require admin/root permissions
            var logger = new TestLogger<EpubBuilder>();
            var loggerFactory = new TestLoggerFactory();
            var builder = EpubBuilder.Create("Test Book", "en", logger, loggerFactory)
                .AddChapter(1, "Chapter 1", new[] { "Test paragraph" });
            
            var writer = await builder.BuildAsync();
            
            // Use a path that's likely to be protected
            string restrictedPath;
            if (OperatingSystem.IsWindows())
            {
                restrictedPath = @"C:\Windows\System32\restricted_test.epub";
            }
            else
            {
                restrictedPath = "/root/restricted_test.epub";
            }

            try
            {
                // Act
                await writer.SaveToFileAsync(restrictedPath);
                
                // If we got here without exception, the test is inconclusive,
                // but we shouldn't fail it because permissions vary by environment
            }
            catch (UnauthorizedAccessException)
            {
                // This is the expected exception on most systems
                Assert.Contains(logger.LogMessages, msg => msg.Contains("Error saving"));
            }
            catch (IOException)
            {
                // This might also happen depending on the OS
                Assert.Contains(logger.LogMessages, msg => msg.Contains("Error saving"));
            }
        }

        [Fact]
        public async Task BuildAsync_WithNoChapters_CreatesValidWriter()
        {
            // Arrange
            var logger = new TestLogger<EpubBuilder>();
            var loggerFactory = new TestLoggerFactory();
            var builder = EpubBuilder.Create("Test Book", "en", logger, loggerFactory);
            
            // Don't add any chapters
            
            // Act
            var writer = await builder.BuildAsync();
            
            // Assert
            Assert.NotNull(writer);
            
            // Should be able to save even without chapters
            using var stream = await writer.SaveAsStreamAsync();
            Assert.True(stream.Length > 0);
        }

        [Fact]
        public async Task BuildAsync_WithExtremelyLongTitle_HandlesGracefully()
        {
            // Arrange
            var logger = new TestLogger<EpubBuilder>();
            var loggerFactory = new TestLoggerFactory();
            
            // Create a very long title (10KB of text)
            var longTitle = new string('A', 10240);
            
            var builder = EpubBuilder.Create(longTitle, "en", logger, loggerFactory)
                .AddChapter(1, "Chapter 1", new[] { "Test paragraph" });
            
            // Act - Should not throw
            var writer = await builder.BuildAsync();
            
            // Assert
            Assert.NotNull(writer);
            
            // Should be able to save with long title
            using var stream = await writer.SaveAsStreamAsync();
            Assert.True(stream.Length > 0);
        }
    }
} 