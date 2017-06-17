using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MonitorCore
{
    class UDPSocket
    {
        //初始化成功
        private bool bInit = false;
        //UDP Thread
        private static Thread udpThread = null;
        private bool bStop = false;

        // 定义节点
        private IPEndPoint ipEndPoint = null;
        private EndPoint remoteEP = null;

        //Socket
        private Socket server = null;
        // buffer大小限制
        private int BufferSize = 1000;
        //string message;

        private List<APParser> apOrderList;//AP Order List
        private List<APParser> dbResultList;//Databse Result List
        private List<APParser> alarmList;//Alarm List
        private List<APParser> apLotList;//AP Lot List

        //encapsulator
        private APEncapsulator apEncapsulator;

        byte[] buffer = new byte[1024];
        public event EventHandler Event_ResumeAPMsgHandler;
        public event EventHandler Event_ResumeAlarmHandler;
        public event EventHandler Event_ResumeAPLotHandler;
        public event EventHandler Event_Timeout;
        private object Locker_Sender=new object();
        //超时监控
        private System.Timers.Timer timeout = new System.Timers.Timer(1000);//Timer
        private int timeout_acc = 0;//accumulator
        private int iTimeout = 2000;//timeout time
        private bool timeout_isSent = false;
        private bool timeout_isResponse = false;
        private bool timeout_isConnected = true;
        private bool isRefreshStatus = false;
        public UDPSocket(List<APParser> apOrderList, List<APParser> dbResultList,List<APParser> alarmList,List<APParser> apLotList, int BufferSize)
        {
            this.apOrderList = apOrderList;
            this.dbResultList = dbResultList;
            this.alarmList = alarmList;
            this.apLotList = apLotList;
            this.BufferSize = BufferSize;

            apEncapsulator = new APEncapsulator();
        }

        public void Init(int listenPort, string remoteIP, int remotePort)
        {
            try
            {
                // 本机节点
                if (ipEndPoint == null)
                {
                    ipEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
                }

                // 远程节点
                if (remoteEP == null)
                {
                    remoteEP = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
                }

                // 实例化
                server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                server.Bind(ipEndPoint);//绑定端口号和IP
                server.ReceiveTimeout = 500;
                server.SendTimeout = 500;
                //线程创建
                if (udpThread == null)
                {
                    bStop = false;
                    udpThread = new Thread(new ThreadStart(ReceiveFunc));
                }

                //超时检测
                timeout.Elapsed += new System.Timers.ElapsedEventHandler(Job_Timer);//到达时间的时候执行事件；

                timeout.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；

                timeout.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；

                timeout.Start();
                bInit = true;
            }
            catch (Exception ex)
            {
                Log.write("UDPSocket---Init:ERROR-" + ex.Message,0);
                bInit = false;
                return;
            }
        }

        public void Close()
        {
            if(server!=null)
            {
                server.Close();
            }
            
            timeout.Stop();
            bStop = true;
            
            /*int threadTimeout = 0;
            while (udpThread.ThreadState ==ThreadState.Running)
            {
                Thread.Sleep(100);
                threadTimeout += 100;
                if (threadTimeout > 1000)
                {
                    udpThread.Abort();
                    Thread.Sleep(100);
                }
            }*/
        }

        public void Start()
        {
            if (!bInit)
            {
                return;
            }
            bStop = false;
            udpThread.Start();
        }

       
        private void ReceiveFunc()
        {
            while (!bStop)
            {
                lock (this)
                {
                    ReceiveMsg();
                    //Thread.Sleep(10);
                }
            }
            Log.write("udpSocket-ThreadEnd");
        }

        private void ReceiveMsg()
        {
            // 调用接收回调函数
            try
            {
                int length = server.ReceiveFrom(buffer, ref remoteEP);//接收数据报
                //message += Encoding.ASCII.GetString(buffer, 0, length);
                //MsgAlloc();
                
                Log.write("udpSocket-Receive size:" + length.ToString(),2);
                if(buffer[0]!=36 || buffer[1]!=36)
                {
                    return;
                }/*$$*/

                int frame_size = buffer[2] * 0x100 + buffer[3];
                if(buffer[2+frame_size]!=37 || buffer[3+frame_size]!=37)
                {
                    return;
                }/*%%*/

                int all_Size = frame_size + 4;

                int valid_sum_check=buffer[frame_size]*0x100+buffer[frame_size+1];
                int valid_sum_recheck=0;
                for(int i=0;i<frame_size;i++)
                {
                    valid_sum_recheck+=buffer[i];
                }

                if (valid_sum_recheck != valid_sum_check)
                {
                    Log.write("udpSocket-Receive:valid sum check error");
                    return;
                }

                byte[] byte_data = new byte[frame_size+4];
                Array.Copy(buffer, byte_data, frame_size + 4);

                //状态更新
                if (!isRefreshStatus)
                {
                    isRefreshStatus = true;
                    Event_Timeout(true, null);
                }


                if(timeout_isSent)
                {
                    if (APParser.IsTimeout(byte_data, all_Size))
                    {
                        timeout_isResponse = true;
                        Log.write("UDPSocket-Receive Heartbeat");
                        return;
                    }
                }//心跳检测

                if (!APParser.IsAvaible(byte_data, all_Size))
                {
                    return;
                }//验证数据是否为有效数据

                APParser apParser = new APParser(byte_data, all_Size);

                switch(apParser.MsgKind())
                {
                    case 1://Ap order
                        {
                            lock (apOrderList)
                            {
                                if (apOrderList.Count > BufferSize)
                                {
                                    Log.write("udpSocket-buffer full!", 1);
                                    apParser.Dispose();
                                    return;
                                }
                            }//检测是否满了

                            lock (apOrderList)
                            {
                                apOrderList.Add(apParser);
                            }//为AP上送数据
                            Event_ResumeAPMsgHandler(null, null); 
                        }
                        break;
                    case 2:// db result
                        {
                            lock (dbResultList)
                            {
                                dbResultList.Add(apParser);
                            }//为数据库操作返回结果
                        }
                        break;
                    case 3://alarm 
                        {
                            lock (alarmList)
                            {
                                alarmList.Add(apParser);
                            }//为报警
                            Event_ResumeAlarmHandler(null, null);
                        }
                        break;
                    case 4://ap lot
                        {
                            lock (apLotList)
                            {
                                apLotList.Add(apParser);
                            }//Ap Lot List
                            Event_ResumeAPLotHandler(null, null);
                        }
                        break;
                    default:
                        break;
                }              

                Log.write("udpSocket-Receive Availble Msg:" + apParser.GetParam_Origin_Hex(),2);
                
                byte_data=null;

                //超时复位
                timeout_acc = 0;
            }
            catch (Exception ex)
            {
                //Log.write("UDPSocket---ReceiveMsg:ERROR-" + ex.Message);
                return;
            }
        }

        // 发送函数

        public void Send(byte[] data)
        {
            try
            {
                lock (Locker_Sender)
                {
                    server.SendTo(data, remoteEP);
                    /*for (int i = 0; i < data.Length-3;i++)
                    {
                        if(data[i]==0x05 && data[i+1]==0x01 && data[i+2]==0x01 && data[i+3]==0x00)
                        {
                            Log.write("UDPSocket---SendMsg:AP驗證");
                        }
                    }*/
                }
            }
            catch(Exception ex)
            {
                Log.write("UDPSocket---SendMsg:ERROR-" + ex.Message,0);
            }
            
        }

        public void Send(char[] msg, int size)
        {
            try
            {
                lock (this)
                {
                    byte[] data = Encoding.ASCII.GetBytes(msg, 0, size);
                    int i = server.SendTo(data, size, SocketFlags.None, remoteEP);
                }
            }
            catch (Exception ex)
            {
                Log.write("UDPSocket---SendMsg:ERROR-" + ex.Message,0);
            }
            
        }

        private void Job_Timer(object source, System.Timers.ElapsedEventArgs e)
        {
            timeout_acc += 1000;
            if (timeout_acc <= iTimeout)
            {
                return;
            }

            if (!timeout_isSent)
            {
                Send(apEncapsulator.HeartBeat());
                Log.write("UDPSocket---Send Heartbeat");
                timeout_isSent = true;
                timeout_isResponse = false;
                Thread.Sleep(100);
                return;
            }//send heartbeat

            if (timeout_isResponse)
            {
                if (!timeout_isConnected)
                {
                    timeout_isConnected = true;
                    Log.write("UDPSocket---Reconnect to AP Server",0);
                    Event_Timeout(timeout_isConnected, null);
                }//Reconnected!
            }
            else
            {
                if (timeout_isConnected)
                {
                    timeout_isConnected = false;
                    Log.write("UDPSocket---Disconnected to AP Server",0);
                    Event_Timeout(timeout_isConnected, null);
                }//Disconnected!
            }

            timeout_acc = 0;
            timeout_isSent = false;
            timeout_isResponse = false;
        }

        
    }
}
