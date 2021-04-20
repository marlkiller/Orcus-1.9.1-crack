using System;
using System.Security.Principal;

namespace Orcus.Utilities
{
    internal static class User
    {
        private static WindowsIdentity _user;
        private static bool? _isAdministrator;

        public static WindowsIdentity UserIdentity => _user ?? (_user = WindowsIdentity.GetCurrent());

        public static bool IsAdministrator => _isAdministrator ?? (_isAdministrator = IsUserAdministrator()).Value;

        private static bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                if (UserIdentity == null)
                    return false;

                var principal = new WindowsPrincipal(UserIdentity);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }

            return isAdmin;
        }
    }
}