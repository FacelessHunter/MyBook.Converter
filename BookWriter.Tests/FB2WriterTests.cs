using FluentAssertions;
using MyBook.Writer;
using MyBook.Writer.Builders;
using MyBook.Writer.Core.Interfaces;
using System.Text;
using System.Xml.Linq;

namespace BookWriter.Tests;

public class FB2WriterTests
{
    [Fact]
    public async Task WithTitle_ShouldAddTitleToDocument()
    {
        // Arrange
        var builder = FB2Builder.Create("Test Book Title", "en");

        // Act
        var writer = await builder.BuildAsync();
        var stream = await writer.SaveAsStreamAsync();
        
        // Assert
        stream.Position = 0;
        var document = XDocument.Load(stream);
        var titleElement = document.Root?
            .Element(XName.Get("description", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("title-info", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("book-title", "http://www.gribuser.ru/xml/fictionbook/2.0"));
        
        titleElement.Should().NotBeNull();
        titleElement?.Value.Should().Be("Test Book Title");
    }

    [Fact]
    public async Task AddChapter_ShouldAddParagraphsToDocument()
    {
        // Arrange
        var builder = FB2Builder.Create("Test Book", "en");
        var sectionTitle = "Test Chapter";
        var paragraphs = new List<string>
        {
            "Paragraph 1",
            "Paragraph 2",
            "Paragraph 3"
        };

        // Act
        builder.AddChapter(1, sectionTitle, paragraphs);
        var writer = await builder.BuildAsync();
        var stream = await writer.SaveAsStreamAsync();
        
        // Assert
        stream.Position = 0;
        var document = XDocument.Load(stream);
        var sectionElement = document.Root?
            .Element(XName.Get("body", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("section", "http://www.gribuser.ru/xml/fictionbook/2.0"));
        
        sectionElement.Should().NotBeNull();
        
        var titleElement = sectionElement?
            .Element(XName.Get("title", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("p", "http://www.gribuser.ru/xml/fictionbook/2.0"));
        
        titleElement.Should().NotBeNull();
        titleElement?.Value.Should().Be(sectionTitle);
        
        // Check paragraphs
        var paragraphElements = sectionElement?
            .Elements(XName.Get("p", "http://www.gribuser.ru/xml/fictionbook/2.0"))
            .ToList();
        
        paragraphElements.Should().NotBeNull();
        paragraphElements.Should().HaveCountGreaterThanOrEqualTo(3);
        
        // The first p element is in the title, so we check from index 1
        paragraphElements?[0].Value.Should().Be("Paragraph 1");
        paragraphElements?[1].Value.Should().Be("Paragraph 2");
        paragraphElements?[2].Value.Should().Be("Paragraph 3");
    }

    [Fact]
    public async Task WithCover_ShouldAddCoverToDocument()
    {
        // Arrange
        var builder = FB2Builder.Create("Test Book", "en");
        var imageData = Encoding.UTF8.GetBytes("Test image data");
        var imageStream = new MemoryStream(imageData);
        var contentType = "image/jpeg";

        // Act
        builder.WithCover(imageStream, "cover.jpg", contentType);
        var writer = await builder.BuildAsync();
        var stream = await writer.SaveAsStreamAsync();
        
        // Assert
        stream.Position = 0;
        var document = XDocument.Load(stream);
        
        // Check binary element
        var binaryElement = document.Root?
            .Element(XName.Get("binary", "http://www.gribuser.ru/xml/fictionbook/2.0"));
        
        binaryElement.Should().NotBeNull();
        binaryElement?.Attribute("id")?.Value.Should().Be("cover-image");  // Default ID
        binaryElement?.Attribute("content-type")?.Value.Should().Be(contentType);
        
        // Decode and verify image data
        var base64Data = binaryElement?.Value;
        base64Data.Should().NotBeNullOrEmpty();
        var decodedData = Convert.FromBase64String(base64Data);
        Encoding.UTF8.GetString(decodedData).Should().Be("Test image data");
        
        // Check coverpage reference
        var coverpageElement = document.Root?
            .Element(XName.Get("description", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("title-info", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("coverpage", "http://www.gribuser.ru/xml/fictionbook/2.0"));
        
        coverpageElement.Should().NotBeNull();
        
        var imageElement = coverpageElement?
            .Element(XName.Get("image", "http://www.gribuser.ru/xml/fictionbook/2.0"));
        
        imageElement.Should().NotBeNull();
        
        // Check it has an href attribute with our ID
        var hrefAttributes = imageElement?.Attributes()
            .Where(a => a.Name.LocalName == "href");
        
        hrefAttributes.Should().NotBeEmpty();
        hrefAttributes.Should().Contain(a => a.Value == "#cover-image");
    }

    [Fact]
    public async Task WithAuthor_ShouldAddAuthorToDocument()
    {
        // Arrange
        var builder = FB2Builder.Create("Test Book", "en");
        var authorName = "John Smith";

        // Act
        builder.WithAuthor(authorName);
        var writer = await builder.BuildAsync();
        var stream = await writer.SaveAsStreamAsync();
        
        // Assert
        stream.Position = 0;
        var document = XDocument.Load(stream);
        
        var authorElement = document.Root?
            .Element(XName.Get("description", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("title-info", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("author", "http://www.gribuser.ru/xml/fictionbook/2.0"));
        
        authorElement.Should().NotBeNull();
        
        var firstName = authorElement?
            .Element(XName.Get("first-name", "http://www.gribuser.ru/xml/fictionbook/2.0"))?.Value;
        var lastName = authorElement?
            .Element(XName.Get("last-name", "http://www.gribuser.ru/xml/fictionbook/2.0"))?.Value;
        
        firstName.Should().Be("John");
        lastName.Should().Be("Smith");
    }

    [Fact]
    public async Task SaveToFileAsync_ShouldCreateFB2File()
    {
        // Arrange
        var builder = FB2Builder.Create("File Saving Test", "en");
        builder.AddChapter(1, "Chapter 1", new List<string> { "Test paragraph" });
        
        // Create temp file path
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"test-fb2-{Guid.NewGuid()}.fb2");
        
        try
        {
            // Act
            var writer = await builder.BuildAsync();
            await writer.SaveToFileAsync(tempFilePath);
            
            // Assert
            File.Exists(tempFilePath).Should().BeTrue();
            var fileInfo = new FileInfo(tempFilePath);
            fileInfo.Length.Should().BeGreaterThan(0);
            
            // Verify file is a valid FB2
            using var fileStream = File.OpenRead(tempFilePath);
            var document = XDocument.Load(fileStream);
            document.Root?.Name.LocalName.Should().Be("FictionBook");
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
    
    [Fact]
    public async Task FactoryCreatedBuilder_ShouldCreateValidFB2()
    {
        // Arrange
        var builder = BookFactory.CreateBuilder(BookFormat.FB2, "Factory Created Book", "en");
        
        // Add content using interface
        builder.WithAuthor("Test Author")
               .AddChapter(1, "Factory Chapter", new[] { "This is content created via factory" });
        
        // Act
        var writer = await builder.BuildAsync();
        var stream = await writer.SaveAsStreamAsync();
        
        // Assert
        stream.Position = 0;
        var document = XDocument.Load(stream);
        
        // Verify FB2 structure
        document.Root?.Name.LocalName.Should().Be("FictionBook");
        document.Root?.Name.NamespaceName.Should().Be("http://www.gribuser.ru/xml/fictionbook/2.0");
        
        // Verify title
        var titleElement = document.Root?
            .Element(XName.Get("description", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("title-info", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("book-title", "http://www.gribuser.ru/xml/fictionbook/2.0"));
            
        titleElement?.Value.Should().Be("Factory Created Book");
        
        // Verify author
        var authorElement = document.Root?
            .Element(XName.Get("description", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("title-info", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("author", "http://www.gribuser.ru/xml/fictionbook/2.0"));
            
        authorElement.Should().NotBeNull();
        
        // Verify chapter
        var sectionElement = document.Root?
            .Element(XName.Get("body", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("section", "http://www.gribuser.ru/xml/fictionbook/2.0"));
            
        sectionElement.Should().NotBeNull();
    }
} 