namespace MyBook.Writer.Core.Models
{
    /// <summary>
    /// Represents a book cover image
    /// </summary>
    public class CoverImage
    {
        /// <summary>
        /// Image file name
        /// </summary>
        public string FileName { get; set; } = "cover.jpg";

        /// <summary>
        /// MIME content type
        /// </summary>
        public string ContentType { get; set; } = "image/jpeg";

        /// <summary>
        /// Raw image data
        /// </summary>
        public MemoryStream Data { get; set; } = new MemoryStream();

        /// <summary>
        /// Create a cover image from a stream
        /// </summary>
        /// <param name="stream">Source image data stream</param>
        /// <param name="fileName">Image file name</param>
        /// <param name="contentType">MIME content type</param>
        /// <returns>CoverImage instance</returns>
        public static async Task<CoverImage> FromStreamAsync(Stream stream, string fileName, string contentType)
        {
            var cover = new CoverImage
            {
                FileName = fileName,
                ContentType = contentType,
                Data = new MemoryStream()
            };

            stream.Position = 0;
            await stream.CopyToAsync(cover.Data);
            cover.Data.Position = 0;

            return cover;
        }

        /// <summary>
        /// Get a copy of the image data stream
        /// </summary>
        /// <returns>A new memory stream with the image data</returns>
        public MemoryStream GetDataCopy()
        {
            var copyStream = new MemoryStream();
            Data.Position = 0;
            Data.CopyTo(copyStream);
            copyStream.Position = 0;
            return copyStream;
        }
    }
}