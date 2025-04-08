namespace MyBook.Epub.Writer.Models.ChapterModels
{
    public class Paragraph : BaseRowModel
    {
        public string Text { get; set; }

        public Paragraph(int id, string text)
            : base(id)
        {
            Text = text;
        }
    }
}
