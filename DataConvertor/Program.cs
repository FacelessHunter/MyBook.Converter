using DataConvertor.Extensions;
using DataConvertor.Services;
using Microsoft.Extensions.Logging;
using MyBook.Writer;
using SharedModels.Models;
using SharedModels.Utilities;
using System.Text.Json;

// Configuration
var outputFormat = ConfigurationService.Defaults.OutputFormat;
var language = ConfigurationService.Defaults.DefaultLanguage;

// Set up logging
var loggerFactory = LoggingService.ConfigureLogging(ConfigurationService.Defaults.LogLevel);
var logger = loggerFactory.CreateLogger("DataConvertor");

logger.LogInformation("Starting DataConvertor application");

try
{
    // Get file paths
    var expandedBaseLocation = ConfigurationService.GetExpandedBaseLocation();
    logger.LogInformation("Using book directory: {Directory}", expandedBaseLocation);

    var fileLocation = Path.Combine(expandedBaseLocation, "Only I level up_2025_04_09__11_53_Translated_UA_2025_04_10__14_15.json");
    logger.LogInformation("Loading book from: {FilePath}", fileLocation);

    // Deserialize book
    var serializedBook = await File.ReadAllTextAsync(fileLocation);
    logger.LogDebug("Book JSON loaded: {Size} bytes", serializedBook.Length);

    var book = JsonSerializer.Deserialize<Book>(serializedBook);
    if (book == null)
    {
        logger.LogError("Failed to deserialize book from JSON");
        return;
    }

    logger.LogInformation("Book deserialized: {Title}, {ChapterCount} chapters", 
        book.Title, book.Chapters.Count);

    // Create book builder
    logger.LogInformation("Creating {Format} book builder for: {Title}", outputFormat, book.Title);
    var bookBuilder = BookFactory.CreateBuilder(outputFormat, book.Title, language);

    // Add cover image
    if (book.CoverPage != null)
    {
        logger.LogDebug("Processing cover image");
        var coverImageStream = book.CoverPage.ConvertToStream();
        coverImageStream.Seek(0, SeekOrigin.Begin);

        // Use explicit namespace to resolve ambiguity
        var coverImageMemoryStream = NetworkHelpers.ToMemoryStream(coverImageStream);
        bookBuilder.WithCover(coverImageMemoryStream, book.CoverPage.FileName, book.CoverPage.ContentType);
        logger.LogInformation("Cover image added: {FileName}", book.CoverPage.FileName);
    }
    else
    {
        logger.LogWarning("No cover image found in the book data");
    }

    // Add chapters
    logger.LogDebug("Adding {ChapterCount} chapters to the book", book.Chapters.Count);
    foreach (var chapter in book.Chapters)
    {
        var title = chapter.Value.Title;
        bookBuilder.AddChapter(chapter.Key, title, chapter.Value.Paragraphs);
    }

    // Build the book
    logger.LogInformation("Building {Format} book", outputFormat);
    var writer = await bookBuilder.BuildAsync();

    // Save the book
    logger.LogDebug("Saving book to memory stream");

    var outputLocation = ConfigurationService.GetExpandedBaseLocation();

    await writer.SaveToFileAsync(outputLocation);

    // Get output path
    logger.LogInformation("Saving book to file: {FilePath}", outputLocation);

    // Output completion message
    var filename = Path.GetFileNameWithoutExtension(fileLocation);
    logger.LogInformation("Book conversion completed");
    logger.LogInformation("Book name: {Title}", filename);
    logger.LogInformation("Book saved to: {FilePath}", outputLocation);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during book conversion");
}

logger.LogInformation("DataConvertor application completed");

//static async Task CreateSampleEpubAsync()
//{
//    Console.WriteLine("Creating sample EPUB using EpubBuilder...");
    
//    // Create a new builder with title and language
//    var builder = MyBook.Writer.EpubBuilder.Create("Sample Book", "en");
    
//    // Add some chapters with paragraphs
//    builder.AddChapterWithParagraphs(1, "Chapter One", new List<string> 
//    {
//        "This is the first paragraph of chapter one.",
//        "This is the second paragraph with some more text.",
//        "And here's a third paragraph to demonstrate how multiple paragraphs appear."
//    });
    
//    builder.AddChapterWithParagraphs(2, "Chapter Two", new List<string>
//    {
//        "Chapter two begins with this paragraph.",
//        "And continues with another paragraph that has a bit more content to demonstrate how text flows within the EPUB reader.",
//        "Finally, we end the chapter with this concluding paragraph."
//    });
    
//    // If you want to add a cover image, you would do something like this:
//    // (Commented out as you'd need an actual image)
//    /*
//    using var imageStream = new MemoryStream();
//    using (var fileStream = File.OpenRead("path/to/cover.jpg"))
//    {
//        await fileStream.CopyToAsync(imageStream);
//    }
//    imageStream.Position = 0;
//    builder.WithCover(imageStream, "cover.jpg", "image/jpeg");
//    */
    
//    // Build the EPUB
//    await builder.BuildAsync();
    
//    // Save to a file
//    string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SampleBook.epub");
//    await builder.SaveToFileAsync(outputPath);
    
//    Console.WriteLine($"EPUB created successfully at: {outputPath}");
//}