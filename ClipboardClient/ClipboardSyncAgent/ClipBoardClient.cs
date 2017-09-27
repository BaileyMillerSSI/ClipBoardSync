using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardSyncAgent
{
    public delegate void OnClipboardBroadcastRecieved(object source, ClipboardBroadcastRecievedArgs e);
    public delegate void OnClipboardErrorRecieved(object source, ClipboardErrorRecievedArgs e);
    public delegate void OnConnectionStatusChanged(object source, ConnectionState e);

    public class ClipBoardClient
    {

        public event OnClipboardBroadcastRecieved OnClipBoardBroadcastRecieved;
        public event OnClipboardErrorRecieved OnClipBoardErrorRecieved;
        public event OnConnectionStatusChanged OnConnectionStatusChanged;

        HubConnection hub;
        IHubProxy HubProxy;

        public ConnectionState HubState
        {
            get { return hub.State; }
        }

        public ClipBoardClient(string serverUrl = "http://localhost:9159/signalr", string hubName = "ClipboardHub")
        {
            hub = new HubConnection(serverUrl);

            hub.StateChanged += Hub_StateChanged;
            
            HubProxy = hub.CreateHubProxy(hubName);

            HubProxy.On<String>("BroadcastClipboardIn", BroadCastClipboardIn);
            HubProxy.On<String>("ErrorRecieved", ErrorRecieved);

        }

        private void Hub_StateChanged(StateChange obj)
        {
            OnConnectionStatusChanged?.Invoke(this, obj.NewState);
        }

        public async Task ConnectToServer()
        {
            await hub.Start();
        }

        public void DisconnectFromServer()
        {
            if (HubState == ConnectionState.Connected)
            {
                hub.Stop();
            }
        }

        public void SendClipBoardToServer(string clipboard)
        {
            if (HubState == ConnectionState.Connected)
            {
                HubProxy.Invoke("BroadcastClipboardOut", clipboard);
            }
        }
        
        private void BroadCastClipboardIn(string newClipboard)
        {
            //Broadcasts an event out
            OnClipBoardBroadcastRecieved?.Invoke(this, new ClipboardBroadcastRecievedArgs(newClipboard));
        }

        private void ErrorRecieved(string eText)
        {
            OnClipBoardErrorRecieved?.Invoke(this, new ClipboardErrorRecievedArgs(eText));
        }

    }

    public class ClipboardBroadcastRecievedArgs : EventArgs
    {
        public string NewClipboardText;
        public ClipboardBroadcastRecievedArgs(string newClipText)
        {
            NewClipboardText = newClipText;
        }
    }

    public class ClipboardErrorRecievedArgs : EventArgs
    {
        public string ErrorText;
        public DateTime TimeRecieved;
        public DateTime TimeRecievedUTC;
        public ClipboardErrorRecievedArgs(string eText)
        {
            ErrorText = eText;
            TimeRecieved = DateTime.Now;
            TimeRecievedUTC = DateTime.UtcNow;
        }
    }
}
