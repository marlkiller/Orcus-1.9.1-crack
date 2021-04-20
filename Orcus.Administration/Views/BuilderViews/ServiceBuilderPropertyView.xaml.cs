using System.Collections.Generic;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for ServiceBuilderPropertyView.xaml
    /// </summary>
    public partial class ServiceBuilderPropertyView : BuilderPropertyViewUserControl<ServiceBuilderProperty>
    {
        public ServiceBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Installation);

        public override string[] Tags { get; } = {"service", "dienst"};

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            ServiceBuilderProperty currentBuilderProperty)
        {
            return InputValidationResult.Successful;
        }
    }
}