using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sorzus.Wpf.Toolkit.Images;
using Size = System.Drawing.Size;

namespace Sorzus.Wpf.Toolkit.Extensions
{
    /// <summary>
    ///     Get an icon from a Windows dll
    /// </summary>
    /// <example>
    ///     <code>&lt;Image Source="{extensions:WindowsIcons LibraryName=imageres.dll, IconId=11, Size=20}" /></code>
    /// </example>
    public class WindowsIcons : MarkupExtension
    {
        private static readonly List<StoredIcon> StoredIcons = new List<StoredIcon>();
        private static readonly object StoreLock = new object();

        public string LibraryName { get; set; }
        public int IconId { get; set; }
        public int Size { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            lock (StoreLock)
            {
                var storedIcon = StoredIcons.FirstOrDefault(x => x.IconId == IconId && x.Size == Size);
                if (storedIcon != null)
                    return storedIcon.ImageSource;

                using (
                    var extractor =
                        new IconExtractor(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
                            LibraryName)))
                using (var icon = extractor.GetIconAt(IconId))
                {
                    var info = new IconInfo(icon);
                    var index = info.GetBestFitIconIndex(new Size(Size, Size));
                    using (var bestIcon = info.Images[index])
                    {
                        var imageSource = Imaging.CreateBitmapSourceFromHIcon(
                            bestIcon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        StoredIcons.Add(new StoredIcon {IconId = IconId, ImageSource = imageSource, Size = Size});
                        return imageSource;
                    }
                }
            }
        }
    }

    internal class StoredIcon
    {
        public ImageSource ImageSource { get; set; }
        public int Size { get; set; }
        public int IconId { get; set; }
    }
}