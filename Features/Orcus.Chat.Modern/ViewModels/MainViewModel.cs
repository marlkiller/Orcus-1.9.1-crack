using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Orcus.Chat.Modern.Core;

namespace Orcus.Chat.Modern.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        private readonly ChatService _chatService;

        private string _currentMessage;

        private RelayCommand _sendMessageCommand;

        public MainViewModel(ChatService chatService)
        {
            _chatService = chatService;
            _chatService.MessageReceived += _chatService_MessageReceived;
            _chatService.Close += _chatService_Close;

            ChatMessages = new ObservableCollection<Message>();
        }

        public ObservableCollection<Message> ChatMessages { get; }

        public string CurrentMessage
        {
            get { return _currentMessage; }
            set
            {
                if (_currentMessage != value)
                {
                    _currentMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand SendMessageCommand
        {
            get
            {
                return _sendMessageCommand ?? (_sendMessageCommand = new RelayCommand(parameter =>
                {
                    if (string.IsNullOrWhiteSpace(CurrentMessage))
                        return;

                    var chatMessage = new Message
                    {
                        Content = CurrentMessage,
                        IsFromMe = true,
                        Timestamp = DateTime.Now
                    };
                    ChatMessages.Add(chatMessage);
                    _chatService.ChatCallback.SendMessage(CurrentMessage);
                    CurrentMessage = null;
                }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void _chatService_Close(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void _chatService_MessageReceived(object sender, Tuple<string, DateTime> e)
        {
            ChatMessages.Add(new Message {Content = e.Item1, IsFromMe = false, Timestamp = e.Item2});
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}