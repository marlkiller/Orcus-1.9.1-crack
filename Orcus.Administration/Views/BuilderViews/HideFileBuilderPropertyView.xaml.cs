using System.Collections.Generic;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for HideFileBuilderPropertyView.xaml
    /// </summary>
    public partial class HideFileBuilderPropertyView : BuilderPropertyViewUserControl<HideFileBuilderProperty>
    {
        public HideFileBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Installation)
                .InGroup(BuilderGroup.Install)
                .ComesAfter<InstallationLocationBuilderProperty>();

        public override string[] Tags { get; } = {"hide", "verstecken", "datei", "file", "unsichtbar", "invisible"};

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            HideFileBuilderProperty currentBuilderProperty)
        {
            return InputValidationResult.Successful;
        }
    }
}