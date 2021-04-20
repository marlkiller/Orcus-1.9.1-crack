using System.Collections.Generic;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for FrameworkVersionBuilderPropertyView.xaml
    /// </summary>
    public partial class FrameworkVersionBuilderPropertyView :
        BuilderPropertyViewUserControl<FrameworkVersionBuilderProperty>
    {
        public FrameworkVersionBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Assembly);

        public override string[] Tags { get; } = {".net", "framework", "version"};

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            FrameworkVersionBuilderProperty currentBuilderProperty)
        {
            return InputValidationResult.Successful;
        }
    }
}