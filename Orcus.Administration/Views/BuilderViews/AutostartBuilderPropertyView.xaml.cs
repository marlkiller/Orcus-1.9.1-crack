using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for AutostartBuilderPropertyView.xaml
    /// </summary>
    public partial class AutostartBuilderPropertyView : BuilderPropertyViewUserControl<AutostartBuilderProperty>
    {
        public AutostartBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Installation).InGroup(BuilderGroup.Install)
                .ComesAfter<ChangeCreationDateBuilderProperty>();

        public override string[] Tags { get; } = {"autostart", "startup", "start", "starten"};

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            AutostartBuilderProperty currentBuilderProperty)
        {
            if (currentBuilderProperty.AutostartMethod == StartupMethod.Disable)
                return InputValidationResult.Successful;

            if ((currentBuilderProperty.TryAllAutostartMethodsOnFail ||
                 currentBuilderProperty.AutostartMethod == StartupMethod.TaskScheduler) &&
                string.IsNullOrWhiteSpace(currentBuilderProperty.TaskSchedulerTaskName))
                return InputValidationResult.Error((string) Application.Current.Resources["ErrorTaskAutostart"]);

            if ((currentBuilderProperty.TryAllAutostartMethodsOnFail ||
                 currentBuilderProperty.AutostartMethod == StartupMethod.Registry) &&
                string.IsNullOrWhiteSpace(currentBuilderProperty.RegistryKeyName))
                return InputValidationResult.Error((string) Application.Current.Resources["ErrorAutostart"]);

            if ((currentBuilderProperty.TryAllAutostartMethodsOnFail ||
                 currentBuilderProperty.AutostartMethod == StartupMethod.TaskScheduler))
            {
                //we check if the tasks have the same name which would fail because they would override each other
                var respawnTaskBuilderProperty =
                    currentBuilderProperties.OfType<RespawnTaskBuilderProperty>().FirstOrDefault();
                if (respawnTaskBuilderProperty != null && respawnTaskBuilderProperty.IsEnabled &&
                    respawnTaskBuilderProperty.TaskName.Equals(currentBuilderProperty.TaskSchedulerTaskName,
                        StringComparison.OrdinalIgnoreCase))
                    return InputValidationResult.Error((string) Application.Current.Resources["ErrorTasksHaveSameName"]);
            }

            return InputValidationResult.Successful;
        }
    }
}