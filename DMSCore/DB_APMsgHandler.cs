using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using System.Data.SqlClient;
using System.Threading;

using System.Data.SqlTypes;
using System.Data.Common;
namespace MonitorCore
{
    class DB_APMsgHandler
    {
        private SqlConnection sqlCnt = null;
        private SqlCommand command_APHandler = null;
        private bool bStored_Init;
        private int cmd_enable;

        private int identity;
        public DB_APMsgHandler(int cmd_enable, int identity)
        {
            sqlCnt = new SqlConnection();

            command_APHandler = new SqlCommand();
            command_APHandler.CommandType = CommandType.StoredProcedure;
            this.identity = identity;
            this.cmd_enable = cmd_enable;
        }


        #region Open database
        public bool Open(string ServerName, string account, string password, string database, int timeout = 5)
        {
            try
            {
                string cntStr = "server=" + ServerName + ";user id=" + account + ";password=" + password +
                    ";MultipleActiveResultSets=true;database=" + database + ";Connection Timeout=" + timeout.ToString() + ";";
                sqlCnt.ConnectionString = cntStr;

                sqlCnt.Open();

                Thread.Sleep(500);
                if (ConnectionState.Open == sqlCnt.State)
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
                Log.write("DB_APMsgHandler---Open:OperationError-" + "Message: " + ex.Message,0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("DB_APMsgHandler---Open:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("DB_APMsgHandler---Open:Error-" + "Message: " + ex.Message,0);
                return false;
            }


        }
        #endregion

        #region Close database
        public bool Close(int timeout = 3000)
        {

            sqlCnt.Close();

           
            return true;
        }
        #endregion

        #region GetStatus
        public ConnectionState Status()
        {
            return sqlCnt.State;
        }
        #endregion

        #region GetSet
        public DataSet GetSet(string strDataset)
        {
            string statement = "Select uid from " + strDataset;
            SqlDataAdapter da = new SqlDataAdapter(statement, sqlCnt);

            DataSet ds = new DataSet();
            try
            {


                da.Fill(ds, strDataset);
                DataTable tbl = ds.Tables[0];
                foreach (DataColumn col in tbl.Columns)
                {
                    Console.WriteLine(col.ColumnName);
                    Console.WriteLine(tbl.Rows[0]);

                }
            }
            catch
            {

            }
            return ds;
        }
        #endregion


        #region tables operation--sqlCnt for APMsgHandler thread
        public bool AP_Validate(string uid, string channelNum)
        {
            try
            {
                //命令设置
                command_APHandler = sqlCnt.CreateCommand();
                command_APHandler.CommandType = CommandType.StoredProcedure;
                command_APHandler.CommandText = "P_AP_VALIDATION";
                bStored_Init = true;

                //参数添加 ap uid
                command_APHandler.Parameters.Add("@uid", SqlDbType.VarChar, 50).Value = uid;

                //参数添加 ap 通道数
                command_APHandler.Parameters.Add("@apid", SqlDbType.VarChar, 3).Value = channelNum;

                //返回值设置
                SqlParameter parReturn = new SqlParameter("@return", SqlDbType.Int);
                parReturn.Direction = ParameterDirection.ReturnValue; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(parReturn);
                //执行
                command_APHandler.ExecuteNonQuery();

                //返回值
                if (parReturn.Value.Equals(1))
                {
                    Log.write("Database " + identity.ToString() + " --AP_Validate:TRUE");
                    return true;
                }
                else
                {
                    Log.write("Database " + identity.ToString() + " --AP_Validate:FALSE");
                    return false;
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database " + identity.ToString() + " --AP_Validate:OperationError-" + "Message: " + ex.Message,0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("Database " + identity.ToString() + " --AP_Validate:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("Database " + identity.ToString() + " --AP_Validate:Error-" + "Message: " + ex.Message,0);
                return false;
            }
        }

        public bool AP_Status_Update(string uid, int status)
        {
            try
            {
                //命令设置
                command_APHandler = sqlCnt.CreateCommand();
                command_APHandler.CommandType = CommandType.StoredProcedure;
                command_APHandler.CommandText = "P_APSTATUS_UPDATE";
                bStored_Init = true;

                //参数添加 ap uid
                command_APHandler.Parameters.Add("@uid", SqlDbType.VarChar, 50).Value = uid;

                //参数添加 ap 通道数
                if (status == 1)
                {
                    command_APHandler.Parameters.Add("@status", SqlDbType.Int, 4).Value = 1;
                }
                else
                {
                    command_APHandler.Parameters.Add("@status", SqlDbType.Int, 4).Value = 0;
                }


                //返回值设置
                SqlParameter parOutput = command_APHandler.Parameters.Add("@message", SqlDbType.NVarChar, 20);　　//定义输出参数  
                parOutput.Direction = ParameterDirection.Output;　　//参数类型为Output  
                //执行
                int iRet = command_APHandler.ExecuteNonQuery();
                //返回值
                if (iRet == 0 || iRet == -1)
                {
                    Log.write("Database " + identity.ToString() + " --AP_Status_Update:NOT EFFECTIVE");
                    return false;
                }

                string strRet = parOutput.SqlValue.ToString();
                if (strRet == "Null" || strRet == "0")
                {
                    Log.write("Database " + identity.ToString() + " --AP_Status_Update:TRUE");
                    return true;
                }
                else
                {
                    Log.write("Database " + identity.ToString() + " --AP_Status_Update:FALSE");
                    return false;
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database " + identity.ToString() + " --AP_Status_Update:OperationError-" + "Message: " + ex.Message,0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("Database " + identity.ToString() + " --AP_Status_Update:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("Database " + identity.ToString() + " --AP_Status_Update:Error-" + "Message: " + ex.Message,0);
                return false;
            }
        }

        public bool Terminal_Status_Update(string uid, string terminal_uid, int status)
        {
            try
            {
                //命令设置
                command_APHandler = sqlCnt.CreateCommand();
                command_APHandler.CommandType = CommandType.StoredProcedure;
                command_APHandler.CommandText = "P_TERMINALSTATUS_UPDATE";
                bStored_Init = true;

                //参数添加 ap uid
                command_APHandler.Parameters.Add("@uid", SqlDbType.VarChar, 50).Value = uid;

                //参数添加 terminal uid
                command_APHandler.Parameters.Add("@terminal_uid", SqlDbType.VarChar, 50).Value = terminal_uid;

                //参数添加 ap 通道数
                if (status == 1)
                {
                    command_APHandler.Parameters.Add("@status", SqlDbType.Int, 4).Value = 1;
                }
                else
                {
                    command_APHandler.Parameters.Add("@status", SqlDbType.Int, 4).Value = 0;
                }


                //返回值设置
                SqlParameter parOutput = command_APHandler.Parameters.Add("@message", SqlDbType.NVarChar, 20);　　//定义输出参数  
                parOutput.Direction = ParameterDirection.Output;　　//参数类型为Output  
                //执行
                int iRet = command_APHandler.ExecuteNonQuery();
                //返回值
                if (iRet == 0 || iRet == -1)
                {
                    Log.write("Database " + identity.ToString() + " --Terminal_Status_Update:NOT EFFECTIVE");
                    return false;
                }

                string strRet = parOutput.SqlValue.ToString();
                if (strRet == "Null" || strRet == "0")
                {
                    Log.write("Database " + identity.ToString() + " --Terminal_Status_Update:TRUE");

                    return true;
                }
                else
                {
                    Log.write("Database " + identity.ToString() + " --Terminal_Status_Update:FALSE");
                    return false;
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database " + identity.ToString() + " --Terminal_Status_Update:OperationError-" + "Message: " + ex.Message,0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("Database " + identity.ToString() + " --Terminal_Status_Update:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("Database " + identity.ToString() + " --Terminal_Status_Update:Error-" + "Message: " + ex.Message,0);
                return false;
            }
        }

        public int Terminal_Labels_Pos_Update(string terminal_uid, string label1, string label2, string label3, string label4, string label5, int[] coilIndex = null)
        {
            try
            {
                //命令设置
                command_APHandler = sqlCnt.CreateCommand();
                command_APHandler.CommandType = CommandType.StoredProcedure;
                command_APHandler.CommandText = "P_LABEL_POS_UPDATE";

                //参数添加 terminal uid
                command_APHandler.Parameters.Add("@terminal_uid", SqlDbType.VarChar, 50).Value = terminal_uid;

                //添加Label RFID
                command_APHandler.Parameters.Add("@label1_ID", SqlDbType.VarChar, 50).Value = label1;
                command_APHandler.Parameters.Add("@label2_ID", SqlDbType.VarChar, 50).Value = label2;
                command_APHandler.Parameters.Add("@label3_ID", SqlDbType.VarChar, 50).Value = label3;
                command_APHandler.Parameters.Add("@label4_ID", SqlDbType.VarChar, 50).Value = label4;
                command_APHandler.Parameters.Add("@label5_ID", SqlDbType.VarChar, 50).Value = label5;

                if (coilIndex == null)
                {
                    command_APHandler.Parameters.Add("@coilIndex1", SqlDbType.Int, 4).Value = 0;
                    command_APHandler.Parameters.Add("@coilIndex2", SqlDbType.Int, 4).Value = 0;
                    command_APHandler.Parameters.Add("@coilIndex3", SqlDbType.Int, 4).Value = 0;
                    command_APHandler.Parameters.Add("@coilIndex4", SqlDbType.Int, 4).Value = 0;
                    command_APHandler.Parameters.Add("@coilIndex5", SqlDbType.Int, 4).Value = 0;
                }
                else
                {
                    command_APHandler.Parameters.Add("@coilIndex1", SqlDbType.Int, 4).Value = coilIndex[0];
                    command_APHandler.Parameters.Add("@coilIndex2", SqlDbType.Int, 4).Value = coilIndex[1];
                    command_APHandler.Parameters.Add("@coilIndex3", SqlDbType.Int, 4).Value = coilIndex[2];
                    command_APHandler.Parameters.Add("@coilIndex4", SqlDbType.Int, 4).Value = coilIndex[3];
                    command_APHandler.Parameters.Add("@coilIndex5", SqlDbType.Int, 4).Value = coilIndex[4];
                }

                //返回值设置
                SqlParameter parOutput1 = command_APHandler.Parameters.Add("@message1", SqlDbType.NVarChar, 200);　　//定义输出参数  
                parOutput1.Direction = ParameterDirection.Output;　　//参数类型为Output  
                //返回值设置
                SqlParameter parOutput2 = command_APHandler.Parameters.Add("@message2", SqlDbType.NVarChar, 200);　　//定义输出参数  
                parOutput2.Direction = ParameterDirection.Output;　　//参数类型为Output 
                //返回值设置
                SqlParameter parOutput3 = command_APHandler.Parameters.Add("@message3", SqlDbType.NVarChar, 200);　　//定义输出参数  
                parOutput3.Direction = ParameterDirection.Output;　　//参数类型为Output 
                //返回值设置
                SqlParameter parOutput4 = command_APHandler.Parameters.Add("@message4", SqlDbType.NVarChar, 200);　　//定义输出参数  
                parOutput4.Direction = ParameterDirection.Output;　　//参数类型为Output 
                //返回值设置
                SqlParameter parOutput5 = command_APHandler.Parameters.Add("@message5", SqlDbType.NVarChar, 200);　　//定义输出参数  
                parOutput5.Direction = ParameterDirection.Output;　　//参数类型为Output 

                //返回值设置
                SqlParameter param_count = command_APHandler.Parameters.Add("@Count", SqlDbType.Int, 4);　　//定义输出参数  
                param_count.Direction = ParameterDirection.Output;　　//参数类型为Output 

                //返回值设置
                SqlParameter param_ok = command_APHandler.Parameters.Add("@ok", SqlDbType.Int, 4);　　//定义输出参数  
                param_ok.Direction = ParameterDirection.Output;　　//参数类型为Output 
                //执行
                int iRet = command_APHandler.ExecuteNonQuery();

                string strRet1 = parOutput1.SqlValue.ToString();
                string strRet2 = parOutput1.SqlValue.ToString();
                string strRet3 = parOutput1.SqlValue.ToString();
                string strRet4 = parOutput1.SqlValue.ToString();
                string strRet5 = parOutput1.SqlValue.ToString();
                int count = Convert.ToInt32(param_count.SqlValue.ToString());
                int ok = Convert.ToInt32(param_ok.SqlValue.ToString());

                if (iRet == 0 || iRet == -1)
                {
                    Log.write("Database " + identity.ToString() + " --Terminal_Labels_Pos_Update:NOT EFFECTIVE");
                    return 0;
                }

                //返回值
                if (ok == 0)
                {
                    Log.write("Database " + identity.ToString() + " --Terminal_Labels_Pos_Update:TRUE" + count.ToString());
                    return count;
                }
                else
                {
                    Log.write("Database " + identity.ToString() + " --Terminal_Labels_Pos_Update:Failed!" + strRet1 + strRet2 + strRet3 + strRet4 + strRet5);
                    return -1;
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database " + identity.ToString() + " --Terminal_Labels_Pos_Update:OperationError-" + "Message: " + ex.Message,0);
                return -1;
            }
            catch (SqlException ex)
            {
                Log.write("Database " + identity.ToString() + " --Terminal_Labels_Pos_Update:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return -1;
            }
            catch (Exception ex)
            {
                Log.write("Database " + identity.ToString() + " --Terminal_Labels_Pos_Update:Error-" + "Message: " + ex.Message,0);
                return -1;
            }
            
        }
        /*
        public bool Terminal_Coil_Alarm(string terminal_uid, byte status,byte no_index)
        {
            int[] coilsstatus = new int[8];
            int[] coilEnable = new int[8];
            coilsstatus[0] = ((status >> 7) & 0x01);
            coilsstatus[1] = ((status >> 6) & 0x01);
            coilsstatus[2] = ((status >> 5) & 0x01);
            coilsstatus[3] = ((status >> 4) & 0x01);
            coilsstatus[4] = ((status >> 3) & 0x01);
            coilsstatus[5] = ((status >> 2) & 0x01);
            coilsstatus[6] = ((status >> 1) & 0x01);
            coilsstatus[7] = ((status >> 0) & 0x01);


            coilEnable[0] = ((no_index >> 7) & 0x01);
            coilEnable[1] = ((no_index >> 6) & 0x01);
            coilEnable[2] = ((no_index >> 5) & 0x01);
            coilEnable[3] = ((no_index >> 4) & 0x01);
            coilEnable[4] = ((no_index >> 3) & 0x01);
            coilEnable[5] = ((no_index >> 2) & 0x01);
            coilEnable[6] = ((no_index >> 1) & 0x01);
            coilEnable[7] = ((no_index >> 0) & 0x01);
            for (int i = 0; i < 8;i++ )
            {
                if (coilsstatus[i] == 0 && coilEnable[i] == 1)
                {
                    coilsstatus[i] = 2;
                }

                if (coilEnable[i] == 0)
                {
                    coilsstatus[i] = 0;
                }
            }
                try
                {
                    //命令设置
                    command_APHandler = sqlCnt.CreateCommand();
                    command_APHandler.CommandType = CommandType.StoredProcedure;
                    command_APHandler.CommandText = "P_TERMINAL_COIL_STATUS_UPDATE";

                    //参数添加 terminal uid
                    command_APHandler.Parameters.Add("@terminal_uid", SqlDbType.VarChar, 50).Value = terminal_uid;

                    //添加Label RFID
                    command_APHandler.Parameters.Add("@coil1", SqlDbType.VarChar, 50).Value = coilsstatus[0];
                    command_APHandler.Parameters.Add("@coil2", SqlDbType.VarChar, 50).Value = coilsstatus[1];
                    command_APHandler.Parameters.Add("@coil3", SqlDbType.VarChar, 50).Value = coilsstatus[2];
                    command_APHandler.Parameters.Add("@coil4", SqlDbType.VarChar, 50).Value = coilsstatus[3];
                    command_APHandler.Parameters.Add("@coil5", SqlDbType.VarChar, 50).Value = coilsstatus[4];
                    command_APHandler.Parameters.Add("@coil6", SqlDbType.VarChar, 50).Value = coilsstatus[5];
                    command_APHandler.Parameters.Add("@coil7", SqlDbType.VarChar, 50).Value = coilsstatus[6];
                    command_APHandler.Parameters.Add("@coil8", SqlDbType.VarChar, 50).Value = coilsstatus[7];


                    //返回值设置
                    SqlParameter parOutput1 = command_APHandler.Parameters.Add("@message", SqlDbType.NVarChar, 200);　　//定义输出参数  
                    parOutput1.Direction = ParameterDirection.Output;　　//参数类型为Output  

                    //返回值设置
                    SqlParameter param_ok = command_APHandler.Parameters.Add("@ok", SqlDbType.Int, 4);　　//定义输出参数  
                    param_ok.Direction = ParameterDirection.Output;　　//参数类型为Output 

                    int iRet = command_APHandler.ExecuteNonQuery();

                    int ok = Convert.ToInt32(param_ok.SqlValue.ToString());

                    if (ok == 0 || ok == -1)
                    {
                        Log.write("Database " + identity.ToString() + " --Terminal_Coil_Alarm:NOT EFFECTIVE");
                        return false;
                    }
                    else if (ok == 1)
                    {
                        Log.write("Database " + identity.ToString() + " --Terminal_Coil_Alarm:TRUE");
                        return true;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Log.write("Database " + identity.ToString() + " --Terminal_Coil_Alarm:OperationError-" + "Message: " + ex.Message, 0);
                    return false;
                }
                catch (SqlException ex)
                {
                    Log.write("Database " + identity.ToString() + " --Terminal_Coil_Alarm:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message, 0);
                    SqlExceptionHelper.Deal(ex);
                    return false;
                }
                catch (Exception ex)
                {
                    Log.write("Database " + identity.ToString() + " --Terminal_Coil_Alarm:Error-" + "Message: " + ex.Message, 0);
                    return false;
                }
            return false;
        }
        */
        public bool CmdLog(string cmdCode, string apuid, string receiveData)
        {
            if (cmd_enable != 1)
            {
                return true;
            }
            try
            {
                //命令设置
                command_APHandler = sqlCnt.CreateCommand();
                command_APHandler.CommandType = CommandType.StoredProcedure;
                command_APHandler.CommandText = "P_CMD_LOG";


                command_APHandler.Parameters.Add("@cmdCode", SqlDbType.VarChar, 20).Value = cmdCode;


                command_APHandler.Parameters.Add("@apuid", SqlDbType.VarChar, 50).Value = apuid;
                command_APHandler.Parameters.Add("@receiveData", SqlDbType.VarChar, 500).Value = receiveData;

                //执行
                int iRet = command_APHandler.ExecuteNonQuery();

                //返回值
                if (iRet == 0 || iRet == -1)
                {
                    Log.write("Database " + identity.ToString() + " --CmdLog:NOT EFFECTIVE");
                    return false;
                }

                Log.write("Database " + identity.ToString() + " --CmdLog:OK");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database " + identity.ToString() + " --CmdLog:OperationError-" + "Message: " + ex.Message,0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("Database " + identity.ToString() + " --CmdLog:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("Database " + identity.ToString() + " --CmdLog:Error-" + "Message: " + ex.Message,0);
                return false;
            }

        }


        public bool Guard_Validate(string guard_uid, string guard_data, int Num)
        {
            try
            {
                //命令设置
                command_APHandler = sqlCnt.CreateCommand();
                command_APHandler.CommandType = CommandType.StoredProcedure;
                command_APHandler.CommandText = "P_GUARD_VALIDATE";

                guard_data = StringSplit(guard_data);
                //参数添加 terminal uid
                command_APHandler.Parameters.Add("@array", SqlDbType.VarChar, 900).Value = guard_data;

                //添加seperator
                command_APHandler.Parameters.Add("@seperator", SqlDbType.VarChar, 5).Value = ",";
                command_APHandler.Parameters.Add("@guard_uid", SqlDbType.VarChar, 50).Value = guard_uid;
                command_APHandler.Parameters.Add("@totallength", SqlDbType.Int, 4).Value = guard_data.Length;

                //执行
                int iRet = command_APHandler.ExecuteNonQuery();

                //返回值设置
                SqlParameter parReturn = new SqlParameter("@return", SqlDbType.Int);
                parReturn.Direction = ParameterDirection.ReturnValue; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(parReturn);

                //返回值
                if (iRet == 0 || iRet == -1)
                {
                    Log.write("Database " + identity.ToString() + " --Guard_Validate:NOT EFFECTIVE");
                    return false;
                }

                if (parReturn.Value.Equals(Num))
                {
                    Log.write("Database " + identity.ToString() + " --Guard_Validate:TRUE");
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database " + identity.ToString() + " --Guard_Validate:OperationError-" + "Message: " + ex.Message);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("Database " + identity.ToString() + " --Guard_Validate:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("Database " + identity.ToString() + " --Guard_Validate:Error-" + "Message: " + ex.Message);
                return false;
            }
        }

        private string StringSplit(string s, int index = 0, int length = 0)
        {

            byte[] b = Encoding.ASCII.GetBytes(s);//按照指定编码将string编程字节数组
            string result = string.Empty;
            if (length == 0)
            {
                length = b.Length;
            }
            if (index + length > b.Length)
            {
                length = b.Length - index;
            }
            for (int i = index; i < index + length; i++)//逐字节变为16进制字符
            {
                if (i == 0)
                {
                    result += Convert.ToString(b[i], 16);
                }
                else if (i % 8 == 0)
                {
                    result += "," + Convert.ToString(b[i], 16);
                }
                else
                {
                    result += Convert.ToString(b[i], 16);
                }

            }
            return result;
        }

        #endregion


    }
}
