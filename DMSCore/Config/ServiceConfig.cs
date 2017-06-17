using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace MonitorCore
{
    class ServiceConfig : ConfigurationSection
    {
        [ConfigurationProperty("log", IsRequired = true)]
        public LogSection log
        {
            get { return (LogSection)this["log"]; }
        }

        [ConfigurationProperty("record", IsRequired = true)]
        public RecordSection record
        {
            get { return (RecordSection)this["record"]; }
        }

        [ConfigurationProperty("APthread", IsRequired = true)]
        public ThreadSection APthread
        {
            get { return (ThreadSection)this["APthread"]; }
        }

        [ConfigurationProperty("Path", IsRequired = true)]
        public PathSection Path
        {
            get { return (PathSection)this["Path"]; }
        }

        [ConfigurationProperty("AP", IsRequired = true)]
        public APSection AP
        {
            get { return (APSection)this["AP"]; }
        }

        public class LogSection : ConfigurationElement
        {
            [ConfigurationProperty("level", IsRequired = true)]
            public string level
            {
                get { return this["level"].ToString(); }
                set { this["level"] = value; }
            }
        }


        public class RecordSection : ConfigurationElement
        {
            [ConfigurationProperty("cmd_enable", IsRequired = true)]
            public string cmd_enable
            {
                get { return this["cmd_enable"].ToString(); }
                set { this["cmd_enable"] = value; }
            }

            [ConfigurationProperty("Buffer", IsRequired = true)]
            public string Buffer
            {
                get { return this["Buffer"].ToString(); }
                set { this["Buffer"] = value; }
            }
        }

        public class ThreadSection : ConfigurationElement
        {
            [ConfigurationProperty("count", IsRequired = true)]
            public string count
            {
                get { return this["count"].ToString(); }
                set { this["count"] = value; }
            }
        }

        public class PathSection : ConfigurationElement
        {
            [ConfigurationProperty("path_video", IsRequired = true)]
            public string path_video
            {
                get { return this["path_video"].ToString(); }
                set { this["path_video"] = value; }
            }

            [ConfigurationProperty("path_dest", IsRequired = true)]
            public string path_dest
            {
                get { return this["path_dest"].ToString(); }
                set { this["path_dest"] = value; }
            }

            [ConfigurationProperty("IP", IsRequired = true)]
            public string IP
            {
                get { return this["IP"].ToString(); }
                set { this["IP"] = value; }
            }

            [ConfigurationProperty("Port", IsRequired = true)]
            public string Port
            {
                get { return this["Port"].ToString(); }
                set { this["Port"] = value; }
            }
        }


        public class APSection : ConfigurationElement
        {
            [ConfigurationProperty("path", IsRequired = true)]
            public string path
            {
                get { return this["path"].ToString(); }
                set { this["path"] = value; }
            }
        }
    }
}
