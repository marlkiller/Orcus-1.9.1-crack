using System;

namespace Orcus.Shared.Connection
{
    [Serializable]
    public class LocationInfo
    {
        public string Country { get; set; }
        public string CountryName { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string ZipCode { get; set; }
        public int Timezone { get; set; }
    }

    [Serializable]
    public class ClientLocation : LocationInfo
    {
        public int ClientId { get; set; }
        public string IpAddress { get; set; }
    }
}