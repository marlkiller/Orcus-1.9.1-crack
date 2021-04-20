using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Orcus.Chat.Simple.Core;
using Orcus.Chat.Simple.Utilities;

namespace Orcus.Chat.Simple
{
    public partial class MainForm : Form
    {
        private readonly ChatService _chatService;
        private readonly Color _meColor = Color.FromArgb(43, 192, 105);
        private readonly Color _otherColor = Color.FromArgb(41, 128, 185);

        private bool _isClosing;

        public MainForm(ChatService chatService)
        {
            _chatService = chatService;
            InitializeComponent();
            if (chatService.Topmost)
                TopMost = true;

            Text = chatService.Title;
            MessageTextBox.KeyDown += MessageTextBox_KeyDown;
            chatService.MessageReceived += ChatService_MessageReceived;

            chatService.Close += ChatService_Close;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Show();
        }

        private void ChatService_Close(object sender, EventArgs e)
        {
            _isClosing = true;
            Application.Exit();
        }

        private void ChatService_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                MainRichTextBox.AppendText($"[{e.Timestamp.ToShortTimeString()} {_chatService.Name}]: {e.Content}\r\n",
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
            e.Cancel = _chatService.PreventClose && !_isClosing;
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
            _chatService.ChatCallback.SendMessage(line);
        }

        private void MessageTextBox_TextChanged(object sender, EventArgs e)
        {
            SendButton.Enabled = !string.IsNullOrEmpty(MessageTextBox.Text);
        }
    }
}