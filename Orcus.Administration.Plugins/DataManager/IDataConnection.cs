using System.Collections.Generic;
using System.Threading.Tasks;
using Orcus.Administration.Plugins.Properties;
using Orcus.Shared.Commands.DataManager;

namespace Orcus.Administration.Plugins.DataManager
{
    /// <summary>
    ///     Download <see cref="DataEntry" /> from the server
    /// </summary>
    public interface IDataConnection
    {
        /// <summary>
        ///     Download one <see cref="DataEntry" /> from the server
        /// </summary>
        /// <param name="dataEntry">The data entry to download</param>
        /// <returns>The data of the <see cref="DataEntry" />. Warning: Can be null if the entry was not found</returns>
        [CanBeNull]
        Task<byte[]> DownloadEntry(DataEntry dataEntry);

        /// <summary>
        ///     Download multiple <see cref="DataEntry" />
        /// </summary>
        /// <param name="dataEntries">The data entries to download</param>
        /// <returns>
        ///     The data of the data entries. Warning: all entries which were not found on the server won't exist in the
        ///     dicitionary
        /// </returns>
        Task<Dictionary<DataEntry, byte[]>> DownloadEntries(IEnumerable<DataEntry> dataEntries);
    }
}