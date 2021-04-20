using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Orcus.Administration.ViewModels.CommandViewModels.VoiceChat;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for AudioVisualisationControl.xaml
    /// </summary>
    public partial class AudioVisualisationControl
    {
        private const int PixelsPerSample = 2;

        public static readonly DependencyProperty DataProviderProperty = DependencyProperty.Register(
            "DataProvider", typeof(AudioVisualizationDataProvider), typeof(AudioVisualisationControl),
            new PropertyMetadata(default(AudioVisualizationDataProvider), DataProviderPropertyChangedCallback));

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            "IsActive", typeof(bool), typeof(AudioVisualisationControl),
            new PropertyMetadata(default(bool), IsActivePropertyChangedCallback));

        private AudioVisualizationDataProvider _currentDataProvider;
        private DispatcherTimer _dispatcherTimer;
        private List<float> _sampleDataLeft;
        private List<float> _sampleDataRight;
        private WriteableBitmap _writeableBitmap;
        private readonly Lazy<Color> _backgroundColor;
        private readonly Lazy<Color> _leftChannelColor;
        private readonly Lazy<Color> _rightChannelColor;
        private readonly object _syncLock = new object();

        public AudioVisualisationControl()
        {
            InitializeComponent();
            _backgroundColor = new Lazy<Color>(() => (Color) Application.Current.Resources["FlyoutColor"]);
            _leftChannelColor = new Lazy<Color>(() => (Color) Application.Current.Resources["LightColor"]);
            _rightChannelColor = new Lazy<Color>(() => (Color) Application.Current.Resources["AccentColor"]);
        }

        public bool IsActive
        {
            get { return (bool) GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public AudioVisualizationDataProvider DataProvider
        {
            get { return (AudioVisualizationDataProvider) GetValue(DataProviderProperty); }
            set { SetValue(DataProviderProperty, value); }
        }

        private static void IsActivePropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((AudioVisualisationControl) dependencyObject)._dispatcherTimer.IsEnabled =
                (bool) dependencyPropertyChangedEventArgs.NewValue;
        }

        private static void DataProviderPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((AudioVisualisationControl) dependencyObject).DataProviderChanged(
                dependencyPropertyChangedEventArgs.NewValue as AudioVisualizationDataProvider);
        }

        public void DataProviderChanged(AudioVisualizationDataProvider dataProvider)
        {
            if (_currentDataProvider != null)
                _currentDataProvider.SamplesAdded -= CurrentDataProviderOnSamplesAdded;

            _currentDataProvider = dataProvider;
            _sampleDataLeft = new List<float>();
            _sampleDataRight = new List<float>();

            if (_currentDataProvider != null)
            {
                _currentDataProvider.SamplesAdded += CurrentDataProviderOnSamplesAdded;
                if (_dispatcherTimer == null)
                {
                    _dispatcherTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(40)};
                    _dispatcherTimer.Tick += DispatcherTimerOnTick;
                }
            }
        }

        private void DispatcherTimerOnTick(object sender, EventArgs eventArgs)
        {
            var width = (int) ActualWidth;
            var height = (int) ActualHeight;

            if (_writeableBitmap == null || _writeableBitmap.Width != width ||
                _writeableBitmap.Height != height)
            {
                _writeableBitmap = BitmapFactory.New(width, height);
                DisplayImage.Source = _writeableBitmap;
            }
            
            using (_writeableBitmap.GetBitmapContext())
            {
                _writeableBitmap.Clear(_backgroundColor.Value);

                var samplesLeft = GetSamplesToDraw(_sampleDataLeft, width / PixelsPerSample).ToArray();
                var samplesRight = GetSamplesToDraw(_sampleDataRight, width / PixelsPerSample).ToArray();

                var points = GetPoints(samplesLeft, PixelsPerSample, width, height).ToArray();
                for (int i = 1; i < points.Length; i++)
                {
                    var previousPoint = points[i - 1];
                    var currentPoint = points[i];

                    _writeableBitmap.DrawLine((int) previousPoint.X, (int) previousPoint.Y, (int) currentPoint.X,
                        (int) currentPoint.Y, _leftChannelColor.Value);
                }

                points = GetPoints(samplesRight, PixelsPerSample, width, height).ToArray();
                for (int i = 1; i < points.Length; i++)
                {
                    var previousPoint = points[i - 1];
                    var currentPoint = points[i];

                    _writeableBitmap.DrawLine((int) previousPoint.X, (int) previousPoint.Y, (int) currentPoint.X,
                        (int) currentPoint.Y, _leftChannelColor.Value);
                }
            }
        }

        private IEnumerable<Point> GetPoints(float[] samples, int pixelsPerSample, int width, int height)
        {
            int halfY = height / pixelsPerSample;
            if (samples.Length >= 2)
            {
                for (int i = 0; i < samples.Length; i++)
                {
                    yield return new Point
                    {
                        X = i * pixelsPerSample,
                        Y = halfY + (int)(samples[i] * halfY)
                    };
                }
            }
            else
            {
                yield return new Point(0, halfY);
                yield return new Point(width, halfY);
            }
        }

        private IEnumerable<float> GetSamplesToDraw(List<float> inputSamples, int count)
        {
            float[] samples;

            lock (_syncLock)
            {
                samples = inputSamples.ToArray();
                inputSamples.Clear();
            }

            var resolution = samples.Length / count;
            int index = 0;
            float currentMax = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                if (i > index * resolution)
                {
                    yield return currentMax;
                    currentMax = 0;
                    index++;
                }

                if (Math.Abs(currentMax) < Math.Abs(samples[i]))
                    currentMax = samples[i];
            }
        }

        private void CurrentDataProviderOnSamplesAdded(object sender, Tuple<float, float> tuple)
        {
            lock (_syncLock)
            {
                _sampleDataLeft.Add(tuple.Item1);
                _sampleDataRight.Add(tuple.Item2);
            }
        }
    }
}