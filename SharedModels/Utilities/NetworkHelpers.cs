namespace SharedModels.Utilities;

public static class NetworkHelpers
{
    public static async Task<MemoryStream> DownloadAsStream(string url)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            var networkStream = await httpClient.GetStreamAsync(url);

            var memoryStream = new MemoryStream();
            networkStream.CopyTo(memoryStream);
            return memoryStream;
        }
    }

    public static EncodedImage ConvertToSerializableImage(this Stream stream, string fileName)
    {
        stream.Seek(0, SeekOrigin.Begin);

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        memoryStream.Seek(0, SeekOrigin.Begin);

        var base64 = Convert.ToBase64String(memoryStream.ToArray());

        string contentType = Path.GetExtension(fileName).ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };

        return new EncodedImage
        {
            FileName = fileName,
            ContentType = contentType,
            Base64Data = base64
        };
    }

    public static Stream ConvertToStream(this EncodedImage coverImage)
    {
        var imageBytes = Convert.FromBase64String(coverImage.Base64Data);
        return new MemoryStream(imageBytes);
    }
    public static MemoryStream ToMemoryStream(this Stream input)
    {
        if (input is MemoryStream ms)
        {
            ms.Position = 0;
            return ms;
        }

        var memoryStream = new MemoryStream();
        input.CopyTo(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
}
