using System.Net.Http.Headers;
using System.Text.Json;

public class ImageEnhancer
{
    private static readonly string apiUrl = "http://127.0.0.1:7860/sdapi/v1/extra-single-image";

    public static async Task<Stream> UpscaleImageAsync(Stream imageStream, string upscaler = "ESRGAN_4x", int scale = 2)
    {
        if (imageStream == null || !imageStream.CanRead)
            throw new ArgumentException("Input stream is null or unreadable.");

        // Convert stream to byte array and then base64
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        byte[] imageBytes = memoryStream.ToArray();
        string base64Image = Convert.ToBase64String(imageBytes);

        // Build the JSON payload
        var payload = new
        {
            image = base64Image,
            upscaling_resize = 0, // required to avoid conflict
            upscaling_resize_w = "1600",
            upscaling_resize_h = "2560",
            upscaler_1 = upscaler
        };

        // Make the HTTP call
        using var client = new HttpClient();
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await client.PostAsync(apiUrl, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API Error {response.StatusCode}: {responseBody}");
        }

        // Parse and return the upscaled image stream
        using var doc = JsonDocument.Parse(responseBody);
        string upscaledBase64 = doc.RootElement.GetProperty("image").GetString();
        byte[] upscaledBytes = Convert.FromBase64String(upscaledBase64);

        return new MemoryStream(upscaledBytes);
    }


    public static async Task<Stream> UpscaleAndEnhanceImageAsync(Stream imageStream, int width = 1600, int height = 2560,
        string firstUpscaler = "R-ESRGAN 4x+", string qualityEnhancer = "4x_foolhardy_Remacri")
    {
        if (imageStream == null || !imageStream.CanRead)
            throw new ArgumentException("Input stream is null or unreadable.");
        imageStream.Seek(0, SeekOrigin.Begin);
        // Convert stream to base64
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        byte[] originalBytes = memoryStream.ToArray();
        string base64Original = Convert.ToBase64String(originalBytes);

        // Step 1: Upscale to target size
        var upscaledBase64 = await SendToWebUIAsync(base64Original, width, height, firstUpscaler);

        // Step 2: Enhance using another upscaler (same size, just cleaner)
        var enhancedBase64 = await SendToWebUIAsync(upscaledBase64, width, height, qualityEnhancer);

        // Return final enhanced stream
        byte[] finalBytes = Convert.FromBase64String(enhancedBase64);
        return new MemoryStream(finalBytes);
    }

    private static async Task<string> SendToWebUIAsync(string base64Image, int width, int height, string upscaler)
    {
        var payload = new
        {
            image = base64Image,
            //upscaling_resize = 0,
            upscaling_resize_w = width,
            upscaling_resize_h = height,
            upscaler_1 = upscaler
        };

        using var client = new HttpClient();
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await client.PostAsync(apiUrl, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API Error {response.StatusCode}: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        return doc.RootElement.GetProperty("image").GetString();
    }
}
