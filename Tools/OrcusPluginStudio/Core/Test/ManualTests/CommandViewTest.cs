using System;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Plugins;
using OrcusPluginStudio.Core.Test.AdministrationVirtualisation;
using Command = Orcus.Plugins.Command;

namespace OrcusPluginStudio.Core.Test.ManualTests
{
    public class CommandViewTest : IManualTest
    {
        private readonly PluginFactory _factory;
        private ClientVirtualizer _clientVirtualizer;

        public CommandViewTest(ICommandAndViewPlugin commandView, Type command)
        {
            Initialize((Command) Activator.CreateInstance(command),
                (Orcus.Administration.Plugins.CommandViewPlugin.Command) Activator.CreateInstance(commandView.Command),
                (ICommandView) Activator.CreateInstance(commandView.CommandView),
                (FrameworkElement) Activator.CreateInstance(commandView.View));
        }

        public CommandViewTest(ICommandAndViewPlugin commandView, IFactoryClientCommand factoryClientCommand)
        {
            _factory = factoryClientCommand.Factory;
            try
            {
                _factory.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var clientCommand = (FactoryCommand) Activator.CreateInstance(factoryClientCommand.FactoryCommandType);
            try
            {
                clientCommand.Initialize(_factory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var administrationCommand =
                (Orcus.Administration.Plugins.CommandViewPlugin.Command) Activator.CreateInstance(commandView.Command);

            Initialize(clientCommand, administrationCommand,
                (ICommandView) Activator.CreateInstance(commandView.CommandView),
                (FrameworkElement) Activator.CreateInstance(commandView.View));
        }

        public FrameworkElement FrameworkElement { get; private set; }
        public ICommandView CommandView { get; private set; }

        private void Initialize(Command clientCommand, Orcus.Administration.Plugins.CommandViewPlugin.Command administrationCommand,
            ICommandView commandView, FrameworkElement view)
        {
            _clientVirtualizer = new ClientVirtualizer(administrationCommand, clientCommand);
            try
            {
                commandView.Initialize(_clientVirtualizer.ClientController, new CrossViewManager());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CommandView = commandView;
            CommandView.LoadView(false);
            FrameworkElement = view;
            FrameworkElement.DataContext = commandView;
        }

        public void Dispose()
        {
            _factory?.Shutdown();
            CommandView.Dispose();
            _clientVirtualizer.Dispose();
        }
    }
}