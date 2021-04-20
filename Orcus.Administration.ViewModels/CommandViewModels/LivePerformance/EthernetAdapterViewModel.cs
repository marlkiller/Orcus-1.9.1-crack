using System;
using System.Collections.ObjectModel;
using System.Windows;
using Orcus.Shared.Commands.LivePerformance;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.LivePerformance
{
    public class EthernetAdapterViewModel : PropertyChangedBase
    {
        private int _maximum = 1000;
        private string _maximumString;
        private int _receiveBytes;
        private string _receiveBytesString;
        private int _sendBytes;
        private string _sendBytesString;
        private DateTime _time;

        public EthernetAdapterViewModel(EthernetAdapter ethernetAdapter)
        {
            Points = new ObservableCollection<GraphPoint>();
            _time = DateTime.Now;
            for (int i = 0; i < 60; i++)
            {
                Points.Add(new GraphPoint(_time, 0, 0));
                _time = _time.AddSeconds(1);
            }
            EthernetAdapter = ethernetAdapter;
        }

        public int ReceiveBytes
        {
            get { return _receiveBytes; }
            set
            {
                if (SetProperty(value, ref _receiveBytes))
                    ReceiveBytesString = FormatBytesPerSeconds(value);
            }
        }

        public int SendBytes
        {
            get { return _sendBytes; }
            set
            {
                if (SetProperty(value, ref _sendBytes))
                    SendBytesString = FormatBytesPerSeconds(value);
            }
        }

        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if (SetProperty(value, ref _maximum))
                {
                    MaximumString = FormatBytesPerSeconds(value);
                    MaximumChanged?.Invoke(this, value);
                }
            }
        }

        public string MaximumString
        {
            get { return _maximumString; }
            set { SetProperty(value, ref _maximumString); }
        }

        public string SendBytesString
        {
            get { return _sendBytesString; }
            set { SetProperty(value, ref _sendBytesString); }
        }

        public string ReceiveBytesString
        {
            get { return _receiveBytesString; }
            set { SetProperty(value, ref _receiveBytesString); }
        }

        public Shared.Commands.LivePerformance.EthernetAdapter EthernetAdapter { get; set; }
        public ObservableCollection<GraphPoint> Points { get; }

        public event EventHandler<int> MaximumChanged;

        public void NewData(EthernetAdapterData ethernetAdapterData)
        {
            Points.RemoveAt(0);
            Points.Add(new GraphPoint(_time, ethernetAdapterData.BytesReceive, ethernetAdapterData.BytesSend));
            _time = _time.AddSeconds(1);

            if (ethernetAdapterData.BytesSend > Maximum)
            {
                Maximum = (int) Math.Round(ethernetAdapterData.BytesSend/1000, 0)*1000;
            }
            if (ethernetAdapterData.BytesReceive > Maximum)
            {
                Maximum = (int) Math.Round(ethernetAdapterData.BytesReceive/1000, 0)*1000;
            }

            ReceiveBytes = (int) ethernetAdapterData.BytesReceive;
            SendBytes = (int) ethernetAdapterData.BytesSend;
        }

        private static string FormatBytesPerSeconds(int bytes)
        {
            var ordinals = new[] {"", "K", "M", "G", "T", "P", "E"};

            long bandwidth = bytes*8;

            double rate = bandwidth;

            var ordinal = 0;

            while (rate > 1024)
            {
                rate /= 1024;
                ordinal++;
            }

            return
                $"{Math.Round(rate, 2, MidpointRounding.AwayFromZero)} {ordinals[ordinal] + (string) Application.Current.Resources["BitsPerSecondOrdinal"]}";
        }
    }
}