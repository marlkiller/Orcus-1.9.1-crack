using System.Collections.Generic;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for ClientTagBuilderPropertyView.xaml
    /// </summary>
    public partial class ClientTagBuilderPropertyView : BuilderPropertyViewUserControl<ClientTagBuilderProperty>
    {
        public ClientTagBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.GeneralSettings);

        public override string[] Tags { get; } = {"Group", "Client", "Gruppe", "Tag"};

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            ClientTagBuilderProperty currentBuilderProperty)
        {
            if (currentBuilderProperty.ClientTag == null)
                return
                    InputValidationResult.Error(
                        "The client tag can not be null. If you see this message, please report it to the developer because that shouldn't be possible");

            return InputValidationResult.Successful;
        }
    }
}