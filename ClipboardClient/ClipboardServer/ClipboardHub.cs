using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace ClipboardServer
{
    [HubName("ClipboardHub")]
    public class ClipboardHub : Hub
    {
        public void BroadcastClipboardOut(string clipboard)
        {
            Clients.AllExcept(this.Context.ConnectionId).BroadcastClipboardIn(clipboard);
        }

        public void BroadCastError(string eText)
        {
            Clients.All.ErrorRecieved(eText);
        }
    }
}