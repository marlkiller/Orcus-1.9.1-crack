using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orcus.Administration.Core;
using Orcus.Administration.ViewModels.ClientMap;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class ClientMapViewModel : PropertyChangedBase
    {
        private readonly ConnectionManager _connectionManager;
        private List<ClientMarker> _clientLocations;

        public ClientMapViewModel(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            Load();
        }

        public List<ClientMarker> ClientLocations
        {
            get { return _clientLocations; }
            set { SetProperty(value, ref _clientLocations); }
        }

        private async void Load()
        {
            var locations = await Task.Run(() => _connectionManager.GetClientLocations());
            var result = new List<ClientMarker>();
            foreach (var clientLocation in locations)
            {
                result.Add(new ClientMarker(clientLocation,
                    _connectionManager.ClientProvider.Clients.First(x => x.Id == clientLocation.ClientId)));
            }

            ClientLocations = result;
        }
    }
}