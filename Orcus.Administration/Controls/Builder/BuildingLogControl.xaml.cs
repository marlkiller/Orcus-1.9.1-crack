using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using Orcus.Administration.Core.Build;
using Application = System.Windows.Application;

namespace Orcus.Administration.Controls.Builder
{
    /// <summary>
    ///     Interaction logic for BuildingLogControl.xaml
    /// </summary>
    public partial class BuildingLogControl
    {
        public static readonly DependencyProperty BuildLoggerProperty = DependencyProperty.Register(
            "BuildLogger", typeof (BuildLogger), typeof (BuildingLogControl),
            new PropertyMetadata(default(BuildLogger), PropertyChangedCallback));

        private readonly Paragraph _paragraph;

        public BuildingLogControl()
        {
            InitializeComponent();
            _paragraph = new Paragraph();
            MainRichTextBox.Document.Blocks.Add(_paragraph);
        }

        public BuildLogger BuildLogger
        {
            get { return (BuildLogger) GetValue(BuildLoggerProperty); }
            set { SetValue(BuildLoggerProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = dependencyObject as BuildingLogControl;
            control?.BuildLoggerChanged((BuildLogger) dependencyPropertyChangedEventArgs.NewValue,
                (BuildLogger) dependencyPropertyChangedEventArgs.OldValue);
        }

        private void BuildLoggerChanged(BuildLogger buildLogger, BuildLogger oldLogger)
        {
            if (oldLogger != null)
                oldLogger.NewLogMessage -= BuildLoggerOnNewLogMessage;
            buildLogger.NewLogMessage += BuildLoggerOnNewLogMessage;
            _paragraph.Inlines.Clear();
        }

        private void BuildLoggerOnNewLogMessage(object sender, NewBuildLogMessageEventArgs newBuildLogMessageEventArgs)
        {
            Dispatcher.BeginInvoke(
                (MethodInvoker)
                    delegate
                    {
                        Brush foreground;
                        string prefix;

                        switch (newBuildLogMessageEventArgs.BuildLogType)
                        {
                            case BuildLogType.Status:
                                foreground = (Brush) Application.Current.Resources["BlackBrush"];
                                prefix = (string) Application.Current.Resources["Status"];
                                break;
                            case BuildLogType.Warning:
                                foreground = new SolidColorBrush(Color.FromArgb(255, 231, 76, 60));
                                prefix = (string) Application.Current.Resources["Warning"];
                                break;
                            case BuildLogType.Error:
                                foreground = new SolidColorBrush(Color.FromArgb(255, 192, 57, 43));
                                prefix = (string) Application.Current.Resources["Error"];
                                break;
                            case BuildLogType.Success:
                                foreground = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96));
                                prefix = (string) Application.Current.Resources["Success"];
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        _paragraph.Inlines.Add(
                            new Run($"[{prefix.ToUpper()}] \t" + newBuildLogMessageEventArgs.Content + "\r\n")
                            {
                                Foreground = foreground
                            });

                        MainRichTextBox.ScrollToEnd();
                    });
        }
    }
}