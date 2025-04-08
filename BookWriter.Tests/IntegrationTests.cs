using FluentAssertions;
using MyBook.Writer;
using MyBook.Writer.Builders;
using MyBook.Writer.Core.Interfaces;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace BookWriter.Tests;

public class IntegrationTests
{
    [Fact]
    public async Task SameContentInBothFormats_ShouldCreateValidFiles()
    {
        // Common book data
        var bookTitle = "Integration Test Book";
        var chapters = new Dictionary<int, (string Title, List<string> Paragraphs)>
        {
            [1] = ("Chapter One", new List<string> 
            { 
                "This is paragraph 1 of chapter 1.",
                "This is paragraph 2 of chapter 1."
            }),
            [2] = ("Chapter Two", new List<string>
            {
                "This is paragraph 1 of chapter 2.",
                "This is paragraph 2 of chapter 2."
            })
        };
        var coverImageData = Encoding.UTF8.GetBytes("Test cover image data");
        
        // Create EPUB book
        var epubBuilder = EpubBuilder.Create(bookTitle, "en");
        
        // Add cover
        epubBuilder.WithCover(new MemoryStream(coverImageData), "cover.jpg", "image/jpeg");
        
        // Add chapters
        foreach (var (chapterNumber, chapterData) in chapters)
        {
            epubBuilder.AddChapter(chapterNumber, chapterData.Title, chapterData.Paragraphs);
        }
        
        // Build EPUB
        var epubWriter = await epubBuilder.BuildAsync();
        var epubStream = await epubWriter.SaveAsStreamAsync();
        
        // Create FB2 book
        var fb2Builder = FB2Builder.Create(bookTitle, "en");
        
        // Add cover
        fb2Builder.WithCover(new MemoryStream(coverImageData), "cover.jpg", "image/jpeg");
        
        // Add chapters
        foreach (var (chapterNumber, chapterData) in chapters)
        {
            fb2Builder.AddChapter(chapterNumber, chapterData.Title, chapterData.Paragraphs);
        }
        
        // Build and save FB2
        var fb2Writer = await fb2Builder.BuildAsync();
        var fb2Stream = await fb2Writer.SaveAsStreamAsync();
        
        // Verify both streams contain valid data
        epubStream.Length.Should().BeGreaterThan(0);
        fb2Stream.Length.Should().BeGreaterThan(0);
        
        // Verify EPUB structure
        using (var epubZip = new ZipArchive(epubStream, ZipArchiveMode.Read))
        {
            epubZip.GetEntry("mimetype").Should().NotBeNull();
            epubZip.GetEntry("OEBPS/content.opf").Should().NotBeNull();
            epubZip.GetEntry("OEBPS/chapter_1.xhtml").Should().NotBeNull();
            epubZip.GetEntry("OEBPS/chapter_2.xhtml").Should().NotBeNull();
            epubZip.GetEntry("OEBPS/cover.jpg").Should().NotBeNull();
            
            // Verify book title in content.opf
            using var contentOpfStream = epubZip.GetEntry("OEBPS/content.opf").Open();
            using var contentOpfReader = new StreamReader(contentOpfStream);
            var contentOpf = await contentOpfReader.ReadToEndAsync();
            contentOpf.Should().Contain($"<dc:title>{bookTitle}</dc:title>");
        }
        
        // Verify FB2 structure
        fb2Stream.Position = 0;
        var fb2Doc = XDocument.Load(fb2Stream);
        fb2Doc.Root?.Name.LocalName.Should().Be("FictionBook");
        
        // Verify book title
        var titleElement = fb2Doc.Root?
            .Element(XName.Get("description", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("title-info", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Element(XName.Get("book-title", "http://www.gribuser.ru/xml/fictionbook/2.0"));
        
        titleElement?.Value.Should().Be(bookTitle);
        
        // Verify sections count
        var sections = fb2Doc.Root?
            .Element(XName.Get("body", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
            .Elements(XName.Get("section", "http://www.gribuser.ru/xml/fictionbook/2.0"))
            .ToList();
        
        sections.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task BookFactoryTest_ShouldCreateBooksInBothFormats()
    {
        // Test data
        var title = "Factory Test Book";
        var author = "Test Author";
        var chapter = new[] { "This is test content created with BookFactory" };
        
        // Create a test image
        var coverData = Encoding.UTF8.GetBytes("Test cover image for factory test");
        var coverStream = new MemoryStream(coverData);
        
        // Test with both formats
        await TestBookFormat(BookFormat.Epub, title, author, chapter, coverStream);
        
        // Reset the stream and test FB2
        coverStream.Position = 0;
        await TestBookFormat(BookFormat.FB2, title, author, chapter, coverStream);
    }
    
    private async Task TestBookFormat(BookFormat format, string title, string author, 
        string[] chapterContent, Stream coverStream)
    {
        // Create builder through factory
        var builder = BookFactory.CreateBuilder(format, title, "en");
        
        // Configure the book
        builder.WithAuthor(author)
               .WithCover(coverStream, "cover.jpg", "image/jpeg")
               .AddChapter(1, "Test Chapter", chapterContent);
        
        // Build and save to stream
        var writer = await builder.BuildAsync();
        var outputStream = await writer.SaveAsStreamAsync();
        
        // Verify stream has content
        outputStream.Length.Should().BeGreaterThan(0);
        
        // Format-specific verification
        if (format == BookFormat.Epub)
        {
            // Verify EPUB structure
            using var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Read);
            zipArchive.GetEntry("mimetype").Should().NotBeNull();
            zipArchive.GetEntry("META-INF/container.xml").Should().NotBeNull();
            zipArchive.GetEntry("OEBPS/content.opf").Should().NotBeNull();
            zipArchive.GetEntry("OEBPS/chapter_1.xhtml").Should().NotBeNull();
        }
        else if (format == BookFormat.FB2)
        {
            // Verify FB2 structure
            outputStream.Position = 0;
            var fb2Doc = XDocument.Load(outputStream);
            fb2Doc.Root?.Name.LocalName.Should().Be("FictionBook");
            
            // Verify book title
            var titleElement = fb2Doc.Root?
                .Element(XName.Get("description", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
                .Element(XName.Get("title-info", "http://www.gribuser.ru/xml/fictionbook/2.0"))?
                .Element(XName.Get("book-title", "http://www.gribuser.ru/xml/fictionbook/2.0"));
            
            titleElement?.Value.Should().Be(title);
        }
    }
} 