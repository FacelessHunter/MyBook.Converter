
using SharedModels;

namespace MyBook.Epub.Writer.Models
{
    public class EpubBook
    {
        public string MimeType { get; } = "application/epub+zip";

        public FileModel CoverPage { get; set; }

        public EpubBookMetaData Metadata { get; init; } = new EpubBookMetaData();

        public EpubBookContent Content { get; init; } = new EpubBookContent();
    }
}
