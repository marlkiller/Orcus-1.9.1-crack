using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Plugins.DataManager;
using Orcus.Plugins;
using Orcus.Shared.Commands.DataManager;

namespace Orcus.Administration.ViewModels.DataManager.DataTypes
{
    public class FileManagerFile : IDataManagerType
    {
        public bool CanDownload { get; } = true;
        public string GetFileExtension(DataEntry dataEntry)
        {
            return Path.GetExtension(dataEntry.EntryName);
        }

        public void ModifyDownloadedFile(string fileName)
        {
        }

        public string TypeId { get; } = (string) Application.Current.Resources["File"];
        public bool SupportsMultipleEntries { get; } = false;
        public bool IsDataViewable { get; } = false;
        public Guid DataTypeGuid { get; } = DataMode.File.Guid;

        public ImageSource GetIconForEntry(DataEntry dataEntry)
        {
            return new BitmapImage(new Uri("/Resources/Images/File.png", UriKind.Relative));
        }

        public void ChangeEntryData(DataEntry dataEntry)
        {
        }

        public Task<DataViewer> GetDataViewer(DataEntry dataEntry, IDataConnection dataConnection)
        {
            throw new NotImplementedException();
        }

        public Task<DataViewer> GetDataViewer(List<DataEntry> dataEntries, IDataConnection dataConnection)
        {
            throw new NotImplementedException();
        }
    }
}