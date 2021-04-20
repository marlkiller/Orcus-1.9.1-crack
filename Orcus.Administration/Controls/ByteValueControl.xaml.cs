using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Be.Windows.Forms;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for ByteValueControl.xaml
    /// </summary>
    public partial class ByteValueControl
    {
        public static readonly DependencyProperty BytesProperty = DependencyProperty.Register(
            "Bytes", typeof (IList<byte>), typeof (ByteValueControl),
            new PropertyMetadata(default(IList<byte>), PropertyChangedCallback));

        public static readonly DependencyProperty SelectionColorProperty = DependencyProperty.Register(
            "SelectionColor", typeof (Color), typeof (ByteValueControl), new PropertyMetadata(default(Color), SelectionColorPropertyChangedCallback));

        private static void SelectionColorPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var byteValueControl = dependencyObject as ByteValueControl;
            var value = (Color) dependencyPropertyChangedEventArgs.NewValue;

            if (byteValueControl != null)
                byteValueControl.HexBox.SelectionBackColor = System.Drawing.Color.FromArgb(value.A, value.R, value.G,
                    value.B);
        }

        public Color SelectionColor
        {
            get { return (Color) GetValue(SelectionColorProperty); }
            set { SetValue(SelectionColorProperty, value); }
        }

        public static readonly DependencyProperty ShadowSelectionColorProperty = DependencyProperty.Register(
            "ShadowSelectionColor", typeof (Color), typeof (ByteValueControl), new PropertyMetadata(default(Color), ShadowSelectionColorPropertyChangedCallback));

        private static void ShadowSelectionColorPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var byteValueControl = dependencyObject as ByteValueControl;
            var value = (Color)dependencyPropertyChangedEventArgs.NewValue;

            if (byteValueControl != null)
                byteValueControl.HexBox.ShadowSelectionColor = System.Drawing.Color.FromArgb(100, value.R, value.G,
                    value.B);
        }

        public Color ShadowSelectionColor
        {
            get { return (Color) GetValue(ShadowSelectionColorProperty); }
            set { SetValue(ShadowSelectionColorProperty, value); }
        }

        private DynamicByteProvider _currentByteProvider;

        public ByteValueControl()
        {
            InitializeComponent();
            var background = (Color) Application.Current.Resources["WhiteColor"];
            HexBox.BackColor = System.Drawing.Color.FromArgb(background.R, background.G, background.B);

            var foreground = (Color) Application.Current.Resources["BlackColor"];
            HexBox.ForeColor = System.Drawing.Color.FromArgb(foreground.R, foreground.G, foreground.B);
        }

        public IList<byte> Bytes
        {
            get { return (IList<byte>) GetValue(BytesProperty); }
            set { SetValue(BytesProperty, value); }
        }

        private void InitializeBytes(byte[] bytes)
        {
            if (_currentByteProvider != null && bytes.SequenceEqual(_currentByteProvider.Bytes))
                return;

            _currentByteProvider = new DynamicByteProvider(bytes);
            _currentByteProvider.Changed += (sender, args) => Bytes = _currentByteProvider.Bytes.ToArray();
            HexBox.ByteProvider = _currentByteProvider;
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var byteValueControl = dependencyObject as ByteValueControl;
            byteValueControl?.InitializeBytes((byte[]) dependencyPropertyChangedEventArgs.NewValue);
        }
    }
}