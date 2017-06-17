using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Data;
using System.Data.SqlTypes;
using System.Data.Common;
using System.Data.SqlClient;

using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;

namespace MonitorCore
{
    public class Core
    {
        
        //Function object
        //UDP
        private UDPSocket udpsocket;
        //video
        VideoFilesManager videoFilesManager;
        //database
        private DB_Core db_Base;
        private DB_DBMsgHandler db_DBMsgHandler;
        private DB_AlarmHandler db_AlarmHandler;
        private DB_APLotHandler db_APLotHandler;
        //private APMsgHandler apMsgHandler;
        private DBMsgHandler dbMsgHandler;
        private DBOrderMonitor dbOrderMonitor;
        private AlarmHandler alarmHandler;
        private APLotHandler apLotHandler;

        //HIKVision
        public HIKVISION hikvision;


        private ConfigManager configManager;

        //List for saving Message
        private List<APParser> apOrderList;//AP Order List
        private List<APParser> dbResultList;//Database Result List
        private List<APParser> dbOrderList;//Database Result List  
        private List<APParser> alarmList;
        private List<APParser> apLotList;
        //list vessel
        private List<APMsgHandler> apMsgHandlerList;
        private List<DB_APMsgHandler> DB_ApMsgHandlerList;
        //Scheduler
        private Scheduler scheduler;
        private System.Timers.Timer timeout = new System.Timers.Timer(2000);

        //data
        APEncapsulator apEncapsulator;

        private bool bRunningNormal=true;
        private bool bInit = false;
        private int count_Handler = 1;
        public Core()
        {

           
            
            //超时检测
            timeout.Elapsed += new System.Timers.ElapsedEventHandler(Job_Timer);//到达时间的时候执行事件；

            timeout.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；

            timeout.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；

            SqlExceptionHelper.Event_Restart += Restart;

            timeout.Start();

            /*VideoIncise videoIncise = new VideoIncise();
            if (videoIncise.open("F:\\3.rmvb", "F:\\2.rmvb", "F:\\test1.avi"))
            {
                videoIncise.Combine(10000, 300,20000,300);
            }*/
        }

        ~Core()
        {
            
        }

        public void Restart(Object sender, EventArgs e)
        {
            Log.write("Dealing:Restarting server");
            bRunningNormal = false;
        }

        public bool Init()
        {
            //List创建
            try
            {
                Log.write("Launching service");
                apOrderList = new List<APParser>();
                dbResultList = new List<APParser>();
                dbOrderList = new List<APParser>();
                alarmList = new List<APParser>();
                apLotList = new List<APParser>();

                apMsgHandlerList = new List<APMsgHandler>();
                DB_ApMsgHandlerList = new List<DB_APMsgHandler>();

                apEncapsulator = new APEncapsulator();
                Log.write("Initilizing Scheduler");
                scheduler = new Scheduler(apMsgHandlerList, apOrderList);

                //本地數據
                Log.write("Get Local data", 1);
                configManager = new ConfigManager();
                string server_address = configManager.dbConfig.db.address;// "192.168.0.200,2433";
                string server_name = configManager.dbConfig.users.userName; //"sa";
                string server_password = configManager.dbConfig.users.password;// "witop_402507";
                string table = configManager.dbConfig.db.table; //"FilesDB";
                int cmd_enable = Convert.ToInt32(configManager.serviceConfig.record.cmd_enable);
                count_Handler = Convert.ToInt32(configManager.serviceConfig.APthread.count);
                int bufferSize = Convert.ToInt32(configManager.serviceConfig.record.Buffer);
                Log.level=Convert.ToInt32(configManager.serviceConfig.log.level);
                string APPath = configManager.serviceConfig.AP.path;

                //Launch AP
                RunBat(APPath);


                /*DateTime time = new DateTime(2017, 04, 19, 19,30, 00);
                string resultFile = "";
                if (videoFilesManager.PathExist(time, 1))
                {
                    videoFilesManager.GetFile(time, 1, ref resultFile);
                }//get the files*/
                //对象创建

                Log.write("Initialize HikVision",1);
                hikvision = new HIKVISION();
                if(hikvision.Init())
                {
                    Log.write("HikVision Initialize succeed", 1);
                    if(hikvision.login_IP("192.168.0.92",8000,"admin","admin123"))
                    {
                        Log.write("HikVision login succeed!", 1);
                    }
                    else
                    {
                        Log.write("HikVision login failed!", 1);
                    }
                }
                else
                {
                    Log.write("HikVision Initialize failed!", 1);
                }
              

                Log.write("Initialize UDP Socket",1);
                udpsocket = new UDPSocket(apOrderList, dbResultList,alarmList, apLotList,bufferSize);

                string strLog = "Connect to database-server_address:" + server_address + "  server_name:" + server_name + "  table:" + table;
                Log.write(strLog, 2);
                //AP多线程处理
                for (int i = 0; i < count_Handler;i++)
                {
                    Log.write("Initialize DB_APMsgHandler"+i.ToString(), 1);
                    DB_APMsgHandler db_APMsgHandler = new DB_APMsgHandler(cmd_enable,i);

                    Log.write("Initialize APMsgHandler" + i.ToString(), 1);
                    APMsgHandler apMsgHandler = new APMsgHandler(udpsocket, db_APMsgHandler, apOrderList,i);

                    if (!db_APMsgHandler.Open(server_address, server_name, server_password, table))
                    {
                        Log.write("Open database "+i.ToString()+" failed!", 1);
                        bRunningNormal = false;
                    }
                    DB_ApMsgHandlerList.Add(db_APMsgHandler);
                    apMsgHandlerList.Add(apMsgHandler);
                }

                //数据库DB_Base
                Log.write("Initialize DB_Base", 1);
                db_Base = new DB_Core(cmd_enable);
                if (!db_Base.Open(server_address, server_name, server_password, table))
                {
                    Log.write("Open database failed!", 1);
                    bRunningNormal = false;
                }
                //数据库db_DBMsgHandler
                Log.write("Initialize db_DBMsgHandler", 1);
                db_DBMsgHandler = new DB_DBMsgHandler(cmd_enable);
                if (!db_DBMsgHandler.Open(server_address, server_name, server_password, table))
                {
                    Log.write("Open database failed!", 1);
                    bRunningNormal = false;
                }

                //数据库db_DBMsgHandler
                Log.write("Initialize db_AlarmHandler", 1);
                db_AlarmHandler = new DB_AlarmHandler(cmd_enable,1);
                if (!db_AlarmHandler.Open(server_address, server_name, server_password, table))
                {
                    Log.write("Open database failed!", 1);
                    bRunningNormal = false;
                }

                //数据库dbOrderMonitor
                Log.write("Initialize dbOrderMonitor", 1);
                dbOrderMonitor = new DBOrderMonitor(dbOrderList);
                if(!dbOrderMonitor.Open(server_address, server_name, server_password, table))
                {
                    Log.write("Open database failed!", 1);
                    bRunningNormal = false;
                }

                //数据库DB_APLotHandler
                Log.write("Initialize db_DB_APLotHandler", 1);
                db_APLotHandler = new DB_APLotHandler(cmd_enable,1);
                if (!db_APLotHandler.Open(server_address, server_name, server_password, table))
                {
                    Log.write("Open database failed!", 1);
                    bRunningNormal = false;
                }
                
               // Log.write("Initialize APMsgHandler", 1);
                //apMsgHandler = new APMsgHandler(udpsocket, database, apOrderList);
                //处理线程----------------

                //Video operator
                videoFilesManager = new VideoFilesManager(db_Base, configManager.serviceConfig.Path.path_video,
                         configManager.serviceConfig.Path.path_dest,
                         configManager.serviceConfig.Path.IP,
                         Convert.ToInt32(configManager.serviceConfig.Path.Port));


                Log.write("Initialize DBMsgHandler", 1);
                dbMsgHandler = new DBMsgHandler(udpsocket, db_DBMsgHandler, dbOrderList, dbResultList);
                
                Log.write("Initialize alarmHandler", 1);
                alarmHandler = new AlarmHandler(udpsocket, alarmList, db_AlarmHandler, videoFilesManager);
                Log.write("Initialize alarmHandler", 1);
                apLotHandler = new APLotHandler(udpsocket,db_APLotHandler,apLotList,1);

                

                return true;
            }
            catch (Exception ex)
            {
                Log.write("Init:ERROR-" + ex.Message,0);
                bRunningNormal = false;
                return false;
            }
            finally
            {
                int listenPort = Convert.ToInt32(configManager.apConfig.local.localPort); //6668; 
                string RemoteIP = configManager.apConfig.remote.remoteIP;// "192.168.0.27";
                int RemotePort = Convert.ToInt32(configManager.apConfig.remote.remotePort);// 5555;
                string strLog = "Open UDP Port-Listen Port:" + listenPort.ToString() + "  Remote IP:" + RemoteIP + "  Remote Port:" + RemotePort;
                Log.write(strLog, 2);
                udpsocket.Init(listenPort, RemoteIP, RemotePort);

                udpsocket.Event_ResumeAPMsgHandler += scheduler.onWorking;//事件通知关联
                udpsocket.Event_Timeout += this.Job_Timeout;//事件通知关联
                udpsocket.Event_ResumeAPLotHandler += apLotHandler.Resume;//事件通知关联
                udpsocket.Event_ResumeAlarmHandler += alarmHandler.Resume;
                dbOrderMonitor.Event_ResumeDBHandler += dbMsgHandler.Resume;//事件通知关联

                Log.write("Starting thread", 1);

                udpsocket.Start();

                for (int i = 0; i < apMsgHandlerList.Count;i++ )
                {
                    APMsgHandler apMsgHandler=apMsgHandlerList[i];
                    apMsgHandler.Start();
                }
                    //apMsgHandler.Start();
                dbMsgHandler.Start();
                dbOrderMonitor.Start();
                alarmHandler.Start();
                apLotHandler.Start();
                videoFilesManager.Start();

                if(bRunningNormal)
                {
                    UpdateAllStatus();
                    Log.write("Update manager server status", 1);
                    if (!db_Base.ManagerServer_Status(true))
                    {
                        bRunningNormal = false;
                    }
                }
                bInit = true;
            }
        }

        private void RunBat(string batPath)
        {

            Process[] proc = Process.GetProcesses();
            for (int i = 0; i < proc.Length; i++)
            {
                if (String.Compare(proc[i].ProcessName, "conhost", true) == 0)
                {
                    proc[i].Kill();
                }

                if (String.Compare(proc[i].ProcessName, "java", true) == 0)
                {
                    proc[i].Kill();
                }
            }


            Process pro = new Process();
            FileInfo file = new FileInfo(batPath);
            pro.StartInfo.WorkingDirectory = file.Directory.FullName;
            pro.StartInfo.FileName = batPath;
            pro.StartInfo.CreateNoWindow = false;
            pro.Start();
        } 
        public void UpdateAllStatus()
        {
            SqlDataReader reader = null;
            if (db_Base.GetDataGroup("Select uid from [FilesDB].[dbo].[T_PLAT_ApInfo]", ref reader))
            {
                if (reader.HasRows)
                {
                    int c1 = reader.GetOrdinal("uid");
                    reader.Read();
                    reader.Read();
                    reader.Read();
                    while (reader.Read())
                    {
                        string EmpName = reader.GetString(0);

                        if (EmpName == "apserver" || EmpName == "manager server")
                        {
                            continue;
                        }

                        if(EmpName.Length<20)
                        {
                            return;
                        }
                        udpsocket.Send(apEncapsulator.GetApStatus(Encoding.ASCII.GetBytes(EmpName)));
                    }
                    reader.Close();
                }
            }
            //Log.write("Get all AP status");
            if (db_Base.GetDataGroup("Select ai.uid,ri.uid from dbo.T_PLAT_ReaderConfig as rc left join dbo.T_PLAT_ReaderInfo as ri on ri.pk_id=rc.fk_reader left join dbo.T_PLAT_ApInfo as ai on ai.pk_id=rc.fk_ap", ref reader))
            {
                if (reader.HasRows)
                {
                    int c1 = reader.GetOrdinal("uid");
                    //int c2 = reader.GetOrdinal("uid_1");
                    while (reader.Read())
                    {
                        string EmpName1 = reader.GetString(0);
                        string EmpName2 = reader.GetString(1);
                        udpsocket.Send(apEncapsulator.Response_Terminal_StatusUpdate(Encoding.ASCII.GetBytes(EmpName1), Encoding.ASCII.GetBytes(EmpName2)));
                        break;
                    }
                    reader.Close();
                }
            }
           // Log.write("Get all Terminal status");
        }

        private byte[] HexStringToByte(string strHex)
        {
            //以,分割字符串，并去掉空字符
            byte[] b = new byte[24];
            //逐个字符变为16进制字节数据
            for (int i = 0; i < 24; i++)
            {
                string tmp = strHex.Substring(i * 2, 20);
                b[i] = Convert.ToByte(tmp, 16);
            }
            //按照指定编码将字节数组变为字符串
            return b;
        }


        public void Job_Timeout(object sender, EventArgs e)
        {
            bool IsConnected = (bool)sender;
            db_Base.APServer_Status(IsConnected);
            Log.write("APMsgHandler-Job_APServer_Timeout");
        }

        public void Close()
        {
            timeout.Enabled = false;

            if (videoFilesManager!=null)
            {
                videoFilesManager.Stop();
            }//stop video File Manager

            if(db_Base!=null)
            {
                db_Base.ManagerServer_Status(false);
            }//update manager server status
            
            if(apMsgHandlerList!=null)
            {
                for (int i = 0; i < apMsgHandlerList.Count; i++)
                {
                    APMsgHandler apMsgHandler = apMsgHandlerList[i];
                    if (apMsgHandler != null)
                    {
                        apMsgHandler.Stop();
                    }
                }

                apMsgHandlerList.Clear();
            }//Stop Threads apMsgHandler
            

            if (dbMsgHandler != null)
            {
                dbMsgHandler.Stop();
            }//Stop Thread dbMsgHandler

            if (dbOrderMonitor != null)
            {
                dbOrderMonitor.Stop();
                dbOrderMonitor.Close();
            }//Stop Thread dbOrderMonitor

            if(alarmHandler!=null)
            {
                alarmHandler.Stop();
            }//Stop Thread alarmHandler

            if(apLotHandler!=null)
            {
                apLotHandler.Stop();
            }//stop Thread apLotHandler

            if (udpsocket != null)
            {
                udpsocket.Close();
            }//stop Thread udpSocket

            if (db_Base != null)
            {
                db_Base.Close();
            }//stop databse 

            if(db_DBMsgHandler!=null)
            {
                db_DBMsgHandler.Close();
            }//stop databse DBMsgHandler

            if(db_AlarmHandler!=null)
            {
                db_AlarmHandler.Close();
            }//stop database AlarmHandler

            if(DB_ApMsgHandlerList!=null)
            {
                for (int i = 0; i < DB_ApMsgHandlerList.Count; i++)
                {
                    DB_APMsgHandler db_apMsgHandler = DB_ApMsgHandlerList[i];
                    if (db_apMsgHandler != null)
                    {
                        db_apMsgHandler.Close();
                    }
                }

                DB_ApMsgHandlerList.Clear();
            }//stop databse ApMsgHandler

            if(db_APLotHandler!=null)
            {
                db_APLotHandler.Close();
            }


            if(hikvision!=null)
            {
                hikvision.Close();
            }

            bRunningNormal = false;
        }


        private void Job_Timer(object source, System.Timers.ElapsedEventArgs e)
        {
            if (!bInit)
            {
                bInit = true;
                Init();
                return;
            }
            if (db_Base != null)
            {
                if (bRunningNormal && db_Base.Status() == ConnectionState.Open)
                {
                    return;
                }
            }

            if(configManager==null || dbOrderMonitor==null || db_Base==null)
            {
                return;
            }

            bool bRestart = true;

            string server_address = configManager.dbConfig.db.address;// "192.168.0.200,2433";
            string server_name = configManager.dbConfig.users.userName; //"sa";
            string server_password = configManager.dbConfig.users.password;// "witop_402507";
            string table = configManager.dbConfig.db.table; //"FilesDB";
            if (dbOrderMonitor != null)
            {
                dbOrderMonitor.Close();
                if (!dbOrderMonitor.Open(server_address, server_name, server_password, table))
                {
                    Log.write("Open databse failed!", 1);
                    bRunningNormal = false;
                    bRestart = false;
                    return;
                }
                
            }
            
            if (db_Base != null)
            {
                db_Base.Close();
                if (!db_Base.Open(server_address, server_name, server_password, table))
                {
                    Log.write("Open database failed!", 1);
                    bRunningNormal = false;
                    bRestart = false;
                    return;
                }
            }

            if (db_DBMsgHandler != null)
            {
                db_DBMsgHandler.Close();
                if (!db_DBMsgHandler.Open(server_address, server_name, server_password, table))
                {
                    Log.write("Open database failed!", 1);
                    bRunningNormal = false;
                    bRestart = false;
                    return;
                }
            }

            if (db_AlarmHandler != null)
            {
                db_AlarmHandler.Close();
                if (!db_AlarmHandler.Open(server_address, server_name, server_password, table))
                {
                    Log.write("Open database failed!", 1);
                    bRunningNormal = false;
                    bRestart = false;
                    return;
                }
            }

            if (db_APLotHandler != null)
            {
                db_APLotHandler.Close();
                if (!db_APLotHandler.Open(server_address, server_name, server_password, table))
                {
                    Log.write("Open database failed!", 1);
                    bRunningNormal = false;
                    bRestart = false;
                    return;
                }
            }

            for (int i = 0; i < DB_ApMsgHandlerList.Count; i++)
            {
                DB_APMsgHandler db_apMsgHandler = DB_ApMsgHandlerList[i];
                if (db_apMsgHandler != null)
                {
                    db_apMsgHandler.Close();
                    if (!db_apMsgHandler.Open(server_address, server_name, server_password, table))
                    {
                        Log.write("Open database failed!", 1);
                        bRunningNormal = false;
                        bRestart = false;
                        return;
                    }
                }
            }

            if(bRestart)
            {
                bRunningNormal = true;
                UpdateAllStatus();
                Log.write("Update manager server status", 1);
                if (!db_Base.ManagerServer_Status(true))
                {
                    bRunningNormal = false;
                }
            }
        }

        public string GetLocalIP()
        {
            List<string> strIPs = new List<string>();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            string strLocalAddr="";
            foreach (NetworkInterface adapter in nics)
            {

                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    var mac = adapter.GetPhysicalAddress(); Console.WriteLine(mac);
                    IPInterfaceProperties ip = adapter.GetIPProperties();
                    UnicastIPAddressInformationCollection ipCollection = ip.UnicastAddresses;
                    foreach (UnicastIPAddressInformation ipadd in ipCollection)
                    {
                        //InterNetwork    IPV4地址      
                        //InterNetworkV6        IPV6地址 
                        if (ipadd.Address.AddressFamily == AddressFamily.InterNetwork)
                        {  
                            //判断是否为ipv4
                            strIPs.Add(ipadd.Address.ToString());
                            strLocalAddr=ipadd.Address.ToString();
                        }
                    }
                }
            }
            return strLocalAddr;
        }
    }
}

