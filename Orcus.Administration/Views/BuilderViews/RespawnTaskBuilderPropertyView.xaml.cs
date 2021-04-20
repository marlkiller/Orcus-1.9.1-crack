using System.Collections.Generic;
using System.Windows;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for RespawnTaskBuilderPropertyView.xaml
    /// </summary>
    public partial class RespawnTaskBuilderPropertyView : BuilderPropertyViewUserControl<RespawnTaskBuilderProperty>
    {
        public RespawnTaskBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Protection);

        public override string[] Tags { get; } = {"Respawn", "Task", "Neustarten", "Aufgabe"};

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            RespawnTaskBuilderProperty currentBuilderProperty)
        {
            if (string.IsNullOrWhiteSpace(currentBuilderProperty.TaskName))
                return InputValidationResult.Error((string) Application.Current.Resources["ErrorRespawnTaskName"]);

            return InputValidationResult.Successful;
        }
    }
}