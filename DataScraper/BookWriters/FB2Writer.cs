using System.Xml.Linq;

namespace DataScraper.BookWriters;

public class FB2Writer : WriterEngine
{
    private static readonly XNamespace ns = "http://www.gribuser.ru/xml/fictionbook/2.0";
    private readonly XDocument document;

    public FB2Writer()
    {
        document = InitializeDocument();
    }

    public FB2Writer(Stream stream)
    {
        document = XDocument.Load(stream);
    }

    public override void AddSection(string title, IEnumerable<string> lines)
    {
        var section = new XElement(FormatName("section"),
            new XElement(FormatName("title"),
            new XElement(FormatName("p"), title)));

        section.Add(lines.Select(line =>
            new XElement(FormatName("p"), line)));

        document?
            .Element(FormatName("FictionBook"))?
            .Element(FormatName("body"))?
            .Add(section);
    }

    public override async Task<MemoryStream> SaveAsStreamAsync()
    {
        var outputStream = new MemoryStream();

        await document.SaveAsync(outputStream, SaveOptions.None, CancellationToken.None);

        return outputStream;
    }

    public override void SetBookTitle(string name)
    {
        document?
            .Element(FormatName("FictionBook"))?
            .Element(FormatName("description"))?
            .Element(FormatName("title-info"))?
            .Add(new XElement(FormatName("book-title"), name));
    }

    public override void SetCoverpage(MemoryStream stream, string id, string contentType)
    {
        var encodedCoverpage = Convert.ToBase64String(stream.ToArray());
        var binary = new XElement(FormatName("binary"),
            new XAttribute("id", id),
            new XAttribute("content-type", contentType),
            encodedCoverpage);

        document?
            .Element(FormatName("FictionBook"))?
            .Add(binary);

        document?
            .Element(FormatName("FictionBook"))?
            .Element(FormatName("description"))?
            .Element(FormatName("title-info"))?
            .Add(
                new XElement(FormatName("coverpage"),
                    new XElement(FormatName("image"),
                        new XAttribute(XNamespace.Xmlns + "href", "#" + id)
                        )
                    )
            );
    }

    private static XName FormatName(string name) => ns + name;

    private static XDocument InitializeDocument()
    {
        var newDocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

        var fictionBook = new XElement(FormatName("FictionBook"));

        fictionBook.Add(new XAttribute(XNamespace.Xmlns + "l", "http://www.w3.org/1999/xlink"));
        fictionBook.Add(new XAttribute(XNamespace.Xmlns + "xlink", "http://www.w3.org/1999/xlink"));

        var description = new XElement(FormatName("description"));
        fictionBook.Add(description);

        var titleInfo = new XElement(FormatName("title-info"));
        description.Add(titleInfo);

        var body = new XElement(FormatName("body"));
        fictionBook.Add(body);

        newDocument.Add(fictionBook);

        return newDocument;
    }
}