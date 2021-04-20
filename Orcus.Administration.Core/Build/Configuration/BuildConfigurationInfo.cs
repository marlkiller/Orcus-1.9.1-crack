namespace Orcus.Administration.Core.Build.Configuration
{
    public class BuildConfigurationInfo
    {
        public BuildConfigurationInfo(BuildConfiguration buildConfiguration, string path)
        {
            BuildConfiguration = buildConfiguration;
            Path = path;
        }

        public BuildConfiguration BuildConfiguration { get; }
        public string Path { get; }
    }
}