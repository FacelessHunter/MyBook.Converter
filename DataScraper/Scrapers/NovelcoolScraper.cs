using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace DataScraper.Scrapers;

public sealed class NovelcoolScraper : ScraperEngine
{
    private const string unloadedDocumentMessage = "NovelcoolScraper.ScrapeContentAsync: Document is unloaded, maybe you have forgot to load web page";

    private readonly HtmlWeb web = new HtmlWeb();
    private HtmlDocument? document;

    public NovelcoolScraper(string url)
    {
        Load(url);
    }

    private void Load(string url)
    {
        document = web.Load(url);
    }

    private async Task LoadAsync(string url)
    {
        document = await web.LoadFromWebAsync(url);
    }

    public override async Task<bool> NextChapterAsync()
    {
        if (document is null)
        {
            throw new InvalidOperationException(unloadedDocumentMessage);
        }

        var nextChapterUrl = document.DocumentNode
            .SelectNodes("//*[@class='row-item chapter-reading-pageitem']")?
            .Take(2)?
            .LastOrDefault()?
            .ChildNodes.FirstOrDefault()?
            .Attributes["href"].Value;

        if (string.IsNullOrWhiteSpace(nextChapterUrl))
        {
            return false;
        }

        await LoadAsync(nextChapterUrl);
        return true;
    }

    public override Task<bool> PreviousChapterAsync()
    {
        throw new NotImplementedException();
    }

    public override List<string> ScrapeContent()
    {
        if (document is null)
        {
            throw new InvalidOperationException(unloadedDocumentMessage);
        }

        return document.DocumentNode
            .QuerySelectorAll("p")
            .Select(x => x.InnerText)
            .ToList();
    }

    public override string ScrapeTitle()
    {
        if (document is null)
        {
            throw new InvalidOperationException(unloadedDocumentMessage);
        }

        var chapterTitle = document.DocumentNode
            .QuerySelector("h2")
            .InnerText;

        return chapterTitle;
    }

    public override string ScrapeBookName()
    {
        throw new NotImplementedException();
    }
}