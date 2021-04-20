using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class DisableInstallationPromptBuilderProperty : IBuilderProperty
    {
        public bool IsDisabled { get; set; }

        public IBuilderProperty Clone()
        {
            return new DisableInstallationPromptBuilderProperty {IsDisabled = IsDisabled};
        }
    }
}