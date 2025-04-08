namespace DataScraper.Utilities;

internal class NetworkHelpers
{
    internal static async Task<MemoryStream> DownloadAsStream(string url)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            var networkStream = await httpClient.GetStreamAsync(url);

            var memoryStream = new MemoryStream();
            networkStream.CopyTo(memoryStream);
            return memoryStream;
        }
    }
}
