namespace MyBook.Writer.Core.Models
{
    /// <summary>
    /// Core book model containing all book data
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Book metadata
        /// </summary>
        public BookMetadata Metadata { get; set; } = new BookMetadata();

        /// <summary>
        /// Chapters contained in the book
        /// </summary>
        public SortedDictionary<int, Chapter> Chapters { get; } = new SortedDictionary<int, Chapter>();

        /// <summary>
        /// Cover image data
        /// </summary>
        public CoverImage? Cover { get; set; }

        /// <summary>
        /// Add a chapter to the book
        /// </summary>
        /// <param name="sequence">Chapter sequence number</param>
        /// <param name="chapter">Chapter content</param>
        public void AddChapter(int sequence, Chapter chapter)
        {
            Chapters[sequence] = chapter;
        }
    }
}