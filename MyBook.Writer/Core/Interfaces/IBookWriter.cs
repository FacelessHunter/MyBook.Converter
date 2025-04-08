namespace MyBook.Writer.Core.Interfaces
{
    /// <summary>
    /// Interface for book writers that can save books in different formats
    /// </summary>
    public interface IBookWriter
    {
        /// <summary>
        /// Save the book to a memory stream
        /// </summary>
        /// <returns>Stream containing the book data</returns>
        Task<Stream> SaveAsStreamAsync();

        /// <summary>
        /// Save the book to a file
        /// </summary>
        /// <param name="outputPath">File path to save the book</param>
        Task SaveToFileAsync(string outputPath);
    }
}