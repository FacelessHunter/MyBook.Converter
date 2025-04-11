using Microsoft.Extensions.Logging;
using MyBook.Writer.Builders;
using MyBook.Writer.Core.Interfaces;
using MyBook.Writer.Tests.TestHelpers;
using System.Text;
using Xunit;

namespace MyBook.Writer.Tests
{
    public class BookBuilderTests
    {
        private readonly ILogger<EpubBuilder> _epubLogger;
        private readonly ILogger<FB2Builder> _fb2Logger;
        private readonly ILoggerFactory _loggerFactory;

        public BookBuilderTests()
        {
            // Create substitution loggers and factory without using Moq
            _epubLogger = new TestLogger<EpubBuilder>();
            _fb2Logger = new TestLogger<FB2Builder>();
            
            _loggerFactory = new TestLoggerFactory();
        }

        [Fact]
        public void EpubBuilder_Create_ReturnsValidInstance()
        {
            // Act
            var builder = EpubBuilder.Create("Test Book", "en", _epubLogger, _loggerFactory);

            // Assert
            Assert.NotNull(builder);
            Assert.IsType<EpubBuilder>(builder);
        }

        [Fact]
        public void EpubBuilder_FluentAPI_AllowsMethodChaining()
        {
            // Arrange
            var builder = EpubBuilder.Create("Test Book", "en", _epubLogger, _loggerFactory);
            var author = "Test Author";
            var paragraphs = new[] { "Paragraph 1", "Paragraph 2" };

            // Act - Test method chaining
            var result = builder
                .WithAuthor(author)
                .WithTitle("Updated Title")
                .WithLanguage("fr")
                .AddChapter(1, "Chapter 1", paragraphs);

            // Assert
            Assert.Same(builder, result); // Ensure the same instance is returned for chaining
        }

        [Fact]
        public void FB2Builder_Create_ReturnsValidInstance()
        {
            // Act
            var builder = FB2Builder.Create("Test Book", "en", _fb2Logger, _loggerFactory);

            // Assert
            Assert.NotNull(builder);
            Assert.IsType<FB2Builder>(builder);
        }

        [Fact]
        public void WithCover_UsingStream_AddsImageToBook()
        {
            // Arrange
            var builder = EpubBuilder.Create("Test Book", "en", _epubLogger, _loggerFactory);
            
            using var imageStream = new MemoryStream(Encoding.UTF8.GetBytes("Fake image data"));
            var fileName = "cover.jpg";
            var contentType = "image/jpeg";

            // Act
            var result = builder.WithCover(imageStream, fileName, contentType);

            // Assert
            Assert.Same(builder, result);
            // Note: We're only testing that the method returns the builder instance
            // A more comprehensive test would verify the image was actually stored
        }

        [Fact]
        public async Task BuildAsync_ReturnsBookWriter()
        {
            // Arrange
            var builder = EpubBuilder.Create("Test Book", "en", _epubLogger, _loggerFactory)
                .WithAuthor("Test Author")
                .AddChapter(1, "Chapter 1", new[] { "Paragraph 1", "Paragraph 2" });

            // Act
            var writer = await builder.BuildAsync();

            // Assert
            Assert.NotNull(writer);
            Assert.IsAssignableFrom<IBookWriter>(writer);
        }
    }
} 