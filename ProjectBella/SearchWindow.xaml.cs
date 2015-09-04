using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using ProjectX;

namespace PBellaP
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public class SearchResult
    {
        public string Path;
        public long Size;
    }

    public partial class SearchWindow : Window
    {
        bool isSearching = false;
        public static SearchWindow _CurrentSearchWindow = null;
        public SearchWindow()
        {
            InitializeComponent();
            if (_CurrentSearchWindow == null)
            {
                _CurrentSearchWindow = this;
                this.Show();
            }
            else
            {
                MessageBox.Show("You can't open more than one Search Window at a time.");
                this.Close();
            }
        }
        public static void Search(string FileName, string Addr)
        {
            List<SearchResult> Results = new List<SearchResult>();

            foreach (var r in FileSharing.sharedFiles.Select(S => S).Where(S => S.Contains(FileName)).ToList())
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(r);
                Results.Add(new SearchResult { Path = r, Size = fi.Length});
            }

            if (Results.Count != 0)
            {
                FileSharing.Tell(Results, Addr);
            }
        }

        public static void NewResult(string Result, string size, string Addr)
        {
            var x = new { FileName = Result, FileSize = (long.Parse(size) / (1024)).ToString(), IPAddr = Addr };
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                if (_CurrentSearchWindow != null && Result.Contains(_CurrentSearchWindow.txtSearch.Text) && !_CurrentSearchWindow.lstResults.Items.Contains(x))
                {
                    _CurrentSearchWindow.lstResults.Items.Add(x);
                }
            });
        }

        public void Get_Click(object sender, RoutedEventArgs e)
        {
            ConnectToPeer Peer = new ConnectToPeer();
            Peer.TCPConnect(((dynamic)lstResults.SelectedItem).IPAddr);
            Peer.SendMsg("sendme:" + ((dynamic)lstResults.SelectedItem).FileName + ":" + ((dynamic)lstResults.SelectedItem).FileSize + ":" + txtParts.Text);
        }

        public void Search_Click(object sender, RoutedEventArgs e)
        {
            Multicaster Caster = null;
            Thread SearchThread = null;
            try
            {

                string message = "give:" + txtSearch.Text;
                Caster = new Multicaster(MainWindow.MulticastIP.ToString(), MainWindow.MulticastPort);
                SearchThread = new Thread(new ParameterizedThreadStart(Caster.Cast));
                SearchThread.Start(message);

                new Thread(new ThreadStart(
                delegate
                {
                    int i = Caster._CastTimes;
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        btnSearch.IsEnabled = false;
                    });
                    
                    while (i-- > 0)
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            btnSearch.Content = "Search (" + i + ")";
                        });
                        Thread.Sleep(1000);
                    }
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        btnSearch.IsEnabled = true;
                        btnSearch.Content = "Search";
                    });
                })).Start();

            }
            catch (Exception ex)
            {
                btnSearch.Content = "Search";
                if (SearchThread != null)
                    SearchThread.Abort();
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            _CurrentSearchWindow = null;
        }
    }
}
