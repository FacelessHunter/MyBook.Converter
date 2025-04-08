using DataScraper.Scrapers;
using SharedModels.Models;
using System.Text.Json;

//ScraperEngine scraper = new NovelliveScraper("https://novellive.com/book/blood-warlock-succubus-partner-in-the-apocalypse-novel");
ScraperEngine scraper = new NovelliveScraper("https://novellive.com/book/lewd-apocalypse");
//ScraperEngine scraper = new NovelliveScraper("https://novellive.app/book/super-gene/chapter-1");
Console.WriteLine("Collecting started");
var bookName = scraper.ScrapeBookName();
var coverPage = await scraper.LoadCoverPageAsync();

var book = new Book(bookName);
book.SetCoverPage(coverPage);

await scraper.GetToFirstChapter();

var i = 1;
do
{
    var title = scraper.ScrapeChapterTitle();
    var content = scraper.ScrapeChapterContent();

    var chapter = new Chapter(title, content);
    book.AddChapter(i, chapter);
    Console.WriteLine($"{title} added");
    i++;
}
while (await scraper.NextChapterAsync());

var serializedBook = JsonSerializer.Serialize(book, options: new JsonSerializerOptions() 
{ 
    WriteIndented = true
});

var normalizedBookName = bookName
    .Where(i => char.IsLetterOrDigit(i) || i == ' ')
    .Aggregate(string.Empty, (acc, c) => acc + c);

var fileLocation = string.Format("%USERPROFILE%/ScrapedBooks/{0}_{1}.json", normalizedBookName, DateTime.UtcNow.ToString("yyyy_MM_dd__HH_mm"));

var outputLocation = Environment.ExpandEnvironmentVariables(fileLocation);

File.WriteAllText(outputLocation, serializedBook);

Console.WriteLine();
Console.WriteLine("Book name: {0}", bookName);
Console.WriteLine("Book saved in {0}", outputLocation);
Console.WriteLine("Done");