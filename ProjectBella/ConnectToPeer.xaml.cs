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
using System.Net;
using System.Net.Sockets;
using System.IO;
using PBellaP;

namespace ProjectX
{
    /// <summary>
    /// Interaction logic for ConnectToPeer.xaml
    /// </summary>
    public partial class ConnectToPeer : Window
    {
        IPEndPointEx _PeerAddr;
        Host _Peer;
        TcpClient _Client;
        int _PeerPort;
        bool isConnected = false;

        public ConnectToPeer()
        {
            InitializeComponent();
        }

        public void SendSharedFile(object Args)
        {
            object[] args = (object[])Args;
            string FileName = (string)args[0];
            long FileSize = (long)args[1];
            int NoParts = (int)args[2];


            string filePath = FileSharing.sharedFiles.Select(P => P).Where(P => P.EndsWith(FileName)).ToList<string>()[0];

            /////Split//////
            List<BellaFile> Parts = new List<BellaFile>();
            if (NoParts != 1)
            {

                try
                {
                    FileStream _File = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    int noOfParts = NoParts;
                    int sizeOfPart = (int)Math.Ceiling((double)_File.Length / noOfParts);
                    string baseFileName = Path.GetFileNameWithoutExtension(filePath);
                    string Extension = Path.GetExtension(filePath);
                    for (int i = 0; i < noOfParts; i++)
                    {
                        string PartName = Path.GetDirectoryName(filePath) + @"\" + baseFileName + "." +
                            i.ToString().PadLeft(5, Convert.ToChar("0")) + Extension + ".tmp";
                        FileStream outputFile = new FileStream(PartName, FileMode.Create, FileAccess.Write);
                        int bytesRead = 0;
                        byte[] buffer = new byte[sizeOfPart];
                        if ((bytesRead = _File.Read(buffer, 0, sizeOfPart)) > 0)
                        {
                            outputFile.Write(buffer, 0, bytesRead);
                            Parts.Add(new BellaFile() { Id = i, Size = bytesRead, Name = Path.GetFileName(PartName), Path = PartName });
                        }
                        outputFile.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            /////////

            try
            {
                if (!isConnected)
                {
                    //_Client = new TcpClient();
                    //_Client.Connect(new IPEndPoint(IPAddress.Parse(_PeerAddr.Address.ToString()), _PeerPort));
                    //isConnected = true;
                    this.btnConnect(null, null);
                }
                Stream stream = _Client.GetStream();
                int no_parts = NoParts;
                FileStream _file = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                string Message = "take:" + no_parts;
                byte[] data = new byte[Message.Length + 2];
                data = Encoding.ASCII.GetBytes(Message.ToCharArray());
                stream.Write(data, 0, data.Length);

                ////////////////
                {
                    byte[] msg = new byte[100];
                    stream.Read(msg, 0, msg.Length);
                    string ack = Encoding.ASCII.GetString(msg);
                }
                ////////////////


                if (NoParts > 1)
                {
                    List<string> tempFiles = new List<string>();
                    foreach (var item in Parts)
                    {
                        BellaFile _Part = (BellaFile)item;
                        _file = new FileStream(_Part.Path, FileMode.Open, FileAccess.Read);
                        byte[] buffer = new byte[_file.Length];
                        _file.Read(buffer, 0, (int)_file.Length);

                        string fileMsg = "file~" + _file.Length + "~" + Path.GetFileName(_file.Name);
                        byte[] msg = new byte[fileMsg.Length];
                        msg = Encoding.ASCII.GetBytes(fileMsg.ToCharArray());
                        stream.Write(msg, 0, msg.Length);

                        ////////////////
                        {
                            byte[] m = new byte[100];
                            stream.Read(m, 0, m.Length);
                            string ack = Encoding.ASCII.GetString(m);
                        }
                        ////////////////

                        stream.Write(buffer, 0, (int)_file.Length);
                        tempFiles.Add(_file.Name);
                        _file.Close();
                    }
                    foreach (string tempFile in tempFiles)
                        File.Delete(tempFile);
                    Dispatcher.Invoke((Action)delegate
                    {
                        Parts.Clear();
                    });
                    MessageBox.Show("Bella :-) sent all the parts successfully!");
                }
                else
                {
                    string fileMsg = "file~" + _file.Length + "~" + Path.GetFileName(_file.Name);
                    byte[] msg = new byte[fileMsg.Length];
                    msg = Encoding.ASCII.GetBytes(fileMsg.ToCharArray());
                    stream.Write(msg, 0, msg.Length);

                    ////////////////
                    {
                        byte[] m = new byte[100];
                        stream.Read(m, 0, m.Length);
                        string ack = Encoding.ASCII.GetString(m);
                    }
                    ////////////////

                    byte[] buffer = new byte[_file.Length];
                    _file.Read(buffer, 0, (int)_file.Length);
                    stream.Write(buffer, 0, (int)_file.Length);
                    MessageBox.Show("Bella =) sent something sucessfully!");
                    _file.Close();
                }
                stream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public ConnectToPeer(Host Peer, IPEndPointEx PeerAddr, int PeerPort)
        {
            InitializeComponent();
            _Peer = Peer;
            _PeerAddr = PeerAddr;
            _PeerPort = PeerPort;
            UpdateView();
        }
        public void UpdateView()
        {
            try
            {
                PeerIP.Text = _PeerAddr.Address.ToString();
                PeerName.Text = _Peer.Name;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnConnect(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Client == null || !_Client.Connected)
                {
                    _Client = new TcpClient();
                    _Client.Connect(new IPEndPoint(IPAddress.Parse(_PeerAddr.Address.ToString()), _PeerPort));
                    Stream stream = _Client.GetStream();
                    isConnected = true;
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        btnconnect.Content = "Disconnect";
                    });
                }
                else
                {
                    if (_Client.Connected)
                    {
                        _Client.Close();
                    }

                    btnconnect.Content = "Connect";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                isConnected = false;
            }


        }

        public void SendMsg(string Msg)
        {
            try
            {
                Stream stream = _Client.GetStream();
                byte[] data = Encoding.ASCII.GetBytes(Msg);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void TCPConnect(string IPAddr)
        {
            try
            {
                _Client = new TcpClient();
                _Client.Connect(new IPEndPoint(IPAddress.Parse(IPAddr), MainWindow.ConnectionPort));
                Stream stream = _Client.GetStream();
                isConnected = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                isConnected = false;
            }
        }

        private void btnbrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog _OpenFileDialog = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = _OpenFileDialog.ShowDialog();
            if (result == true)
            {
                filePath.Text = _OpenFileDialog.FileName;
            }
        }

        private void btnsend_Click(object sender, RoutedEventArgs e)
        {
            new Thread(new ParameterizedThreadStart(SendFile)).Start(filePath.Text);
        }

        public void SendFile(object FilePath)
        {
            string filePath = (string)FilePath;
            try
            {
                if (!isConnected)
                {
                    _Client = new TcpClient();
                    _Client.Connect(new IPEndPoint(IPAddress.Parse(_PeerAddr.Address.ToString()), _PeerPort));
                    isConnected = true;
                }
                Stream stream = _Client.GetStream();
                int no_parts = (lstParts.Items.Count == 0) ? 1 : lstParts.Items.Count;
                FileStream _file = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                string Message = "take:" + no_parts;
                byte[] data = new byte[Message.Length + 2];
                data = Encoding.ASCII.GetBytes(Message.ToCharArray());
                stream.Write(data, 0, data.Length);

                ////////////////
                {
                    byte[] msg = new byte[100];
                    stream.Read(msg, 0, msg.Length);
                    string ack = Encoding.ASCII.GetString(msg);
                }
                ////////////////


                if (lstParts.Items.Count != 0)
                {// Multi Parts
                    List<string> tempFiles = new List<string>();
                    int i = 0;
                    foreach (var item in lstParts.Items)
                    {
                        BellaFile _Part = (BellaFile)item;
                        _file = new FileStream(_Part.Path, FileMode.Open, FileAccess.Read);
                        byte[] buffer = new byte[_file.Length];
                        _file.Read(buffer, 0, (int)_file.Length);

                        string fileMsg = "file~" + _file.Length + "~" + Path.GetFileName(_file.Name);
                        byte[] msg = new byte[fileMsg.Length];
                        msg = Encoding.ASCII.GetBytes(fileMsg.ToCharArray());
                        stream.Write(msg, 0, msg.Length);

                        ////////////////
                        {
                            byte[] m = new byte[100];
                            stream.Read(m, 0, m.Length);
                            string ack = Encoding.ASCII.GetString(m);
                        }
                        ////////////////

                        stream.Write(buffer, 0, (int)_file.Length);
                        tempFiles.Add(_file.Name);
                        _file.Close();
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            progressBar.Value = (((double)++i) / no_parts) * 100;
                        }, System.Windows.Threading.DispatcherPriority.Send);
                    }
                    foreach (string tempFile in tempFiles)
                        File.Delete(tempFile);
                    Dispatcher.Invoke((Action)delegate
                    {
                        lstParts.Items.Clear();
                    });
                    MessageBox.Show("You sent all the parts successfully!");
                }
                else
                {
                    string fileMsg = "file~" + _file.Length + "~" + Path.GetFileName(_file.Name);
                    byte[] msg = new byte[fileMsg.Length];
                    msg = Encoding.ASCII.GetBytes(fileMsg.ToCharArray());
                    stream.Write(msg, 0, msg.Length);

                    ////////////////
                    {
                        byte[] m = new byte[100];
                        stream.Read(m, 0, m.Length);
                        string ack = Encoding.ASCII.GetString(m);
                    }
                    ////////////////

                    byte[] buffer = new byte[_file.Length];
                    _file.Read(buffer, 0, (int)_file.Length);
                    stream.Write(buffer, 0, (int)_file.Length);
                    MessageBox.Show("You sent something sucessfully!");
                    _file.Close();
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        progressBar.Value = 100;
                    });
                }
                stream.Close();
                _Client.Close();
                isConnected = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnsplit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileStream _File = new FileStream(filePath.Text, FileMode.Open, FileAccess.Read);
                int noOfParts = int.Parse(txt_no_parts.Text);
                int sizeOfPart = (int)Math.Ceiling((double)_File.Length / noOfParts);
                string baseFileName = Path.GetFileNameWithoutExtension(filePath.Text);
                string Extension = Path.GetExtension(filePath.Text);
                for (int i = 0; i < noOfParts; i++)
                {
                    string PartName = Path.GetDirectoryName(filePath.Text) + @"\" + baseFileName + "." +
                        i.ToString().PadLeft(5, Convert.ToChar("0")) + Extension + ".tmp";
                    FileStream outputFile = new FileStream(PartName, FileMode.Create, FileAccess.Write);
                    int bytesRead = 0;
                    byte[] buffer = new byte[sizeOfPart];
                    if ((bytesRead = _File.Read(buffer, 0, sizeOfPart)) > 0)
                    {
                        outputFile.Write(buffer, 0, bytesRead);
                        lstParts.Items.Add(new BellaFile() { Id = i, Size = bytesRead, Name = Path.GetFileName(PartName), Path = PartName });
                    }
                    outputFile.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    public class BellaFile
    {
        string _Path;

        public string Path
        {
            get { return _Path; }
            set { _Path = value; }
        }
        string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        int size;

        public int Size
        {
            get { return size; }
            set { size = value; }
        }
    }

}
