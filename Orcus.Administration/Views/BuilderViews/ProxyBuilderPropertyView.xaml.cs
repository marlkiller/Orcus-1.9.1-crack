using System.Collections.Generic;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for ProxyBuilderPropertyView.xaml
    /// </summary>
    public partial class ProxyBuilderPropertyView : BuilderPropertyViewUserControl<ProxyBuilderProperty>
    {
        public ProxyBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Connection).ComesAfter<ConnectionBuilderProperty>();

        public override string[] Tags { get; } = {"Connection", "Proxy", "Tunnel", "Network"};

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            ProxyBuilderProperty currentBuilderProperty)
        {
            return InputValidationResult.Successful;
        }
    }
}