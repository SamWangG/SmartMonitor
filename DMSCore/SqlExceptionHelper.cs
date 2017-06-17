using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
namespace MonitorCore
{
    static class SqlExceptionHelper
    {

        public static event EventHandler Event_Restart; 
        public static void Deal(SqlException ex)
        {
            switch(ex.Number)
            {
                case 208://-- 208: Invalid object name '%.*ls'.
                    break;
                case 2601://-- 2601: Cannot insert duplicate key row in object '%.*ls' with unique index '%.*ls'.
                    break;
                case 10054://A transport-level error has occurred when sending the request to the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)
                    if (Event_Restart != null)
                    {
                        Event_Restart(null, null);
                    }
                    break;
                case 10060://A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. 
                    if (Event_Restart != null)
                    {
                        Event_Restart(null, null);
                    }
                    break;
                case 10061://A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. 
                    if (Event_Restart != null)
                    {
                        Event_Restart(null, null);
                    }
                    break;
                default:
                    if (Event_Restart != null)
                    {
                        Event_Restart(null, null);
                    }
                    break;
            }
        }
        

        public static void restart()
        {
            if (Event_Restart != null)
            {
                Event_Restart(null, null);
            }
        }
    }
}
