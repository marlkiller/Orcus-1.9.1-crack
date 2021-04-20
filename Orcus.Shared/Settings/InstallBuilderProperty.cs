using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class InstallBuilderProperty : IBuilderProperty
    {
        public bool Install { get; set; } = true;

        public IBuilderProperty Clone()
        {
            return new InstallBuilderProperty {Install = Install};
        }
    }
}