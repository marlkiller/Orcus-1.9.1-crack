using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class ChangeIconBuilderProperty : IBuilderProperty
    {
        public bool ChangeIcon { get; set; }
        public string IconPath { get; set; }

        public IBuilderProperty Clone()
        {
            return new ChangeIconBuilderProperty {ChangeIcon = ChangeIcon, IconPath = IconPath};
        }
    }
}