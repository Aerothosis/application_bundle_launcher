using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ApplicationBundleLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow m= new MainWindow();
            if(e.Args.Length >= 1)
            {
                var target = "";
                foreach(var arg in e.Args)
                {
                    if(arg.Contains("--"))
                    {
                        target = arg.Replace("--", "").Replace("\"", "");
                    }
                }
                m.Show();
                m.AutoStartProjectFromCmdLine(target);
            } else
            {
                m.Show();
            }
        }
    }
}
