using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Orcus.Chat.Modern.Core;

namespace Orcus.Chat.Modern.Controls
{
    /// <summary>
    ///     Interaction logic for ConversationView.xaml
    /// </summary>
    public partial class ConversationView : INotifyPropertyChanged
    {
        public static readonly DependencyProperty MessagesProperty = DependencyProperty.Register(
            "Messages", typeof (ObservableCollection<Message>), typeof (ConversationView),
            new PropertyMetadata(default(ObservableCollection<Message>), MessagesPropertyChangedCallback));

        public ConversationView()
        {
            InitializeComponent();
        }

        public ObservableCollection<Message> Messages
        {
            get { return (ObservableCollection<Message>) GetValue(MessagesProperty); }
            set { SetValue(MessagesProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void MessagesPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = dependencyObject as ConversationView;
            control?.UpdateProperty("Messages");
        }


        private void UpdateProperty(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}