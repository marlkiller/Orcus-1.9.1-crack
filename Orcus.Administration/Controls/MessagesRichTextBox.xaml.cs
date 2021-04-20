using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Orcus.Administration.Commands.TextChat;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for MessagesRichTextBox.xaml
    /// </summary>
    public partial class MessagesRichTextBox
    {
        public static readonly DependencyProperty MessagesProperty = DependencyProperty.Register(
            "Messages", typeof (ObservableCollection<ChatMessage>), typeof (MessagesRichTextBox),
            new PropertyMetadata(default(ObservableCollection<ChatMessage>), PropertyChangedCallback));

        private readonly SolidColorBrush _himBrush = new SolidColorBrush(Color.FromRgb(41, 128, 185));
        private readonly SolidColorBrush _meBrush = new SolidColorBrush(Color.FromRgb(43, 192, 105));
        private Paragraph _currentParagraph;
        private readonly int _leftPartLength;

        public MessagesRichTextBox()
        {
            InitializeComponent();
            var defaultShitLength = DateTime.Now.ToLongTimeString().Length + 3; //3 = [ ]
            var meLength = ((string) Application.Current.Resources["Me"]).Length;
            var clientLength = ((string) Application.Current.Resources["Client"]).Length;
            var senderNameLength = meLength > clientLength ? meLength : clientLength;

            _leftPartLength = senderNameLength + defaultShitLength + 8; //8 spaces = one tab
        }

        public ObservableCollection<ChatMessage> Messages
        {
            get { return (ObservableCollection<ChatMessage>) GetValue(MessagesProperty); }
            set { SetValue(MessagesProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var messagesRichTextBox = dependencyObject as MessagesRichTextBox;
            if (messagesRichTextBox == null)
                throw new InvalidOperationException();

            messagesRichTextBox.InitializeCollection(
                (ObservableCollection<ChatMessage>) dependencyPropertyChangedEventArgs.NewValue);
        }

        private void InitializeCollection(ObservableCollection<ChatMessage> messages)
        {
            MainRichTextBox.Document.Blocks.Clear();
            if (messages != null)
            {
                _currentParagraph = new Paragraph();
                MainRichTextBox.Document.Blocks.Add(_currentParagraph);
                messages.CollectionChanged += Messages_CollectionChanged;
            }
        }

        private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    foreach (var message in e.NewItems.Cast<ChatMessage>())
                    {
                        AppendMessage(message);
                    }
            }));
        }

        private void AppendMessage(ChatMessage chatMessage)
        {
            var foreground = chatMessage.IsFromMe ? _meBrush : _himBrush;

            var leftPart =
                $"[{chatMessage.Timestamp.ToLongTimeString()} {(chatMessage.IsFromMe ? (string) Application.Current.Resources["Me"] : (string) Application.Current.Resources["Client"])}]";
            _currentParagraph.Inlines.Add(
                new Run(
                    $"{leftPart}{new string(' ', _leftPartLength - leftPart.Length)}{chatMessage.Content}\r\n")
                {
                    Foreground = foreground
                });
            MainRichTextBox.ScrollToEnd();
        }
    }
}