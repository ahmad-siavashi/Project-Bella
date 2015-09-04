using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using PBellaP;

namespace ProjectX
{
    class Listener
    {
        IPEndPoint _MulticastAddress;

        public IPEndPoint MulticastAddress
        {
            get { return _MulticastAddress; }
            set { _MulticastAddress = value; }
        }
        IPEndPoint _LocalEntryPoint;

        public IPEndPoint LocalEntryPoint
        {
            get { return _LocalEntryPoint; }
            set { _LocalEntryPoint = value; }
        }

        public Listener(string GroupIP, int MulticastPort)
        {
            MulticastAddress = new IPEndPoint(IPAddress.Parse(GroupIP), MulticastPort);
        }

        public void ReceiveCallBack(IAsyncResult Result)
        {
            var Args = (object[])Result.AsyncState;
            var UdpClient = (UdpClient) Args[0];
            byte[] Data = UdpClient.EndReceive(Result, ref _LocalEntryPoint);
            string Message = Encoding.ASCII.GetString(Data);
            string[] MessageParts = Message.Split(':');
            switch (MessageParts[0])
            {
                case "iam":
                    Host.AddHost(MessageParts[1], _LocalEntryPoint.Address.ToString(), _LocalEntryPoint.Port.ToString());
                    break;
                case "give":
                    SearchWindow.Search(MessageParts[1], _LocalEntryPoint.Address.ToString());
                    break;
                case "have":
                    SearchWindow.NewResult(MessageParts[1], MessageParts[2], _LocalEntryPoint.Address.ToString());
                    break;
            }
            UdpClient.BeginReceive(ReceiveCallBack, Args);
        }

        public void Listen()
        {
            foreach (var IP in (from ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList where ip.AddressFamily == AddressFamily.InterNetwork select ip.ToString()).ToList())
            {
                UdpClient UdpClient = new UdpClient();
                UdpClient.AllowNatTraversal(true);
                LocalEntryPoint = new IPEndPoint(IPAddress.Parse(IP), MulticastAddress.Port);
                UdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                UdpClient.Client.Bind(LocalEntryPoint);
                UdpClient.JoinMulticastGroup(MulticastAddress.Address, IPAddress.Parse(IP));
                UdpClient.BeginReceive(ReceiveCallBack, new object[] {
                     UdpClient, new IPEndPoint(IPAddress.Parse(IP), ((IPEndPoint)UdpClient.Client.LocalEndPoint).Port)
                    });
            }
        }
    }
}
