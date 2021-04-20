using System.IO;

namespace Orcus.Shared.Data
{
    /// <summary>
    ///     A class providing unmerged data which can be efficiently converted without the need of a temporary buffer
    /// </summary>
    public interface IDataInfo
    {
        /// <summary>
        ///     The full length provided by this <see cref="IDataInfo" />
        /// </summary>
        int Length { get; }

        /// <summary>
        ///     Merge the data into one single array
        /// </summary>
        /// <returns>Return the array of data provided by this object</returns>
        byte[] ToArray();

        /// <summary>
        ///     Write the data into an already existing buffer
        /// </summary>
        /// <param name="buffer">The buffer which should receive the data</param>
        /// <param name="index">The position the data should be written to</param>
        void WriteToBuffer(byte[] buffer, int index);

        /// <summary>
        ///     Write the data into a stream
        /// </summary>
        /// <param name="outStream">The stream which takes the data</param>
        void WriteIntoStream(Stream outStream);
    }
}