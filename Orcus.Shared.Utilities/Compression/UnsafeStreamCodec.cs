using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Size = System.Drawing.Size;

namespace Orcus.Shared.Utilities.Compression
{
    public class UnsafeStreamCodec : IStreamCodec
    {
        //every 2 seconds
        protected const int FullImageTime = 2000;

        private readonly bool _sendFullImage;
        private readonly bool _keepCompressor;
        private readonly Stopwatch _fullImageStopwatch;
        private int _stride;
        private byte[] _encodeBuffer;
        private readonly Size _checkBlock;
        private WriteableBitmap _decodedBitmap;

        public UnsafeStreamCodec(IImageCompression imageCompression, UnsafeStreamCodecParameters parameters)
        {
            _sendFullImage = (parameters & UnsafeStreamCodecParameters.UpdateImageEveryTwoSeconds) ==
                             UnsafeStreamCodecParameters.UpdateImageEveryTwoSeconds;
            _keepCompressor = (parameters & UnsafeStreamCodecParameters.DontDisposeImageCompressor) ==
                              UnsafeStreamCodecParameters.DontDisposeImageCompressor;

            if (_sendFullImage)
                _fullImageStopwatch = Stopwatch.StartNew();
            _checkBlock = new Size(50, 1);
            ImageCompression = imageCompression;
            ImageQuality = 70;
        }

        public UnsafeStreamCodec(UnsafeStreamCodecParameters parameters) : this(new JpgCompression(70), parameters)
        {
        }

        public int ImageQuality
        {
            get { return ImageCompression.Quality; }
            set { ImageCompression.Quality = value; }
        }

        public CodecOption CodecOptions { get; } = CodecOption.AutoDispose | CodecOption.HasBuffers |
                                                   CodecOption.RequireSameSize;

        public IImageCompression ImageCompression { get; }

        protected virtual unsafe RemoteDesktopDataInfo GetFullImageData(IntPtr scan0, Size imageSize, int stride, PixelFormat pixelFormat, int rawLength)
        {
            //just send the full image
            byte[] data;
            if ((ImageCompression.CompressionMode & CompressionMode.ByteArray) == CompressionMode.ByteArray)
            {
                data = ImageCompression.Compress(scan0, stride, imageSize, pixelFormat);
            }
            else if ((ImageCompression.CompressionMode & CompressionMode.Stream) == CompressionMode.Stream)
            {
                using (var ms = new MemoryStream(_encodeBuffer.Length / 200))
                {
                    ImageCompression.Compress(scan0, stride, imageSize, pixelFormat, ms);
                    data = ms.ToArray();
                }
            }
            else
                throw new NotSupportedException(ImageCompression.CompressionMode.ToString());

            //Copy the image data to our byte array
            fixed (byte* ptr = _encodeBuffer)
                CoreMemoryApi.memcpy(new IntPtr(ptr), scan0, (UIntPtr) rawLength);

            return new RemoteDesktopDataInfo(data,
                new FrameInfo(new Rectangle(0, 0, imageSize.Width, imageSize.Height), FrameFlags.UpdatedRegion),
                new HeaderInfo(ImageMetadata.FullImage, pixelFormat, imageSize.Width, imageSize.Height));
        }

        public unsafe RemoteDesktopDataInfo CodeImage(IntPtr scan0, Rectangle scanArea, Size imageSize, PixelFormat pixelFormat)
        {
            var pixelSize = GetPixelSize(pixelFormat);
            var stride = pixelSize * imageSize.Width;
            var rawLength = stride*imageSize.Height;

            if (_encodeBuffer == null || (_sendFullImage && _fullImageStopwatch.ElapsedMilliseconds > FullImageTime))
            {
                _encodeBuffer = new byte[rawLength];
                _stride = stride;

                return GetFullImageData(scan0, imageSize, stride, pixelFormat, rawLength);
            }

            if (stride != _stride)
                throw new InvalidOperationException("Image size is not equal to previous Bitmap");

            //the block lines which need an update
            var blocks = new List<Rectangle>();

            //the current stride
            var s = new Size(scanArea.Width, _checkBlock.Height);

            //the last size to scan
            var lastSize = new Size(scanArea.Width%_checkBlock.Width, scanArea.Height%_checkBlock.Height);

            var lastY = scanArea.Height + scanArea.Y - lastSize.Height;
            var lastX = scanArea.Width + scanArea.X - lastSize.Width;

            //different blocks but all blocks have the static width of scanArea.Width

            var finalUpdates = new List<Rectangle>();

            byte* pScan0 = (byte*) scan0.ToPointer();

            fixed (byte* encBuffer = _encodeBuffer)
            {
                int index;

                //loop until the end of the scan area is reached (y)
                Rectangle cBlock;
                for (int y = scanArea.Y; scanArea.Height + scanArea.Y >= y;)
                {
                    if (y == lastY)
                        s = new Size(scanArea.Width, lastSize.Height);

                    if (s.Height == 0)
                        break;

                    cBlock = new Rectangle(scanArea.X, y, scanArea.Width, s.Height);
                    var offset = y * stride + scanArea.X * pixelSize;

                    //if the byte arrays are different
                    if (
                        CoreMemoryApi.memcmp(encBuffer + offset, pScan0 + offset, (UIntPtr) (scanArea.Width * pixelSize)) !=
                        IntPtr.Zero)
                    {
                        //get the last block index
                        index = blocks.Count - 1;
                        //if the last block index is directly above the current one, we just add the stride to it
                        if (blocks.Count != 0 && blocks[index].Y + blocks[index].Height == cBlock.Y)
                        {
                            cBlock = new Rectangle(blocks[index].X, blocks[index].Y, blocks[index].Width,
                                blocks[index].Height + cBlock.Height);
                            blocks[index] = cBlock;
                        }
                        else
                        {
                            //else we just add a new block
                            blocks.Add(cBlock);
                        }
                    }

                    y += s.Height;
                }

                //we go through all blocks
                for (var i = 0; i < blocks.Count; i++)
                {
                    //we set the height to the current block height but the width to the check block width
                    s = new Size(_checkBlock.Width, blocks[i].Height);
                    var x = scanArea.X;

                    //we loop in steps of checkblock.width to the end of the line
                    while (x < (scanArea.Width + scanArea.X))
                    {
                        if (x == lastX)
                            s = new Size(lastSize.Width, blocks[i].Height);

                        //the block has the height of the saved block but the width of the check block
                        cBlock = new Rectangle(x, blocks[i].Y, s.Width, blocks[i].Height);
                        bool foundChanges = false;
                        int blockStride = pixelSize * cBlock.Width;

                        //we loop throught the strides of this block
                        for (int j = 0; j < cBlock.Height; j++)
                        {
                            int blockOffset = stride * (cBlock.Y + j) + pixelSize * cBlock.X;
                            if (
                                CoreMemoryApi.memcmp(encBuffer + blockOffset, pScan0 + blockOffset,
                                    (UIntPtr) blockStride) != IntPtr.Zero)
                                foundChanges = true;

                            //we copy always because if foundChanges = true we have to invalidate the full block
                            CoreMemoryApi.memcpy(encBuffer + blockOffset, pScan0 + blockOffset, (UIntPtr) blockStride);
                        }

                        if (foundChanges)
                        {
                            index = finalUpdates.Count - 1;

                            //if the last addded block ends where this block begins
                            if (finalUpdates.Count > 0 && finalUpdates[index].X + finalUpdates[index].Width == cBlock.X)
                            {
                                //we just add the width
                                Rectangle rect = finalUpdates[index];
                                int newWidth = cBlock.Width + rect.Width;
                                cBlock = new Rectangle(rect.X, rect.Y, newWidth, rect.Height);
                                finalUpdates[index] = cBlock;
                            }
                            else
                            {
                                //else add a new block
                                finalUpdates.Add(cBlock);
                            }
                        }
                        x += s.Width;
                    }
                }

                return WriteChanges(finalUpdates.ToArray(), null, pixelSize, encBuffer, pixelFormat, imageSize, stride);
            }
        }

        public unsafe RemoteDesktopDataInfo CodeImage(IntPtr scan0, Rectangle[] updatedAreas, MovedRegion[] movedRegions, Size imageSize, PixelFormat pixelFormat)
        {
            var pixelSize = GetPixelSize(pixelFormat);
            var stride = pixelSize * imageSize.Width;
            var rawLength = stride * imageSize.Height;

            if (_encodeBuffer == null || (_sendFullImage && _fullImageStopwatch.ElapsedMilliseconds > FullImageTime))
            {
                _encodeBuffer = new byte[rawLength];
                _stride = stride;

                return GetFullImageData(scan0, imageSize, stride, pixelFormat, rawLength);
            }

            if (stride != _stride)
                throw new InvalidOperationException("Image size is not equal to previous Bitmap");

            return WriteChanges(updatedAreas, movedRegions, pixelSize, (byte*) scan0, pixelFormat, imageSize, stride);
        }

        protected virtual unsafe RemoteDesktopDataInfo WriteChanges(Rectangle[] changedAreas, MovedRegion[] movedRegions,
            int pixelSize, byte* imageBuffer, PixelFormat pixelFormat, Size imageSize, int imageStride)
        {
            MemoryStream memoryStream = null;
            byte[][] dataArrays = null;
            var changedAreasLength = changedAreas?.Length ?? 0;
            var movedRegionsLength = movedRegions?.Length ?? 0;
            FrameInfo[] frames = null;

            if ((ImageCompression.CompressionMode & CompressionMode.Stream) == CompressionMode.Stream)
            {
                memoryStream = new MemoryStream(_encodeBuffer.Length/200);
                RemoteDesktopDataInfo.WriteHeader(pixelFormat, ImageMetadata.Frames, imageSize.Width, imageSize.Height, memoryStream);
            }
            else if ((ImageCompression.CompressionMode & CompressionMode.ByteArray) == CompressionMode.ByteArray)
            {
                dataArrays = new byte[changedAreasLength + movedRegionsLength][];
                frames = new FrameInfo[changedAreasLength + movedRegionsLength];
            }
            else
                throw new NotSupportedException(ImageCompression.CompressionMode.ToString());

            if (movedRegions != null)
                for (int i = 0; i < movedRegions.Length; i++)
                {
                    var movedRegion = movedRegions[i];

                    if (memoryStream != null)
                    {
                        RemoteDesktopDataInfo.WriteFrameInfo(movedRegion.Destination, 8, FrameFlags.MovedRegion, memoryStream);

                        memoryStream.Write(BitConverter.GetBytes(movedRegion.Source.X), 0, 4);
                        memoryStream.Write(BitConverter.GetBytes(movedRegion.Source.Y), 0, 4);
                    }
                    else
                    {
                        var data = new byte[8];
                        Buffer.BlockCopy(BitConverter.GetBytes(movedRegion.Source.X), 0, data, 0, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(movedRegion.Source.Y), 0, data, 4, 4);

                        dataArrays[i] = data;
                        frames[i] = new FrameInfo(movedRegion.Destination, FrameFlags.MovedRegion);
                    }
                }

            if (changedAreas != null)
                for (int i = 0; i < changedAreas.Length; i++)
                {
                    var blockArea = changedAreas[i];
                    var blockStride = blockArea.Width*pixelSize;
                    var blockPointer = Marshal.AllocHGlobal(blockStride*blockArea.Height);

                    try
                    {
                        //build bitmap in memory
                        for (int j = 0, offset = 0; j < blockArea.Height; j++)
                        {
                            int blockOffset = imageStride*(blockArea.Y + j) + pixelSize*blockArea.X;
                            CoreMemoryApi.memcpy(blockPointer.Add(offset), (IntPtr) (imageBuffer + blockOffset),
                                (UIntPtr) blockStride);
                            offset += blockStride;
                        }

                        if (memoryStream != null)
                        {
                            RemoteDesktopDataInfo.WriteFrameInfo(blockArea, 0, FrameFlags.UpdatedRegion, memoryStream);
                            var framePos = memoryStream.Position;
                            ImageCompression.Compress(blockPointer, blockStride, blockArea.Size, pixelFormat,
                                memoryStream);

                            var currentPos = memoryStream.Position;
                            memoryStream.Position = framePos - 4;
                            memoryStream.Write(BitConverter.GetBytes(currentPos - framePos), 0, 4);
                            memoryStream.Position = currentPos;
                        }
                        else
                        {
                            dataArrays[i + movedRegionsLength] = ImageCompression.Compress(blockPointer, blockStride,
                                blockArea.Size, pixelFormat);
                            frames[i + movedRegionsLength] = new FrameInfo(blockArea, FrameFlags.UpdatedRegion);
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(blockPointer);
                    }
                }

            if (memoryStream != null)
            {
                using (memoryStream)
                    return new RemoteDesktopDataInfo(memoryStream.ToArray());
            }

            return new RemoteDesktopDataInfo(dataArrays, frames,
                new HeaderInfo(ImageMetadata.Frames, pixelFormat, imageSize.Width, imageSize.Height));
        }

        public unsafe IModifiedDecoder AppendModifier<T>(T writeableBitmapModifierTask) where T : IWriteableBitmapModifierTask
        {
            return new UnsafeStreamModifiedDecoder(writeableBitmapModifierTask, DecodeData);
        }

        public unsafe WriteableBitmap DecodeData(byte* codecBuffer, uint length, Dispatcher dispatcher)
        {
            return DecodeData(codecBuffer, length, dispatcher, null);
        }

        protected unsafe WriteableBitmap DecodeData(byte* codecBuffer, uint length, Dispatcher dispatcher, IEnumerable<IWriteableBitmapModifierTask> modifierTasks)
        {
            if (length < 20)
                return _decodedBitmap;

            var modifyingTasks = modifierTasks?.ToArray();
            if (modifyingTasks?.Length == 0)
                modifyingTasks = null;

            ImageMetadata metadata;
            PixelFormat pixelFormat;
            int width;
            int height;

            var position = RemoteDesktopDataInfo.ReadHeader(codecBuffer, out metadata, out pixelFormat, out width, out height);
            var pixelSize = GetPixelSize(pixelFormat);
            IntPtr backBuffer = IntPtr.Zero;
            int backBufferStride = 0;

            if (_decodedBitmap == null)
                dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    _decodedBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
                    _decodedBitmap.Lock();
                    backBuffer = _decodedBitmap.BackBuffer;
                    backBufferStride = _decodedBitmap.BackBufferStride;
                })).Wait();
            else
                dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    _decodedBitmap.Lock();
                    backBuffer = _decodedBitmap.BackBuffer;
                    backBufferStride = _decodedBitmap.BackBufferStride;
                })).Wait();

            if (backBufferStride != width * pixelSize)
                throw new InvalidOperationException("Invalid stride");

            if ((metadata & ImageMetadata.FullImage) == ImageMetadata.FullImage)
            {
                Rectangle frameRectangle;
                int frameLength;
                FrameFlags frameFlags;
                position += RemoteDesktopDataInfo.ReadFrameInfo(codecBuffer + position, out frameFlags, out frameRectangle, out frameLength);

                var changedAreas = new List<Int32Rect>();

                if (modifyingTasks != null)
                    foreach (var modifierTask in modifyingTasks)
                        modifierTask.PreProcessing(backBuffer, backBufferStride, pixelSize, new Size(width, height),
                            changedAreas);

                if ((ImageCompression.DecompressionMode & DecompressionMode.Pointer) == DecompressionMode.Pointer)
                {
                    ImageCompression.Decompress((IntPtr) (codecBuffer + position), (uint) (backBufferStride*height),
                        backBuffer, backBufferStride*height, pixelFormat);
                }
                else if ((ImageCompression.DecompressionMode & DecompressionMode.Bitmap) == DecompressionMode.Bitmap)
                {
                    using (var memoryStream = new UnmanagedMemoryStream(codecBuffer+ position, frameLength))
                    using (var tempImage = (Bitmap) Image.FromStream(memoryStream))
                    {
                        var dataLock = tempImage.LockBits(new Rectangle(0, 0, tempImage.Width, tempImage.Height),
                            ImageLockMode.ReadOnly, pixelFormat);

                        CoreMemoryApi.memcpy(backBuffer, dataLock.Scan0, (UIntPtr) (backBufferStride*height));

                        tempImage.UnlockBits(dataLock);
                    }
                }
                else
                    throw new NotSupportedException(ImageCompression.DecompressionMode.ToString());

                changedAreas.Add(new Int32Rect(0, 0, width, height));

                if (modifyingTasks != null)
                    foreach (var modifierTask in modifyingTasks)
                        modifierTask.PostProcessing(backBuffer, backBufferStride, pixelSize, new Size(width, height),
                            changedAreas);

                dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    foreach (var changedArea in changedAreas)
                        _decodedBitmap.AddDirtyRect(changedArea);

                    _decodedBitmap.Unlock();
                })).Wait();
            }
            else if ((metadata & ImageMetadata.Frames) == ImageMetadata.Frames)
            {
                var changedAreas = new List<Int32Rect>();

                if (modifyingTasks != null)
                    foreach (var modifierTask in modifyingTasks)
                        modifierTask.PreProcessing(backBuffer, backBufferStride, pixelSize, new Size(width, height), changedAreas);

                while (position != length)
                {
                    Rectangle rectangle;
                    int frameLength;
                    FrameFlags frameFlags;
                    position += RemoteDesktopDataInfo.ReadFrameInfo(codecBuffer + position, out frameFlags, out rectangle, out frameLength);

                    if ((frameFlags & FrameFlags.UpdatedRegion) == FrameFlags.UpdatedRegion)
                    {
                        if ((ImageCompression.DecompressionMode & DecompressionMode.ByteArray) == DecompressionMode.ByteArray)
                        {
                            var decompressedData = ImageCompression.Decompress((IntPtr)(codecBuffer + position), (uint)frameLength, pixelFormat);
                            fixed (byte* decompressedPtr = decompressedData)
                            {
                                ReplaceRectangle((byte*) backBuffer, backBufferStride, pixelSize, rectangle,
                                    decompressedPtr, 0);
                            }

                            changedAreas.Add(new Int32Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height));
                            position += frameLength;
                        }
                        else if ((ImageCompression.DecompressionMode & DecompressionMode.Bitmap) == DecompressionMode.Bitmap)
                        {
                            using (var memoryStream = new UnmanagedMemoryStream(codecBuffer + position, frameLength))
                            using (var tempImage = (Bitmap) Image.FromStream(memoryStream))
                            {
                                var dataLock = tempImage.LockBits(
                                    new Rectangle(0, 0, tempImage.Width, tempImage.Height), ImageLockMode.ReadOnly,
                                    PixelFormat.Format32bppArgb);

                                ReplaceRectangle((byte*) backBuffer, backBufferStride, pixelSize, rectangle,
                                    (byte*) dataLock.Scan0, dataLock.Stride * dataLock.Height);

                                tempImage.UnlockBits(dataLock);
                            }

                            changedAreas.Add(new Int32Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height));
                            position += frameLength;
                        }
                        else
                            throw new NotSupportedException(ImageCompression.DecompressionMode.ToString());
                    }
                    else if ((frameFlags & FrameFlags.MovedRegion) == FrameFlags.MovedRegion)
                    {
                        var sourceX = *(int*) (codecBuffer + position);
                        var sourceY = *(int*) (codecBuffer + position + 4);
                        var rectangleStride = (UIntPtr) (rectangle.Width*pixelSize);

                        Debug.Print("Moved region. Rectangle: " + rectangle + " Source: " + sourceX + ", " + sourceY);

                        //only moved in x, we need a temp buffer
                        if (sourceY == rectangle.Y)
                        {
                            for (int i = rectangle.Height; i >= 0; i--)
                            {
                                var sourcePtr = (byte*) backBuffer + backBufferStride*(sourceY + i) + pixelSize*sourceX;
                                var destPtr = (byte*) backBuffer + backBufferStride*(rectangle.Y + i) +
                                              pixelSize*rectangle.X;

                                CoreMemoryApi.memmove(destPtr, sourcePtr, rectangleStride);
                            }
                        }
                        else
                        {
                            //moved to bottom
                            if (sourceY < rectangle.Y)
                            {
                                for (int i = rectangle.Height; i >= 0; i--)
                                {
                                    var sourcePtr = (byte*) backBuffer + backBufferStride*(sourceY + i) +
                                                    pixelSize*sourceX;
                                    var destPtr = (byte*) backBuffer + backBufferStride*(rectangle.Y + i) +
                                                  pixelSize*rectangle.X;

                                    CoreMemoryApi.memmove(destPtr, sourcePtr, rectangleStride);
                                }
                            }
                            else //moved to top
                            {
                                for (int i = 0; i < rectangle.Height; i++)
                                {
                                    var sourcePtr = (byte*) backBuffer + backBufferStride*(sourceY + i) +
                                                    pixelSize*sourceX;
                                    var destPtr = (byte*) backBuffer + backBufferStride*(rectangle.Y + i) +
                                                  pixelSize*rectangle.X;

                                    CoreMemoryApi.memmove(destPtr, sourcePtr, rectangleStride);
                                }
                            }
                        }
                        changedAreas.Add(rectangle.ToInt32Rect());

                        position += 8;
                    }
                }

                if(modifyingTasks != null)
                    foreach (var modifierTask in modifyingTasks)
                        modifierTask.PostProcessing(backBuffer, backBufferStride, pixelSize, new Size(width, height),
                            changedAreas);

                dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    foreach (var changedArea in changedAreas)
                        _decodedBitmap.AddDirtyRect(changedArea);

                    _decodedBitmap.Unlock();
                })).Wait();
            }

            return _decodedBitmap;
        }

        protected static unsafe void ReplaceRectangle(byte* imagePtr, int imageStride, int pixelSize, Rectangle updatedArea, byte* dataBuffer, int dataBufferLen)
        {
            var rectangleStride = updatedArea.Width*pixelSize;

            for (int i = 0; i < updatedArea.Height; i++)
            {
                var positionImage = (updatedArea.Y + i)*imageStride + updatedArea.X*pixelSize;
                CoreMemoryApi.memcpy(imagePtr + positionImage, dataBuffer + i*rectangleStride, (UIntPtr) rectangleStride);
            }
        }

        internal static int GetPixelSize(PixelFormat pixelFormat)
        {
            //return Image.GetPixelFormatSize(pixelFormat) / 8;
            switch (pixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                    return 3;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return 4;
                default:
                    throw new NotSupportedException(pixelFormat.ToString());
            }
        }

        protected static System.Windows.Media.PixelFormat ConvertToPixelFormats(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                    return PixelFormats.Bgr24;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return PixelFormats.Bgr32;
                default:
                    throw new NotSupportedException(pixelFormat.ToString());
            }
        }

        public void Dispose()
        {
            if (!_keepCompressor)
                ImageCompression.Dispose();
        }
    }
}