using System;
using System.IO;

namespace Orcus.Shared.Utilities
{
    /// <summary>
    ///     Extensions for <see cref="Stream" />s
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        ///     Copy one stream into another
        /// </summary>
        /// <param name="input">The source stream which should be copied</param>
        /// <param name="output">The destination stream which should filled</param>
        public static void CopyToEx(this Stream input, Stream output)
        {
            byte[] buffer = new byte[32768]; // Fairly arbitrary size
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        /// <summary>
        ///     Copy one stream into another
        /// </summary>
        /// <param name="input">The source stream which should be copied</param>
        /// <param name="output">The destination stream which should filled</param>
        /// <param name="bytes">The number of bytes to copy</param>
        public static void CopyToEx(this Stream input, Stream output, int bytes)
        {
            byte[] buffer = new byte[32768];
            int read;
            while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}