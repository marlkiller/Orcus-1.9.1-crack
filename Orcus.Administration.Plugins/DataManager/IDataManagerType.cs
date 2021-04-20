using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using Orcus.Plugins;
using Orcus.Shared.Commands.DataManager;

namespace Orcus.Administration.Plugins.DataManager
{
    /// <summary>
    ///     Data type for the Data Manager
    /// </summary>
    public interface IDataManagerType
    {
        /// <summary>
        ///     Set to true if the data can be downloaded
        /// </summary>
        bool CanDownload { get; }

        /// <summary>
        ///     The name which data type visible for the user
        /// </summary>
        string TypeId { get; }

        /// <summary>
        ///     Set to true if multiple data entries can be opened at the same time in one data viewer
        /// </summary>
        bool SupportsMultipleEntries { get; }

        /// <summary>
        ///     Set to true if the data is viewable in a data viewer
        /// </summary>
        bool IsDataViewable { get; }

        /// <summary>
        ///     The guid of the data type. This must match the guid given in the <see cref="IDatabaseConnection" /> as
        ///     <see cref="DataMode" /> (<see cref="DataMode.Guid" />)
        /// </summary>
        Guid DataTypeGuid { get; }

        /// <summary>
        ///     Get the file extension of the data entry. This is redundant if <see cref="CanDownload" /> is set to false
        /// </summary>
        /// <param name="dataEntry">The data entry</param>
        /// <returns>The file extension with a dot. Example: .html, .exe, etc.</returns>
        string GetFileExtension(DataEntry dataEntry);

        /// <summary>
        ///     Make some last modifications to the file which was downloaded. This is redundant if <see cref="CanDownload" /> is
        ///     set to false
        /// </summary>
        /// <param name="fileName">The path to the downloaded file</param>
        void ModifyDownloadedFile(string fileName);

        /// <summary>
        ///     Get an icon for the data entry
        /// </summary>
        /// <param name="dataEntry">The data entry</param>
        /// <returns>The icon for the given data entry</returns>
        ImageSource GetIconForEntry(DataEntry dataEntry);

        /// <summary>
        ///     Give the possibility to make changes to the <see cref="DataEntry" /> before it gets displayed
        /// </summary>
        /// <param name="dataEntry"></param>
        void ChangeEntryData(DataEntry dataEntry);

        /// <summary>
        ///     Get the data viewer for one entry. This is redundant if <see cref="IsDataViewable" /> is set to false
        /// </summary>
        /// <param name="dataEntry">The data entry to display</param>
        /// <param name="dataConnection">The data connection allows to download the entry's data</param>
        /// <returns>The data viewer for the given entry</returns>
        Task<DataViewer> GetDataViewer(DataEntry dataEntry, IDataConnection dataConnection);

        /// <summary>
        ///     Get the data viewer for one entry. This is redundant if <see cref="IsDataViewable" /> or
        ///     <see cref="SupportsMultipleEntries" /> is set to false
        /// </summary>
        /// <param name="dataEntries">The data entries to display</param>
        /// <param name="dataConnection">The data connection allows to download the data of the entries</param>
        /// <returns>The data viewer for the given entries</returns>
        Task<DataViewer> GetDataViewer(List<DataEntry> dataEntries, IDataConnection dataConnection);
    }
}