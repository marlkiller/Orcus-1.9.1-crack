using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class MutexBuilderProperty : IBuilderProperty
    {
        public MutexBuilderProperty()
        {
            Mutex = Guid.NewGuid().ToString("N");
        }

        public string Mutex { get; set; }

        public IBuilderProperty Clone()
        {
            return new MutexBuilderProperty {Mutex = Mutex};
        }
    }
}