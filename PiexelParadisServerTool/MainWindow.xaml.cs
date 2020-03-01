using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PiexelParadisServerTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SelectedServer.ItemsSource = SSH.Servers.ToList().ConvertAll(lambda => new ComboBoxItem() { Content = lambda.name });
        }
        //   public async void RunAsync(Action actions) => await Task.Run(actions);
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (((ToggleButton)sender).IsChecked == true)
            {
                ((PackIcon)((ToggleButton)sender).Content).Kind = PackIconKind.ArrowCollapseAll;
                WindowState = WindowState.Maximized;
            }
            else
            {
                ((PackIcon)((ToggleButton)sender).Content).Kind = PackIconKind.ArrowExpandAll;
                WindowState = WindowState.Normal;
            }
        }
        private void Move_window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContentArea.IsEnabled = false;
            ((Grid)sender).Cursor = Cursors.SizeAll;
            try { DragMove(); }
            catch { }
            ContentArea.IsEnabled = true;
            ((Grid)sender).Cursor = Cursors.Arrow;
        }
        private void Close_Button_Click(object sender, RoutedEventArgs e) => Close();
        private void SelectedServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            for (int i = 0; i < SSH.Servers.Length; i++)
            { SSH.DisConnect(ref SSH.Servers[i]); }
            if (SelectedServer.SelectedIndex != -1)
            {
                int i = SelectedServer.SelectedIndex;
                OperationAreaSet();
            }
        }
        private int pwdhash = 0;
        private readonly string dataPath = Environment.CurrentDirectory + "\\data\\";
        private void OperationAreaSet()
        {
            EditTextbox.Clear();
            if (SelectedServer.SelectedIndex != -1)
            {
                if (!PasswordCheck()) { return; }
                int i = SelectedServer.SelectedIndex;
                switch (((ListBoxItem)operationSelection.SelectedItem).Content)
                {
                    case "player.log":
                        DoBgTask("正在从服务器上下载player.log\n请稍候...", () =>
                        {
                            string get = SSH.RunShell(ref SSH.Servers[i], $"cat /mc/{SSH.Servers[i].dirPath}/player.log");
                            Dispatcher.Invoke(new Action(() => EditTextbox.Text = get));
                        });
                        break;
                    case "screenlog.0":
                        DoBgTask("正在从服务器上下载screenlog.0\n请稍候...", () =>
                        {
                            string get = SSH.RunShell(ref SSH.Servers[i], $"cat /mc/{SSH.Servers[i].dirPath}/screenlog.0");
                            Dispatcher.Invoke(new Action(() => EditTextbox.Text = get));
                        });
                        break;
                    case "/config/cmd.json":
                        SSH.DownloadFile(ref SSH.Servers[i], $"/mc/{SSH.Servers[i].dirPath}/config/cmd.json", dataPath + SSH.Servers[i].name + "\\cmd.json");
                        EditTextbox.Text = null;
                        break;
                    case "Announcement":
                        DoBgTask("正在从服务器上获取数据Announcement\n请稍候...", () =>
                        {
                            SSH.DownloadFile(ref SSH.Servers[i], $"/mc/{SSH.Servers[i].dirPath}/config/cmd.json", dataPath + SSH.Servers[i].name + "\\cmd.json");
                            try
                            {
                                string get = JArray.Parse(File.ReadAllText(dataPath + SSH.Servers[i].name + "\\cmd.json")).First(lambda => FindJToken((JObject)lambda, "name", "announcement"))["text"].ToString();
                                Dispatcher.Invoke(new Action(() => EditTextbox.Text = get));
                            }
                            catch (Exception) { }
                        });
                        break;
                    default:
                        break;
                }
            }
        }
        private bool FindJToken(JToken jToken, string index, string vaule)
        {
            try
            { if (jToken[index].ToString() == vaule) { return true; } }
            catch (Exception) { }
            return false;
        }
        private void OperationSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            drawerHost.IsLeftDrawerOpen = false;
            OperationAreaSet();
        }
        private bool PasswordCheck()
        {
            if (pwdhash == 0)
            {
                DoBgTask("获取网络信息中...\n请稍候...", () =>
               {
                   Match getPWD = Regex.Match(HttpStringGet.GetHtmlStr("https://gxh.wodemo.net/entry/528363"), @"\>\<p\>pwdTestHash=(?<hashcode>\-?\d+?)\</p\>\</");
                   Dispatcher.Invoke(new Action(() => pwdhash = int.Parse(getPWD.Groups["hashcode"].Value)));
               });
            }
            else
            {
                if (string.IsNullOrEmpty(pwdBox.Password))
                {
                    pwdCheck.Text = "Password";
                    pwdCheck.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    return false;
                }
                if (pwdBox.Password.GetHashCode() == pwdhash)
                {
                    pwdCheck.Text = "Password Verification Passed";
                    pwdCheck.Foreground = new SolidColorBrush(Color.FromRgb(48, 125, 50));
                    //xsba233666
                    return true;
                }
                pwdCheck.Text = "Invalid Password";
                pwdCheck.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
            return false;
        }
        private void PwdBox_LostFocus(object sender, RoutedEventArgs e) => OperationAreaSet(); 
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!PasswordCheck()) { return; }
            int i = SelectedServer.SelectedIndex;
            switch (SSH.Servers[i].name)
            {
                case "生存服":
                    if (((ListBoxItem)operationSelection.SelectedItem).Content.ToString() == "Announcement")
                    {
                        string localpath = dataPath + SSH.Servers[i].name + "\\cmd.json";
                        DoBgTask("保存中\n请稍候...", () =>
                        {
                            JArray json = JArray.Parse(File.ReadAllText(localpath));
                            Dispatcher.Invoke(new Action(() => json.First(lambda => FindJToken((JObject)lambda, "name", "announcement"))["text"] = EditTextbox.Text.Replace("\r", null)));
                            File.WriteAllText(localpath, json.ToString());
                            DoBgTask("正在上传到服务器\n请稍候...", () =>
                            {
                                SSH.UploadFile(ref SSH.Servers[i], localpath, $"/mc/{SSH.Servers[i].dirPath}/config/", "cmd.json");
                            });
                        });
                    }
                    break;
                default:
                    break;
            }
        }
        private void ReloadCMD_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!PasswordCheck()) { return; }
            int i = SelectedServer.SelectedIndex;
            switch (SSH.Servers[i].name)
            {
                case "生存服":
                    if (((ListBoxItem)operationSelection.SelectedItem).Content.ToString() == "Announcement")
                    {
                        try
                        {
                            DoBgTask("发送命令 reload_cmd\n请稍候...", () =>
                            {
                                SSH.ScreenCommand(ref SSH.Servers[i], "reload_cmd");
                                SSH.ScreenCommand(ref SSH.Servers[i], "reload_cmd");
                            });
                        }
                        catch (Exception) { }
                    }
                    break;
                default:
                    break;
            }
        }
        private void DoBgTask(string tip, Action action)
        {
            _ = Task.Run(new Action(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    LoadingDialog.IsOpen = true;
                    LoadingTip.Text = tip;
                }));
                try
                {
                    Task BGT = Task.Run(action);
                    BGT.Wait();
                    BGT.Dispose();
                }
                catch (Exception) { }
                Dispatcher.Invoke(new Action(() => { LoadingDialog.IsOpen = false; }));
            }));
        }
    }
}
