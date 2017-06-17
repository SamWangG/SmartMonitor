using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace MonitorCore
{
    class DBMsgHandler
    {
        private List<APParser> dbResultList;//Databse Result List
        private List<APParser> dbOrderList;//Database Order List

        private List<APParser> dbSentList;//Sent List

        private UDPSocket udpsocket;
        private DB_DBMsgHandler database;

        private static Thread DBHandlerThread = null;
        private bool bStop = false;
        ManualResetEvent resumeEvent = new ManualResetEvent(false);
        public DBMsgHandler(UDPSocket udpsocket, DB_DBMsgHandler database, List<APParser> dbOrderList, List<APParser> dbResultList)
        {
            this.dbResultList = dbResultList;
            this.dbOrderList = dbOrderList;
            this.database = database;
            this.udpsocket = udpsocket;
            dbSentList = new List<APParser>();
            DBHandlerThread = new Thread(new ThreadStart(DBHandleFunc));
        }

        ~DBMsgHandler()
        {

        }

        public void Start()
        {
            bStop = false;
            DBHandlerThread.Start();
        }

        public void Stop()
        {
            bStop = true;
            //int threadTimeout = 0;
            resumeEvent.Set();
        }

        public void Resume(object sender,EventArgs e)
        {
            Log.write("DBMsgHandler-Resume");
            resumeEvent.Set();
        }

        private void DBHandleFunc()
        {
            while(!bStop)
            {
                DBHandleJob();
                Thread.Sleep(10);
            }
            Log.write("DBMsgHandler-ThreadEnd");
        }

        private void DBHandleJob()
        {
            try
            {
                if (dbOrderList.Count == 0 && dbSentList.Count == 0)
                {
                    Log.write("DBMsgHandler-WaitOne");
                    resumeEvent.Reset();
                    resumeEvent.WaitOne();
                }

                lock (dbOrderList)
                {
                    if (dbOrderList.Count != 0)
                    {
                        APParser apParser = dbOrderList[0];
                        udpsocket.Send(apParser.GetParam_Origin_byte());
                        dbOrderList.RemoveAt(0);
                        dbSentList.Add(apParser);
                        Log.write("DBMsgHandler---Send a command" ,0);
                    }//处理发送数据
                }//锁存dbOrderList


                int dbSent_count = dbSentList.Count;
                if (dbSent_count == 0)
                {

                    return;
                }//已发送命令

                lock(dbResultList)
                {
                    int dbResult_count = dbResultList.Count;
                    if (dbResult_count == 0)
                    {
                        Thread.Sleep(50);
                        return;
                    }//有处理结果返回

                    for (int i = dbResult_count - 1; i >= 0; i--)
                    {
                        dbResult_count = dbResultList.Count;

                        if(dbResult_count==0)
                        { 
                            break; 
                        }//结果为0，结束

                        bool isReponsed = false;
                        for (int j = dbSent_count - 1; j >= 0; j--)
                        {
                            APParser SentParser = dbSentList[j];
                            APParser ResultParser = dbResultList[i];
                            int iRet = APParser.RelatingMsg(SentParser, ResultParser);
                            if (iRet == 1)
                            {
                                database.DB_Set_Cmd(SentParser.GetID(), ResultParser.GetParam_Result());
                                //Log.write("DBMsgHandler-Get relative message:sent JobDone");
                                Log.write("DBMsgHandler---matching a result:" + SentParser.GetParam_Origin_Hex() + "\r\n" + ResultParser.GetParam_Origin_Hex() + "\r\n",2);
                                dbSentList.RemoveAt(j);
                                dbResultList.RemoveAt(i);
                                SentParser.Dispose();
                                ResultParser.Dispose();
                                isReponsed = true;

                                break;//match and break the circle

                            }
                            else if (iRet == 0)
                            {
                                database.DB_Update_Cmd(SentParser.GetID(), ResultParser.GetParam_Cmd_SendOrNot());
                                
                                Log.write("DBMsgHandler---matching a response:" + SentParser.GetParam_Origin_Hex() + "\r\n" + ResultParser.GetParam_Origin_Hex() + "\r\n",2);
                                isReponsed = true;
                                dbResultList.RemoveAt(i);
                                ResultParser.Dispose();
                                break;//match and break the circle
                            }
                            else
                            {
                                
                            }
                        }

                        if(!isReponsed)
                        {
                            APParser aa = dbResultList[i];
                            aa.Dispose();
                            dbResultList.RemoveAt(i);
                            Log.write("DBMsgHandler---inequal result",0);
                        }
                    }//遍历匹配
                }//锁存
                
            }
            catch (SqlException ex)
            {
                Log.write("DBMsgHandler SqlException Error:" + ex.Message, 0);
                SqlExceptionHelper.Deal(ex);
            }
            catch(Exception ex)
            {
                Log.write("DBMsgHandler-ThreadError:"+ex.Message,0);
                dbOrderList.Clear();
                dbResultList.Clear();
                Log.write("DBMsgHandler-Clear All",0);
            }
            
        }
    }
}

