using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ProjectX
{
    class TCPServer
    {
        TcpListener Server;
        public TCPServer(int Port)
        {
            Server = new TcpListener(IPAddress.Any, Port);

        }
        public void Start()
        {
            Server.Start();
            while (true)
            {
                Receive(Server.AcceptSocket());   
            }
        }
        public void Receive(Socket socket)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                ConnectionReceiveWindow ReceiveWindow = new ConnectionReceiveWindow(socket);
                ReceiveWindow.Show();
            });
        }
    }
}
