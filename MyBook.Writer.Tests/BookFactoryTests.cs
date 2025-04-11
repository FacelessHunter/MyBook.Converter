using Microsoft.Extensions.Logging;
using MyBook.Writer.Builders;
using MyBook.Writer.Core.Interfaces;
using MyBook.Writer.Tests.TestHelpers;
using Xunit;

namespace MyBook.Writer.Tests
{
    public class BookFactoryTests
    {
        [Fact]
        public void CreateBuilder_WithEpubFormat_ReturnsEpubBuilder()
        {
            // Arrange
            var format = BookFormat.Epub;
            var title = "Test Book";
            var language = "en";

            // Act
            var builder = BookFactory.CreateBuilder(format, title, language);

            // Assert
            Assert.NotNull(builder);
            Assert.IsType<EpubBuilder>(builder);
        }

        [Fact]
        public void CreateBuilder_WithFB2Format_ReturnsFB2Builder()
        {
            // Arrange
            var format = BookFormat.FB2;
            var title = "Test Book";
            var language = "en";

            // Act
            var builder = BookFactory.CreateBuilder(format, title, language);

            // Assert
            Assert.NotNull(builder);
            Assert.IsType<FB2Builder>(builder);
        }

        [Fact]
        public void CreateBuilder_WithInvalidFormat_ThrowsArgumentException()
        {
            // Arrange
            var format = (BookFormat)999; // Invalid enum value
            var title = "Test Book";
            var language = "en";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                BookFactory.CreateBuilder(format, title, language));
            
            Assert.Contains("Unsupported book format", exception.Message);
        }

        [Fact]
        public void SetLoggerFactory_WithNullFactory_UsesNullLoggerFactory()
        {
            // Arrange
            ILoggerFactory? nullFactory = null;
            
            // Act - Setting null should not throw
            BookFactory.SetLoggerFactory(nullFactory!);
            
            // We can create a builder after setting null logger factory
            var builder = BookFactory.CreateBuilder(BookFormat.Epub, "Test", "en");
            
            // Assert
            Assert.NotNull(builder);
        }

        [Fact]
        public void SetLoggerFactory_WithCustomFactory_UsesCustomFactory()
        {
            // Arrange
            var testLoggerFactory = new TestLoggerFactory();
            
            // Act
            BookFactory.SetLoggerFactory(testLoggerFactory);
            var builder = BookFactory.CreateBuilder(BookFormat.Epub, "Test Book", "en");
            
            // Assert
            var factoryLogger = testLoggerFactory.GetTestLogger("BookFactory");
            Assert.NotEmpty(factoryLogger.LogMessages);
            Assert.NotNull(builder);
            Assert.IsType<EpubBuilder>(builder);
        }
    }
} 