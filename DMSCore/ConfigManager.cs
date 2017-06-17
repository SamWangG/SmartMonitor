using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;


namespace MonitorCore
{
    class ConfigManager : ConfigurationSection
    {
        public DBConfig dbConfig;
        public APConfig apConfig;
        public ServiceConfig serviceConfig;
        
        public ConfigManager()
        {
            dbConfig = new DBConfig();
            apConfig = new APConfig();
            serviceConfig = new ServiceConfig();

            string file = System.Windows.Forms.Application.ExecutablePath;
            //Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            apConfig = (APConfig)System.Configuration.ConfigurationManager.GetSection("APServer");
            dbConfig = (DBConfig)System.Configuration.ConfigurationManager.GetSection("DataBase");
            serviceConfig = (ServiceConfig)System.Configuration.ConfigurationManager.GetSection("ServiceConfig");
        }

        
    }
}
