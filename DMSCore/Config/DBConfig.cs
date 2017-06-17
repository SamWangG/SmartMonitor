using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace MonitorCore
{
    class DBConfig : ConfigurationSection
    {
        [ConfigurationProperty("users", IsRequired = true)]
        public UserSection users
        {
            get { return (UserSection)this["users"]; }
        }

        [ConfigurationProperty("db", IsRequired = true)]
        public DBSection db
        {
            get { return (DBSection)this["db"]; }
        }

        public class UserSection : ConfigurationElement
        {
            [ConfigurationProperty("username", IsRequired = true)]
            public string userName
            {
                get { return this["username"].ToString(); }
                set { this["username"] = value; }
            }

            [ConfigurationProperty("password", IsRequired = true)]
            public string password
            {
                get { return this["password"].ToString(); }
                set { this["password"] = value; }
            }
        }

        public class DBSection : ConfigurationElement
        {
            [ConfigurationProperty("address", IsRequired = true)]
            public string address
            {
                get { return this["address"].ToString(); }
                set { this["address"] = value; }
            }

            [ConfigurationProperty("table", IsRequired = true)]
            public string table
            {
                get { return this["table"].ToString(); }
                set { this["table"] = value; }
            }
        }
    }
}
