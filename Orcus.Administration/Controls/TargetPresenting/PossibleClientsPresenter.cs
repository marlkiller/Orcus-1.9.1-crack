using System.Collections.Generic;
using System.Linq;
using Orcus.Administration.Core.ClientManagement;

namespace Orcus.Administration.Controls.TargetPresenting
{
    public class PossibleClientsPresenter : PossibleTargetPresenter
    {
        public List<ClientViewModel> Clients { get; set; }
        public override List<object> Targets => Clients.Cast<object>().ToList();
    }
}