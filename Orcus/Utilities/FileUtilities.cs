using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Orcus.Extensions;

namespace Orcus.Utilities
{
    public static class FileUtilities
    {
        public static byte[] GetIconFromProcess(string filename)
        {
            var icon = Icon.ExtractAssociatedIcon(filename);
            if (icon == null)
                return null;

            using (icon)
            using (var img = icon.ToBitmap().ResizeImage(20, 20))
            {
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }
    }
}