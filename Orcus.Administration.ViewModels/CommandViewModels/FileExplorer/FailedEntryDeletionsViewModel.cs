using System.Collections.Generic;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class FailedEntryDeletionsViewModel
    {
        public FailedEntryDeletionsViewModel(List<EntryDeletionFailed> failedList)
        {
            FailedList = failedList;
        }

        public List<EntryDeletionFailed> FailedList { get; }
    }
}