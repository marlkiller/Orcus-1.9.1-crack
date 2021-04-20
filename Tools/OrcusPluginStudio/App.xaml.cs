using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Fclp;
using OrcusPluginStudio.Core;

namespace OrcusPluginStudio
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var p = new FluentCommandLineParser<CommandLineSettings> {IsCaseSensitive = false};
            p.Setup(x => x.Project).As('p', "Project");
            p.Setup(x => x.Compile).As('c', "Compile");

            var result = p.Parse(Environment.GetCommandLineArgs());
            if (result.HasErrors)
            {
                MessageBox.Show(result.ErrorText, "Command Line Parser", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(-1);
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

            OrcusPluginProject pluginProject = null;

            if (!string.IsNullOrEmpty(p.Object.Project))
            {
                var pluginFile = new FileInfo(p.Object.Project);
                if (pluginFile.Exists)
                {
                    try
                    {
                        pluginProject = OrcusPluginProjectUtilities.LoadPluginProject(p.Object.Project);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Project could not be loaded", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }

                    if (!string.IsNullOrEmpty(p.Object.Compile))
                    {
                        try
                        {
                            Builder.BuildPlugin(pluginProject, p.Object.Compile);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), "Build", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            Environment.Exit(-1);
                        }

                        Environment.Exit(0);
                    }
                }
            }

            new MainWindow(pluginProject, p.Object.Project).Show();
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var file = new FileInfo(args.Name.Split(',')[0] + ".dll");
                if (file.Exists)
                    return Assembly.LoadFile(file.FullName);
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }
    }
}