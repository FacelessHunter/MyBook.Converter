using DataScraper.BookWriters;
using DataScraper.Scrapers;
using DataScraper.Utilities;

ScraperEngine scraper = new NovelliveScraper("https://novellive.com/book/blood-warlock-succubus-partner-in-the-apocalypse-novel/chapter-1");
WriterEngine writer = new LocalEpubWriter();

var image = await NetworkHelpers.DownloadAsStream("https://encrypted-tbn1.gstatic.com/images?q=tbn:ANd9GcSNiypETsTrBQ3V8CqmXdUJsu5f2GvfN0PFdAci-MBKwtZiY3YI");

var bookName = scraper.ScrapeBookName();

writer.SetCoverpage(image, "coverpage.jpg", "image/jpeg");

writer.SetBookTitle(bookName);

do
{
    var title = scraper.ScrapeTitle();
    var content = scraper.ScrapeContent();

    writer.AddSection(title, content);
    Console.WriteLine($"Chapter {title} added");
}
while (await scraper.NextChapterAsync());

using var stream = await writer.SaveAsStreamAsync();

var normalizedBookName = bookName
    .Where(i => char.IsLetterOrDigit(i) || i == ' ')
    .Aggregate(string.Empty, (acc, c) => acc + c);

//var fileLocation = string.Format("./{0}.fb2", normalizedBookName);
var fileLocation = string.Format("./{0}.epub", normalizedBookName);

File.WriteAllBytes(fileLocation, stream.ToArray());

Console.WriteLine();
Console.WriteLine("Book name: {0}", bookName);
Console.WriteLine("Book saved in {0}", fileLocation);
Console.WriteLine("Done");