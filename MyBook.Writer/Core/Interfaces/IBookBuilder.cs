namespace MyBook.Writer.Core.Interfaces
{
    /// <summary>
    /// Interface for book builders with fluent API
    /// </summary>
    public interface IBookBuilder
    {
        /// <summary>
        /// Set the book title
        /// </summary>
        /// <param name="title">Book title</param>
        /// <returns>Builder instance for method chaining</returns>
        IBookBuilder WithTitle(string title);

        /// <summary>
        /// Set the book language
        /// </summary>
        /// <param name="language">Language code (e.g., "en", "fr")</param>
        /// <returns>Builder instance for method chaining</returns>
        IBookBuilder WithLanguage(string language);

        /// <summary>
        /// Set the book author
        /// </summary>
        /// <param name="author">Author name</param>
        /// <returns>Builder instance for method chaining</returns>
        IBookBuilder WithAuthor(string author);

        /// <summary>
        /// Add cover image to the book
        /// </summary>
        /// <param name="imageStream">Image data stream</param>
        /// <param name="fileName">Image file name</param>
        /// <param name="contentType">Image content type (e.g., "image/jpeg")</param>
        /// <returns>Builder instance for method chaining</returns>
        IBookBuilder WithCover(Stream imageStream, string fileName, string contentType);

        /// <summary>
        /// Add a chapter to the book
        /// </summary>
        /// <param name="chapterId">Chapter ID/sequence number</param>
        /// <param name="title">Chapter title</param>
        /// <param name="paragraphs">List of paragraph texts</param>
        /// <returns>Builder instance for method chaining</returns>
        IBookBuilder AddChapter(int chapterId, string title, IEnumerable<string> paragraphs);

        /// <summary>
        /// Build the book and get the appropriate writer
        /// </summary>
        /// <returns>Book writer object</returns>
        Task<IBookWriter> BuildAsync();
    }
}