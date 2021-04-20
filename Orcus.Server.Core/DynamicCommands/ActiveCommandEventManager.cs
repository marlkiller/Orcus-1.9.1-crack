using System;

namespace Orcus.Server.Core.DynamicCommands
{
    public class ActiveCommandEventManager
    {
        public event EventHandler<ActiveCommandInfo> ActiveCommandAdded;
        public event EventHandler<ActiveCommandInfo> ActiveCommandRemoved;
        public event EventHandler<ActiveCommandClientEventArgs> ClientAdded;
        public event EventHandler<ActiveCommandClientEventArgs> ClientRemoved;

        public void AddActiveCommand(ActiveCommandInfo activeCommandInfo)
        {
            ActiveCommandAdded?.Invoke(this, activeCommandInfo);
        }

        public void RemoveActiveCommand(ActiveCommandInfo activeCommandInfo)
        {
            ActiveCommandRemoved?.Invoke(this, activeCommandInfo);
        }

        public void AddClient(ActiveCommandInfo activeCommandInfo, Client client)
        {
            ClientAdded?.Invoke(this, new ActiveCommandClientEventArgs(activeCommandInfo, client));
        }

        public void RemoveClient(ActiveCommandInfo activeCommandInfo, Client client)
        {
            ClientRemoved?.Invoke(this, new ActiveCommandClientEventArgs(activeCommandInfo, client));
        }
    }

    public class ActiveCommandClientEventArgs : EventArgs
    {
        public ActiveCommandClientEventArgs(ActiveCommandInfo activeCommandInfo, Client client)
        {
            ActiveCommandInfo = activeCommandInfo;
            Client = client;
        }

        public ActiveCommandInfo ActiveCommandInfo { get; }
        public Client Client { get; }
    }
}