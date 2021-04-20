using System.Collections.Generic;
using Orcus.Administration.Core.Build;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Shared.Core;

namespace Orcus.Administration.ViewModels.ClientBuilder
{
    public class BuilderArguments : IBuilderArguments
    {
        public BuilderArguments(IReadOnlyList<IBuilderProperty> settings, string standardFilter)
        {
            Settings = settings;
            SaveDialog = SaveDialogType.SaveFileDialog;
            SaveDialogFilter = standardFilter;
            BuildPluginEvents = new List<BuildPluginEvent>();
        }

        public SaveDialogType SaveDialog { get; set; }
        public string SaveDialogFilter { get; set; }
        public IReadOnlyList<IBuilderProperty> Settings { get; }

        public List<BuildPluginEvent> BuildPluginEvents { get; }
        public IPlugin CurrentBuildPlugin { get; set; }

        public void SubscribeBuilderEvent(BuilderEvent builderEvent)
        {
            BuildPluginEvents.Add(new BuildPluginEvent(builderEvent, CurrentBuildPlugin));
        }
    }
}