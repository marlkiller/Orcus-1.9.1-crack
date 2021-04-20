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
    ///     Interaction logic for WatchdogBuilderPropertyView.xaml
    /// </summary>
    public partial class WatchdogBuilderPropertyView : BuilderPropertyViewUserControl<WatchdogBuilderProperty>
    {
        public WatchdogBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Protection).ComesAfter<RespawnTaskBuilderProperty>();

        public override string[] Tags { get; } = {"watchdog", "protection", "golem", "schutz"};

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            WatchdogBuilderProperty currentBuilderProperty)
        {
            if (currentBuilderProperty.IsEnabled && string.IsNullOrWhiteSpace(currentBuilderProperty.Name))
                return InputValidationResult.Error((string) Application.Current.Resources["ErrorWatchdogFilename"]);

            return InputValidationResult.Successful;
        }
    }
}