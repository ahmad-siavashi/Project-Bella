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
using System.IO;
using ProjectX;
using Microsoft.Win32;
using System.Threading;

namespace PBellaP
{
    /// <summary>
    /// Interaction logic for FileSharing.xaml
    /// </summary>
    public partial class FileSharing : Window
    {
        public static List<string> sharedFiles = new List<string>();

        public static void Tell(List<SearchResult> Results, string Addr)
        {
            Multicaster Caster = null;
            Thread TellThread = null;
            try
            {
                List<string> message = new List<string>();
                foreach (var r in Results)
                {
                    message.Add("have:" + Path.GetFileName(r.Path) + ":" + r.Size);
                }
                Caster = new Multicaster(MainWindow.MulticastIP.ToString(), MainWindow.MulticastPort);
                TellThread = new Thread(new ParameterizedThreadStart(Caster.Cast));
                TellThread.Start(message.ToArray());

            }
            catch (Exception ex)
            {
                if (TellThread != null)
                    TellThread.Abort();
                MessageBox.Show(ex.Message);
            }
        }

        public FileSharing()
        {
            InitializeComponent();
            foreach (string s in sharedFiles)
            {
                lstSharedFiles.Items.Add(new { Name = System.IO.Path.GetFileName(s), Path = System.IO.Path.GetDirectoryName(s) });
            }
        }

        private void AddFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog _OpenFileDialog = new Microsoft.Win32.OpenFileDialog();
                Nullable<bool> result = _OpenFileDialog.ShowDialog();
                if (result == true)
                {
                    sharedFiles.Add(_OpenFileDialog.FileName);
                    lstSharedFiles.Items.Add(new { Name = System.IO.Path.GetFileName(_OpenFileDialog.FileName), Path = _OpenFileDialog.FileName });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ExcludeFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dynamic selectedItem = lstSharedFiles.SelectedItem;
                sharedFiles.Remove(selectedItem.Path);
                lstSharedFiles.Items.Remove(selectedItem);
                lstSharedFiles.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddDirectoryFiles(string dir)
        {
            string[] folders = System.IO.Directory.GetDirectories(dir);
            if (folders.Length == 0)
            {
                string[] files = System.IO.Directory.GetFiles(dir);
                foreach (string file in files)
                {
                    sharedFiles.Add(file);
                    lstSharedFiles.Items.Add(new { Name = System.IO.Path.GetFileName(file), Path = file });
                }
            }
            foreach (string folder in folders)
            {
                AddDirectoryFiles(folder);
                foreach (string file in System.IO.Directory.GetFiles(dir))
                {
                    sharedFiles.Add(file);
                    lstSharedFiles.Items.Add(new { Name = System.IO.Path.GetFileName(file), Path = file });
                }
            }
        }

        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string Folder = "";
                System.Windows.Forms.FolderBrowserDialog FolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult result = FolderBrowser.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    Folder = FolderBrowser.SelectedPath;
                }
                AddDirectoryFiles(Folder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
