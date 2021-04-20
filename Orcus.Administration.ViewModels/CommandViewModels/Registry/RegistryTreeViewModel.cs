using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Orcus.Administration.Commands.Registry;
using Orcus.Administration.FileExplorer.Helpers;
using Orcus.Administration.FileExplorer.Models;
using Orcus.Shared.Commands.Registry;

namespace Orcus.Administration.ViewModels.CommandViewModels.Registry
{
    public class RegistryTreeViewModel : ISupportTreeSelector<SubKeyNodeViewModel, AdvancedRegistrySubKey>
    {
        public RegistryTreeViewModel(RegistryCommand registryCommand)
        {
            Entries = new EntriesHelper<SubKeyNodeViewModel>();
            Selection = new TreeRootSelector<SubKeyNodeViewModel, AdvancedRegistrySubKey>(Entries)
            {
                Comparers = new[] {new RegistryPathComparer()}
            };

            var hiveEntries = new List<RegistryHive>
            {
                RegistryHive.ClassesRoot,
                RegistryHive.CurrentUser,
                RegistryHive.LocalMachine,
                RegistryHive.Users,
                RegistryHive.CurrentConfig
            };

            Entries.SetEntries(UpdateMode.Replace,
                hiveEntries.ToDictionary(x => x, y => y.ToReadableString()).Select(
                    x =>
                        new SubKeyNodeViewModel(this,
                            new AdvancedRegistrySubKey
                            {
                                IsEmpty = false,
                                Name = x.Value,
                                Path = x.Value,
                                RegistryHive = x.Key,
                                RelativePath = ""
                            }, null, registryCommand)).ToArray());
        }

        public IEntriesHelper<SubKeyNodeViewModel> Entries { get; set; }
        public ITreeSelector<SubKeyNodeViewModel, AdvancedRegistrySubKey> Selection { get; set; }
        public ObservableCollection<SubKeyNodeViewModel> AutoCompleteEntries => Entries.All;

        public async Task SelectAsync(AdvancedRegistrySubKey value)
        {
            await Selection.LookupAsync(value,
                RecrusiveSearch<SubKeyNodeViewModel, AdvancedRegistrySubKey>.LoadSubentriesIfNotLoaded,
                SetSelected<SubKeyNodeViewModel, AdvancedRegistrySubKey>.WhenSelected,
                SetExpanded<SubKeyNodeViewModel, AdvancedRegistrySubKey>.WhenChildSelected);
        }
    }
}