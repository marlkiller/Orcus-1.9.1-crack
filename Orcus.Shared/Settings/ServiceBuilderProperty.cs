using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class ServiceBuilderProperty : IBuilderProperty
    {
        public bool Install { get; set; }

        public IBuilderProperty Clone()
        {
            return new ServiceBuilderProperty {Install = Install};
        }
    }
}