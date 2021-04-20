using System;
using System.Windows;
using System.Windows.Threading;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for DateTimeDifferenceTextBlock.xaml
    /// </summary>
    public partial class DateTimeDifferenceTextBlock
    {
        public static readonly DependencyProperty DateTimeProperty = DependencyProperty.Register(
            "DateTime", typeof (DateTime), typeof (DateTimeDifferenceTextBlock),
            new PropertyMetadata(default(DateTime), PropertyChangedCallback));

        private readonly DispatcherTimer _dispatcherTimer;

        public DateTimeDifferenceTextBlock()
        {
            InitializeComponent();
            _dispatcherTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
        }

        public DateTime DateTime
        {
            get { return (DateTime) GetValue(DateTimeProperty); }
            set { SetValue(DateTimeProperty, value); }
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            RefreshView();
        }

        private void RefreshView()
        {
            var difference = DateTime.Now - DateTime;
            MainControl.Text =
                $"{difference.Days}d : {difference.Hours}h : {difference.Minutes}m : {difference.Seconds}s";
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var dateTimeTextBlock = dependencyObject as DateTimeDifferenceTextBlock;
            if (dateTimeTextBlock != null)
            {
                if (dependencyPropertyChangedEventArgs.NewValue != null)
                {
                    dateTimeTextBlock.RefreshView();
                    dateTimeTextBlock._dispatcherTimer.Start();
                }
                else
                    dateTimeTextBlock._dispatcherTimer.Stop();
            }
        }
    }
}