using System;
using System.Collections.Generic;
using System.Linq;

namespace Orcus.Shared.Commands.ClipboardManager
{
    [Serializable]
    public class ClipboardInfo
    {
        public DateTime Timestamp { get; set; }
        public ClipboardData Data { get; set; }
    }

    [Serializable]
    public class ClipboardData : ICloneable
    {
        public ClipboardData(ClipboardFormat clipboardFormat)
        {
            ClipboardFormat = clipboardFormat;
        }

        public ClipboardFormat ClipboardFormat { get; set; }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public virtual ClipboardData Clone()
        {
            return new ClipboardData(ClipboardFormat);
        }
    }

    [Serializable]
    public class StringClipboardData : ClipboardData
    {
        public StringClipboardData(string text, ClipboardFormat clipboardFormat) : base(clipboardFormat)
        {
            Text = text;
        }

        public string Text { get; set; }

        public override ClipboardData Clone()
        {
            return new StringClipboardData(Text, ClipboardFormat);
        }
    }

    [Serializable]
    public class StringListClipboardData : ClipboardData
    {
        public StringListClipboardData(List<StringListEntry> stringList, ClipboardFormat clipboardFormat) : base(clipboardFormat)
        {
            StringList = stringList;
        }

        public List<StringListEntry> StringList { get; set; }

        public override ClipboardData Clone()
        {
            return new StringListClipboardData(StringList.ToList(), ClipboardFormat);
        }
    }

    [Serializable]
    public class StringListEntry
    {
        public string Value { get; set; }   
    }

    [Serializable]
    public class ImageClipboardData : ClipboardData
    {
        public ImageClipboardData(byte[] bitmapData, int originalWidth, int originalHeight, ClipboardFormat clipboardFormat) : base(clipboardFormat)
        {
            BitmapData = bitmapData;
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
        }

        public byte[] BitmapData { get; set; }
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }

        public override ClipboardData Clone()
        {
            return new ImageClipboardData(BitmapData, OriginalWidth, OriginalHeight, ClipboardFormat);
        }
    }
}