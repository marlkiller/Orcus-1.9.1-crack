using System.Collections.Generic;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for RequireAdministratorPrivilegesInstallerBuilderPropertyView.xaml
    /// </summary>
    public partial class RequireAdministratorPrivilegesInstallerBuilderPropertyView :
        BuilderPropertyViewUserControl<RequireAdministratorPrivilegesInstallerBuilderProperty>
    {
        public RequireAdministratorPrivilegesInstallerBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Installation)
                .InGroup(BuilderGroup.Install)
                .ComesAfter<SetRunProgramAsAdminFlagBuilderProperty>();

        public override string[] Tags { get; } = {
            "installer", "administrator", "privileges", "installation", "berechtigung",
            "rights", "rechte"
        };

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            RequireAdministratorPrivilegesInstallerBuilderProperty currentBuilderProperty)
        {
            return InputValidationResult.Successful;
        }
    }
}