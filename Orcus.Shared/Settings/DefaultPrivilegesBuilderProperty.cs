using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class DefaultPrivilegesBuilderProperty : IBuilderProperty
    {
        public bool RequireAdministratorRights { get; set; }

        public IBuilderProperty Clone()
        {
            return new DefaultPrivilegesBuilderProperty {RequireAdministratorRights = RequireAdministratorRights};
        }
    }
}