using FluentAssertions;
using MyBook.Writer;
using MyBook.Writer.Builders;
using MyBook.Writer.Core.Interfaces;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace BookWriter.Tests;

public class EpubWriterTests
{
    [Fact]
    public async Task BuildAsync_ShouldCreateValidEpubStructure()
    {
        // Arrange
        var builder = EpubBuilder.Create("Test Book", "en");
        
        // Add test chapters
        builder.AddChapter(1, "Chapter 1", new List<string>
        {
            "This is paragraph 1 of chapter 1.",
            "This is paragraph 2 of chapter 1."
        });
        
        builder.AddChapter(2, "Chapter 2", new List<string>
        {
            "This is paragraph 1 of chapter 2.",
            "This is paragraph 2 of chapter 2."
        });

        // Act
        var writer = await builder.BuildAsync();
        var stream = await writer.SaveAsStreamAsync();

        // Assert
        using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
        
        // Validate expected files exist
        zipArchive.GetEntry("mimetype").Should().NotBeNull();
        zipArchive.GetEntry("META-INF/container.xml").Should().NotBeNull();
        zipArchive.GetEntry("OEBPS/content.opf").Should().NotBeNull();
        zipArchive.GetEntry("OEBPS/toc.xhtml").Should().NotBeNull();
        zipArchive.GetEntry("OEBPS/style.css").Should().NotBeNull();
        zipArchive.GetEntry("OEBPS/chapter_1.xhtml").Should().NotBeNull();
        zipArchive.GetEntry("OEBPS/chapter_2.xhtml").Should().NotBeNull();
        
        // Verify mimetype content
        using var mimetypeEntryStream = zipArchive.GetEntry("mimetype").Open();
        using var reader = new StreamReader(mimetypeEntryStream);
        var mimetypeContent = await reader.ReadToEndAsync();
        mimetypeContent.Should().Be("application/epub+zip");
        
        // Verify content.opf has the correct title
        using var contentOpfStream = zipArchive.GetEntry("OEBPS/content.opf").Open();
        using var contentOpfReader = new StreamReader(contentOpfStream);
        var contentOpf = await contentOpfReader.ReadToEndAsync();
        contentOpf.Should().Contain("<dc:title>Test Book</dc:title>");
        contentOpf.Should().Contain("<dc:language>en</dc:language>");
    }

    [Fact]
    public async Task WithCover_ShouldAddCoverImage()
    {
        // Arrange
        var builder = EpubBuilder.Create("Book With Cover", "en");
        
        // Create a sample image
        var imageData = Encoding.UTF8.GetBytes("This is a mock image for testing");
        var coverStream = new MemoryStream(imageData);
        
        // Add cover image
        builder.WithCover(coverStream, "cover.jpg", "image/jpeg");
        
        // Add a test chapter
        builder.AddChapter(1, "Chapter 1", new List<string> { "Test paragraph" });

        // Act
        var writer = await builder.BuildAsync();
        var stream = await writer.SaveAsStreamAsync();

        // Assert
        using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
        
        // Verify cover files exist
        zipArchive.GetEntry("OEBPS/cover.jpg").Should().NotBeNull();
        zipArchive.GetEntry("OEBPS/cover.xhtml").Should().NotBeNull();
        
        // Verify content.opf has cover references
        using var contentOpfStream = zipArchive.GetEntry("OEBPS/content.opf").Open();
        using var contentOpfReader = new StreamReader(contentOpfStream);
        var contentOpf = await contentOpfReader.ReadToEndAsync();
        contentOpf.Should().Contain("id=\"cover-image\"");
        contentOpf.Should().Contain("properties=\"cover-image\"");
        
        // Verify cover image content
        using var coverImageStream = zipArchive.GetEntry("OEBPS/cover.jpg").Open();
        using var coverImageReader = new StreamReader(coverImageStream);
        var coverImageContent = await coverImageReader.ReadToEndAsync();
        coverImageContent.Should().Be("This is a mock image for testing");
    }

    [Fact]
    public async Task AddChapter_ShouldAddContentCorrectly()
    {
        // Arrange
        var builder = EpubBuilder.Create("Book with Chapter", "en");
        
        // Add chapter with multiple paragraphs
        builder.AddChapter(1, "Test Chapter", new List<string>
        {
            "This is a paragraph 1.",
            "This is a paragraph 2."
        });

        // Act
        var writer = await builder.BuildAsync();
        var stream = await writer.SaveAsStreamAsync();

        // Assert
        using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
        
        // Verify chapter file exists
        var chapterEntry = zipArchive.GetEntry("OEBPS/chapter_1.xhtml");
        chapterEntry.Should().NotBeNull();
        
        // Verify chapter content
        using var chapterStream = chapterEntry.Open();
        using var chapterReader = new StreamReader(chapterStream);
        var chapterContent = await chapterReader.ReadToEndAsync();
        
        chapterContent.Should().Contain("<h1 class=\"chapter-title\">Test Chapter</h1>");
        chapterContent.Should().Contain("<p>This is a paragraph 1.</p>");
        chapterContent.Should().Contain("<p>This is a paragraph 2.</p>");
    }

    [Fact]
    public async Task SaveToFileAsync_ShouldCreateEpubFile()
    {
        // Arrange
        var builder = EpubBuilder.Create("File Saving Test", "en");
        builder.AddChapter(1, "Chapter 1", new List<string> { "Test paragraph" });
        
        // Create temp file path
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"test-epub-{Guid.NewGuid()}.epub");
        
        try
        {
            // Act
            var writer = await builder.BuildAsync();
            await writer.SaveToFileAsync(tempFilePath);
            
            // Assert
            File.Exists(tempFilePath).Should().BeTrue();
            var fileInfo = new FileInfo(tempFilePath);
            fileInfo.Length.Should().BeGreaterThan(0);
            
            // Verify file is a valid EPUB
            using var fileStream = File.OpenRead(tempFilePath);
            using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read);
            zipArchive.GetEntry("mimetype").Should().NotBeNull();
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
    public async Task FactoryCreatedBuilder_ShouldCreateValidEpub()
    {
        // Arrange
        var builder = BookFactory.CreateBuilder(BookFormat.Epub, "Factory Created Book", "en");
        
        // Add content using interface
        builder.WithAuthor("Test Author")
               .AddChapter(1, "Factory Chapter", new[] { "This is content created via factory" });
        
        // Act
        var writer = await builder.BuildAsync();
        var stream = await writer.SaveAsStreamAsync();
        
        // Assert
        using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
        
        // Verify basic structure
        zipArchive.GetEntry("mimetype").Should().NotBeNull();
        zipArchive.GetEntry("OEBPS/chapter_1.xhtml").Should().NotBeNull();
        
        // Verify content.opf has the correct metadata
        using var contentOpfStream = zipArchive.GetEntry("OEBPS/content.opf").Open();
        using var contentOpfReader = new StreamReader(contentOpfStream);
        var contentOpf = await contentOpfReader.ReadToEndAsync();
        
        contentOpf.Should().Contain("<dc:title>Factory Created Book</dc:title>");
        contentOpf.Should().Contain("<dc:language>en</dc:language>");
        contentOpf.Should().Contain("<dc:creator");
        contentOpf.Should().Contain(">Test Author</dc:creator>");
    }
} 