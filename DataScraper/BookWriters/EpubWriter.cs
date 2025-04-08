using MyBook.Epub.Writer;
using MyBook.Epub.Writer.ModelBuilders;

namespace DataScraper.BookWriters;

public class LocalEpubWriter : WriterEngine
{
    private EpubBookBuilder _bookBuilder;
    int _count = 1;
    public LocalEpubWriter()
    {
        _bookBuilder = new EpubBookBuilder()
            .CreateBook();
    }

    public override void AddSection(string title, IEnumerable<string> lines)
    {
        EpubChapterBuilder chapterBuilder = new EpubChapterBuilder();
        chapterBuilder.SetTitle(title);
        foreach (string line in lines)
        {
            chapterBuilder.AddTextParagraph(line);
        }
        var chapter = chapterBuilder.Build();

        _bookBuilder.AddChapter(_count, chapter);

        _count++;
    }

    public override async Task<MemoryStream> SaveAsStreamAsync()
    {
        EpubWriter epubWriter = new EpubWriter("./OutputDir", _bookBuilder.Build());
        await epubWriter.Write();
        var stream = epubWriter.ArchiveAsStream();

        return stream;
    }

    public override void SetBookTitle(string name)
    {
        _bookBuilder.SetTitle(name);
        _bookBuilder.SetLanguage("en");
        _bookBuilder.SetIdentifier(Guid.NewGuid().ToString());
    }

    public override void SetCoverpage(MemoryStream stream, string id, string contentType)
    {

    }
}