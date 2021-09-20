using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationBundleLauncher
{
    public class ApplicationBundle
    {
        public string Name { get; set; } = "";
        public List<ManagedApp> ManagedApps { get; set; } = new List<ManagedApp>();

        public ApplicationBundle() { }
    }

    public class ManagedApp
    {
        public string Name { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string ProcessName { get; set; } = "";
        public string CmdArgs { get; set; } = "";
        public bool IsURL { get; set; } = false;

        public ManagedApp() { }
    }
}
