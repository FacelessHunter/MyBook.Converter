using System.IO;

namespace DataConvertor.Extensions
{
    /// <summary>
    /// Extension methods for working with streams
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Converts a Stream to a MemoryStream
        /// </summary>
        /// <param name="stream">The stream to convert</param>
        /// <returns>A MemoryStream containing the same data</returns>
        public static MemoryStream ToMemoryStream(this Stream stream)
        {
            var memoryStream = new MemoryStream();
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
} 