using MyBook.Writer.Builders;

namespace MyBook.Writer.Examples
{
    /// <summary>
    /// Examples demonstrating the usage of the MyBook.Writer library
    /// </summary>
    public static class BookWriterExample
    {
        /// <summary>
        /// Create a simple EPUB book with chapters and a cover image
        /// </summary>
        /// <param name="outputPath">Path to save the EPUB file</param>
        public static async Task CreateEpubBookAsync(string outputPath)
        {
            // Create a new EPUB builder
            var builder = EpubBuilder.Create("My Sample Book", "en")
                .WithAuthor("John Smith");

            // Add a cover image (from a file)
            using (var coverStream = File.OpenRead("cover.jpg"))
            {
                builder.WithCover(coverStream, "cover.jpg", "image/jpeg");
            }

            // Add chapters
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

            // Build and save the book
            var writer = await builder.BuildAsync();
            await writer.SaveToFileAsync(outputPath);

            Console.WriteLine($"EPUB book created at: {outputPath}");
        }

        /// <summary>
        /// Create a simple FB2 book with chapters and a cover image
        /// </summary>
        /// <param name="outputPath">Path to save the FB2 file</param>
        public static async Task CreateFB2BookAsync(string outputPath)
        {
            // Create a new FB2 builder
            var builder = FB2Builder.Create("My Sample FB2 Book", "en")
                .WithAuthor("Jane Doe");

            // Add a cover image (from a file)
            using (var coverStream = File.OpenRead("cover.jpg"))
            {
                builder.WithCover(coverStream, "cover.jpg", "image/jpeg");
            }

            // Add chapters
            builder.AddChapter(1, "Preface", new List<string>
            {
                "This is the preface to our FB2 book.",
                "It explains what the reader can expect."
            });

            builder.AddChapter(2, "First Chapter", new List<string>
            {
                "The FB2 book's first chapter begins here.",
                "We continue with more content.",
                "And conclude with this final paragraph."
            });

            // Build and save the book
            var writer = await builder.BuildAsync();
            await writer.SaveToFileAsync(outputPath);

            Console.WriteLine($"FB2 book created at: {outputPath}");
        }

        /// <summary>
        /// Create a book using the factory approach
        /// </summary>
        /// <param name="format">Format to create</param>
        /// <param name="outputPath">Path to save the book</param>
        public static async Task CreateBookWithFactoryAsync(BookFormat format, string outputPath)
        {
            // Use the factory to create the appropriate builder
            var builder = BookFactory.CreateBuilder(format, "Factory Created Book", "en");

            // Add common book content
            builder.WithAuthor("Factory Author");

            using (var coverStream = File.OpenRead("cover.jpg"))
            {
                builder.WithCover(coverStream, "cover.jpg", "image/jpeg");
            }

            builder.AddChapter(1, "Factory Chapter", new List<string>
            {
                "This chapter was created using the BookFactory.",
                "The book format is: " + format
            });

            // Build and save
            var writer = await builder.BuildAsync();
            await writer.SaveToFileAsync(outputPath);

            Console.WriteLine($"{format} book created with factory at: {outputPath}");
        }

        /// <summary>
        /// Example method to run all demos
        /// </summary>
        public static async Task RunAllDemosAsync()
        {
            await CreateEpubBookAsync("sample_book.epub");
            await CreateFB2BookAsync("sample_book.fb2");
            await CreateBookWithFactoryAsync(BookFormat.Epub, "factory_book.epub");
            await CreateBookWithFactoryAsync(BookFormat.FB2, "factory_book.fb2");
        }
    }
}