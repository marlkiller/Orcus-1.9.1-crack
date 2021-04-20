using Orcus.Shared.Commands.Password;

namespace Orcus.Administration.ViewModels
{
    public class PasswordsViewModel
    {
        public PasswordsViewModel(PasswordData passwordData)
        {
            PasswordData = passwordData;
        }

        public PasswordData PasswordData { get; }
    }
}