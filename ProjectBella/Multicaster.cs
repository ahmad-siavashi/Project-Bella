using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ProjectX
{
    class Multicaster
    {
        public int _CastTimes = 3;

        private int _CastDelay = 5000; // 1000ms = 1sec

        public int CastDelay
        {
            get { return _CastDelay; }
            set { _CastDelay = value; }
        }
        List<UdpClient> _UdpClients;

        public List<UdpClient> UdpClients
        {
            get { return _UdpClients; }
            set { _UdpClients = value; }
        }
        IPAddress _MulticastAddress;

        public IPAddress MulticastAddress
        {
            get { return _MulticastAddress; }
            set { _MulticastAddress = value; }
        }
        IPEndPoint _MulticastGroup;

        public IPEndPoint MulticastGroup
        {
            get { return _MulticastGroup; }
            set { _MulticastGroup = value; }
        }

        public Multicaster(string GroupIP, int MulticastPort)
        {
            MulticastAddress = IPAddress.Parse(GroupIP);
            MulticastGroup = new IPEndPoint(MulticastAddress, MulticastPort);

        }

        public void Cast()
        {
            UdpClients = new List<UdpClient>();
            string HostName = Dns.GetHostName();
            foreach (var IP in (from ip in Dns.GetHostEntry(HostName).AddressList where ip.AddressFamily == AddressFamily.InterNetwork select ip.ToString()).ToList())
            {
                UdpClient UdpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(IP), 0));
                UdpClient.AllowNatTraversal(true);
                UdpClient.JoinMulticastGroup(MulticastGroup.Address);
                UdpClients.Add(UdpClient);
            }
            string Message = "iam:" + HostName;
            byte[] Data = Encoding.ASCII.GetBytes(Message.ToCharArray());
            while (true)
            {
                foreach (UdpClient client in UdpClients)
                {
                    client.BeginSend(Data, Data.Length, MulticastGroup, null, null);
                }
                Thread.Sleep(CastDelay);
            }
        }

        public void Cast(dynamic msg)
        {
            List<UdpClient> UdpClients = new List<UdpClient>();
            string HostName = Dns.GetHostName();
            foreach (var IP in (from ip in Dns.GetHostEntry(HostName).AddressList where ip.AddressFamily == AddressFamily.InterNetwork select ip.ToString()).ToList())
            {
                UdpClient UdpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(IP), 0));
                UdpClient.AllowNatTraversal(true);
                UdpClient.JoinMulticastGroup(MulticastGroup.Address);
                UdpClients.Add(UdpClient);
            }
            if (msg.GetType() == typeof(string[]))
            {
                foreach (var m in msg)
                {
                    byte[] Data = Encoding.ASCII.GetBytes(m.ToCharArray());
                    while (_CastTimes-- > 0)
                    {
                        foreach (UdpClient client in UdpClients)
                        {
                            client.BeginSend(Data, Data.Length, MulticastGroup, null, null);
                        }
                        Thread.Sleep(CastDelay);
                    }
                }
            }
            else
            {
                byte[] Data = Encoding.ASCII.GetBytes(msg.ToCharArray());
                while (_CastTimes-- > 0)
                {
                    foreach (UdpClient client in UdpClients)
                    {
                        client.BeginSend(Data, Data.Length, MulticastGroup, null, null);
                    }
                    Thread.Sleep(CastDelay);
                }
            }
        }
    }
}
