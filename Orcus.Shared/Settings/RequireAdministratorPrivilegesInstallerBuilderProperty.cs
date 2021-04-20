using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class RequireAdministratorPrivilegesInstallerBuilderProperty : IBuilderProperty
    {
        public bool RequireAdministratorPrivileges { get; set; } = true;

        public IBuilderProperty Clone()
        {
            return new RequireAdministratorPrivilegesInstallerBuilderProperty
            {
                RequireAdministratorPrivileges = RequireAdministratorPrivileges
            };
        }
    }
}