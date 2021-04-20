using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Orcus.Administration.ViewModels.Statistics;

namespace Orcus.Administration.Controls.PieControls
{
    /// <summary>
    ///     Interaction logic for PieChart.xaml
    ///     Taken from http://www.codeproject.com/Articles/442506/Simple-and-Easy-to-Use-Pie-Chart-Controls-in-WPF
    /// </summary>
    public partial class PieChart
    {
        public ObservableCollection<PieSegment> Values;

        public PieChart()
        {
            InitializeComponent();
        }

        public Brush PopupBrush
        {
            get { return Pie.PopupBrush; }
            set { Pie.PopupBrush = value; }
        }

        public ObservableCollection<PieSegment> Data
        {
            get { return Values; }
            set
            {
                Values = value;
                Pie.Data = value;
                foreach (var v in Values)
                {
                    v.PropertyChanged += PieSegment_PropertyChanged;
                }
                Dispatcher.Invoke(InvalidateVisual);
            }
        }

        public double PieWidth
        {
            get { return Pie.Width; }
            set { Pie.Width = value; }
        }

        public double PieHeight
        {
            get { return Pie.Height; }
            set { Pie.Height = value; }
        }

        void PieSegment_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(InvalidateVisual);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            if (Values != null)
            {
                double height = Values.Count*20;
                double top = (Height - height)/2;
                foreach (PieSegment ps in Values)
                {
                    dc.DrawRectangle(ps.SolidBrush, null, new Rect(Pie.Width + 10, top, 8, 8));
                    dc.DrawText(GetFormattedText(ps.Name + " (" + ps.Value + ")", 12, Brushes.Black),
                        new Point(Pie.Width + 20, top));
                    top += 20;
                }
            }
        }

        public FormattedText GetFormattedText(string textToFormat, double fontSize, Brush brush)
        {
            Typeface typeface = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal,
                FontStretches.Normal);
            return new FormattedText(textToFormat, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight,
                typeface, fontSize, brush);
        }
    }
}