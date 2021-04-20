using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class InstallationLocationBuilderProperty : IBuilderProperty
    {
        public string Path { get; set; } = @"%programfiles%\Orcus\Orcus.exe";

        public IBuilderProperty Clone()
        {
            return new InstallationLocationBuilderProperty {Path = Path};
        }
    }
}