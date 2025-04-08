namespace DataScraper.BookWriters;

public abstract class WriterEngine
{
    public abstract void SetBookTitle(string name);

    public abstract void AddSection(string title, IEnumerable<string> lines);

    public abstract void SetCoverpage(MemoryStream stream, string id, string contentType);

    public abstract Task<MemoryStream> SaveAsStreamAsync();
}