using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
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
    ///     Interaction logic for ChangeAssemblyInformationBuilderPropertyView.xaml
    /// </summary>
    public partial class ChangeAssemblyInformationBuilderPropertyView :
        BuilderPropertyViewUserControl<ChangeAssemblyInformationBuilderProperty>
    {
        private ChangeAssemblyInformationBuilderProperty _assemblyInformation;
        private RelayCommand _loadAssemblyInformationFromExistingFileCommand;

        public ChangeAssemblyInformationBuilderPropertyView()
        {
            InitializeComponent();
        }

        public ChangeAssemblyInformationBuilderProperty AssemblyInformation
        {
            get { return _assemblyInformation; }
            set
            {
                if (_assemblyInformation != value)
                {
                    _assemblyInformation = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand LoadAssemblyInformationFromExistingFileCommand
        {
            get
            {
                return _loadAssemblyInformationFromExistingFileCommand ??
                       (_loadAssemblyInformationFromExistingFileCommand = new RelayCommand(parameter =>
                       {
                           var ofd = new OpenFileDialog
                           {
                               Filter = $"{Application.Current.Resources["Executable"]}|*.exe",
                               CheckFileExists = true
                           };

                           if (ofd.ShowDialog(Window.GetWindow(this)) != true)
                               return;

                           try
                           {
                               var fileVersionInfo = FileVersionInfo.GetVersionInfo(ofd.FileName);
                               AssemblyInformation.AssemblyTitle = fileVersionInfo.InternalName;
                               AssemblyInformation.AssemblyDescription = fileVersionInfo.FileDescription;
                               AssemblyInformation.AssemblyCompanyName = fileVersionInfo.CompanyName;
                               AssemblyInformation.AssemblyProductName = fileVersionInfo.ProductName;
                               AssemblyInformation.AssemblyCopyright = fileVersionInfo.LegalCopyright;
                               AssemblyInformation.AssemblyTrademarks = fileVersionInfo.LegalTrademarks;
                               AssemblyInformation.AssemblyFileVersion =
                                   $"{fileVersionInfo.FileMajorPart}.{fileVersionInfo.FileMinorPart}.{fileVersionInfo.FileBuildPart}.0";
                               AssemblyInformation.AssemblyProductVersion =
                                   $"{fileVersionInfo.ProductMajorPart}.{fileVersionInfo.ProductMinorPart}.{fileVersionInfo.ProductBuildPart}.0";
                               AssemblyInformation.ChangeAssemblyInformation = true;

                               var assemblyInformation = AssemblyInformation;
                               AssemblyInformation = null;
                               AssemblyInformation = assemblyInformation;
                           }
                           catch (Exception ex)
                           {
                               MessageBoxEx.Show(Window.GetWindow(this), ex.Message,
                                   (string)Application.Current.Resources["Error"],
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                           }
                       }));
            }
        }

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Assembly).ComesAfter<ChangeIconBuilderProperty>();

        public override string[] Tags { get; } = {
            "assembly", "information", "metadata", "title", "titel", "beschreibung",
            "datei", "file"
        };

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            ChangeAssemblyInformationBuilderProperty currentBuilderProperty)
        {
            return InputValidationResult.Successful;
        }

        protected override void OnCurrentBuilderPropertyChanged(ChangeAssemblyInformationBuilderProperty newValue)
        {
            AssemblyInformation = newValue;
        }
    }
}