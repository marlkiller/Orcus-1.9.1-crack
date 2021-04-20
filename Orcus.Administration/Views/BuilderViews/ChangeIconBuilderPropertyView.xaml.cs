using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for ChangeIconBuilderPropertyView.xaml
    /// </summary>
    public partial class ChangeIconBuilderPropertyView : BuilderPropertyViewUserControl<ChangeIconBuilderProperty>
    {
        private string _iconPath;
        private BitmapImage _selectedIcon;
        private RelayCommand _selectIconPathCommand;

        public ChangeIconBuilderPropertyView()
        {
            InitializeComponent();
        }

        protected override void OnCurrentBuilderPropertyChanged(ChangeIconBuilderProperty newValue)
        {
            SelectIcon(newValue.IconPath);
        }

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties, ChangeIconBuilderProperty currentBuilderProperty)
        {
            if (currentBuilderProperty.ChangeIcon && string.IsNullOrWhiteSpace(IconPath))
                return InputValidationResult.Error((string) Application.Current.Resources["ErrorIcon"]);

            return InputValidationResult.Successful;
        }

        public BitmapImage SelectedIcon
        {
            get { return _selectedIcon; }
            set
            {
                if (value != _selectedIcon)
                {
                    _selectedIcon = value;
                    OnPropertyChanged();
                }
            }
        }

        public string IconPath
        {
            get { return _iconPath; }
            set
            {
                if (_iconPath != value)
                {
                    _iconPath = value;
                    CurrentBuilderProperty.IconPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand SelectIconPathCommand
        {
            get
            {
                return _selectIconPathCommand ?? (_selectIconPathCommand = new RelayCommand(parameter =>
                {
                    var ofd = new OpenFileDialog {CheckFileExists = true, Filter = "Icon|*.ico", Multiselect = false};
                    if (ofd.ShowDialog(Window.GetWindow(this)) == true)
                    {
                        IconPath = ofd.FileName;
                        OnPropertyChanged(nameof(IconPath));
                        SelectIcon(ofd.FileName);
                    }
                }));
            }
        }

        private void SelectIcon(string path)
        {
            SelectedIcon = string.IsNullOrEmpty(path) ? null : new BitmapImage(new Uri(path, UriKind.Absolute));
        }

        public override string[] Tags { get; } = {"Change", "icon", "ändern", "bild", "image"};

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Assembly).ComesAfter<FrameworkVersionBuilderProperty>();
    }
}