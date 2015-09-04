using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
namespace ProjectX
{


    public class IPEndPointEx : IPEndPoint
    {
        public IPEndPointEx(IPAddress address, int port)
            : base(address, port)
        {
        }
        string _LastSeen;

        public string LastSeen
        {
            get { return _LastSeen; }
            set { _LastSeen = value; }
        }
    }
    public class Host
    {
        private static Mutex AddHostMux = new Mutex();
        private static ObservableCollection<Host> _Hosts = new ObservableCollection<Host>();
        public static ObservableCollection<Host> Hosts
        {

            get { return Host._Hosts; }
            set { Host._Hosts = value; }
        }

        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        private ObservableCollection<IPEndPointEx> _Entries = new ObservableCollection<IPEndPointEx>();

        public ObservableCollection<IPEndPointEx> Entries
        {
            get { return _Entries; }
            set { _Entries = value; }
        }
        public static bool AddHost(string Name, string Address, string Port)
        {
            AddHostMux.WaitOne();
            Host _Host = null;
            if (Hosts.Any(H => H.Name.Equals(Name)) == true)
                _Host = Hosts.Where(H => H.Name == Name).Select(H => H).ToList()[0];
            try
            {
                if (_Host == null)
                {
                    IPEndPointEx IPEndPoint = new IPEndPointEx(IPAddress.Parse(Address), int.Parse(Port));
                    IPEndPoint.LastSeen = DateTime.Now.ToString("HH:mm:ss tt");
                    _Host = new Host() { Name = Name };
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (Hosts.Any(H => H.Name.Equals(Name)) == false)
                            Hosts.Add(_Host);
                        if (_Host.Entries.Any(E => E.Equals(IPEndPoint)) == false)
                            _Host.Entries.Add(IPEndPoint);
                    });
                }
                else
                {
                    IPEndPointEx IPEndPoint = null;
                    if (_Host.Entries.Any(E => E.Address.Equals(IPAddress.Parse(Address)) && E.Port.Equals(int.Parse(Port))) == true)
                    {
                        IPEndPoint = _Host.Entries.Where(E => E.Address.Equals(IPAddress.Parse(Address)) && E.Port.Equals(int.Parse(Port))).Select(H => H).ToList()[0];
                        IPEndPoint.LastSeen = DateTime.Now.ToString("HH:mm:ss tt");
                    }
                    if (IPEndPoint == null)
                    {
                        IPEndPointEx NewIPEndPoint = new IPEndPointEx(IPAddress.Parse(Address), int.Parse(Port));
                        NewIPEndPoint.LastSeen = DateTime.Now.ToString("HH:mm:ss tt");
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            if (_Host.Entries.Any(E => E.Address == IPAddress.Parse(Address) && E.Port == int.Parse(Port) && E.LastSeen == NewIPEndPoint.LastSeen) == false)
                                _Host.Entries.Add(NewIPEndPoint);
                        });
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            AddHostMux.ReleaseMutex();
            return true;
        }


    }
}
