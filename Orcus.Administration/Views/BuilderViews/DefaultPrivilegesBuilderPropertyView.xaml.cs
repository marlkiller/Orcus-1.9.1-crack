using System.Collections.Generic;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for DefaultPrivilegesBuilderPropertyView.xaml
    /// </summary>
    public partial class DefaultPrivilegesBuilderPropertyView :
        BuilderPropertyViewUserControl<DefaultPrivilegesBuilderProperty>
    {
        public DefaultPrivilegesBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override string[] Tags { get; } = {"privileges", "administrator", "manifest", "berechtigung"};

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.GeneralSettings)
                .ComesAfter<DataFolderBuilderProperty>();

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            DefaultPrivilegesBuilderProperty currentBuilderProperty)
        {
            return InputValidationResult.Successful;
        }
    }
}