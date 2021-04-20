using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using Orcus.Native;
using Orcus.Shared.Commands.ClipboardManager;

namespace Orcus.Utilities
{
    /// <summary>
    ///     Provides notifications when the contents of the clipboard is updated. This class is thread safe.
    /// </summary>
    public sealed class ClipboardNotification
    {
        private static OnClipboardChangeEventHandler _clipboardChangeEventHandler;
        private static readonly object EventLock = new object();

        /// <summary>
        ///     The change event delegate
        /// </summary>
        /// <param name="format">The format of the clipboard data</param>
        /// <param name="data">The data in the clipboard</param>
        public delegate void OnClipboardChangeEventHandler(ClipboardFormat? format, IDataObject data);

        private static NotificationForm _form;
        private static readonly string[] Formats = Enum.GetNames(typeof(ClipboardFormat));

        /// <summary>
        ///     Occurs when the contents of the clipboard is updated.
        /// </summary>
        public static event OnClipboardChangeEventHandler ClipboardUpdate
        {
            add
            {
                lock (EventLock)
                {
                    if (_clipboardChangeEventHandler == null)
                    {
                        var staThread = new Thread(() => Application.Run(new NotificationForm()));
                        staThread.SetApartmentState(ApartmentState.STA);
                        staThread.Start();
                    }

                    _clipboardChangeEventHandler += value;
                }
            }
            remove
            {
                lock (EventLock)
                {
                    _clipboardChangeEventHandler -= value;
                    if (_clipboardChangeEventHandler == null)
                    {
                        _form.Invoke(new Action(() => _form.Close()));
                        _form.Dispose();
                        _form = null;
                    }
                }
            }
        }

        /// <summary>
        ///     Raises the <see cref="ClipboardUpdate" /> event.
        /// </summary>
        /// <param name="e">Event arguments for the event.</param>
        private static void OnClipboardUpdate(EventArgs e)
        {
            var dataObject = Clipboard.GetDataObject();

            if (dataObject == null)
            {
                _clipboardChangeEventHandler?.Invoke(null, null);
                return;
            }

            var format = GetClipboardFormat(dataObject);

            if (format == null)
            {
                _clipboardChangeEventHandler?.Invoke(null, null);
                return;
            }

            //important for thread safety
            var handler = _clipboardChangeEventHandler;
            handler?.Invoke(format, dataObject);
        }

        public static ClipboardFormat? GetClipboardFormat(IDataObject dataObject)
        {
            foreach (var f in Formats)
            {
                if (dataObject.GetDataPresent(f))
                {
                    return (ClipboardFormat) Enum.Parse(typeof(ClipboardFormat), f);
                }
            }

            return null;
        }

        /// <summary>
        ///     Hidden form to receive the WM_CLIPBOARDUPDATE message.
        /// </summary>
        private class NotificationForm : Form
        {
            private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

            public NotificationForm()
            {
                NativeMethods.SetParent(Handle, HWND_MESSAGE);
                NativeMethods.AddClipboardFormatListener(Handle);
            }

            protected override void SetVisibleCore(bool value)
            {
                _form = this;

                base.SetVisibleCore(false);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == (int) WM.CLIPBOARDUPDATE)
                {
                    OnClipboardUpdate(null);
                }
                base.WndProc(ref m);
            }

            protected override void OnClosing(CancelEventArgs e)
            {
                base.OnClosing(e);
                NativeMethods.RemoveClipboardFormatListener(Handle);
            }
        }
    }

    public class ClipboardChangedEventArgs
    {
        
    }
}