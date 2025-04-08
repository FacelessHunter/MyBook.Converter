using MyBook.Writer.Builders;

namespace MyBook.Writer.Examples
{
    /// <summary>
    /// Example demonstrating the new file saving functionality
    /// </summary>
    public static class FileSavingExample
    {
        /// <summary>
        /// Demonstrate various ways to save files
        /// </summary>
        public static async Task RunFileSavingExamplesAsync()
        {
            Console.WriteLine("\n=== File Saving Examples ===\n");

            // Create basic book content
            var title = "File Saving Example Book";
            var author = "Demo Author";
            var chapterContent = new List<string>
            {
                "This is the first paragraph of our example book.",
                "Here's another paragraph with some text content."
            };

            // Example 1: Save with automatic extension
            await SaveWithAutomaticExtensionAsync(title, author, chapterContent);

            // Example 2: Save to directory
            await SaveToDirectoryAsync(title, author, chapterContent);

            // Example 3: Save with mixed extensions
            await SaveWithMixedExtensionsAsync(title, author, chapterContent);

            Console.WriteLine("\n=== End of File Saving Examples ===\n");
        }

        private static async Task SaveWithAutomaticExtensionAsync(string title, string author, List<string> content)
        {
            Console.WriteLine("Example 1: Save with automatic extension");

            // Create EPUB book
            var epubBuilder = EpubBuilder.Create(title, "en")
                .WithAuthor(author)
                .AddChapter(1, "Chapter One", content);

            // Create FB2 book
            var fb2Builder = FB2Builder.Create(title, "en")
                .WithAuthor(author)
                .AddChapter(1, "Chapter One", content);

            // Save both books without extensions
            var epubWriter = await epubBuilder.BuildAsync();
            var fb2Writer = await fb2Builder.BuildAsync();

            var epubPath = Path.Combine(GetExamplesOutputPath(), "example1_no_extension");
            var fb2Path = Path.Combine(GetExamplesOutputPath(), "example1_no_extension");

            // The correct extensions (.epub and .fb2) should be added automatically
            await epubWriter.SaveToFileAsync(epubPath);
            await fb2Writer.SaveToFileAsync(fb2Path);

            Console.WriteLine($"  - EPUB book saved to: {epubPath}.epub");
            Console.WriteLine($"  - FB2 book saved to: {fb2Path}.fb2");
        }

        private static async Task SaveToDirectoryAsync(string title, string author, List<string> content)
        {
            Console.WriteLine("\nExample 2: Save to directory with timestamped filename");

            // Create directory for output
            var outputDir = Path.Combine(GetExamplesOutputPath(), "SavedBooks");
            Directory.CreateDirectory(outputDir);

            // Create EPUB and FB2 books
            var epubBuilder = EpubBuilder.Create(title, "en").WithAuthor(author);
            var fb2Builder = FB2Builder.Create(title, "en").WithAuthor(author);

            // Add content to both books
            epubBuilder.AddChapter(1, "Introduction", content);
            fb2Builder.AddChapter(1, "Introduction", content);

            // Save both books to the directory
            // The files will be saved with format: {BookName}/{timestamp}_{BookName}.{extension}
            var epubWriter = await epubBuilder.BuildAsync();
            var fb2Writer = await fb2Builder.BuildAsync();

            await epubWriter.SaveToFileAsync(outputDir);
            await fb2Writer.SaveToFileAsync(outputDir);

            Console.WriteLine($"  - Books saved to directory: {outputDir}");
            Console.WriteLine($"  - Check for files in: {outputDir}/{title}/");
        }

        private static async Task SaveWithMixedExtensionsAsync(string title, string author, List<string> content)
        {
            Console.WriteLine("\nExample 3: Save with wrong extensions (will be corrected)");

            // Create books
            var epubBuilder = EpubBuilder.Create(title, "en").WithAuthor(author);
            var fb2Builder = FB2Builder.Create(title, "en").WithAuthor(author);

            // Add content to both books
            epubBuilder.AddChapter(1, "First Chapter", content);
            fb2Builder.AddChapter(1, "First Chapter", content);

            // Save with wrong extensions - they will be corrected
            var epubWriter = await epubBuilder.BuildAsync();
            var fb2Writer = await fb2Builder.BuildAsync();

            var epubPath = Path.Combine(GetExamplesOutputPath(), "example3_epub_with_wrong_extension.fb2");
            var fb2Path = Path.Combine(GetExamplesOutputPath(), "example3_fb2_with_wrong_extension.epub");

            await epubWriter.SaveToFileAsync(epubPath);
            await fb2Writer.SaveToFileAsync(fb2Path);

            Console.WriteLine($"  - EPUB book saved with corrected extension: {Path.ChangeExtension(epubPath, ".epub")}");
            Console.WriteLine($"  - FB2 book saved with corrected extension: {Path.ChangeExtension(fb2Path, ".fb2")}");
        }

        private static string GetExamplesOutputPath()
        {
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "FileSavingExamples");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            return outputPath;
        }
    }
}