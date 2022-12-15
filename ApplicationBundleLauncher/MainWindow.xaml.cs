using IWshRuntimeLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ApplicationBundleLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string dataStoragePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\ApplicationBundleLauncher\\saveddata.json";
        private List<ApplicationBundle> appBundles = new List<ApplicationBundle>();
        private List<string> appBundleNames = new List<string>();
        private int selAppBundleIndex = -2;
        private int selAppIndex = -2;
        private string autoStartTarget = "";

        public MainWindow()
        {
            InitializeComponent();
            RetrieveCurrentSavedData();
            appBundles_LB.SelectionChanged += AppBundles_LB_SelectionChanged;
            appBundles_LB.MouseDoubleClick += AppBundles_LB_MouseDoubleClick;
            apps_LB.SelectionChanged += Apps_LB_SelectionChanged;
            apps_LB.MouseDoubleClick += Apps_LB_MouseDoubleClick;
        }

        public void LogLine(string txt)
        {
            log_TB.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                log_TB.AppendText(txt + "\n");
                log_TB.ScrollToEnd();
            }));
        }
        
        public void AddNewBundle(ApplicationBundle ab)
        {
            appBundles.Add(ab);
            SaveData();
            ResetUI();
        }

        public void ModifyBundle(ApplicationBundle ab, int appBundleIndex)
        {
            if(appBundleIndex < appBundles.Count)
            {
                appBundles[appBundleIndex] = ab;
                SaveData();
                PopulateAppBundles();
                //ResetUI();
            }
        }

        public void AddNewManagedApp(ManagedApp ma, int appBundleIndex)
        {
            if(appBundleIndex < appBundles.Count)
            {
                appBundles[appBundleIndex].ManagedApps.Add(ma);
                SaveData();
                PopulateManagedApps(appBundleIndex);
            }
        }

        public void ModifyManagedApp(ManagedApp ma, int appBundleIndex, int appIndex)
        {
            if(appBundleIndex < appBundles.Count)
            {
                if(appIndex < appBundles[appBundleIndex].ManagedApps.Count)
                {
                    appBundles[appBundleIndex].ManagedApps[appIndex] = ma;
                    SaveData();
                    PopulateManagedApps(appBundleIndex);
                }
            }
        }

        public void AutoStartProjectFromCmdLine(string target)
        {
            autoStartTarget = target;
            if (appBundleNames.Contains(target))
            {
                int index = appBundleNames.IndexOf(target);
                appBundles_LB.SelectedIndex = index;
                AppBundles_LB_SelectionChanged(null, null);
                selAppBundleIndex = index;
                launch_BTN_Click(null, null);
            }
        }

        private void RetrieveCurrentSavedData()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dataStoragePath));
            if(System.IO.File.Exists(dataStoragePath))
            {
                string jsonString = System.IO.File.ReadAllText(dataStoragePath);
                appBundles = JsonConvert.DeserializeObject<List<ApplicationBundle>>(jsonString);
                PopulateAppBundles();
            }
        }

        private void SaveData()
        {
            LogLine("Saving changes...");
            string output = JsonConvert.SerializeObject(appBundles);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dataStoragePath));
                if (System.IO.File.Exists(dataStoragePath))
                {
                    System.IO.File.Delete(dataStoragePath);
                }

                System.IO.File.WriteAllText(dataStoragePath, output);
                LogLine("Changes saved successfully.");
            } catch(Exception e)
            {
                LogLine("Failed to save data. Please save manually to ensure changes are saved.");
            }
        }

        private void ResetUI()
        {
            selAppIndex = -2;
            selAppBundleIndex = -2;
            PopulateAppBundles();
        }

        private void PopulateAppBundles()
        {
            // appBundleNames = new List<string>();
            for(int x = 0; x < appBundles.Count; x++)
            {
                appBundleNames.Add(appBundles[x].Name);
            }

            apps_LB.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                ClearListObject(false);
            }));
            appBundles_LB.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                appBundles_LB.ItemsSource = appBundleNames;
            }));
        }

        private void PopulateManagedApps(int appBundleIndex = -2)
        {
            if(appBundleIndex == -2)
            {
                appBundleIndex = selAppBundleIndex;
            }
            if(appBundleIndex >= 0 && appBundleIndex < appBundles.Count)
            {
                List<string> appNames = new List<string>();
                for(int x = 0; x < appBundles[appBundleIndex].ManagedApps.Count; x++)
                {
                    appNames.Add(appBundles[appBundleIndex].ManagedApps[x].Name);
                }

                apps_LB.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    //ClearListObject(false);
                    apps_LB.ItemsSource = appNames;
                }));
            }
        }

        private bool CheckAppBundleIndex()
        {
            bool output = (selAppBundleIndex >= 0) && (selAppBundleIndex < appBundles.Count);
            if(!output)
            {
                LogLine("No app bundle selected. Please select an app bundle and try again.");
            }
            return output;
        }

        private bool CheckAppIndex()
        {
            if(CheckAppBundleIndex())
            {
                bool output = (selAppIndex >= 0) && (selAppIndex < appBundles[selAppBundleIndex].ManagedApps.Count);
                if(!output)
                {
                    LogLine("No application selected. Please select an application and try again.");
                }
                return output;
            } else
            {
                return false;
            }
        }

        private void ClearListObject(bool appBundleList)
        {
            if (appBundleList)
            {
                appBundles_LB.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    try
                    {
                        appBundles_LB.ItemsSource = null;
                        appBundles_LB.Items.Clear();
                    }
                    catch (InvalidOperationException ioe)
                    {
                        Console.WriteLine(ioe.Message + "\n" + ioe.StackTrace);
                    }
                }));
            }
            else
            {
                apps_LB.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    try
                    {
                        apps_LB.ItemsSource = null;
                        apps_LB.Items.Clear();
                    }
                    catch (InvalidOperationException ioe)
                    {
                        Console.WriteLine(ioe.Message + "\n" + ioe.StackTrace);
                    }
                }));
            }
        }

        private void UpdateSelectedLabel()
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                selAppBundle_LBL.Content = "Selected App Bundle ID: " + (selAppBundleIndex < 0 ? 0 : (selAppBundleIndex + 1));
                selApp_LBL.Content = "Selected Application ID: " + (selAppIndex < 0 ? 0 : (selAppIndex + 1));
            }));
        }

        private bool CheckIfUrl(int appIndex)
        {
            bool output = false;
            if((selAppBundleIndex >= 0) && (selAppBundleIndex < appBundles.Count))
            {
                if((selAppIndex >= 0) && (selAppIndex < appBundles[selAppBundleIndex].ManagedApps.Count))
                {
                    output = appBundles[selAppBundleIndex].ManagedApps[appIndex].IsURL;
                }
            }
            return output;
        }

        private void AppBundles_LB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selAppBundleIndex = appBundles_LB.SelectedIndex;
            UpdateSelectedLabel();
            PopulateManagedApps();
        }

        private void AppBundles_LB_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            appBundleEdit_BTN_Click(null, null);
        }

        private void Apps_LB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selAppIndex = apps_LB.SelectedIndex;
            UpdateSelectedLabel();
        }

        private void Apps_LB_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CheckIfUrl(selAppIndex))
            {
                editURL_BTN_Click(null, null);
            }
            else
            {
                appEdit_BTN_Click(null, null);
            }
        }

        private void launch_BTN_Click(object sender, RoutedEventArgs e)
        {
            if(CheckAppBundleIndex())
            {
                List<ManagedApp> apps = appBundles[selAppBundleIndex].ManagedApps;
                LogLine("App bundle found. Launching [" + apps.Count + "] applications...");
                bool anyFailed = false;

                ProcessStartInfo psi;
                ManagedApp a;
                for(int x = 0; x < apps.Count; x++)
                {
                    a = apps[x];
                    if(a.IsURL)
                    {
                        LogLine("Launching [" + a.Name + "] URL");
                        try
                        {
                            Process.Start(a.FilePath);
                        } catch(Exception ex)
                        {
                            LogLine("Failed to launch target URL.");
                            Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                            anyFailed = true;
                        }
                    } 
                    else
                    {
                        LogLine("Launching [" + a.Name + "]");
                        if (System.IO.File.Exists(a.FilePath))
                        {
                            psi = new ProcessStartInfo();
                            psi.FileName = a.FilePath;
                            psi.WorkingDirectory = Path.GetDirectoryName(a.FilePath);
                            if (a.CmdArgs.Length > 0)
                            {
                                psi.Arguments = a.CmdArgs;
                            }
                            Process.Start(psi);
                            LogLine("Successfully started [" + a.Name + "]");
                        }
                        else
                        {
                            LogLine("Failed to launch [" + a.Name + "], file path invalid.");
                            anyFailed = true;
                        }
                    }
                }

                if(anyFailed)
                {
                    LogLine("Some applications failed to launch. You can try again or just close this window.");
                } else
                {
                    LogLine("All applications successfully launched! You can now close this window.");
                }
            }
        }

        private void close_BTN_Click(object sender, RoutedEventArgs e)
        {
            if(CheckAppBundleIndex())
            {
                List<ManagedApp> apps = appBundles[selAppBundleIndex].ManagedApps;
                List<string> appNames = new List<string>();
                foreach(ManagedApp m in apps)
                {
                    if(!m.IsURL)
                    {
                        appNames.Add(m.ProcessName);
                    }
                }
                LogLine("App bundle found. Closing [" + apps.Count + "] applications...");

                int count = 0;
                Process[] processes = Process.GetProcesses();
                foreach(Process p in processes)
                {
                    if(appNames.Contains(p.ProcessName))
                    {
                        p.Kill();
                        count++;
                    }
                }
                LogLine("Successfully closed [" + appNames.Count + "] applications with [" + count + "] total processes.");
            }
        }

        private void appBundleAdd_BTN_Click(object sender, RoutedEventArgs e)
        {
            ModifyLineWindow m = new ModifyLineWindow(this, 1);
            m.Owner = this;
            m.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            m.Show();
        }

        private void appBundleEdit_BTN_Click(object sender, RoutedEventArgs e)
        {
            if(CheckAppBundleIndex())
            {
                ModifyLineWindow m = new ModifyLineWindow(this, 1);
                m.Owner = this;
                m.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                m.EditTargetAppBundle(appBundles[selAppBundleIndex], selAppBundleIndex);
                m.Show();
            }
        }

        private void appBundleRemove_BTN_Click(object sender, RoutedEventArgs e)
        {
            if(CheckAppBundleIndex())
            {
                string name = appBundles[selAppBundleIndex].Name;
                MessageBoxResult res = MessageBox.Show("Are you sure you wish to remove the app bundle titled: \n" + name, "DELETE APP BUNDLE", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if(res == MessageBoxResult.Yes)
                {
                    appBundles.RemoveAt(selAppBundleIndex);
                    SaveData();
                    ResetUI();
                }
            }
        }

        private void appAdd_BTN_Click(object sender, RoutedEventArgs e)
        {
            if(CheckAppBundleIndex())
            {
                ModifyLineWindow m = new ModifyLineWindow(this, 2, selAppBundleIndex);
                m.SetAppBundleName(appBundles[selAppBundleIndex].Name);
                m.Show();
            }
        }

        private void appEdit_BTN_Click(object sender, RoutedEventArgs e)
        {
            if(CheckAppBundleIndex())
            {
                if(CheckAppIndex())
                {
                    if(CheckIfUrl(selAppIndex))
                    {
                        editURL_BTN_Click(null, null);
                    } else
                    {
                        ModifyLineWindow m = new ModifyLineWindow(this, 2, selAppBundleIndex);
                        m.EditTargetApp(appBundles[selAppBundleIndex].ManagedApps[selAppIndex], selAppIndex);
                        m.SetAppBundleName(appBundles[selAppBundleIndex].Name);
                        m.Show();
                    }
                }
            }
        }

        private void appRemove_BTN_Click(object sender, RoutedEventArgs e)
        {
            if(CheckAppBundleIndex())
            {
                if(CheckAppIndex())
                {
                    if(CheckIfUrl(selAppIndex))
                    {
                        delURL_BTN_Click(null, null);
                    } else
                    {
                        string name = appBundles[selAppBundleIndex].ManagedApps[selAppIndex].Name;
                        MessageBoxResult res = MessageBox.Show("Are you sure you wish to remove the selected application: \n" + name, "DELETE APPLICATION", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (res == MessageBoxResult.Yes)
                        {
                            appBundles[selAppBundleIndex].ManagedApps.RemoveAt(selAppIndex);
                            SaveData();
                            ResetUI();
                        }
                    }
                }
            }
        }

        private void addUrl_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAppBundleIndex())
            {
                ModifyLineWindow m = new ModifyLineWindow(this, 3, selAppBundleIndex);
                m.SetAppBundleName(appBundles[selAppBundleIndex].Name);
                m.Show();
            }
        }
        
        private void editURL_BTN_Click(object sender, RoutedEventArgs e)
        {
            if(CheckAppBundleIndex()) 
            {
                if (CheckAppIndex())
                {
                    ModifyLineWindow m = new ModifyLineWindow(this, 3, selAppBundleIndex);
                    m.EditTargetApp(appBundles[selAppBundleIndex].ManagedApps[selAppIndex], selAppIndex);
                    m.SetAppBundleName(appBundles[selAppBundleIndex].Name);
                    m.Show();
                }
            }
        }

        private void delURL_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAppBundleIndex())
            {
                if (CheckAppIndex())
                {
                    string name = appBundles[selAppBundleIndex].ManagedApps[selAppIndex].Name;
                    MessageBoxResult res = MessageBox.Show("Are you sure you wish to remove the selected URL: \n" + name, "DELETE URL", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.Yes)
                    {
                        appBundles[selAppBundleIndex].ManagedApps.RemoveAt(selAppIndex);
                        SaveData();
                        ResetUI();
                    }
                }
            }
        }

        private void save_BTN_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void mkShortcut_BTN_Click(object sender, RoutedEventArgs e)
        {
            if ((selAppBundleIndex >= 0) && (selAppBundleIndex < appBundleNames.Count))
            {
                string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string target = appBundleNames[selAppBundleIndex];
                string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string shortcutName = desktopDir + "\\" + target + " - ABL.lnk";
                string iconSource = app.Replace('\\', '/');

                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.FileName = "Source Icon";
                dialog.DefaultExt = ".exe";
                dialog.Filter = "Executables and Icons|*.exe;*.ico";
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    iconSource = dialog.FileName.Replace("\\", "/");
                }

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutName);
                shortcut.Description = "Quick launch shortcut for bundle name [" + target + "]. Changing the bundle name will BREAK this shortcut!!";
                shortcut.IconLocation = iconSource;
                shortcut.TargetPath = app;
                shortcut.Arguments = "--" + target;
                shortcut.Save();
            }
        }
    }
}
