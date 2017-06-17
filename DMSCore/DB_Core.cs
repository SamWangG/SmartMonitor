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
    class DB_Core
    {
        private SqlConnection sqlCnt_APServerStatus = null;//AP server status

        private SqlConnection sqlCnt_MngServerStatus = null;//manager server status

        private SqlConnection sqlCnt_VideoServerStatus = null;

        private SqlCommand command_UDP = null;//traversal all device status

        private SqlCommand command_APServerStatus = null;

        private SqlCommand command_MngServerStatus = null;

        private SqlCommand command_VideoServerStatus = null;

        private bool bStored_Init;

        private int cmd_enable;

        private int timeout=1000;
        public DB_Core(int cmd_enable)
        {
            sqlCnt_APServerStatus = new SqlConnection();

            sqlCnt_MngServerStatus = new SqlConnection();

            sqlCnt_VideoServerStatus = new SqlConnection();
            //command_Set = new SqlCommand();
            command_APServerStatus = new SqlCommand();
            command_APServerStatus.CommandType = CommandType.StoredProcedure;

            command_MngServerStatus = new SqlCommand();
            command_MngServerStatus.CommandType = CommandType.StoredProcedure;

            command_VideoServerStatus = new SqlCommand();
            command_VideoServerStatus.CommandType = CommandType.StoredProcedure;

            command_UDP = new SqlCommand();
            command_UDP.CommandType = CommandType.Text;
            
            this.cmd_enable=cmd_enable;
        }

        ~DB_Core()
        {

        }

#region Open database
        public bool Open(string ServerName,string account,string password,string database,int timeout=5)
        {
            try
            {
                string cntStr = "server=" + ServerName + ";user id=" + account + ";password=" + password +
                    ";MultipleActiveResultSets=true;database=" + database + ";Connection Timeout=" + timeout.ToString() + ";";
                sqlCnt_APServerStatus.ConnectionString = cntStr;

                sqlCnt_APServerStatus.Open();

                sqlCnt_MngServerStatus.ConnectionString = cntStr;

                sqlCnt_MngServerStatus.Open();


                sqlCnt_VideoServerStatus.ConnectionString = cntStr;

                sqlCnt_VideoServerStatus.Open();

                Thread.Sleep(500);
                if (ConnectionState.Open == sqlCnt_APServerStatus.State)
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
                Log.write("Database---Open:SqlError-Number:"+ex.Number.ToString()+";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch(Exception ex)
            {
                Log.write("Database---Open:Error-" + "Message: " + ex.Message,0);
                return false;
            }
            
           
        }
#endregion

#region Close database
        public bool Close(int timeout=3000)
        {
            sqlCnt_APServerStatus.Close();
            sqlCnt_MngServerStatus.Close();
            sqlCnt_VideoServerStatus.Close();
           
            return true;
        }
#endregion

#region GetStatus
        public ConnectionState Status()
        {
            return sqlCnt_APServerStatus.State;
        }
#endregion

        public bool GetDataGroup(string strOrder, ref SqlDataReader reader)
        {

            try
            {
                string statement = strOrder;
                SqlDataAdapter da = new SqlDataAdapter(statement, sqlCnt_MngServerStatus);

                command_UDP = sqlCnt_MngServerStatus.CreateCommand();
                command_UDP.CommandText = statement;
                reader = command_UDP.ExecuteReader();
                return true;
            }
            catch
            {
                return false;
            }

        }

        #region AP Server timeouot
        public bool APServer_Status(bool IsConnected)
        {
            try
            {
                //命令设置
                command_APServerStatus = sqlCnt_APServerStatus.CreateCommand();
                command_APServerStatus.CommandType = CommandType.StoredProcedure;
                command_APServerStatus.CommandText = "P_AP_SERVER_STATUS_UPDATE";
                bStored_Init = true;

                //超时
                if (IsConnected == true)
                {
                    command_APServerStatus.Parameters.Add("@status", SqlDbType.Int, 4).Value = 1;
                }
                else
                {
                    command_APServerStatus.Parameters.Add("@status", SqlDbType.Int, 4).Value = 0;
                }


                //返回值设置
                SqlParameter parOutput = command_APServerStatus.Parameters.Add("@message", SqlDbType.NVarChar, 200);　　//定义输出参数  
                parOutput.Direction = ParameterDirection.Output;　　//参数类型为Output  
                //执行
                int iRet = command_APServerStatus.ExecuteNonQuery();
                //返回值
                if (iRet == 0 || iRet == -1)
                {
                    Log.write("Database---APServer_Timeout:NOT EFFECTIVE");
                    return false;
                }

                string strRet = parOutput.SqlValue.ToString();
                if (strRet == "Null" || strRet == "0")
                {
                    Log.write("Database---APServer_Timeout:TRUE");
                    return true;
                }
                else
                {
                    Log.write("Database---APServer_Timeout:FALSE-" + strRet);
                    return false;
                }
               
            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database---APServer_Timeout:OperationError-" + "Message: " + ex.Message,0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("Database---APServer_Timeout:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("Database---APServer_Timeout:Error-" + "Message: " + ex.Message,0);
                return false;
            }
        }
        #endregion

        #region Manager Server timeout
        public bool ManagerServer_Status(bool IsConnected)
        {
            try
            {
                //命令设置
                command_MngServerStatus = sqlCnt_MngServerStatus.CreateCommand();
                command_MngServerStatus.CommandType = CommandType.StoredProcedure;
                command_MngServerStatus.CommandText = "P_MNG_SERVER_STATUS_UPDATE";
                bStored_Init = true;

                //超时
                if (IsConnected == true)
                {
                    command_MngServerStatus.Parameters.Add("@status", SqlDbType.Int, 4).Value = 1;
                }
                else
                {
                    command_MngServerStatus.Parameters.Add("@status", SqlDbType.Int, 4).Value = 0;
                }


                //返回值设置
                SqlParameter parOutput = command_MngServerStatus.Parameters.Add("@message", SqlDbType.NVarChar, 200);　　//定义输出参数  
                parOutput.Direction = ParameterDirection.Output;　　//参数类型为Output  
                //执行
                int iRet = command_MngServerStatus.ExecuteNonQuery();
                //返回值
                if (iRet == 0 || iRet == -1)
                {
                    Log.write("Database---ManagerServer_Status:NOT EFFECTIVE");
                    return false;
                }

                string strRet = parOutput.SqlValue.ToString();
                if (strRet == "Null" || strRet == "0")
                {
                    Log.write("Database---ManagerServer_Status:TRUE");

                    return true;
                }
                else
                {
                    Log.write("Database---ManagerServer_Status:FALSE-" + strRet);
                    return false;
                }
            }
            catch(InvalidOperationException ex)
            {
                Log.write("Database---ManagerServer_Status:OperationError-" + "Message: " + ex.Message,0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("Database---ManagerServer_Status:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("Database---ManagerServer_Status:Error-" + "Message: " + ex.Message,0);
                return false;
            }
        }
        #endregion

        #region Video
        public bool VideoServer_Status(bool IsConnected)
        {

            try
            {
                //命令设置
                command_MngServerStatus = sqlCnt_MngServerStatus.CreateCommand();
                command_MngServerStatus.CommandType = CommandType.StoredProcedure;
                command_MngServerStatus.CommandText = "P_VIDEO_SERVER_UPDATE";
                bStored_Init = true;

                //超时
                if (IsConnected == true)
                {
                    command_MngServerStatus.Parameters.Add("@status", SqlDbType.Int, 4).Value = 1;
                }
                else
                {
                    command_MngServerStatus.Parameters.Add("@status", SqlDbType.Int, 4).Value = 0;
                }


                //返回值设置
                SqlParameter parOutput = command_MngServerStatus.Parameters.Add("@message", SqlDbType.NVarChar, 200);　　//定义输出参数  
                parOutput.Direction = ParameterDirection.Output;　　//参数类型为Output  
                //执行
                int iRet = command_MngServerStatus.ExecuteNonQuery();
                //返回值
                if (iRet == 0 || iRet == -1)
                {
                    Log.write("Database---VideoServer_Status:NOT EFFECTIVE");
                    return false;
                }

                string strRet = parOutput.SqlValue.ToString();
                if (strRet == "Null" || strRet == "0")
                {
                    Log.write("Database---VideoServer_Status:TRUE");

                    return true;
                }
                else
                {
                    Log.write("Database---VideoServer_Status:FALSE-" + strRet);
                    return false;
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database---VideoServer_Status:OperationError-" + "Message: " + ex.Message, 0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("Database---VideoServer_Status:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message, 0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("Database---VideoServer_Status:Error-" + "Message: " + ex.Message, 0);
                return false;
            }
        }
        #endregion

        public void OnstateChange(object sender, EventArgs e)
        {
            
        }
    }
}
