using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Orcus.Commands.TextChat.Utilities;
using Orcus.Native;
using Orcus.Shared.Commands.TextChat;

namespace Orcus.Commands.TextChat
{
    public partial class TextChatForm : Form
    {
        private readonly ChatSettings _chatSettings;
        private readonly Color _meColor = Color.FromArgb(43, 192, 105);
        private readonly Color _otherColor = Color.FromArgb(41, 128, 185);

        private bool _isClosing;

        public TextChatForm(ChatSettings chatSettings)
        {
            _chatSettings = chatSettings;
            InitializeComponent();

            Text = chatSettings.Title;
            MessageTextBox.KeyDown += MessageTextBox_KeyDown;
        }

        public bool IsClosed { get; set; }

        public event EventHandler<SendTextMessageEventArgs> SendMessage;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Show();
        }

        public void ForceClose()
        {
            _isClosing = true;
            Close();
        }

        public void MessageReceived(DateTime timestamp, string message)
        {
            BeginInvoke(new Action(() =>
            {
                MainRichTextBox.AppendText($"[{timestamp.ToShortTimeString()} {_chatSettings.YourName}]: {message}\r\n",
                    _otherColor);
                MainRichTextBox.ScrollToEnd();
            }));
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SendText();
                e.SuppressKeyPress = true;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = _chatSettings.PreventClose && !_isClosing;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            SendText();
        }

        private void SendText()
        {
            var line = MessageTextBox.Text;
            if (string.IsNullOrEmpty(line))
                return;

            MainRichTextBox.AppendText($"[{DateTime.Now.ToShortTimeString()} Me]: {line}\r\n", _meColor);
            MainRichTextBox.ScrollToEnd();
            MessageTextBox.Text = null;

            SendMessage?.Invoke(this, new SendTextMessageEventArgs(line));
        }

        private void MessageTextBox_TextChanged(object sender, EventArgs e)
        {
            SendButton.Enabled = !string.IsNullOrEmpty(MessageTextBox.Text);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ActiveControl = MessageTextBox;
            Activate();

            //normal topmost doesn't work
            if (_chatSettings.Topmost)
                NativeMethods.SetWindowPos(Handle, new IntPtr(-1), 0, 0, 0, 0,
                    SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize | SetWindowPosFlags.ShowWindow);
        }
    }
}