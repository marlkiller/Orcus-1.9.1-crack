using System;

namespace Orcus.Shared.Data
{
    public class DataInfoAvailableEventArgs : EventArgs
    {
        public DataInfoAvailableEventArgs(IDataInfo dataInfo)
        {
            DataInfo = dataInfo;
        }

        public IDataInfo DataInfo { get; }
    }
}