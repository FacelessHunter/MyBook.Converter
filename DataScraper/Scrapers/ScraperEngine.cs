namespace DataScraper.Scrapers;

public abstract class ScraperEngine
{
    /// <summary>
    /// Scrape title from loaded web page.
    /// </summary>
    /// <returns></returns>
    public abstract string ScrapeTitle();

    /// <summary>
    /// Scrape content from loaded web page.
    /// </summary>
    /// <returns></returns>
    public abstract List<string> ScrapeContent();

    /// <summary>
    /// Load next chapter from loaded web page.
    /// </summary>
    /// <returns></returns>
    public abstract Task<bool> NextChapterAsync();

    /// <summary>
    /// Load previous chapter from loaded web page.
    /// </summary>
    /// <returns></returns>
    public abstract Task<bool> PreviousChapterAsync();

    /// <summary>
    /// Scrape book name from loaded web page.
    /// </summary>
    /// <returns></returns>
    public abstract string ScrapeBookName();
}