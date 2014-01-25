using System;
using System.Net;
using System.Net.Sockets;
using Org.Mentalis.Proxy.Http;
using HttpListener = Org.Mentalis.Proxy.Http.HttpListener;

namespace Org.Mentalis.Proxy.HttpNms
{
    public class HttpNmsListener:HttpListener
    {
        public HttpNmsListener(int Port) : base(Port)
        {
        }

        public HttpNmsListener(IPAddress Address, int Port) : base(Address, Port)
        {
        }

        public override void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket NewSocket = ListenSocket.EndAccept(ar);
                if (NewSocket != null)
                {
                    var NewClient = new HttpNmsClient(NewSocket, new DestroyDelegate(this.RemoveClient));
                    AddClient(NewClient);
                    NewClient.StartHandshake();
                }
            }
            catch { }
            try
            {
                //Restart Listening
                ListenSocket.BeginAccept(new AsyncCallback(this.OnAccept), ListenSocket);
            }
            catch
            {
                Dispose();
            }
        }
    }
}
