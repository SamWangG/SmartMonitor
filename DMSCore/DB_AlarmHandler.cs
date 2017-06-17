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
    class DB_AlarmHandler
    {
        private SqlConnection sqlCnt = null;
        private SqlCommand command_APHandler = null;
        private bool bStored_Init;
        private int cmd_enable;

        private bool isWaiting;//check if thread is running
        private int identity;//ID

        private object Locker = new object();//more than one thread invoke this databse.
        public DB_AlarmHandler(int cmd_enable, int identity)
        {
            sqlCnt = new SqlConnection();

            command_APHandler = new SqlCommand();
            command_APHandler.CommandType = CommandType.StoredProcedure;
            this.cmd_enable = cmd_enable;
            this.identity = identity;
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
                Log.write("DB_AlarmHandler---Open:OperationError-" + "Message: " + ex.Message,0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("DB_AlarmHandler---Open:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("DB_AlarmHandler---Open:Error-" + "Message: " + ex.Message,0);
                return false;
            }


        }
        #endregion

        public bool Terminal_Coil_Alarm(string terminal_uid, byte status, byte no_index)
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
            for (int i = 0; i < 8; i++)
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


        #region Close database
        public bool Close(int timeout = 3000)
        {

            sqlCnt.Close();
            return true;
        }
        #endregion

        public int Guard_Validate(string guard_uid, string guard_data, int Num,ref int index1,ref int index2,ref int index3,ref int pk_id)
        {
            lock (Locker)
            {
                try
                {
                    guard_data = StringSplit(guard_data, 0, Num * 16);
                    //命令设置
                    command_APHandler = sqlCnt.CreateCommand();
                    command_APHandler.CommandType = CommandType.StoredProcedure;
                    command_APHandler.CommandText = "P_GUARD_VALIDATE";


                    //参数添加 terminal uid
                    command_APHandler.Parameters.Add("@array", SqlDbType.VarChar, 900).Value = guard_data;

                    //添加seperator
                    command_APHandler.Parameters.Add("@seperator", SqlDbType.VarChar, 5).Value = ",";
                    command_APHandler.Parameters.Add("@guard_uid", SqlDbType.VarChar, 50).Value = guard_uid;
                    command_APHandler.Parameters.Add("@totallength", SqlDbType.Int, 4).Value = guard_data.Length;

                    //Index1
                    SqlParameter cameraIndex1 = new SqlParameter("@cameraIndex1", SqlDbType.VarChar, 200);
                    cameraIndex1.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                    command_APHandler.Parameters.Add(cameraIndex1);

                    //Index2
                    SqlParameter cameraIndex2 = new SqlParameter("@cameraIndex2", SqlDbType.VarChar, 200);
                    cameraIndex2.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                    command_APHandler.Parameters.Add(cameraIndex2);

                    //Index3
                    SqlParameter cameraIndex3 = new SqlParameter("@cameraIndex3", SqlDbType.VarChar, 200);
                    cameraIndex3.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                    command_APHandler.Parameters.Add(cameraIndex3);

                    //pk_id
                    SqlParameter param_pk_id = new SqlParameter("@pk_id_ret", SqlDbType.Int);
                    param_pk_id.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                    command_APHandler.Parameters.Add(param_pk_id);

                    //返回值设置
                    SqlParameter parReturn = new SqlParameter("@return", SqlDbType.Int);
                    parReturn.Direction = ParameterDirection.ReturnValue; 　　//参数类型为ReturnValue                     
                    command_APHandler.Parameters.Add(parReturn);

                    //执行
                    int iRet = command_APHandler.ExecuteNonQuery();

                    index1 = Convert.ToInt32(cameraIndex1.Value.ToString());
                    index2 = Convert.ToInt32(cameraIndex2.Value.ToString());
                    index3 = Convert.ToInt32(cameraIndex3.Value.ToString());
                    pk_id = Convert.ToInt32(param_pk_id.Value.ToString());
                    //返回值
                    if (iRet == 0 || iRet == -1)
                    {
                        Log.write("DB_AlarmHandler " + identity.ToString() + " --Guard_Validate:NOT EFFECTIVE", 1);
                        return 0;
                    }
                    int iRetNum = Convert.ToInt32(parReturn.Value.ToString());
                    if (iRetNum > 0)
                    {
                        Log.write("DB_AlarmHandler " + identity.ToString() + " --Guard_Validate:FALSE", 1);
                        return iRetNum;
                    }
                    else
                    {
                        Log.write("DB_AlarmHandler " + identity.ToString() + " --Guard_Validate:TRUE", 1);
                        return iRetNum;
                    }

                }
                catch (InvalidOperationException ex)
                {
                    Log.write("DB_AlarmHandler " + identity.ToString() + " --Guard_Validate:OperationError-" + "Message: " + ex.Message, 0);
                    return -1;
                }
                catch (SqlException ex)
                {
                    Log.write("DB_AlarmHandler " + identity.ToString() + " --Guard_Validate:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message, 0);
                    SqlExceptionHelper.Deal(ex);
                    return -1;
                }
                catch (Exception ex)
                {
                    Log.write("DB_AlarmHandler " + identity.ToString() + " --Guard_Validate:Error-" + "Message: " + ex.Message, 0);
                    return -1;
                }
            }
            
        }

        public bool SetVideoFile(int pk_id,string videoPath,string videoFile1,string videoFile2,string videoFile3,string videoFile4)
        {
            lock(Locker)
            {
                try
                {
                    //命令设置
                    command_APHandler = sqlCnt.CreateCommand();
                    command_APHandler.CommandType = CommandType.StoredProcedure;
                    command_APHandler.CommandText = "P_GUARD_VIDEO_FILE";

                    //参数添加 terminal uid
                    command_APHandler.Parameters.Add("@pk_id", SqlDbType.Int).Value = pk_id;

                    //添加参数
                    command_APHandler.Parameters.Add("@videoPath", SqlDbType.VarChar, 100).Value = videoPath;
                    command_APHandler.Parameters.Add("@videoFile1", SqlDbType.VarChar, 100).Value = videoFile1;
                    command_APHandler.Parameters.Add("@videoFile2", SqlDbType.VarChar, 100).Value = videoFile2;
                    command_APHandler.Parameters.Add("@videoFile3", SqlDbType.VarChar, 100).Value = videoFile3;
                    command_APHandler.Parameters.Add("@videoFile4", SqlDbType.VarChar, 100).Value = videoFile4;

                    //返回值设置
                    SqlParameter parReturn = new SqlParameter("@return", SqlDbType.Int);
                    parReturn.Direction = ParameterDirection.ReturnValue; 　　//参数类型为ReturnValue                     
                    command_APHandler.Parameters.Add(parReturn);

                    //执行
                    int iRet = command_APHandler.ExecuteNonQuery();

                    //返回值
                    if (iRet == 0 || iRet == -1)
                    {
                        Log.write("DB_AlarmHandler " + identity.ToString() + " --SetVideoFile:NOT EFFECTIVE");
                        return false;
                    }
                    int iRetNum = Convert.ToInt32(parReturn.Value.ToString());
                    if (iRetNum == 1)
                    {
                        Log.write("DB_AlarmHandler " + identity.ToString() + " --SetVideoFile:TRUE");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Log.write("DB_AlarmHandler " + identity.ToString() + " --SetVideoFile:OperationError-" + "Message: " + ex.Message, 0);
                    return false;
                }
                catch (SqlException ex)
                {
                    Log.write("DB_AlarmHandler " + identity.ToString() + " --SetVideoFile:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message, 0);
                    SqlExceptionHelper.Deal(ex);
                    return false;
                }
                catch (Exception ex)
                {
                    Log.write("DB_AlarmHandler " + identity.ToString() + " --SetVideoFile:Error-" + "Message: " + ex.Message, 0);
                    return false;
                }
            }
            
        }

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
                    Log.write("DB_AlarmHandler " + identity.ToString() + " --CmdLog:NOT EFFECTIVE");
                    return false;
                }

                Log.write("DB_AlarmHandler " + identity.ToString() + " --CmdLog:OK");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                Log.write("DB_AlarmHandler " + identity.ToString() + " --CmdLog:OperationError-" + "Message: " + ex.Message,0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("DB_AlarmHandler " + identity.ToString() + " --CmdLog:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("DB_AlarmHandler " + identity.ToString() + " --CmdLog:Error-" + "Message: " + ex.Message,0);
                return false;
            }

        }


        private string StringSplit(string s, int index = 0, int length = 0)
        {

            //byte[] b = Encoding.Default.GetBytes(s);//按照指定编码将string编程字节数组
            char[] cc = s.ToCharArray();
            string result = string.Empty;
            if (length == 0)
            {
                length = cc.Length;
            }
            if (index + length > cc.Length)
            {
                length = cc.Length - index;
            }

            
            for (int i = index; i < index + length; i++)//逐字节变为16进制字符
            {
                if (i == 0)
                {
                    result += cc[i];//Convert.ToString(b[i], 16);
                }
                else if (i % 16 == 0)
                {
                    result += "," + cc[i]; //Convert.ToString(b[i], 16);
                }
                else if(i==index + length-1)
                {
                    result += cc[i]+","; //Convert.ToString(b[i], 16);
                }
                else
                {
                    result += cc[i] ; //Convert.ToString(b[i], 16);
                }

            }
            return result;
        }
    }
}
