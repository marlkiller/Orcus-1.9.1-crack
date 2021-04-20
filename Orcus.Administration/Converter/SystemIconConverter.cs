using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Orcus.Administration.Native;
using Orcus.Shared.Commands.MessageBox;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (string), typeof (BitmapSource))]
    public class SystemIconConverter : IValueConverter
    {
        public object Convert(object value, Type type, object parameter, CultureInfo culture)
        {
            var messageBoxIcon = (SystemIcon) value;
            if (messageBoxIcon == SystemIcon.None)
                return null;

            SHSTOCKICONID iconId;
            switch (messageBoxIcon)
            {
                case SystemIcon.Error:
                    iconId = SHSTOCKICONID.SIID_ERROR;
                    break;
                case SystemIcon.Question:
                    iconId = SHSTOCKICONID.SIID_HELP;
                    break;
                case SystemIcon.Warning:
                    iconId = SHSTOCKICONID.SIID_WARNING;
                    break;
                case SystemIcon.Info:
                    iconId = SHSTOCKICONID.SIID_INFO;
                    break;
                default:
                    return null;
            }

            var sii = new SHSTOCKICONINFO {cbSize = (uint) Marshal.SizeOf(typeof (SHSTOCKICONINFO))};

            Marshal.ThrowExceptionForHR(NativeMethods.SHGetStockIconInfo(iconId,
                SHGSI.SHGSI_ICON,
                ref sii));

            Icon icon = Icon.FromHandle(sii.hIcon);
            var bs = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            NativeMethods.DestroyIcon(sii.hIcon);
            return bs;
        }

        public object ConvertBack(object value, Type type, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}