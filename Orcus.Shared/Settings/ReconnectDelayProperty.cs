using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class ReconnectDelayProperty : IBuilderProperty
    {
        public int Delay { get; set; } = 10000;

        public IBuilderProperty Clone()
        {
            return new ReconnectDelayProperty {Delay = Delay};
        }
    }
}