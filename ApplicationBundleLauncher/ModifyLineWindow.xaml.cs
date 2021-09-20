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

namespace ApplicationBundleLauncher
{
    /// <summary>
    /// Interaction logic for ModifyLineWindow.xaml
    /// </summary>
    public partial class ModifyLineWindow : Window
    {
        private MainWindow mw;
        private int type = 0;
        private int appBundleIndex = -2;
        private int appIndex = -2;
        private bool editingTarget = false;
        private string appBundleName = "NOT SET";

        private ApplicationBundle appBundle = new ApplicationBundle();
        private ManagedApp managedApp = new ManagedApp();

        /// <summary>
        /// Opens line item modification window
        /// </summary>
        /// <param name="type">1 = Bundle, 2 = Application, 3 = URL</param>
        public ModifyLineWindow(MainWindow mw, int type, int appBundleIndex = -2)
        {
            InitializeComponent();
            this.mw = mw;
            this.type = type;
            this.appBundleIndex = appBundleIndex;
            this.Owner = mw;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            name_TB.KeyDown += Textbox_KeyDown;
            path_TB.KeyDown += Textbox_KeyDown;
            processName_TB.KeyDown += Textbox_KeyDown;
            cmdArgs_TB.KeyDown += Textbox_KeyDown;

            HandleType();

            name_TB.Focus();
        }

        public void EditTargetAppBundle(ApplicationBundle ab, int appBundleIndex)
        {
            editingTarget = true;
            this.appBundleIndex = appBundleIndex;
            this.appBundle = ab;

            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                name_TB.Text = appBundle.Name;
            }));
        }

        public void EditTargetApp(ManagedApp app, int appIndex)
        {
            editingTarget = true;
            this.appIndex = appIndex;
            this.managedApp = app;

            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                name_TB.Text = managedApp.Name;
                path_TB.Text = managedApp.FilePath;
                processName_TB.Text = managedApp.ProcessName;
                cmdArgs_TB.Text = managedApp.CmdArgs;
            }));
        }

        public void SetAppBundleName(string name)
        {
            appBundleName = name;
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                appBundle_LBL.Content = "Selected App Bundle: " + appBundleName;
            }));
        }

        private void HandleType()
        {
            if(type == 1)
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    path_LBL.Visibility = Visibility.Hidden;
                    path_TB.Visibility = Visibility.Hidden;
                    processName_LBL.Visibility = Visibility.Hidden;
                    processName_TB.Visibility = Visibility.Hidden;
                    cmdArgs_LBL.Visibility = Visibility.Hidden;
                    cmdArgs_TB.Visibility = Visibility.Hidden;
                    appBundle_LBL.Visibility = Visibility.Hidden;
                }));
            } else if(type == 3)
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    path_LBL.Content = "URL";
                    processName_LBL.Visibility = Visibility.Hidden;
                    processName_TB.Visibility = Visibility.Hidden;
                    cmdArgs_LBL.Visibility = Visibility.Hidden;
                    cmdArgs_TB.Visibility = Visibility.Hidden;
                }));
            }
        }

        private void save_BTN_Click(object sender, RoutedEventArgs e)
        {
            if(type == 1)
            {
                // AppBundle
                appBundle.Name = name_TB.Text;

                if(editingTarget)
                {
                    mw.ModifyBundle(appBundle, appBundleIndex);
                }
                else
                {
                    mw.AddNewBundle(appBundle);
                }

            } else if(type == 2)
            {
                // ManagedApp
                managedApp.Name = name_TB.Text;
                managedApp.FilePath = path_TB.Text;
                managedApp.ProcessName = processName_TB.Text;
                managedApp.CmdArgs = cmdArgs_TB.Text;

                if (editingTarget)
                {
                    mw.ModifyManagedApp(managedApp, appBundleIndex, appIndex);
                }
                else
                {
                    mw.AddNewManagedApp(managedApp, appBundleIndex);
                }
            } else if(type == 3)
            {
                // URL ManagedApp
                managedApp.Name = name_TB.Text;
                managedApp.FilePath = path_TB.Text;
                managedApp.IsURL = true;
                if (editingTarget)
                {
                    mw.ModifyManagedApp(managedApp, appBundleIndex, appIndex);
                }
                else
                {
                    mw.AddNewManagedApp(managedApp, appBundleIndex);
                }
            }

            this.Close();
        }

        private void cancel_BTN_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return || e.Key == Key.Enter)
            {
                save_BTN_Click(null, null);
            }
        }
    }
}
