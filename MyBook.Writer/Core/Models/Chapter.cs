namespace MyBook.Writer.Core.Models
{
    /// <summary>
    /// Represents a chapter in a book
    /// </summary>
    public class Chapter
    {
        /// <summary>
        /// Title of the chapter
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// List of paragraphs in the chapter
        /// </summary>
        public List<Paragraph> Paragraphs { get; } = new List<Paragraph>();

        /// <summary>
        /// Add a paragraph to the chapter
        /// </summary>
        /// <param name="text">Paragraph text</param>
        public void AddParagraph(string text)
        {
            Paragraphs.Add(new Paragraph { Text = text });
        }

        /// <summary>
        /// Add multiple paragraphs to the chapter
        /// </summary>
        /// <param name="paragraphs">Collection of paragraph texts</param>
        public void AddParagraphs(IEnumerable<string> paragraphs)
        {
            foreach (var text in paragraphs)
            {
                AddParagraph(text);
            }
        }
    }
}