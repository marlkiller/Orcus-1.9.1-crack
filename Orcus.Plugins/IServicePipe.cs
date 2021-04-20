using System.Collections.Generic;
using System.ServiceModel;
using Orcus.Shared.Commands.EventLog;
using Orcus.Shared.Commands.Registry;

namespace Orcus.Plugins
{
    /// <summary>
    ///     Named pipeline connection to the Windows service
    /// </summary>
    [ServiceContract]
    public interface IServicePipe
    {
        /// <summary>
        ///     Equal to File.WriteAllText()
        /// </summary>
        /// <returns>False, if an exception was thrown</returns>
        [OperationContract]
        bool WriteFile(string fileName, string content);

        /// <summary>
        ///     Equal to Process.Start(<see cref="path" />, <see cref="arguments" />)
        /// </summary>
        /// <returns>Null, if no exception was thrown, else the string contains the message</returns>
        [OperationContract]
        string StartProcess(string path, string arguments);

        /// <summary>
        ///     Equal to File.Delete
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>False, if an exception was thrown</returns>
        [OperationContract]
        bool DeleteFile(string fileName);

        /// <summary>
        ///     Return all registry sub keys
        /// </summary>
        [OperationContract]
        List<RegistrySubKey> GetRegistrySubKeys(string path, RegistryHive registryHive);

        /// <summary>
        ///     Return all registry values
        /// </summary>
        [OperationContract]
        List<RegistryValue> GetRegistryValues(string path, RegistryHive registryHive);

        /// <summary>
        ///     Create a new registry sub key
        /// </summary>
        /// <returns>False, if an exception was thrown</returns>
        [OperationContract]
        bool CreateSubKey(string path, RegistryHive registryHive);

        /// <summary>
        ///     Create a new registry value
        /// </summary>
        /// <returns>False, if an exception was thrown</returns>
        [OperationContract]
        bool CreateValue(string path, RegistryHive registryHive, RegistryValue registryValue);

        /// <summary>
        ///     Delete a registry value
        /// </summary>
        /// <returns>False, if an exception was thrown</returns>
        [OperationContract]
        bool DeleteValue(string path, RegistryHive registryHive, string name);

        /// <summary>
        ///     Delete a registry sub key
        /// </summary>
        /// <returns>False, if an exception was thrown</returns>
        [OperationContract]
        bool DeleteSubKey(string path, RegistryHive registryHive);

        /// <summary>
        ///     Return the windows security event log
        /// </summary>
        [OperationContract]
        List<EventLogEntry> GetSecurityEventLog(int entryCount);

        /// <summary>
        ///     Return, if the service is running
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool IsAlive();

        /// <summary>
        ///     Return the path to the service file
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string GetPath();
    }
}