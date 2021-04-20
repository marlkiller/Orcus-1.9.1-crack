using System.ComponentModel;
using Orcus.Chat.Modern.Core;
using Orcus.Chat.Modern.ViewModels;

namespace Orcus.Chat.Modern
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ChatService _chatService;

        public MainWindow(ChatService chatService)
        {
            _chatService = chatService;
            InitializeComponent();
            Topmost = chatService.Topmost;
            Title = chatService.Title;
            DataContext = new MainViewModel(chatService);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_chatService.PreventClose)
                e.Cancel = true;
        }
    }
}