using System.Collections.Generic;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for ReconnectDelayBuilderPropertyView.xaml
    /// </summary>
    public partial class ReconnectDelayBuilderPropertyView : BuilderPropertyViewUserControl<ReconnectDelayProperty>
    {
        public ReconnectDelayBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Connection).ComesAfter<ProxyBuilderProperty>();

        public override string[] Tags { get; } = {"Reconnect", "Connect", "Delay", "Verzögerung", "Neuverbindung"};

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            ReconnectDelayProperty currentBuilderProperty)
        {
            return InputValidationResult.Successful;
        }
    }
}