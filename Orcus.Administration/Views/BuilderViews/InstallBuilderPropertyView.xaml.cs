using System.Collections.Generic;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for InstallBuilderPropertyView.xaml
    /// </summary>
    public partial class InstallBuilderPropertyView : BuilderPropertyViewUserControl<InstallBuilderProperty>,
        ILeaderBuilderPropertyView
    {
        private bool _enableSubSettings;

        public InstallBuilderPropertyView()
        {
            InitializeComponent();
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Installation).InGroup(BuilderGroup.Install).SetLeader()
            ;

        public override string[] Tags { get; } = {"install", "installation"};

        public bool EnableSubSettings
        {
            get { return _enableSubSettings; }
            set
            {
                if (_enableSubSettings != value)
                {
                    _enableSubSettings = value;
                    OnPropertyChanged();
                    ((InstallBuilderProperty) DataContext).Install = value;
                }
            }
        }

        protected override void OnCurrentBuilderPropertyChanged(InstallBuilderProperty newValue)
        {
            EnableSubSettings = newValue.Install;
        }

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            InstallBuilderProperty currentBuilderProperty)
        {
            return InputValidationResult.Successful;
        }
    }
}