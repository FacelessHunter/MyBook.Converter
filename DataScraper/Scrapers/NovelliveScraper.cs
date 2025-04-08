using HtmlAgilityPack;
using Microsoft.Playwright;
using System;
using System.Net;

namespace DataScraper.Scrapers;

public sealed class NovelliveScraper : ScraperEngine
{
    private const string unloadedDocumentMessage = "NovelliveScraper.ScrapeContentAsync: Document is unloaded, maybe you have forgot to load web page";
    private const string ForbiddenSequences = "Visit and read more novel to help us update chapter quickly. Thank you so much!";
    private readonly HtmlWeb web = new HtmlWeb();
    private HtmlDocument document = new HtmlDocument();
    private IBrowser _browser;
    private IBrowserContext _browserContext;
    public NovelliveScraper(string url)
    {
        //web.UseCookies = true;
        //web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";

        //var playwright = Playwright.CreateAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        //_browser = playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
        //{
        //    Headless = false,
        //    SlowMo = 100
        //})
        //    .ConfigureAwait(false).GetAwaiter().GetResult();

        //_browserContext = _browser.NewContextAsync(new BrowserNewContextOptions
        //{
        //    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36",
        //    JavaScriptEnabled = true,
        //}).ConfigureAwait(false).GetAwaiter().GetResult();

        //_browserContext.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
        //{
        //    ["Cookie"] = "cf_clearance=eYWd.b3y3btNz9OJ61CwuijALppiIFAzKRnYuYSf6xk-1744105376-1.2.1.1-qKqFYF6Gn34R2NmUB8X9dCw_UVtaPzJSg1nXgcugL084CnbxFlujVd5O.QDL1.G2jxsR_lHjMq35fnfC9tRgoAG9j_DHB_h2EtZCguR.cAbEOnuJZyYTXkAvRbDETCqRBVqKMoQv1WKmsp2ak_.rrGfeES3SvVTxWW2fhLkn6NR8dk40ZV5TwMvDqx8j4jCN7SLyzld7Wj0EHLN4bb_9Q6goqa5.KFaND4snrkCEbwhBAv418OcUd4K2X8wxDI6b2clGBq1S3ruas04MO_VE4_yHf_yZN.QaeSVwCENtqTS2BayLb5SZEro6fmD.L1Zn4XkqvsvZW7YyxY7moTZ7Aks7oqEiLhzAOWhRtyMKBDXnBGgVvsShU6evuqdb91le"
        //}).ConfigureAwait(false).GetAwaiter().GetResult();

        Load(url).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private async Task Load(string url)
    {
        //var page = await _browserContext.NewPageAsync();

        //await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        //// Optional: Wait more to pass challenge
        //// Wait for visible chapter text to appear
        //await page.WaitForTimeoutAsync(15000); // wait 5 sec

        //var content = await page.ContentAsync();

        var content = await Start(url);

        document.LoadHtml(content);
    }

    private async Task LoadAsync(string url)
    {
        //var page = await _browserContext.NewPageAsync();
        //await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        //// Optional: Wait more to pass challenge
        //await page.WaitForTimeoutAsync(15000); // wait 5 sec
        //var content = await page.ContentAsync();

        var content = await Start(url);

        document.LoadHtml(content);
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

    public override List<string> ScrapeContent()
    {
        ValidateDocument();

        return document.DocumentNode
            .SelectSingleNode("//div[@class='txt']")
            .ChildNodes.Where(i => i.Name == "p" && i.InnerText != ForbiddenSequences)
            .Select(x => NormalizeHtmlEntities(x.InnerText))
            .ToList();
    }

    public override string ScrapeTitle()
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

    public async Task<string> Start(string url)
    {

        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36");
        request.Headers.Add("Cookie", "cf_clearance=fJy8MuW516NIjiYBhMLucGfc8Ra_qxJCS6pCzVTTpS0-1744107281-1.2.1.1-GUAxpaXIg7X_9ifiRTYOfbRFDVa7WuQ4.5zdCXzA6G63dkNo3p0p8uwiknPDbHqJfWbiHinBuKc73s2KUqeXdB6UF0RPYHfd2SylIg7tYS1vRsFmhztXv4u_sswbSBzpMcMw2r9lf.2sXmFgOBePBhpEe9TMmHqCmO4eDGA3NjChKKuwA6.k9iitU916st9AMjtDrJZWblM_ZEEFfIWxyPVd8kcIfxbbHymWVjQRnM9eOFQnmt6LAb5vFCSCgz6OBqDN5l.j2ayO6swvMb3MJWzGSF45sx04rJbhtaM455UCks0HdqI8LwU5wQvvYh5a3D__CfdwMR9yBDVI3dVKJ4_LYCnTKufmLJGx9_P97hwNU7bCqSKxf.Wsg_3uiQ87");
        
        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();

    }
    private static string NormalizeHtmlEntities(string input)
    {
        return WebUtility.HtmlDecode(input);
    }
}