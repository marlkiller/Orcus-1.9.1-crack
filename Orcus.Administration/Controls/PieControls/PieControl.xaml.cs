using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Orcus.Administration.Extensions;
using Orcus.Administration.ViewModels.Statistics;

namespace Orcus.Administration.Controls.PieControls
{
    /// <summary>
    ///     Interaction logic for PieControl.xaml
    ///     Taken from http://www.codeproject.com/Articles/442506/Simple-and-Easy-to-Use-Pie-Chart-Controls-in-WPF
    /// </summary>
    public partial class PieControl
    {
        public static readonly DependencyProperty PopupBrushProperty = DependencyProperty.Register("PopupBrush",
            typeof (Brush), typeof (PieControl));

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            "Data", typeof (ObservableCollection<PieSegment>), typeof (PieControl),
            new PropertyMetadata(default(ObservableCollection<PieSegment>), DataPropertyChangedCallback));

        private readonly Dictionary<Path, PieSegment> _pathDictionary = new Dictionary<Path, PieSegment>();

        public PieControl()
        {
            DataContext = this;
            PopupBrush = Brushes.White;
            InitializeComponent();
        }

        public Brush PopupBrush
        {
            get { return (Brush) GetValue(PopupBrushProperty); }
            set { SetValue(PopupBrushProperty, value); }
        }

        public ObservableCollection<PieSegment> Data
        {
            get { return (ObservableCollection<PieSegment>) GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private static void DataPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var pieControl = dependencyObject as PieControl;
            if (dependencyPropertyChangedEventArgs.NewValue == null)
                return;

            pieControl?.DataChanged((ObservableCollection<PieSegment>) dependencyPropertyChangedEventArgs.NewValue);
        }

        private void DataChanged(ObservableCollection<PieSegment> values)
        {
            values.CollectionChanged += values_CollectionChanged;
            foreach (var v in values)
            {
                v.PropertyChanged += pieSegment_PropertyChanged;
            }
            ResetPie();
        }

        private void AddPathToDictionary(Path path, PieSegment ps)
        {
            _pathDictionary.Add(path, ps);
            path.MouseEnter += Path_MouseEnter;
        }

        private void Path_MouseEnter(object sender, MouseEventArgs e)
        {
            var seg = _pathDictionary[(Path) sender];
            var toolTip = new ToolTip
            {
                Content = seg.Name + " : " + (seg.Value/Data.GetTotal()*100).ToString("N2") + "%"
            };
            ((Path) sender).ToolTip = toolTip;
            ToolTipService.SetShowDuration(toolTip, 500);
            ToolTipExtensions.SetAutoMove(toolTip, true);
        }

        private void ClearPathDictionary()
        {
            foreach (var path in _pathDictionary.Keys)
            {
                path.MouseEnter -= Path_MouseEnter;
            }
            _pathDictionary.Clear();
        }

        private void CreatePiePathAndGeometries()
        {
            ClearPathDictionary();
            if (Data != null)
            {
                var total = Data.GetTotal();
                if (total > 0)
                {
                    var angle = -Math.PI/2;
                    foreach (var ps in Data)
                    {
                        //PieSegment ps = Data[1];
                        Geometry geometry;
                        var path = new Path();
                        if (ps.Value == total)
                        {
                            geometry = new EllipseGeometry(new Point(Width/2, Height/2), Width/2, Height/2);
                        }
                        else
                        {
                            geometry = new PathGeometry();
                            var x = Math.Cos(angle)*Width/2 + Width/2;
                            var y = Math.Sin(angle)*Height/2 + Height/2;
                            var lingeSeg = new LineSegment(new Point(x, y), true);
                            var angleShare = ps.Value/total*360;
                            angle += DegreeToRadian(angleShare);
                            x = Math.Cos(angle)*Width/2 + Width/2;
                            y = Math.Sin(angle)*Height/2 + Height/2;
                            var arcSeg = new ArcSegment(new Point(x, y), new Size(Width/2, Height/2), angleShare,
                                angleShare > 180, SweepDirection.Clockwise, false);
                            var lingeSeg2 = new LineSegment(new Point(Width/2, Height/2), true);
                            var fig = new PathFigure(new Point(Width/2, Height/2),
                                new PathSegment[] {lingeSeg, arcSeg, lingeSeg2}, false);
                            ((PathGeometry) geometry).Figures.Add(fig);
                        }
                        path.Fill = ps.SolidBrush;
                        path.Data = geometry;
                        AddPathToDictionary(path, ps);
                        drawingCanvas.Children.Add(path);
                    }
                }
            }
        }

        private void ResetPie()
        {
            Dispatcher.Invoke(CreatePiePathAndGeometries);
        }

        private void pieSegment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ResetPie();
        }

        private void values_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ResetPie();
        }

        private double DegreeToRadian(double angle)
        {
            return Math.PI*angle/180.0;
        }
    }
}