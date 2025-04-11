using MyBook.Writer.Examples;
using System.IO;
using Xunit;

namespace MyBook.Writer.Tests
{
    public class ExampleTests
    {
        [Fact]
        public async Task CreateBookWithFactoryAsync_CreatesValidBook()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"factory_book_{Guid.NewGuid()}.epub");
            var coverPath = CreateTemporaryCoverImage();

            try
            {
                // Adapt the example to work in a test environment
                // by providing a custom cover path instead of the hardcoded "cover.jpg"
                var builder = BookFactory.CreateBuilder(BookFormat.Epub, "Test Factory Book", "en")
                    .WithAuthor("Factory Author");
                
                // Use a separate scope for the stream to ensure it's closed before saving
                using (var originalStream = File.OpenRead(coverPath))
                {
                    builder.WithCover(originalStream, "cover.jpg", "image/jpeg");
                }
                
                builder.AddChapter(1, "Factory Chapter", new List<string>
                {
                    "This chapter was created using the BookFactory.",
                    "The book format is: " + BookFormat.Epub
                });
                
                var writer = await builder.BuildAsync();
                await writer.SaveToFileAsync(tempPath);

                // Assert
                Assert.True(File.Exists(tempPath));
                Assert.True(new FileInfo(tempPath).Length > 0);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempPath))
                {
                    try 
                    {
                        File.Delete(tempPath);
                    }
                    catch (IOException)
                    {
                        // Ignore file in use errors during cleanup
                    }
                }
                
                if (File.Exists(coverPath))
                {
                    try
                    {
                        File.Delete(coverPath);
                    }
                    catch (IOException)
                    {
                        // Ignore file in use errors during cleanup
                    }
                }
            }
        }

        [Fact]
        public async Task CreateEpubBookAsync_WithTestEnvironment_CreatesValidBook()
        {
            // Arrange
            var tempPath = Path.Combine(Path.GetTempPath(), $"sample_book_{Guid.NewGuid()}.epub");
            var coverPath = CreateTemporaryCoverImage();

            try
            {
                // Similar to the example, but with a test-specific cover file
                var builder = MyBook.Writer.Builders.EpubBuilder.Create("My Sample Book", "en")
                    .WithAuthor("John Smith");

                // Use a separate scope for the stream to ensure it's closed before saving
                using (var coverStream = File.OpenRead(coverPath))
                {
                    builder.WithCover(coverStream, "cover.jpg", "image/jpeg");
                }

                builder.AddChapter(1, "Introduction", new List<string>
                {
                    "This is the first paragraph of the introduction.",
                    "This is the second paragraph with more interesting content.",
                    "And here is a third paragraph that concludes this chapter."
                });

                builder.AddChapter(2, "Chapter One", new List<string>
                {
                    "Chapter one begins with this compelling paragraph.",
                    "The plot thickens in this second paragraph.",
                    "An unexpected twist occurs in this paragraph!"
                });

                var writer = await builder.BuildAsync();
                await writer.SaveToFileAsync(tempPath);

                // Assert
                Assert.True(File.Exists(tempPath));
                Assert.True(new FileInfo(tempPath).Length > 0);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch (IOException)
                    {
                        // Ignore file in use errors during cleanup
                    }
                }
                
                if (File.Exists(coverPath))
                {
                    try
                    {
                        File.Delete(coverPath);
                    }
                    catch (IOException)
                    {
                        // Ignore file in use errors during cleanup
                    }
                }
            }
        }

        // Helper method to create a temporary image file for testing
        private string CreateTemporaryCoverImage()
        {
            var tempImagePath = Path.Combine(Path.GetTempPath(), $"cover_{Guid.NewGuid()}.jpg");
            
            // Create a simple 1x1 pixel JPEG file (minimal valid JPEG)
            byte[] jpegData = new byte[]
            {
                0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x48,
                0x00, 0x48, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01,
                0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xC4, 0x00, 0x14, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xC4, 0x00, 0x14,
                0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3F, 0x00, 0x7F, 0x00, 0xFF, 0xD9
            };
            
            File.WriteAllBytes(tempImagePath, jpegData);
            return tempImagePath;
        }
    }
} 