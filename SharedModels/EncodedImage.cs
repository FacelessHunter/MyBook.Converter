namespace SharedModels
{
    public class EncodedImage
    {
        public string FileName { get; set; }           // "cover.jpg"

        public string ContentType { get; set; }        // "image/jpeg"

        public string Base64Data { get; set; }         // Actual image data
    }
}
