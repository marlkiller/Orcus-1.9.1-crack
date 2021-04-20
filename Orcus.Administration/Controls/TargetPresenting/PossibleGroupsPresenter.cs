using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Orcus.Administration.Controls.TargetPresenting
{
    public class PossibleGroupsPresenter : PossibleTargetPresenter
    {
        public ObservableCollection<string> Group { get; set; }
        public override List<object> Targets => Group.Cast<object>().ToList();
    }
}