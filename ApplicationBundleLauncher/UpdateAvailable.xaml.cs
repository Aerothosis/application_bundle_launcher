using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
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

namespace ApplicationBundleLauncher
{
    /// <summary>
    /// Interaction logic for UpdateAvailable.xaml
    /// </summary>
    public partial class UpdateAvailable : Window
    {
        private NewUpdateInfo newUpdateInfo;

        public UpdateAvailable(NewUpdateInfo updateInfo)
        {
            InitializeComponent();
            newUpdateInfo = updateInfo;
            versionNew_TB.Text = updateInfo.VersionNew;
            versionCurrent_TB.Text = updateInfo.VersionCurrent;
            releaseDate_TB.Text = updateInfo.ReleaseDate;
            releaseNotes_TB.Text = updateInfo.ReleaseNotes;
        }

        private void cancel_BTN_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void downloadInstall_BTN_Click(object sender, RoutedEventArgs e)
        {
            ToggleDownloadStatus(true);
            WebClient wc = new WebClient();
            wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
            string downloadTarget = ValidateTargetFilename(Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString() + "\\" + newUpdateInfo.DownloadFileName);
            try
            {
                Task.Run(() =>
                {
                    System.Threading.Thread.Sleep(1000);
                    wc.DownloadFileAsync(new Uri(newUpdateInfo.DownloadUrl), downloadTarget);
                    ToggleDownloadStatus(false);
                    if(MessageBox.Show("Download complete, would you like to run the installer now?", "Run Install?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        if(File.Exists(downloadTarget))
                        {
                            Process.Start(downloadTarget);
                            Process.GetCurrentProcess().Kill();
                        }
                    }
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private string ValidateTargetFilename(string sourceTargetName)
        {
            string output = sourceTargetName;
            string ext = System.IO.Path.GetExtension(sourceTargetName);
            string directory = System.IO.Path.GetDirectoryName(sourceTargetName);
            int count = 0;
            while(File.Exists(output))
            {
                count++;
                output = directory + "\\" + System.IO.Path.GetFileNameWithoutExtension(sourceTargetName) + " (" + count + ")" + ext;
            }

            return output;
        }

        private void ToggleDownloadStatus(bool isRunning)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                downloadInstall_BTN.IsEnabled = !isRunning;
                cancel_BTN.IsEnabled = !isRunning; 
                if (!isRunning)
                {
                    download_PB.Value = 0;
                    downloadProg_LBL.Content = String.Format("{0:#%}", 0);
                }
                download_PB.Visibility = isRunning ? Visibility.Visible : Visibility.Hidden;
                downloadProg_LBL.Visibility = isRunning ? Visibility.Visible : Visibility.Hidden;
            }));
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                int val = e.ProgressPercentage;
                download_PB.Value = val;
                downloadProg_LBL.Content = String.Format("{0:#%}", val);
            }));
        }
    }

    public class NewUpdateInfo
    {
        public string VersionCurrent { get; set; }
        public string VersionNew { get; set; }
        public string ReleaseDate { get; set; }
        public string ReleaseNotes { get; set; }
        public string DownloadUrl { get; set; }
        public string DownloadFileName { get; set; }

        public NewUpdateInfo() { }
    }
}
