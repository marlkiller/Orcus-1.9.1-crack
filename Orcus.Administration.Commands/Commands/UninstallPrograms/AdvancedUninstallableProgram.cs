using System;
using System.IO;
using System.Windows.Media.Imaging;
using Orcus.Shared.Commands.UninstallPrograms;

namespace Orcus.Administration.Commands.UninstallPrograms
{
    [Serializable]
    public class AdvancedUninstallableProgram : UninstallableProgram, IDisposable
    {
        [NonSerialized] private BitmapImage _icon;

        public BitmapImage Icon
        {
            get
            {
                if (_icon != null)
                    return _icon;

                if (IconData == null)
                    return null;

                _icon = new BitmapImage();
                _icon.BeginInit();
                _icon.StreamSource = new MemoryStream(IconData);
                _icon.EndInit();
                return _icon;
            }
        }

        public void Dispose()
        {
            _icon?.Dispatcher.Invoke(() => _icon.StreamSource.Dispose());
        }
    }
}