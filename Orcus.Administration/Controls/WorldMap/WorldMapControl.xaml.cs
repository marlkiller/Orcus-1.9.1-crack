using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.ViewModels.ClientMap;
using Orcus.Shared.Connection;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace Orcus.Administration.Controls.WorldMap
{
    /// <summary>
    ///     Interaction logic for WorldMapControl.xaml
    /// </summary>
    public partial class WorldMapControl : INotifyPropertyChanged
    {
        public static readonly DependencyProperty MarkersProperty = DependencyProperty.Register(
            "Markers", typeof (IEnumerable<ClientMarker>), typeof (WorldMapControl),
            new PropertyMetadata(default(IEnumerable<ClientMarker>), PropertyChangedCallback));

        public static readonly DependencyProperty RenderOfflineClientsProperty = DependencyProperty.Register(
            "RenderOfflineClients", typeof (bool), typeof (WorldMapControl), new PropertyMetadata(default(bool), RenderOfflinePropertyChangedCallback));

        private readonly ToolTip _markerToolTip;
        private ImageSource _currentMapImage;
        private ClientLocation _currentMarker;
        private Point? _lastCenterPositionOnTarget;
        private Point? _lastDragPoint;
        private Point? _lastMousePositionOnTarget;
        private Bitmap _mapBitmap;

        public WorldMapControl()
        {
            InitializeComponent();
            ZoomViewer.ScrollChanged += ZoomViewer_ScrollChanged;
            ZoomViewer.MouseLeftButtonUp += ZoomViewer_MouseLeftButtonUp;
            ZoomViewer.PreviewMouseLeftButtonUp += ZoomViewerOnPreviewMouseLeftButtonUp;
            ZoomViewer.PreviewMouseWheel += ZoomViewer_PreviewMouseWheel;
            ZoomViewer.PreviewMouseLeftButtonDown += ZoomViewer_PreviewMouseLeftButtonDown;
            ZoomViewer.MouseMove += ZoomViewer_MouseMove;
            ZoomSlider.ValueChanged += ZoomSliderOnValueChanged;

            SizeChanged += PerformanceMapControl_SizeChanged;

            _markerToolTip = (ToolTip) Resources["MarkerToolTip"];
        }

        public IEnumerable<ClientMarker> Markers
        {
            get { return (IEnumerable<ClientMarker>) GetValue(MarkersProperty); }
            set { SetValue(MarkersProperty, value); }
        }

        public bool RenderOfflineClients
        {
            get { return (bool)GetValue(RenderOfflineClientsProperty); }
            set { SetValue(RenderOfflineClientsProperty, value); }
        }

        public ImageSource CurrentMapImage
        {
            get { return _currentMapImage; }
            private set
            {
                _currentMapImage = value;
                OnPropertyChanged(nameof(CurrentMapImage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void PerformanceMapControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth/2 < ActualHeight)
            {
                ZoomGrid.Width = ActualHeight*2;
                ZoomGrid.Height = ActualHeight;
            }
            else
            {
                ZoomGrid.Width = ActualWidth;
                ZoomGrid.Height = ActualWidth/2;
            }
        }

        private void ZoomViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_lastDragPoint.HasValue)
            {
                var posNow = e.GetPosition(ZoomViewer);

                double dX = posNow.X - _lastDragPoint.Value.X;
                double dY = posNow.Y - _lastDragPoint.Value.Y;

                _lastDragPoint = posNow;

                ZoomViewer.ScrollToHorizontalOffset(ZoomViewer.HorizontalOffset - dX);
                ZoomViewer.ScrollToVerticalOffset(ZoomViewer.VerticalOffset - dY);
            }
        }

        private void ZoomViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(ZoomViewer);
            if (mousePos.X <= ZoomViewer.ViewportWidth && mousePos.Y < ZoomViewer.ViewportHeight)
                //make sure we still can use the scrollbars
            {
                ZoomViewer.Cursor = Cursors.SizeAll;
                _lastDragPoint = mousePos;
                Mouse.Capture(ZoomViewer);
            }
        }

        private void ZoomViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _lastMousePositionOnTarget = Mouse.GetPosition(ZoomGrid);

            if (e.Delta > 0)
            {
                ZoomSlider.Value += .5;
            }
            if (e.Delta < 0)
            {
                ZoomSlider.Value -= .5;
            }

            e.Handled = true;
        }

        private void ZoomViewerOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ZoomViewer.Cursor = Cursors.Arrow;
            ZoomViewer.ReleaseMouseCapture();
            _lastDragPoint = null;
        }

        private void ZoomSliderOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ZoomTransform.ScaleX = e.NewValue;
            ZoomTransform.ScaleY = e.NewValue;

            var centerOfViewport = new Point(ZoomViewer.ViewportWidth/2, ZoomViewer.ViewportHeight/2);
            _lastCenterPositionOnTarget = ZoomViewer.TranslatePoint(centerOfViewport, ZoomGrid);
        }

        private void ZoomViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!_lastMousePositionOnTarget.HasValue)
                {
                    if (_lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(ZoomViewer.ViewportWidth/2, ZoomViewer.ViewportHeight/2);
                        Point centerOfTargetNow = ZoomViewer.TranslatePoint(centerOfViewport, ZoomGrid);

                        targetBefore = _lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = _lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(ZoomGrid);

                    _lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth/ZoomGrid.Width;
                    double multiplicatorY = e.ExtentHeight/ZoomGrid.Height;

                    double newOffsetX = ZoomViewer.HorizontalOffset - dXInTargetPixels*multiplicatorX;
                    double newOffsetY = ZoomViewer.VerticalOffset - dYInTargetPixels*multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    ZoomViewer.ScrollToHorizontalOffset(newOffsetX);
                    ZoomViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }

        private void ZoomViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ZoomViewer.Cursor = Cursors.Arrow;
            ZoomViewer.ReleaseMouseCapture();
            _lastDragPoint = null;
        }

        private void Render()
        {
            Render(Markers);
        }

        private void Render(IEnumerable<ClientMarker> mapLocations)
        {
            _mapBitmap?.Dispose();

            var worldMapBitmapImage =
                new BitmapImage(
                    new Uri(
                        "pack://application:,,,/Orcus.Administration.Resources;component/Images/world-map-black.jpg",
                        UriKind.Absolute));

            if (mapLocations == null)
            {
                CurrentMapImage = worldMapBitmapImage;
                _mapBitmap = BitmapConverter.BitmapImage2Bitmap(worldMapBitmapImage);
                return;
            }

            var worldMap = BitmapConverter.BitmapImage2Bitmap(worldMapBitmapImage);

            var offlineComputer =
                BitmapConverter.BitmapImage2Bitmap(
                    new BitmapImage(
                        new Uri(
                            "pack://application:,,,/Orcus.Administration;component/Resources/Images/OfflineComputer.png",
                            UriKind.Absolute)));
            var onlineComputer =
                BitmapConverter.BitmapImage2Bitmap(
                    new BitmapImage(
                        new Uri(
                            "pack://application:,,,/Orcus.Administration;component/Resources/Images/OnlineComputer.png",
                            UriKind.Absolute)));

            using (var g = Graphics.FromImage(worldMap))
            {
                foreach (var clientLocation in mapLocations.OrderBy(x => x.Client.IsOnline))
                {
                    if (!clientLocation.Client.IsOnline && !RenderOfflineClients)
                        continue;
                    g.DrawImage(clientLocation.Client.IsOnline ? onlineComputer : offlineComputer,
                        new RectangleF(
                            new PointF((float) worldMap.Width/(180*2)*(clientLocation.Longitude + 180) - 8,
                                worldMap.Height - (float) worldMap.Height/(90*2)*(clientLocation.Latitude + 90) - 8),
                            new SizeF(16, 16)));
                }
            }

            offlineComputer.Dispose();
            onlineComputer.Dispose();

            CurrentMapImage = BitmapConverter.ToBitmapSource(worldMap);
            _mapBitmap = worldMap;
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var userControl = dependencyObject as WorldMapControl;
            userControl?.Render((IEnumerable<ClientMarker>) dependencyPropertyChangedEventArgs.NewValue);
        }

        private static void RenderOfflinePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var userControl = dependencyObject as WorldMapControl;
            userControl?.Render();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void WorldMap_MouseMove(object sender, MouseEventArgs e)
        {
            var imageControl = (Image) sender;
            var position = e.GetPosition(imageControl);
            var lon = position.X/imageControl.ActualWidth*360 - 180;
            var lat = (position.Y/imageControl.ActualHeight - 1)*-1*180 - 90;

            foreach (var marker in Markers)
            {
                if (marker.Longitude > lon - 1 && marker.Longitude < lon + 1)
                {
                    if (marker.Latitude > lat - 1 && marker.Latitude < lat + 1)
                    {
                        if (marker != _currentMarker)
                        {
                            _currentMarker = marker;
                            _markerToolTip.Placement = PlacementMode.Relative;
                            _markerToolTip.PlacementTarget = imageControl;
                            _markerToolTip.DataContext = marker;
                            _markerToolTip.IsOpen = true;
                        }

                        if (_currentMarker != null)
                        {
                            _markerToolTip.HorizontalOffset = position.X + 5;
                            _markerToolTip.VerticalOffset = position.Y + 5;
                        }
                        //ZoomViewer.Cursor = Cursors.Hand;
                        return;
                    }
                }
            }

            //ZoomViewer.Cursor = Cursors.Arrow;
            _markerToolTip.IsOpen = false;
            _currentMarker = null;
        }

        private void SaveWorldMap_OnClick(object sender, RoutedEventArgs e)
        {
            if (_mapBitmap == null)
                return;

            var sfd = new SaveFileDialog
            {
                Filter = "JPEG Image|*.jpg|PNG Image|*.png|GIF Image|*.gif|Bitmap Image|*.bmp",
                AddExtension = true
            };

            if (sfd.ShowDialog(Application.Current.MainWindow) == true)
            {
                switch (sfd.FilterIndex)
                {
                    case 1:
                        _mapBitmap.Save(sfd.FileName, ImageFormat.Jpeg);
                        break;
                    case 2:
                        _mapBitmap.Save(sfd.FileName, ImageFormat.Png);
                        break;
                    case 3:
                        _mapBitmap.Save(sfd.FileName, ImageFormat.Gif);
                        break;
                    case 4:
                        _mapBitmap.Save(sfd.FileName, ImageFormat.Bmp);
                        break;
                }
            }
        }
    }
}