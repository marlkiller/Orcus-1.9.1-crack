using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class HideFileBuilderProperty : IBuilderProperty
    {
        public bool HideFile { get; set; }

        public IBuilderProperty Clone()
        {
            return new HideFileBuilderProperty {HideFile = HideFile};
        }
    }
}