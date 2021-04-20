using System.IO;
using System.Windows;
using Mono.Cecil;
using Orcus.Administration.Plugins.BuildPlugin;

namespace Orcus.Administration.Core.Build
{
    public class AssemblyResolver : IAssemblyResolver
    {
        private readonly IBuildLogger _buildLogger;

        public AssemblyResolver(IBuildLogger buildLogger)
        {
            _buildLogger = buildLogger;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return ResolveInternal(name.Name);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            return ResolveInternal(name.Name);
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            return ResolveInternal(fullName);
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            return ResolveInternal(fullName);
        }

        private AssemblyDefinition ResolveInternal(string fullName)
        {
            if (fullName == "System" || fullName == "mscorlib")
                return null;

            _buildLogger.Status(string.Format((string) Application.Current.Resources["BuildStatusResolveLibrary"],
                fullName));

            var file = new FileInfo(fullName + ".dll");
            if (!file.Exists)
                file = new FileInfo(Path.Combine("libraries", fullName + ".dll"));

            if (file.Exists)
                _buildLogger.Success((string) Application.Current.Resources["BuildStatusAssemblyResolved"]);

            return file.Exists ? AssemblyDefinition.ReadAssembly(file.FullName) : null;
        }
    }
}