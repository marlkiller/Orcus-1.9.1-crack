using System.Collections.Generic;

namespace Orcus.Administration.Controls.TargetPresenting
{
    public abstract class PossibleTargetPresenter
    {
        public abstract List<object> Targets { get; }
    }
}