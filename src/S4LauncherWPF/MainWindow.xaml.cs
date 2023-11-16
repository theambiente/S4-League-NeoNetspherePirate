using Memory;
using S4LauncherWPF.LoginAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace S4LauncherWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int count = 0;
        WebClient client = new WebClient();
        private List<string> DownloadList = new List<string>();
        public static Mem mem = new Mem();
        internal string AuthCode = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        public string Stats(string url)
        {
            return new WebClient().DownloadString(url);
        }

        private void FileCheck()
        {
            bt_StartGame.IsEnabled = false;
            foreach (var file in Stats("https://crygod.de/dekirai/s4season10/updatelist.txt").Split('\n'))
            {
                if (string.IsNullOrEmpty(file))
                    continue;

                if (file.Split('|')[0] == "resource.s4hd")
                    File.Delete("resource.s4hd");

                lb_Status.Content = $"Checking {file.Split('|')[0]}";
                count++;

                if (File.Exists(file.Split('|')[0]))
                {
                    var md5 = System.Security.Cryptography.MD5.Create();
                    FileStream FileStream = new FileStream(file.Split('|')[0], FileMode.Open, FileAccess.Read, FileShare.Read, 8192);

                    if (BitConverter.ToString(md5.ComputeHash(FileStream)).Replace("-", "").ToLowerInvariant() == file.Split('|')[1])
                        DownloadList.Add(file.Split('|')[0]);

                    FileStream.Close();
                }
                else
                    DownloadList.Add(file.Split('|')[0]);

            }
            if (DownloadList.Count == 0)
            {
                lb_Status.Content = "Ready for Login";
                bt_StartGame.IsEnabled = true;
                Reset();
                return;
            }

            prg_Bar2.Maximum = DownloadList.Count();
            count = 0;

            client.DownloadProgressChanged += client_ProgressChanged;
            client.DownloadFileCompleted += client_DownloadCompleted;

            UpdateCheck();
        }

        private void UpdateCheck()
        {
            try
            {
                client.DownloadFileAsync(new Uri("https://crygod.de/dekirai/s4season10/files/" + DownloadList[count]), Environment.CurrentDirectory + @"\" + DownloadList[count]);

                count++;
            }
            catch
            {
            }
        }

        public static string FormatSizeBinary(Int64 size)
        {
            string[] sizes = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double formattedSize = size;
            Int32 sizeIndex = 0;
            while (formattedSize >= 1024 & sizeIndex < sizes.Length)
            {
                formattedSize /= 1024;
                sizeIndex += 1;
            }
            return Math.Round(formattedSize, 2).ToString() + sizes[sizeIndex];
        }

        private void client_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                lb_Status.Content = $"{string.Format("Downloading ({0}%)", Math.Truncate(e.BytesReceived / (double)e.TotalBytesToReceive * 100))} {DownloadList[count - 1]}";
                prg_Bar1.Value = Math.Truncate(e.BytesReceived / (double)e.TotalBytesToReceive * 100);
            }
            catch
            {

            }
        }

        private void client_DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (count == DownloadList.Count())
            {
                lb_Status.Content = "Ready for Login";
                bt_StartGame.IsEnabled = true;
                Reset();
                return;
            }

            UpdateCheck();

            prg_Bar2.Value += 1;
        }

        public void Ready(string code)
        {
            Dispatcher.Invoke(() =>
            {
                AuthCode = code;
            });
        }

        public void UpdateLabel(string message)
        {
            Dispatcher.Invoke(() => { lb_Status.Content = message; });
        }

        public void UpdateErrorLabel(string message)
        {
            Dispatcher.Invoke(() => { lb_Error.Content = message; });
        }

        public string GetUsername()
        {
            return Dispatcher.Invoke(() => { return tb_Username.Text; });
        }

        public string GetPassword()
        {
            return Dispatcher.Invoke(() => { return tb_Password.Password; });
        }

        public void Reset()
        {
            Constants.LoginWindow = this;
            Dispatcher.Invoke(() =>
            {
                prg_Bar1.Value = prg_Bar1.Maximum;
                prg_Bar2.Value = prg_Bar2.Maximum;
                lb_Error.Content = "";
                lb_Status.Content = "Ready for Authentication.";
                bt_StartGame.IsEnabled = true;
            });
        }

        private void bt_StartGame_Click(object sender, RoutedEventArgs e)
        {
            bt_StartGame.IsEnabled = false;
            Task.Run(() => LoginClient.Connect(Constants.ConnectEndPoint));
            Properties.Settings.Default.username = tb_Username.Text;
            Properties.Settings.Default.password = tb_Password.Password;
            Properties.Settings.Default.Save();
            WriteMemory();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists("_resources"))
            {

                MessageBox.Show("Patcher is not in Game Folder!");
                Environment.Exit(0);
            }

            Reset();
            if (Stats("https://crygod.de/dekirai/s4season10/updatelist.txt").Length == 0)
            {
                lb_Status.Content = "Ready for Login";
                Reset();

            }
            else
            {
                lb_Status.Content = "Patching";
            }

            FileCheck();

            client.DownloadProgressChanged += client_ProgressChanged;
            client.DownloadFileCompleted += client_DownloadCompleted;

            Properties.Settings.Default.Reload();

            if (Properties.Settings.Default.username != "")
            {
                tb_Username.Text = Properties.Settings.Default.username;
                tb_Password.Password = Properties.Settings.Default.password;
            }
        }

        private void GetPID()
        {
            int pid = mem.GetProcIdFromName("S4Client");
            bool openProc = false;

            if (pid > 0) openProc = mem.OpenProcess(pid);
        }

        private async void WriteMemory()
        {
            await Task.Delay(4500);
            GetPID();
            mem.WriteMemory("S4client.exe+154B02C", "string", "/cmd"); //Change /chg to /cmd
            Environment.Exit(0);
        }
    }
}
