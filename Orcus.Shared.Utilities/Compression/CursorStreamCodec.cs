using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using Size = System.Drawing.Size;

namespace Orcus.Shared.Utilities.Compression
{
    public class CursorStreamCodec : ICursorStreamCodec
    {
        private const int HeaderLength = 21;
        private CursorInfo _cursorInfo;
        private bool _cursorUpdated;
        private ImageInfo _imageInfo;
        protected Bitmap CursorImage;
        protected MemoryStream CursorMemoryStream;
        protected PixelFormat CursorPixelFormat;
        protected Int32Rect ModifiedAreaRectangle;
        protected bool RestoreUnmodifiedImage;
        protected byte[] UnmodifiedImage;

        public void Dispose()
        {
            CursorImage?.Dispose();
            CursorMemoryStream?.Dispose();
        }

        public void UpdateCursorInfo(int x, int y, bool visible)
        {
            _cursorInfo = new CursorInfo {X = x, Y = y, Visible = visible};
        }

        public void UpdateCursorImage(IntPtr data, int stride, int width, int height, PixelFormat pixelFormat)
        {
            if (CursorImage?.Width == width && CursorImage.Height == height && CursorImage.PixelFormat == pixelFormat)
            {
                var lockBits = CursorImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                    pixelFormat);

                try
                {
                    if (CoreMemoryApi.memcmp(lockBits.Scan0, data, (UIntPtr) (stride * height)) == IntPtr.Zero)
                        return;
                }
                finally
                {
                    CursorImage.UnlockBits(lockBits);
                }
            }

            CursorImage = (Bitmap) new Bitmap(width, height, stride, pixelFormat, data).Clone();
            if (pixelFormat != PixelFormat.Format32bppArgb)
            {
                var oldImage = CursorImage;
                CursorImage = new Bitmap(CursorImage.Width, CursorImage.Height, PixelFormat.Format32bppArgb);
                using (var graphics = Graphics.FromImage(CursorImage))
                    graphics.DrawImageUnscaled(oldImage, 0, 0);
                oldImage.Dispose();
            }

            _imageInfo = new ImageInfo {Width = width, Height = height, PixelFormat = PixelFormat.Format32bppArgb };
            _cursorUpdated = true;
        }

        public void UpdateCursorImage(Bitmap bitmap)
        {
            if (CursorImage == null)
            {
                CursorImage = bitmap;
            }
            else
            {
                if (CursorImage.Width == bitmap.Width && CursorImage.Height == bitmap.Height &&
                    CursorImage.PixelFormat == bitmap.PixelFormat)
                {
                    var newImageLock = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly, bitmap.PixelFormat);
                    var currentImageLock = CursorImage.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly, bitmap.PixelFormat);

                    bool imagesEqual = false;
                    if (newImageLock.Stride == currentImageLock.Stride)
                    {
                        var imageSize = newImageLock.Stride * newImageLock.Height;

                        imagesEqual =
                            CoreMemoryApi.memcmp(newImageLock.Scan0, currentImageLock.Scan0, (UIntPtr) imageSize) ==
                            IntPtr.Zero;
                    }

                    bitmap.UnlockBits(newImageLock);
                    CursorImage.UnlockBits(currentImageLock);

                    if (imagesEqual)
                    {
                        bitmap.Dispose();
                        return;
                    }
                }

                CursorImage = bitmap;
            }

            _imageInfo = new ImageInfo {Width = bitmap.Width, Height = bitmap.Height, PixelFormat = bitmap.PixelFormat};
            _cursorUpdated = true;
        }

        public byte[] CodeCursor()
        {
            if (_cursorUpdated)
            {
                using (var ms = new MemoryStream(CursorImage.Width * 4 * CursorImage.Height))
                {
                    var header = new byte[HeaderLength];
                    WriteHeader(header, 0, _cursorInfo, _imageInfo);
                    ms.Write(header, 0, header.Length);
                    CursorImage.Save(ms, ImageFormat.Png);

                    return ms.ToArray();
                }
            }

            var packet = new byte[HeaderLength];
            WriteHeader(packet, 0, _cursorInfo, _imageInfo);
            return packet;
        }

        public IWriteableBitmapModifierTask CreateModifierTask(byte[] packet, int index, int length)
        {
            return new CursorWriteableBitmapModifierTask(packet, index, length, this);
        }

        protected unsafe void CopyPixels(byte* source, byte* destination, int count)
        {
            for (int i = 0; i < count; i++)
                destination[i] = source[i];
        }

        private static void WriteHeader(byte[] data, int index, CursorInfo cursorInfo, ImageInfo imageInfo)
        {
            data[index] = cursorInfo.Visible ? (byte) 1 : (byte) 0;
            Buffer.BlockCopy(BitConverter.GetBytes(cursorInfo.X), 0, data, index + 1, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(cursorInfo.Y), 0, data, index + 5, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(imageInfo.Width), 0, data, index + 9, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(imageInfo.Height), 0, data, index + 13, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((int) imageInfo.PixelFormat), 0, data, index + 17, 4);
        }

        private static void ReadHeader(byte[] data, int index, out CursorInfo cursorInfo, out ImageInfo imageInfo)
        {
            cursorInfo = new CursorInfo
            {
                Visible = data[index] == 1,
                X = BitConverter.ToInt32(data, index + 1),
                Y = BitConverter.ToInt32(data, index + 5)
            };

            imageInfo = new ImageInfo
            {
                Width = BitConverter.ToInt32(data, index + 9),
                Height = BitConverter.ToInt32(data, index + 13),
                PixelFormat = (PixelFormat) BitConverter.ToInt32(data, index + 17)
            };
        }

        public bool HasCursorImage => CursorImage != null;

        private class CursorWriteableBitmapModifierTask : IWriteableBitmapModifierTask
        {
            private readonly CursorStreamCodec _cursorStreamCodec;
            private readonly int _index;
            private readonly int _length;
            private readonly byte[] _packet;

            public CursorWriteableBitmapModifierTask(byte[] packet, int index, int length,
                CursorStreamCodec cursorStreamCodec)
            {
                _packet = packet;
                _index = index;
                _length = length;
                _cursorStreamCodec = cursorStreamCodec;
            }

            /* private unsafe void RestoreImage(List<Int32Rect> updatedAreas, byte* backBuffer, int stride, int pixelSize)
             {
                 if (_cursorStreamCodec.UnmodifiedAreaData == null)
                     return;
 
                 //we dont replace the area when it was updated
                 foreach (var updatedArea in updatedAreas)
                 {
                     if (updatedArea.Contains(_cursorStreamCodec.ModifiedAreaRectangle))
                         return;
                 }
 
                 var rectangle = _cursorStreamCodec.ModifiedAreaRectangle;
                 var rectangleStride = rectangle.Width * pixelSize;
 
                 //copy unmodified data to backbuffer
                 fixed (byte* unmodifiedData = _cursorStreamCodec.UnmodifiedAreaData)
                 {
                     for (int i = 0; i < rectangle.Height; i++)
                     {
                         NativeMethods.memcpy(backBuffer + (rectangle.Y + i) * stride + rectangle.X * pixelSize,
                             unmodifiedData + i * rectangleStride, (UIntPtr) rectangleStride);
                     }
                 }
 
                 updatedAreas.Add(rectangle);
                 _cursorStreamCodec.UnmodifiedAreaData = null;
             }*/

            public unsafe void PreProcessing(IntPtr backBuffer, int stride, int pixelSize, Size size,
                List<Int32Rect> updatedAreas)
            {
                if (_cursorStreamCodec.RestoreUnmodifiedImage)
                {
                    fixed (byte* unmodifiedImagePtr = _cursorStreamCodec.UnmodifiedImage)
                    {
                        CoreMemoryApi.memcpy((byte*) backBuffer, unmodifiedImagePtr,
                            (UIntPtr) _cursorStreamCodec.UnmodifiedImage.Length);
                    }
                    updatedAreas.Add(_cursorStreamCodec.ModifiedAreaRectangle);
                }
            }

            public unsafe void PostProcessing(IntPtr backBuffer, int stride, int pixelSize, Size size,
                List<Int32Rect> updatedAreas)
            {
                CursorInfo cursorInfo;
                ImageInfo imageInfo;

                ReadHeader(_packet, _index, out cursorInfo, out imageInfo);

                var backBufferPtr = (byte*) backBuffer;

                if (_length > HeaderLength)
                {
                    _cursorStreamCodec.CursorImage?.Dispose();
                    _cursorStreamCodec.CursorMemoryStream?.Dispose();

                    _cursorStreamCodec.CursorPixelFormat = imageInfo.PixelFormat;
                    _cursorStreamCodec.CursorMemoryStream = new MemoryStream(_packet, _index + HeaderLength,
                        _length - HeaderLength);
                    _cursorStreamCodec.CursorImage = (Bitmap) Image.FromStream(_cursorStreamCodec.CursorMemoryStream);
                }

                if (cursorInfo.Visible)
                {
                    var cursorRect = new Int32Rect(cursorInfo.X, cursorInfo.Y,
                        Math.Min(imageInfo.Width, size.Width - cursorInfo.X),
                        Math.Min(imageInfo.Height, size.Height - cursorInfo.Y));
                    _cursorStreamCodec.ModifiedAreaRectangle = cursorRect;

                    //backup original image without cursor so we can restore it in pre processing
                    var imageLength = stride * size.Height;
                    if (_cursorStreamCodec.UnmodifiedImage?.Length != imageLength)
                        _cursorStreamCodec.UnmodifiedImage = new byte[imageLength];

                    fixed (byte* unmodifiedPtr = _cursorStreamCodec.UnmodifiedImage)
                        CoreMemoryApi.memcpy(unmodifiedPtr, (byte*) backBuffer, (UIntPtr) imageLength);

                    using (var bitmap = new Bitmap(imageInfo.Width, imageInfo.Height, imageInfo.PixelFormat))
                    {
                        var lockBits = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                            ImageLockMode.ReadWrite, bitmap.PixelFormat);

                        WriteArea(cursorRect, backBufferPtr, stride, pixelSize, (byte*) lockBits.Scan0);
                        bitmap.UnlockBits(lockBits);

                        using (var graphics = Graphics.FromImage(bitmap))
                        {
                            graphics.DrawImageUnscaled(_cursorStreamCodec.CursorImage, new System.Drawing.Point(0, 0));
                        }

                        lockBits = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                            ImageLockMode.ReadWrite, bitmap.PixelFormat);

                        var imagePtr = (byte*) lockBits.Scan0;
                        var rectangleStride = cursorRect.Width * pixelSize;

                        for (int i = 0; i < cursorRect.Height; i++)
                        {
                            CoreMemoryApi.memcpy(backBufferPtr + (i + cursorRect.Y) * stride + cursorRect.X * pixelSize,
                                imagePtr + i * lockBits.Stride,
                                (UIntPtr) rectangleStride);
                        }

                        bitmap.UnlockBits(lockBits);

                        updatedAreas.Add(cursorRect);
                        _cursorStreamCodec.ModifiedAreaRectangle = cursorRect;
                        _cursorStreamCodec.RestoreUnmodifiedImage = true;
                    }
                }
                else
                    _cursorStreamCodec.RestoreUnmodifiedImage = false;
            }

            /* public unsafe void Modify(IntPtr backBuffer, int stride, int pixelSize, Size size, List<Int32Rect> updatedAreas)
             {
                 if (_length < HeaderLength)
                     return;
 
                 CursorInfo cursorInfo;
                 ImageInfo imageInfo;
 
                 ReadHeader(_packet, _index, out cursorInfo, out imageInfo);
 
                 var cursorStride = imageInfo.Width * UnsafeStreamCodec.GetPixelSize(imageInfo.PixelFormat);
                 var backBufferPtr = (byte*) backBuffer;
 
                 if (_length > HeaderLength)
                 {
                     _cursorStreamCodec.CursorPixelFormat = imageInfo.PixelFormat;
                     fixed (byte* packetPointer = _packet)
                     {
                         if ((_cursorStreamCodec.ImageCompression.DecompressionMode & DecompressionMode.ByteArray) ==
                             DecompressionMode.ByteArray)
                         {
                             var buffer =
                                 _cursorStreamCodec.ImageCompression.Decompress(
                                     (IntPtr) (packetPointer + _index + HeaderLength),
                                     (uint) (_length - HeaderLength), imageInfo.PixelFormat);
 
                             fixed (byte* bufferPtr = buffer)
                                 using (
                                     var bitmap = new Bitmap(imageInfo.Width, imageInfo.Height, cursorStride,
                                         imageInfo.PixelFormat, (IntPtr) bufferPtr))
                                 {
                                     _cursorStreamCodec.CursorImage =
                                         bitmap.Clone(new Rectangle(0, 0, imageInfo.Width, imageInfo.Height),
                                             imageInfo.PixelFormat);
                                 }
                         }
                         else if ((_cursorStreamCodec.ImageCompression.DecompressionMode & DecompressionMode.Bitmap) ==
                                  DecompressionMode.Bitmap)
                         {
                             byte[] temp = new byte[_length - HeaderLength];
                             fixed (byte* tempPtr = temp)
                                 NativeMethods.memcpy(tempPtr, packetPointer + _index + HeaderLength,
                                     (UIntPtr) temp.Length);
 
                             using (var memoryStream = new MemoryStream(temp))
                             {
                                 _cursorStreamCodec.CursorImage = (Bitmap) Image.FromStream(memoryStream).Clone();
                             }
                         }
                         else
                             throw new NotSupportedException(
                                 _cursorStreamCodec.ImageCompression.DecompressionMode.ToString());
                     }
                 }
 
                 RestoreImage(updatedAreas, backBufferPtr, stride, pixelSize);
                 if (cursorInfo.Visible && _cursorStreamCodec.CursorImage != null)
                 {
                     var cursorRect = new Int32Rect(cursorInfo.X, cursorInfo.Y, imageInfo.Width,
                         imageInfo.Height);
 
                     _cursorStreamCodec.ModifiedAreaRectangle = cursorRect;
                     _cursorStreamCodec.UnmodifiedAreaData = new byte[pixelSize * imageInfo.Width * imageInfo.Height];
 
                     fixed (byte* bufferPtr = _cursorStreamCodec.UnmodifiedAreaData)
                         WriteArea(cursorRect, backBufferPtr, stride, pixelSize, bufferPtr);
 
                     using (var bitmap = new Bitmap(imageInfo.Width, imageInfo.Height, imageInfo.PixelFormat))
                     {
                         var lockBits = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                             ImageLockMode.ReadWrite, bitmap.PixelFormat);
 
                         WriteArea(cursorRect, backBufferPtr, stride, pixelSize, (byte*) lockBits.Scan0);
                         bitmap.UnlockBits(lockBits);
 
                         using (var graphics = Graphics.FromImage(bitmap))
                         {
                             graphics.DrawImageUnscaled(_cursorStreamCodec.CursorImage, new System.Drawing.Point(0, 0));
                         }
 
                         lockBits = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                             ImageLockMode.ReadWrite, bitmap.PixelFormat);
 
                         var imagePtr = (byte*) lockBits.Scan0;
                         for (int i = 0; i < cursorRect.Height; i++)
                         {
                             NativeMethods.memcpy(backBufferPtr + (i + cursorRect.Y) * stride + cursorRect.X * pixelSize,
                                 imagePtr + i * lockBits.Stride,
                                 (UIntPtr) lockBits.Stride);
                         }
 
                         bitmap.UnlockBits(lockBits);
                     }
 
                     updatedAreas.Add(cursorRect);
                 }
             }*/

            private unsafe void WriteArea(Int32Rect rectangle, byte* backBuffer, int backBufferStride, int pixelSize,
                byte* destination)
            {
                var rectangleStride = rectangle.Width * pixelSize;

                for (int i = 0; i < rectangle.Height; i++)
                {
                    CoreMemoryApi.memcpy(destination + i * rectangleStride,
                        backBuffer + (i + rectangle.Y) * backBufferStride + rectangle.X * pixelSize,
                        (UIntPtr) rectangleStride);
                }
            }
        }
    }
}