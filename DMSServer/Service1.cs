using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using MonitorCore;

namespace DMSServer
{
    public partial class Service1 : ServiceBase
    {
        private static Core core;
        public Service1()
        {
            InitializeComponent(); 
        }

        protected override void OnStart(string[] args)
        {
            core = new Core();

        }

        protected override void OnStop()
        {
            core.Close();
        }
    }
}
