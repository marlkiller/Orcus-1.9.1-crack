using System;
using System.Windows.Media;
using Orcus.Administration.Plugins.DataManager;
using Orcus.Shared.Commands.DataManager;
using Sorzus.Wpf.Toolkit.Converter;

namespace Orcus.Administration.ViewModels.DataManager
{
    public class ViewData : DataEntry
    {
        private ImageSource _icon;

        public ViewData(DataEntry dataEntry)
        {
            Size = dataEntry.Size;
            DataType = dataEntry.DataType;
            Id = dataEntry.Id;
            EntryName = dataEntry.EntryName;
            ClientId = dataEntry.ClientId;
            Timestamp = dataEntry.Timestamp.ToLocalTime();
        }

        public ImageSource Icon => _icon ?? (_icon = DataManagerType?.GetIconForEntry(this));
        public IDataManagerType DataManagerType { get; set; }

        public string FormattedSize => DataType == new Guid("8AA38175-F8E5-45BD-AC6E-F541DE91D753")
            ? $"{Size} passwords"
            : FormatBytesConverter.BytesToString(Size);
    }
}