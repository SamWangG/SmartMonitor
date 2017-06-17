using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
namespace MonitorCore
{
    class APConfig : ConfigurationSection
    {
        [ConfigurationProperty("remote", IsRequired = true)]
        public RemoteSection remote
        {
            get { return (RemoteSection)this["remote"]; }
        }

        [ConfigurationProperty("local", IsRequired = true)]
        public LocalSection local
        {
            get { return (LocalSection)this["local"]; }
        }


    }

    public class RemoteSection : ConfigurationElement
    {
        [ConfigurationProperty("RemoteIP", IsRequired = true)]
        public string remoteIP
        {
            get { return this["RemoteIP"].ToString(); }
            set { this["RemoteIP"] = value; }
        }

        [ConfigurationProperty("RemotePort", IsRequired = true)]
        public string remotePort
        {
            get { return this["RemotePort"].ToString(); }
            set { this["RemotePort"] = value; }
        }
    }

    public class LocalSection : ConfigurationElement
    {
        [ConfigurationProperty("LocalPort", IsRequired = true)]
        public string localPort
        {
            get { return this["LocalPort"].ToString(); }
            set { this["LocalPort"] = value; }
        }
    }
}
