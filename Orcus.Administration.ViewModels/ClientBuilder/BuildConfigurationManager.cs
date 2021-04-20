using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using NLog;
using Orcus.Administration.Core.Build.Configuration;
using Orcus.Administration.Core.Utilities;
using Sorzus.Wpf.Toolkit;
using FileExtensions = Orcus.Shared.Utilities.FileExtensions;

namespace Orcus.Administration.ViewModels.ClientBuilder
{
    public class BuildConfigurationManager : PropertyChangedBase
    {
        private BuildConfigurationViewModel _currentBuildConfiguration;

        public BuildConfigurationManager()
        {
            BuildConfigurations =
                new ObservableCollection<BuildConfigurationViewModel>(
                    BuildConfigurationHelper.LoadBuildConfigurations()?.Select(x => new BuildConfigurationViewModel(x)) ??
                    new BuildConfigurationViewModel[0]);
        }

        public BuildConfigurationViewModel CurrentBuildConfiguration
        {
            get { return _currentBuildConfiguration; }
            set { SetProperty(value, ref _currentBuildConfiguration); }
        }

        public ObservableCollection<BuildConfigurationViewModel> BuildConfigurations { get; }

        public void AddBuildConfiguration(BuildConfiguration buildConfiguration)
        {
            buildConfiguration.CreationDate = DateTime.Now;
            buildConfiguration.LastModified = DateTime.Now;

            var path = Path.Combine(BuildConfigurationHelper.BuildConfigurationFolderName,
                buildConfiguration.Name.RemoveSpecialCharacters() + ".xml");
            path = FileExtensions.MakeUnique(path);

            BuildConfigurationHelper.SaveBuildConfiguration(buildConfiguration, path);

            var buildConfigurationViewModel =
                new BuildConfigurationViewModel(new BuildConfigurationInfo(buildConfiguration, path));
            BuildConfigurations.Add(buildConfigurationViewModel);
            CurrentBuildConfiguration = buildConfigurationViewModel;
        }

        public void UpdateBuildConfiguration(BuildConfigurationViewModel buildConfigurationViewModel, BuildConfiguration buildConfiguration)
        {
            buildConfiguration.CreationDate =
                buildConfigurationViewModel.BuildConfigurationInfo.BuildConfiguration.CreationDate;
            buildConfiguration.LastModified = DateTime.Now;

            var path = buildConfigurationViewModel.BuildConfigurationInfo.Path;

            var fileToDelete = path + $".{Guid.NewGuid().ToString("N")}.delete";
            File.Move(path, fileToDelete);

            string newPath;

            if (buildConfigurationViewModel.BuildConfigurationInfo.BuildConfiguration.Name != buildConfiguration.Name)
            {
                newPath = Path.Combine(BuildConfigurationHelper.BuildConfigurationFolderName,
                    buildConfiguration.Name.RemoveSpecialCharacters() + ".xml");
                newPath = FileExtensions.MakeUnique(newPath);
            }
            else
                newPath = path;

            try
            {
                BuildConfigurationHelper.SaveBuildConfiguration(buildConfiguration, newPath);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, $"Error when serializing build configuration {buildConfiguration.Name}. Reverting changes");

                if (File.Exists(newPath))
                    File.Delete(newPath);

                LogManager.GetCurrentClassLogger().Info($"File removed. Move file \"{fileToDelete}\" to \"{path}\"");
                File.Move(fileToDelete, path);
                LogManager.GetCurrentClassLogger().Info("File moved");

                throw;
            }

            buildConfigurationViewModel.Update(new BuildConfigurationInfo(buildConfiguration, newPath));
        }

        public void RemoveBuildConfiguration(BuildConfigurationViewModel buildConfigurationViewModel)
        {
            BuildConfigurations.Remove(buildConfigurationViewModel);
            File.Delete(buildConfigurationViewModel.BuildConfigurationInfo.Path);
        }
    }
}