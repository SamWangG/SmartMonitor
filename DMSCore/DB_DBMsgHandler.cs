using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading;
//using System.Data.SqlConnection;
using System.Data;
using System.Data.SqlTypes;
using System.Data.Common;

namespace MonitorCore
{
    class DB_DBMsgHandler
    {
        private SqlConnection sqlCnt_SetOrder = null;

        private SqlConnection sqlCnt_GetOrder = null;

        private SqlCommand command_DBHandler = null;
        private bool bStored_Init;
        private int cmd_enable;
        public DB_DBMsgHandler(int cmd_enable)
        {

            sqlCnt_SetOrder = new SqlConnection();

            sqlCnt_GetOrder = new SqlConnection();

            //command_Set = new SqlCommand();
            command_DBHandler = new SqlCommand();
            command_DBHandler.CommandType = CommandType.StoredProcedure;

            this.cmd_enable = cmd_enable;
        }


        #region Open database
        public bool Open(string ServerName, string account, string password, string database, int timeout = 5)
        {
            try
            {
                string cntStr = "server=" + ServerName + ";user id=" + account + ";password=" + password +
                    ";MultipleActiveResultSets=true;database=" + database + ";Connection Timeout=" + timeout.ToString() + ";";

                sqlCnt_SetOrder.ConnectionString = cntStr;

                sqlCnt_SetOrder.Open();


                sqlCnt_GetOrder.ConnectionString = cntStr;

                sqlCnt_GetOrder.Open();

                Thread.Sleep(500);
                if (ConnectionState.Open == sqlCnt_SetOrder.State)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database---Open:OperationError-" + "Message: " + ex.Message,0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("Database---Open:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("Database---Open:Error-" + "Message: " + ex.Message,0);
                return false;
            }
        }
        #endregion

        #region Close database
        public bool Close(int timeout = 3000)
        {
            sqlCnt_SetOrder.Close();
            sqlCnt_GetOrder.Close();
            /*int time = 0;
            while (true)
            {
                if (ConnectionState.Closed == sqlCnt.State)
                {
                    return true;
                }
                Thread.Sleep(1000);
                time += 1000;
                if (time > timeout)
                {
                    return false;
                }
            }*/
            return true;
        }
        #endregion

        #region GetStatus
        public ConnectionState Status()
        {
            return sqlCnt_SetOrder.State;
        }
        #endregion


        public bool DB_Set_Cmd(int pk_id, string receivedata)
        {
            try
            {
                //命令设置
                command_DBHandler = sqlCnt_SetOrder.CreateCommand();
                command_DBHandler.CommandType = CommandType.StoredProcedure;
                command_DBHandler.CommandText = "P_CMD_RECEIVE";

                //参数添加 terminal uid
                command_DBHandler.Parameters.Add("@pk_id", SqlDbType.Int, 4).Value = pk_id;

                //添加receive data
                command_DBHandler.Parameters.Add("@receiveData", SqlDbType.VarChar, 500).Value = receivedata;

                //执行
                int iRet = command_DBHandler.ExecuteNonQuery();
                //返回值
                if (iRet == 0 || iRet == -1)
                {
                    Log.write("Database---DB_Set_Cmd:FALSE");
                    return false;
                }
                Log.write("Database---DB_Set_Cmd:TRUE");
                return true;
            }
            catch (SqlException ex)
            {
                Log.write("Database SqlException DB_Set_Cmd:ERROR-" + ex.Message, 0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("Database---DB_Set_Cmd:ERROR-" + ex.Message,0);
                return false;
            }
        }

        public bool DB_Update_Cmd(int pk_id, int status)
        {
            try
            {
                //命令设置
                command_DBHandler = sqlCnt_GetOrder.CreateCommand();
                command_DBHandler.CommandType = CommandType.StoredProcedure;
                command_DBHandler.CommandText = "P_CMD_RESPONSE";

                //参数添加 terminal uid
                command_DBHandler.Parameters.Add("@pk_id", SqlDbType.Int, 4).Value = pk_id;

                //添加receive data
                command_DBHandler.Parameters.Add("@receiveData", SqlDbType.Int, 4).Value = status;

                //执行
                int iRet = command_DBHandler.ExecuteNonQuery();
                //返回值
                if (iRet == 0 || iRet == -1)
                {
                    Log.write("Database---DB_Update_Cmd:FALSE");
                    return false;
                }
                Log.write("Database---DB_Update_Cmd:TRUE");
                return true;
            }
            catch (SqlException ex)
            {
                Log.write("Database SqlException DB_Update_Cmd:ERROR-" + ex.Message, 0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("Database---DB_Update_Cmd:ERROR-" + ex.Message,0);
                return false;
            }
        }
    }
}
