using SharedModels;

namespace DataScraper.Scrapers;

public abstract class ScraperEngine
{
    /// <summary>
    /// Download CoverPage.
    /// </summary>
    /// <returns></returns>
    public abstract Task<EncodedImage> LoadCoverPageAsync();

    /// <summary>
    /// Load first chapter.
    /// </summary>
    /// <returns></returns>
    public abstract Task<bool> GetToFirstChapter();

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
    /// Scrape title from loaded web page.
    /// </summary>
    /// <returns></returns>
    public abstract string ScrapeChapterTitle();

    /// <summary>
    /// Scrape content from loaded web page.
    /// </summary>
    /// <returns></returns>
    public abstract List<string> ScrapeChapterContent();

    /// <summary>
    /// Scrape book name from loaded web page.
    /// </summary>
    /// <returns></returns>
    public abstract string ScrapeBookName();
}