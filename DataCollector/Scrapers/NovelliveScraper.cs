using HtmlAgilityPack;
using SharedModels;
using SharedModels.Utilities;
using System.Net;

namespace DataScraper.Scrapers;

public sealed class NovelliveScraper : ScraperEngine
{
    private const string unloadedDocumentMessage = "NovelliveScraper.ScrapeContentAsync: Document is unloaded, maybe you have forgot to load web page";
    private const string ForbiddenSequences = "Visit and read more novel to help us update chapter quickly. Thank you so much!";
    private HtmlDocument document = new HtmlDocument();
    public NovelliveScraper(string url)
    {
        Load(url).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private async Task Load(string url)
    {
        var content = await DownloadPage(url);

        document.LoadHtml(content);
    }

    private async Task LoadAsync(string url)
    {
        var content = await DownloadPage(url);

        document.LoadHtml(content);
    }

    public override async Task<bool> GetToFirstChapter()
    {
        ValidateDocument();

        var firstChapterUrl = document.DocumentNode?
            .SelectSingleNode("//div[@class='btn']/a[text()=' Read first']")?
            .Attributes["href"]?.Value;

        if (string.IsNullOrWhiteSpace(firstChapterUrl))
        {
            return false;
        }

        await LoadAsync(firstChapterUrl);

        return true;
    }

    /// <summary>
    /// Works only on title level
    /// </summary>
    /// <returns></returns>
    public override async Task<EncodedImage> LoadCoverPageAsync()
    {
        ValidateDocument();

        var coverpageUrl = document.DocumentNode?
            .SelectSingleNode("//div[@class='pic']/img")?
            .Attributes["src"]?.Value;

        if (string.IsNullOrWhiteSpace(coverpageUrl))
        {
            throw new Exception("CoverPage wasn't found");
        }
        Console.WriteLine("Loading coverimage");
        var downloadedImage = await NetworkHelpers.DownloadAsStream(coverpageUrl!);

        var filename = coverpageUrl!.Split('/').Last();

        Console.WriteLine("Upscaling coverimage");
        var upscaledImage = await ImageEnhancer.UpscaleAndEnhanceImageAsync(downloadedImage);

        return upscaledImage.ConvertToSerializableImage(filename);
    }

    public override async Task<bool> NextChapterAsync()
    {
        ValidateDocument();

        var nextChapterUrl = document.DocumentNode?
            .SelectSingleNode("//a[@title='Read Next Chapter']")?
            .Attributes["href"]?.Value;

        if (string.IsNullOrWhiteSpace(nextChapterUrl))
        {
            return false;
        }

        await LoadAsync(nextChapterUrl);
        return true;
    }

    public override async Task<bool> PreviousChapterAsync()
    {
        ValidateDocument();

        var nextChapterUrl = document.DocumentNode
            .SelectSingleNode("//a[@title='Read Previous Chapter']")
            .Attributes["href"].Value;

        if (string.IsNullOrWhiteSpace(nextChapterUrl))
        {
            return false;
        }

        await LoadAsync(nextChapterUrl);
        return true;
    }

    public override List<string> ScrapeChapterContent()
    {
        ValidateDocument();

        return document.DocumentNode
            .SelectSingleNode("//div[@class='txt']")
            .ChildNodes.Where(i => i.Name == "p" && i.InnerText != ForbiddenSequences)
            .Select(x => NormalizeHtmlEntities(x.InnerText))
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .ToList();
    }

    public override string ScrapeChapterTitle()
    {
        ValidateDocument();

        var title = document.DocumentNode
            .SelectSingleNode("//span[@class='chapter']")
            .InnerText;

        return NormalizeHtmlEntities(title);

    }

    public override string ScrapeBookName()
    {
        ValidateDocument();

        var bookTitle = document.DocumentNode
            .SelectSingleNode("//h1[@class='tit']")
            .InnerText;

        return NormalizeHtmlEntities(bookTitle);
    }

    private void ValidateDocument()
    {
        if (document is null)
        {
            throw new InvalidOperationException(unloadedDocumentMessage);
        }
    }

    public async Task<string> DownloadPage(string url)
    {
        var csrfToken = "SeT9h494yc0btQOt3Jwt_U1xhhQyn83JLFGPg69zQ1Q-1744301990-1.2.1.1-LfPIFMQc_p6pI0QR1CJ4Th2hDj0gCKm1K1OMKoFkHDbS8rNVmiuM6kacALreeg9gUrEirifs2YMIUZiOJDkL5OL6u073N8XCZIR8OVHznAXIaJgw4.OT2wgJLoG62yD7vYxX5GPcx5w9Kven5QZHRoaF7P95ya2HNphek0BqUk3VYnQX1mexYJAHQovkoeNp8qx3gdEIeq.VaWgwIREgXhRo._YwQfuvKOeMD0s_pbwZjfyWvCnP4t162qGbPHKjn3yjhWTDhx1td1sWn5w9VbFM96YhWPt8e49Qnw3i.dnI45oCbdgp6fEhek66vNz292iWZjTiocBbVNziKsFuNThkXxFKri495J_FRdmYqUmXFxZOa1ao2CcxG6dbTHuF; _ga_88GYG4MDVP=GS1.1.1744301985.17.1.1744302457.0.0.0";
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36");
        request.Headers.Add("Cookie", $"cf_clearance={csrfToken}");
        request.Headers.Add("Cookie", $"_csrf={csrfToken}");

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();

    }
    private static string NormalizeHtmlEntities(string input)
    {
        return WebUtility.HtmlDecode(input);
    }
}