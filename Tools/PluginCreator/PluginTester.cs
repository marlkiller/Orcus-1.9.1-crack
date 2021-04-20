using System;
using System.Linq;
using System.Reflection;
using Orcus.Plugins;

namespace PluginCreator
{
    class PluginTester
    {
        private readonly Type[] _types;

        public PluginTester(string path)
        {
            var appDomain = AppDomain.CreateDomain("TEST_DOMAIN");

            var sharedLib = new AssemblyName {CodeBase = "Orcus.Shared.dll"};
            appDomain.Load(sharedLib);

            var pluginLib = new AssemblyName {CodeBase = "Orcus.Plugins.dll"};
            appDomain.Load(pluginLib);

            var assemblyName = new AssemblyName {CodeBase = path};
            var assembly = appDomain.Load(assemblyName);
            _types = assembly.GetTypes();
            AppDomain.Unload(appDomain);
        }

        public bool IsAudioPlugin()
        {
            var audioType = _types.FirstOrDefault(x => x.GetInterface("IAudioPlugin") != null);
            return audioType != null;
        }

        public bool IsBuildPlugin()
        {
            var buildType = _types.FirstOrDefault(x => x.GetInterface("IBuildPlugin") != null);
            return buildType != null;
        }

        public bool IsClientPlugin()
        {
            var clientType = _types.FirstOrDefault(x => x.IsSubclassOf(typeof (ClientController)));
            return clientType != null;
        }

        public bool IsCommandView()
        {
            var commandView = _types.FirstOrDefault(x => x.GetInterface("ICommandAndViewPlugin") != null);
            return commandView != null;
        }

        public bool IsCommand()
        {
            var commandType = _types.FirstOrDefault(x => x.IsSubclassOf(typeof (Command)));
            return commandType != null;
        }

        public bool IsView()
        {
            var view = _types.FirstOrDefault(x => x.GetInterface("IViewPlugin") != null);
            return view != null;
        }
    }
}