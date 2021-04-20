using System.IO;

namespace Orcus.Shared.Utilities
{
    /// <summary>
    ///     Class to write an application configuration file
    /// </summary>
    public static class AppConfigWriter
    {
        private const string FileContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <startup>
    <supportedRuntime version=""v4.0"" sku="".NETFramework,Version=v4.5"" />
    <supportedRuntime version=""v4.0"" />
    <supportedRuntime version=""v2.0.50727"" />
    <supportedRuntime version=""v4.0.30319"" sku="".NETFramework,Version=v4.0,Profile=Client"" />
  </startup>
</configuration>";

        /// <summary>
        ///     Write an app.config file for the given assembly which gurantees support for the .Net Framework versions 3.5, 4.0
        ///     and 4.5
        /// </summary>
        /// <param name="executableFile">The .net assembly</param>
        public static void WriteAppConfig(FileInfo executableFile)
        {
            File.WriteAllText(Path.Combine(executableFile.DirectoryName, executableFile.Name + ".config"), FileContent);
        }
    }
}