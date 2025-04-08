namespace MyBook.Writer.Core.Models
{
    /// <summary>
    /// Book metadata including title, author, language etc.
    /// </summary>
    public class BookMetadata
    {
        /// <summary>
        /// Unique identifier for the book
        /// </summary>
        public string Identifier { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Book title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Book language (ISO 639-1 language code)
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// Book author
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Book publisher
        /// </summary>
        public string Publisher { get; set; } = string.Empty;

        /// <summary>
        /// Publication date
        /// </summary>
        public DateTime? PublicationDate { get; set; }

        /// <summary>
        /// Book description/summary
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}