using System.Collections.Generic;
using System.Linq;
using Orcus.Administration.Plugins.BuildPlugin;

namespace Orcus.Administration.ViewModels.ClientBuilder
{
    public class BuilderInformation : IBuilderInformation
    {
        public BuilderInformation(string mainAssemlyPath, IBuildLogger buildLogger)
        {
            OutputFiles = new List<OutputFile> {new OutputFile(mainAssemlyPath, OutputFileType.MainAssembly)};
            BuildLogger = buildLogger;
        }

        public string AssemblyPath => OutputFiles.Single(x => x.OutputFileType == OutputFileType.MainAssembly).Path;
        public List<OutputFile> OutputFiles { get; }
        public IBuildLogger BuildLogger { get; }
    }
}