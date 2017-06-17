using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.SqlClient;
using System.Data;
using System.Data.SqlTypes;
using System.Data.Common;

namespace MonitorCore
{
    class DBOrderMonitor
    {
        private SqlConnection sqlCnt = null;
        private SqlCommand command_DBThreadHandler = null;

        //database Thread
        private static Thread dbThread = null;
        private bool bStop = false;
        public event EventHandler Event_ResumeDBHandler;

        private List<APParser> dbOrderList;
        
        public DBOrderMonitor(List<APParser> dbOrderList)
        {
            sqlCnt = new SqlConnection();

            command_DBThreadHandler=new SqlCommand();
            command_DBThreadHandler.CommandType=CommandType.StoredProcedure;

            this.dbOrderList = dbOrderList;

            dbThread = new Thread(new ThreadStart(OrderListFunc));
        }


        #region Open database
        public bool Open(string ServerName, string account, string password, string database, int timeout = 5000)
        {
            try
            {
                sqlCnt.ConnectionString = "server=" + ServerName + ";user id=" + account + ";password=" + password + ";database=" + database + ";";
                sqlCnt.Open();
                if (ConnectionState.Open == sqlCnt.State)
                {   
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (SqlException ex)
            {
                Log.write("DBOrderMonitor SqlException Error:" + ex.Message, 0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("DBOrderMonitor---Open:ERROR-" + ex.Message,0);
                return false;
            }


        }
        #endregion

        #region Close database
        public void Close(int timeout = 3000)
        {   
           /* int threadTimeout = 0;
            while (dbThread.ThreadState != ThreadState.Stopped)
            {
                Thread.Sleep(100);
                threadTimeout += 100;
                if (threadTimeout > 1000)
                {
                    dbThread.Abort();
                    Thread.Sleep(100);
                }
            }*/

            sqlCnt.Close();
        }
        #endregion

        public void Start()
        {
            Thread.Sleep(500);
            bStop = false;
            dbThread.Start();
        }
        
        public void Stop()
        {
            bStop = true;
        }

        private void OrderListFunc()
        {
            while (!bStop)
            {
                Thread.Sleep(100);
                OrderList_Monitor();
            }
            Log.write("DBOrderMonitor-ThreadEnd");
        }
        private void OrderList_Monitor()
        {
           
            if (DB_Get_Cmd())
            {
                Event_ResumeDBHandler(null, null);
            }//有数据则启动处理线程
        }

        private bool DB_Get_Cmd()
        {
            try
            {
                //命令设置
                command_DBThreadHandler = sqlCnt.CreateCommand();
                command_DBThreadHandler.CommandType = CommandType.StoredProcedure;
                command_DBThreadHandler.CommandText = "P_CMD_SEND";

                //返回值设置
                SqlParameter Output_cmdCode = command_DBThreadHandler.Parameters.Add("@cmdCode", SqlDbType.VarChar, 20);　　//定义输出参数  
                Output_cmdCode.Direction = ParameterDirection.Output;　　//参数类型为Output  

                //返回值设置
                SqlParameter Output_data = command_DBThreadHandler.Parameters.Add("@sendData", SqlDbType.VarChar, 500);　　//定义输出参数  
                Output_data.Direction = ParameterDirection.Output;　　//参数类型为Output  

                //返回值设置
                SqlParameter Output_ID = command_DBThreadHandler.Parameters.Add("@pk_id", SqlDbType.Int, 4);　　//定义输出参数  
                Output_ID.Direction = ParameterDirection.Output;　　//参数类型为Output  

                //返回值设置
                SqlParameter Output_Return = new SqlParameter("@return", SqlDbType.Int);
                Output_Return.Direction = ParameterDirection.ReturnValue; 　　//参数类型为ReturnValue                     
                command_DBThreadHandler.Parameters.Add(Output_Return);

                //执行
                command_DBThreadHandler.ExecuteNonQuery();

               int iRet=Convert.ToInt32(Output_Return.SqlValue.ToString());
               if (iRet==0)
                {
                    return false;
                }
               else if (iRet == 1)
                {
                    string strCmd_Code = Output_cmdCode.SqlValue.ToString();
                    string strCmd_Data = Output_data.SqlValue.ToString();
                    int iCmd_ID = Convert.ToInt32(Output_ID.SqlValue.ToString());

                    Cmd_AddList(strCmd_Code, strCmd_Data, iCmd_ID);
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            catch (SqlException ex)
            {
                Log.write("DB_Get_Cmd SqlException Error:" + ex.Message, 0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch(Exception ex)
            {
                Log.write("DB_Get_Cmd Error:" + ex.Message,0);
                SqlExceptionHelper.restart();
                return false;
            }
        }

        private byte[] HexStringToByte(string strHex)
        {
            //以,分割字符串，并去掉空字符
            string[] chars = strHex.Split(new char[] { ',' });
            byte[] b = new byte[chars.Length];
            //逐个字符变为16进制字节数据
            for (int i = 0; i < chars.Length; i++)
            {
                b[i] = Convert.ToByte(chars[i], 16);
            }
            //按照指定编码将字节数组变为字符串
            return b;
        }

        private void Cmd_AddList(string strCode, string strData, int ID)
        {
            //逗号分隔
            byte[] byte_data = HexStringToByte(strData);
            string test = Encoding.ASCII.GetString(byte_data);
            //创建协议结构
            int size = byte_data.Length;
            byte frame_size_high = (byte)((size + 9) >> 8);
            byte frame_size_low = (byte)(size + 9);
            byte checksum_high = (byte)((size + 13) >> 8);
            byte checksum_low = (byte)(size + 13);
            byte[] byteArray = new byte[13 + size];
            byteArray[0] = 36;
            byteArray[1] = 36;
            byteArray[2] = frame_size_high;
            byteArray[3] = frame_size_low;
            byteArray[4] = 2;

            byteArray[11 + size] = 37;
            byteArray[12 + size] = 37;
            //数据分配插入
            if (strCode.Equals("05020100"))
            {
                byteArray[5] = 5;
                byteArray[6] = 2;
                byteArray[7] = 1;
                byteArray[8] = 0;
                for (int i = 0; i < size; i++)
                {
                    byteArray[9 + i] = byte_data[i];
                }
            }//终端搜索
            else if (strCode.Equals("05020200"))
            {
                byteArray[5] = 5;
                byteArray[6] = 2;
                byteArray[7] = 2;
                byteArray[8] = 0;
                for (int i = 0; i < size; i++)
                {
                    byteArray[9 + i] = byte_data[i];
                }
            }//终端配置
            else if (strCode.Equals("05020300"))
            {
                byteArray[5] = 5;
                byteArray[6] = 2;
                byteArray[7] = 3;
                byteArray[8] = 0;
                for (int i = 0; i < size; i++)
                {
                    byteArray[9 + i] = byte_data[i];
                }
            }//终端删除
            else if (strCode.Equals("05020400"))
            {
                //命令
                byteArray[5] = 5;
                byteArray[6] = 2;
                byteArray[7] = 4;
                byteArray[8] = 0;
                //数据
                for (int i = 0; i < size; i++)
                {
                    byteArray[9 + i] = byte_data[i];
                }
                /*
                //校驗位
                int T_datasize = byteArray[48];
                int T_recheck = 0;
                for (int i = 0; i < 16 + T_datasize; i++)
                {
                    T_recheck += byteArray[33 + i];
                }//系統協議校驗位

                byteArray[49 + T_datasize] = (byte)(T_recheck >> 8);
                byteArray[50 + T_datasize] = (byte)(T_recheck);*/
            }//终端命令
            else if(strCode.Equals("05010400"))
            {
                //命令
                byteArray[5] = 5;
                byteArray[6] = 1;
                byteArray[7] = 4;
                byteArray[8] = 0;
                //数据
                for (int i = 0; i < size; i++)
                {
                    byteArray[9 + i] = byte_data[i];
                }
                /*
                //校驗位
                int T_datasize = byteArray[36];
                int T_recheck = 0;
                for (int i = 0; i < 16 + T_datasize; i++)
                {
                    T_recheck += byteArray[33 + i];
                }//系統協議校驗位

                byteArray[49 + T_datasize] = (byte)(T_recheck >> 8);
                byteArray[50 + T_datasize] = (byte)(T_recheck);*/
            }//AP命令


            int recheck=0;
            for (int i = 0; i < 8 + size;i++)
            {
                recheck += byteArray[i];
            }//系統協議校驗位

            byteArray[9 + size] = (byte)(recheck>>8);
            byteArray[10 + size] = (byte)(recheck);

            byteArray[9 + size] = (byte)(recheck >> 8);
            byteArray[10 + size] = (byte)(recheck);

            //添加列表
            APParser apParser = new APParser(byteArray, size + 13, ID);
            dbOrderList.Add(apParser);
        }
    }
}
