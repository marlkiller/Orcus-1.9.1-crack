using Orcus.Administration.Core.ClientManagement;
using Orcus.Shared.Connection;

namespace Orcus.Administration.ViewModels.ClientMap
{
    public class ClientMarker : ClientLocation
    {
        public ClientMarker(ClientLocation clientLocation, ClientViewModel client)
        {
            Client = client;
            Country = clientLocation.Country;
            CountryName = clientLocation.CountryName;
            Region = clientLocation.Region;
            City = clientLocation.City;
            Latitude = clientLocation.Latitude;
            Longitude = clientLocation.Longitude;
            ZipCode = clientLocation.ZipCode;
            Timezone = clientLocation.Timezone;
            IpAddress = clientLocation.IpAddress;
        }

        public ClientViewModel Client { get; }
        public string TimezoneString => $"{Timezone.ToString("+00;-00;+00")}:00";
    }
}