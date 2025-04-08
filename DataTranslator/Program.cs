using DataTranslator.Implementations;
using DataTranslator.Interfaces;
using SharedModels.Models;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

const string separationSequence = "```0000__p_a_r_a__0000```";

var baseLocation = "%USERPROFILE%/ScrapedBooks";
var expandedBaseLocation = Environment.ExpandEnvironmentVariables(baseLocation);

var fileLocation = Path.Combine(expandedBaseLocation, "Lewd Apocalypse_2025_04_10__16_37.json");

var serializedBook = await File.ReadAllTextAsync(fileLocation);

var book = JsonSerializer.Deserialize<Book>(serializedBook);


await Parallel.ForEachAsync(book.Chapters, new ParallelOptions { MaxDegreeOfParallelism = 10}, async (chapter, cancellationToken) =>
{
    using ITranslateProvider translateProvider = new DeepLTranslateProvider();

    await translateProvider.InitializeProviderAsync();

    var title = chapter.Value.Title;

    Console.WriteLine($"Translation for {title} started");

    var translatedTitle = await translateProvider.TranslateAsync(title);
    chapter.Value.Title = translatedTitle;

    var textToTranslate = JoinParagraphsWithMarkers(chapter.Value.Paragraphs);
    try
    {
        var translatedText = await translateProvider.TranslateAsync(textToTranslate, separationSequence: separationSequence);

        var splitedTranslatedText = SplitParagraphsWithMarkers(translatedText, chapter.Value.Paragraphs.Count - 1).Values.ToList();

        chapter.Value.Paragraphs= splitedTranslatedText;

        Console.WriteLine($"{title} translated");
    }
    catch (Exception)
    {
        throw;
    }
});

var filename = fileLocation.Split('.').First();

var options = new JsonSerializerOptions
{
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

var translatedBood = JsonSerializer.Serialize(book, options);

var outputLocation = Path.Combine(expandedBaseLocation, string.Format("{0}_Translated_UA_{1}.json", filename, DateTime.UtcNow.ToString("yyyy_MM_dd__HH_mm")));

File.WriteAllText(outputLocation, translatedBood, Encoding.UTF8);

Console.WriteLine();
Console.WriteLine("Book name: {0}", filename);
Console.WriteLine("Book saved in {0}", outputLocation);
Console.WriteLine("Done");

static string JoinParagraphsWithMarkers(List<string> paragraphs)
{
    string ParagraphPrefixFormat = "\n```__p_a_r_a_{0:D4}__```\n"; // e.g., [[[PARA_0001]]]

    var builder = new StringBuilder();
    for (int i = 0; i < paragraphs.Count; i++)
    {
        var paragraph = Regex.Replace(paragraphs[i], @"[\u2000-\u206F\u2E00-\u2E7F\u3000-\u303F\uFF00-\uFFEF]", "");

        builder.AppendFormat(ParagraphPrefixFormat, i);
        builder.Append(paragraph.Replace("\n", string.Empty));
        builder.AppendFormat(ParagraphPrefixFormat, i);
        builder.Append(separationSequence); // separation marker
    }

    return builder.ToString();
}

static SortedList<int, string> SplitParagraphsWithMarkers(string text, int expectedCount)
{
    string searchPattern = @"```__p_a_r_a_(\d{4})__```";
    var parts = text.Split(separationSequence, StringSplitOptions.None)
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .ToList();

    var cleaned = new SortedList<int, string>();

    foreach (var part in parts)
    {
        // This regex extracts the number after [[[PARA_ and captures the rest of the text
        var match = Regex.Match(part, searchPattern, RegexOptions.Singleline);
        if (match.Success)
        {
            int key = int.Parse(match.Groups[1].Value);
            var content = Regex.Replace(part, searchPattern, string.Empty, RegexOptions.Singleline);
            cleaned[key] = content.Replace("\n", string.Empty);
        }
        else
        {
            throw new Exception("No Paragraph markers found");
        }
    }

    if (cleaned.Count != expectedCount)
    {
        Console.WriteLine($"⚠️ Mismatch! Expected {expectedCount}, got {cleaned.Count}.");
        //return ForceSplitByCount(text, expectedCount);
    }

    return cleaned;
}