using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace OrcusPluginStudio.Controls
{
    /// <summary>
    ///     Interaction logic for ChangableImage.xaml
    /// </summary>
    public partial class ChangableImage
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource", typeof (BitmapSource), typeof (ChangableImage), new PropertyMetadata(default(BitmapSource)));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof (ICommand), typeof (ChangableImage), new PropertyMetadata(default(ICommand)));

        public ChangableImage()
        {
            InitializeComponent();
        }

        public BitmapSource ImageSource
        {
            get { return (BitmapSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
    }
}