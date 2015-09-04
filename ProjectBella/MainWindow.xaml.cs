using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using PBellaP;
namespace ProjectX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Multicaster _Multicaster;
        Thread MulticasterThread;
        Thread ListenerThread;
        Thread TCPServerThread;
        Thread RefreshViewThread;
        public const string MulticastIP = "224.1.9.93";
        public const int MulticastPort = 1993;
        public const int ConnectionPort = 1372;
        public MainWindow()
        {
            InitializeComponent();
            lstAvailableHosts.ItemsSource = Host.Hosts;
            _Multicaster = new Multicaster(MulticastIP, MulticastPort);
            Listener _Listener = new Listener(MulticastIP, MulticastPort);
            MulticasterThread = new Thread(new ThreadStart(_Multicaster.Cast));
            ListenerThread = new Thread(new ThreadStart(_Listener.Listen));
            MulticasterThread.Start();
            ListenerThread.Start();
            RefreshViewThread = new Thread(new ThreadStart(RefreshView));
            RefreshViewThread.Start();
            TCPServer _TCPServer = new TCPServer(ConnectionPort);
            TCPServerThread = new Thread(new ThreadStart(_TCPServer.Start));
            TCPServerThread.Start();
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MulticasterThread.Abort();
            ListenerThread.Abort();
            Environment.Exit(0);
        }

        private void lstAvailableHosts_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            Host _Host = (Host)lstAvailableHosts.SelectedItem;
            if (_Host != null)
            {
                lstDetails.ItemsSource = _Host.Entries;
            }
        }

        public void RefreshView()
        {
            while (true)
            {
                Thread.Sleep(1000);
                Dispatcher.Invoke(() =>
                {
                    lstAvailableHosts.Items.Refresh();
                    lstDetails.Items.Refresh();
                });
            }
        }

        private void lstDetails_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ConnectToPeer PeerWindow = new ConnectToPeer((Host) lstAvailableHosts.SelectedItem ,(IPEndPointEx)lstDetails.SelectedItem, ConnectionPort);
            PeerWindow.Show();
        }

        private void Search_Menu(object sender, RoutedEventArgs e)
        {
            SearchWindow SearchWindow = new SearchWindow();
        }

        private void ShareFile_Menu(object sender, RoutedEventArgs e)
        {
            FileSharing FileShare = new FileSharing();
            FileShare.Show();
        }

        private void About_Menu(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Bella\n P2P Application\n Written By: Ahmad Siavashi\n Email: a.siavosh@yahoo.com\n Spring 2014\n Shiraz University");
        }

    }
}
