using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class ChangeCreationDateBuilderProperty : IBuilderProperty
    {
        public ChangeCreationDateBuilderProperty()
        {
            NewCreationDate = DateTime.Now;
        }

        public bool IsEnabled { get; set; }

        [SerializeAsUtc]
        public DateTime NewCreationDate { get; set; }

        public IBuilderProperty Clone()
        {
            return new ChangeCreationDateBuilderProperty {IsEnabled = IsEnabled, NewCreationDate = NewCreationDate};
        }
    }
}