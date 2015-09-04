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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Microsoft.Win32;


namespace ProjectX
{
    /// <summary>
    /// Interaction logic for ConnectionReceiveWindow.xaml
    /// </summary>
    public partial class ConnectionReceiveWindow : Window
    {
        Socket _Socket;
        Thread ReceiveThread;
        public ConnectionReceiveWindow(Socket s)
        {
            InitializeComponent();
            _Socket = s;
            UpdateView();
            ReceiveThread = new Thread(new ThreadStart(Receive));
            ReceiveThread.Start();
        }

        public void Receive()
        {
            try
            {
                byte[] msg = new byte[100];
                _Socket.Receive(msg);
                string[] Message = Encoding.ASCII.GetString(msg).Split(':');
                switch (Message[0])
                {
                    case "sendme":
                        string FileName = Message[1];
                        long FileSize = long.Parse(Message[2]);
                        int NoParts = int.Parse(Message[3]);
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ConnectToPeer Peer = new ConnectToPeer();
                            Peer.TCPConnect(_Socket.RemoteEndPoint.ToString().Split(':')[0]);
                            Thread PeerThread = new Thread(new ParameterizedThreadStart(Peer.SendSharedFile));
                            PeerThread.Start(new object[] { FileName, FileSize, NoParts });
                        });
                        _Socket.Close();
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            this.Close();
                        });
                        break;

                    case "take":
                        {
                            int no_parts = int.Parse(Message[1]);
                            string SaveFolder = null;
                            System.Windows.Forms.FolderBrowserDialog FolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
                            App.Current.Dispatcher.Invoke((Action)delegate
                            {
                                System.Windows.Forms.DialogResult result = FolderBrowser.ShowDialog();
                                if (result == System.Windows.Forms.DialogResult.OK)
                                {
                                    SaveFolder = FolderBrowser.SelectedPath;
                                }
                                else
                                {
                                    MessageBox.Show("File Transfer failed.");
                                    Receive();
                                    return;
                                }
                            });

                            //////////Send Files/////////
                            {
                                string ack = "ok";
                                byte[] data = new byte[ack.Length + 2];
                                data = Encoding.ASCII.GetBytes(ack.ToCharArray());
                                _Socket.Send(data);
                            }
                            ///////////////////

                            for (int i = 0; i < no_parts; i++)
                            {
                                _Socket.Receive(msg);
                                Message = Encoding.ASCII.GetString(msg).Split('~');
                                switch (Message[0])
                                {
                                    case "file":
                                        byte[] data = new byte[int.Parse(Message[1])];
                                        //////////Send Files/////////
                                        {
                                            string ack = "ok";
                                            byte[] d = new byte[ack.Length + 2];
                                            d = Encoding.ASCII.GetBytes(ack.ToCharArray());
                                            _Socket.Send(d);
                                        }
                                        _Socket.ReceiveBufferSize = data.Length;
                                        ///////////////////
                                        _Socket.Receive(data);
                                        string ReceivedFilePath = SaveFolder + "\\" + Message[2].Substring(0, Message[2].IndexOf("\0"));
                                        FileStream _File = new FileStream(ReceivedFilePath, FileMode.Create, FileAccess.Write);
                                        _File.Write(data, 0, data.Length);
                                        _File.Flush();
                                        App.Current.Dispatcher.Invoke((Action)delegate
                                        {
                                            this.progressBar.Value = (((double)i + 1) / no_parts) * 100;
                                            lstReceived.Items.Add(new BellaFile() { Name = System.IO.Path.GetFullPath(_File.Name) });
                                        }, System.Windows.Threading.DispatcherPriority.Send);
                                        _File.Close();
                                        break;
                                }
                            }
                        }
                        MessageBox.Show("You received something!");
                        break;
                }
            }
            catch (ThreadAbortException ex)
            {
                // Delaberately Left Empty.;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void UpdateView()
        {
            string[] PeerInfo = _Socket.RemoteEndPoint.ToString().Split(':');
            ip.Text = PeerInfo[0];
            port.Text = PeerInfo[1];
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string inputfoldername1 = ((BellaFile)lstReceived.Items.GetItemAt(0)).Name.Substring(0,((BellaFile)lstReceived.Items.GetItemAt(0)).Name.LastIndexOf('\\'));
            try
            {
                string[] tmpfiles = Directory.GetFiles(inputfoldername1, "*.tmp");
                FileStream outPutFile = null;
                string PrevFileName = "";
                foreach (string tempFile in tmpfiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(tempFile);
                    string baseFileName = fileName.Substring(0, fileName.IndexOf(Convert.ToChar(".")));
                    string extension = Path.GetExtension(fileName);
                    if (!PrevFileName.Equals(baseFileName))
                    {
                        if (outPutFile != null)
                        {
                            outPutFile.Flush();
                            outPutFile.Close();
                        }
                        outPutFile = new FileStream(inputfoldername1 + "\\" + baseFileName + extension, FileMode.OpenOrCreate, FileAccess.Write);
                    }
                    int bytesRead = 0;
                    byte[] buffer = new byte[1024];
                    FileStream inputTempFile = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.Read);
                    while ((bytesRead = inputTempFile.Read(buffer, 0, 1024)) > 0)
                        outPutFile.Write(buffer, 0, bytesRead);
                    inputTempFile.Close();
                    File.Delete(tempFile);
                    PrevFileName = baseFileName;
                }
                lstReceived.Items.Clear();
                MessageBox.Show("Files have been merged and saved at location " + outPutFile.Name);
                lstReceived.Items.Add(new BellaFile() { Name = outPutFile.Name });
                outPutFile.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_Socket.Connected)
                {
                    ReceiveThread.Abort();
                    _Socket.Close();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
