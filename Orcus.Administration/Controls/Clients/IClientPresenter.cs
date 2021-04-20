using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using Orcus.Administration.Core.ClientManagement;

namespace Orcus.Administration.Controls.Clients
{
    public interface IClientPresenter
    {
        List<ClientViewModel> VisibleClients { get; }
        ContextMenu ItemContextMenu { get; set; }
        ICommands Commands { get; set; }
        IList SelectedItems { get; }

        void UpdateSearchText(FilterParser filterParser);
        void Enable(FilterParser filterParser);
        void Disable();
    }

    public interface ICommands
    {
        ICommand LogInCommand { get; }
        ICommand DeleteCommand { get; }
    }
}