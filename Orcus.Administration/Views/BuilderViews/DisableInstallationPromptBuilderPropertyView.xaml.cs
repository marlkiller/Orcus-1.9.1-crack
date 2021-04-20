using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for DisableInstallationPromptBuilderPropertyView.xaml
    /// </summary>
    public partial class DisableInstallationPromptBuilderPropertyView :
        BuilderPropertyViewUserControl<DisableInstallationPromptBuilderProperty>, INotifyPropertyChanged
    {
        public DisableInstallationPromptBuilderPropertyView()
        {
            InitializeComponent();
        }

        public bool DisableInstallationPrompt
        {
            get { return CurrentBuilderProperty?.IsDisabled ?? false; }
            set
            {
                if (value == CurrentBuilderProperty.IsDisabled)
                    return;

                if (value &&
                    MessageBoxEx.Show(Window.GetWindow(this),
                        (string) Application.Current.Resources["AgreeDisableInstallationPrompt"],
                        (string) Application.Current.Resources["DisableInstallationPrompt"], MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation,
                        MessageBoxResult.No) != MessageBoxResult.Yes)
                    return;

                CurrentBuilderProperty.IsDisabled = value;
                OnPropertyChanged();
            }
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Installation).InGroup(BuilderGroup.Install);

        public override string[] Tags { get; } = {
            "disable", "installation", "prompt", "window", "installieren",
            "fenster"
        };

        protected override void OnCurrentBuilderPropertyChanged(DisableInstallationPromptBuilderProperty newValue)
        {
            DisableInstallationPrompt = newValue.IsDisabled;
        }

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            DisableInstallationPromptBuilderProperty currentBuilderProperty)
        {
            return InputValidationResult.Successful;
        }
    }
}