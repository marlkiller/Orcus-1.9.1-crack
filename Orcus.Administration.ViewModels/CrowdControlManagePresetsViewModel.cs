using System.Linq;
using System.Windows;
using System.Windows.Data;
using Orcus.Administration.Core;
using Orcus.Administration.Core.CrowdControl;
using Orcus.Administration.ViewModels.ViewInterface;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class CrowdControlManagePresetsViewModel : PropertyChangedBase
    {
        private readonly ConnectionManager _connectionManager;
        private RelayCommand _createPresetCommand;
        private RelayCommand _createPresetWithTargetCommand;
        private RelayCommand _removeShortcutCommand;

        public CrowdControlManagePresetsViewModel(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            PresetsViewSource = new CollectionViewSource {Source = CrowdControlPresets.Current.Presets};
            PresetsViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Preset.IsCommandPreset"));
        }

        public CollectionViewSource PresetsViewSource { get; }

        public RelayCommand RemovePresetCommand
        {
            get
            {
                return _removeShortcutCommand ?? (_removeShortcutCommand = new RelayCommand(parameter =>
                {
                    var shortcutInfo = parameter as PresetInfo;
                    CrowdControlPresets.Current.RemovePreset(shortcutInfo);
                }));
            }
        }

        public RelayCommand CreatePresetWithTargetCommand
        {
            get
            {
                return _createPresetWithTargetCommand ?? (_createPresetWithTargetCommand = new RelayCommand(parameter =>
                {
                    var createTaskViewModel = new CrowdControlCreateTaskViewModel(_connectionManager,
                        (string) Application.Current.Resources["CreatePresetCommandWithTarget"],
                        CreationMode.Complete | CreationMode.WithName);

                    if (WindowServiceInterface.Current.OpenWindowDialog(createTaskViewModel) == true)
                    {
                        CrowdControlPresets.Current.AddPreset(new PresetInfo
                        {
                            Name = createTaskViewModel.CustomName,
                            Preset =
                                new CommandPresetWithTarget
                                {
                                    CommandTarget = createTaskViewModel.CommandTarget,
                                    Conditions = createTaskViewModel.Conditions.ToList(),
                                    ExecutionEvent = createTaskViewModel.ExecutionEvent,
                                    StaticCommand = createTaskViewModel.SelectedCommand,
                                    TransmissionEvent = createTaskViewModel.TransmissionEvent,
                                    StopEvent = createTaskViewModel.StopEvent
                                }
                        });
                    }
                }));
            }
        }

        public RelayCommand CreatePresetCommand
        {
            get
            {
                return _createPresetCommand ?? (_createPresetCommand = new RelayCommand(parameter =>
                {
                    var createTaskViewModel = new CrowdControlCreateTaskViewModel(_connectionManager,
                        (string) Application.Current.Resources["CreatePresetCommand"],
                        CreationMode.NoTarget | CreationMode.WithName);

                    if (WindowServiceInterface.Current.OpenWindowDialog(createTaskViewModel) == true)
                    {
                        CrowdControlPresets.Current.AddPreset(new PresetInfo
                        {
                            Name = createTaskViewModel.CustomName,
                            Preset =
                                new CommandPreset
                                {
                                    ExecutionEvent = createTaskViewModel.ExecutionEvent,
                                    StaticCommand = createTaskViewModel.SelectedCommand,
                                    TransmissionEvent = createTaskViewModel.TransmissionEvent,
                                    StopEvent = createTaskViewModel.StopEvent
                                }
                        });
                    }
                }));
            }
        }
    }
}