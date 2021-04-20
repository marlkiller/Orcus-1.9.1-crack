using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class SetRunProgramAsAdminFlagBuilderProperty : IBuilderProperty
    {
        public bool SetFlag { get; set; }

        public IBuilderProperty Clone()
        {
            return new SetRunProgramAsAdminFlagBuilderProperty {SetFlag = SetFlag};
        }
    }
}