using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for InstallationLocationBuilderPropertyView.xaml
    /// </summary>
    public partial class InstallationLocationBuilderPropertyView :
        BuilderPropertyViewUserControl<InstallationLocationBuilderProperty>
    {
        private string _installationPath;

        public InstallationLocationBuilderPropertyView()
        {
            InitializeComponent();
        }

        public string InstallationPath
        {
            get { return _installationPath; }
            set
            {
                if (_installationPath != value)
                {
                    _installationPath = value;
                    CurrentBuilderProperty.Path = value;
                    PreviewInstallationFolder = string.IsNullOrEmpty(value)
                        ? string.Empty
                        : Environment.ExpandEnvironmentVariables(value);

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PreviewInstallationFolder));
                }
            }
        }

        public string PreviewInstallationFolder { get; set; }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Installation)
                .InGroup(BuilderGroup.Install)
                .ComesAfter<DisableInstallationPromptBuilderProperty>();

        public override string[] Tags { get; } = {"path", "pfad", "installation", "location", "ort", "ordner", "folder"}
            ;

        protected override void OnCurrentBuilderPropertyChanged(InstallationLocationBuilderProperty newValue)
        {
            InstallationPath = newValue.Path;
        }

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            InstallationLocationBuilderProperty currentBuilderProperty)
        {
            if (string.IsNullOrWhiteSpace(currentBuilderProperty.Path) &&
                currentBuilderProperties.OfType<InstallBuilderProperty>().First().Install)
                return InputValidationResult.Error((string) Application.Current.Resources["ErrorInstallation"]);

            return InputValidationResult.Successful;
        }
    }
}