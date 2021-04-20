using Orcus.Shared.Commands.ComputerInformation;

namespace Orcus.Administration.ViewModels
{
    public class ComputerInformationViewModel
    {
        public ComputerInformationViewModel(ComputerInformation computerInformation)
        {
            ComputerInformation = computerInformation;
        }

        public ComputerInformation ComputerInformation { get; }
    }
}