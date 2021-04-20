using System.Collections.Generic;
using System.ServiceModel;
using Orcus.Shared.Commands.EventLog;
using Orcus.Shared.Commands.Registry;

namespace Orcus.Service
{
    [ServiceContract]
    public interface IServicePipe
    {
        [OperationContract]
        bool WriteFile(string fileName, string content);

        [OperationContract]
        string StartProcess(string path, string arguments);

        [OperationContract]
        bool DeleteFile(string fileName);

        [OperationContract]
        List<RegistrySubKey> GetRegistrySubKeys(string path, RegistryHive registryHive);

        [OperationContract]
        List<RegistryValue> GetRegistryValues(string path, RegistryHive registryHive);

        [OperationContract]
        bool CreateSubKey(string path, RegistryHive registryHive);

        [OperationContract]
        bool CreateValue(string path, RegistryHive registryHive, RegistryValue registryValue);

        [OperationContract]
        bool DeleteValue(string path, RegistryHive registryHive, string name);

        [OperationContract]
        bool DeleteSubKey(string path, RegistryHive registryHive);

        [OperationContract]
        List<EventLogEntry> GetSecurityEventLog(int entryCount);

        [OperationContract]
        bool IsAlive();

        [OperationContract]
        string GetPath();
    }
}