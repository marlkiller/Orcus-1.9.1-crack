using System.Collections.Generic;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for SetRunProgramAsAdminFlagBuilderPropertyView.xaml
    /// </summary>
    public partial class SetRunProgramAsAdminFlagBuilderPropertyView :
        BuilderPropertyViewUserControl<SetRunProgramAsAdminFlagBuilderProperty>
    {
        public SetRunProgramAsAdminFlagBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Installation)
                .InGroup(BuilderGroup.Install)
                .ComesAfter<HideFileBuilderProperty>();

        public override string[] Tags { get; } = {"administrator", "flag"};

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            SetRunProgramAsAdminFlagBuilderProperty currentBuilderProperty)
        {
            return InputValidationResult.Successful;
        }
    }
}