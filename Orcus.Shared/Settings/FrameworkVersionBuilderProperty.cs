using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class FrameworkVersionBuilderProperty : IBuilderProperty
    {
        public FrameworkVersion FrameworkVersion { get; set; }

        public IBuilderProperty Clone()
        {
            return new FrameworkVersionBuilderProperty {FrameworkVersion = FrameworkVersion};
        }
    }
}