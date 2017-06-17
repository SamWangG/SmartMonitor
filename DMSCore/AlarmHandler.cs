using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MonitorCore
{
    class AlarmHandler
    {
        private List<APParser> alarmList;
        private DB_AlarmHandler database;
        private UDPSocket udpSocket;
        private static Thread AlarmHandlerThread = null;
        VideoFilesManager videoFilesManager;
        private bool bStop = false;
        ManualResetEvent resumeEvent = new ManualResetEvent(false);

        //list for record
        private char[] char_ap_uid = new char[24];
        private byte[] byte_ap_uid = new byte[24];

        private char[] char_terminal_uid = new char[24];
        private byte[] byte_terminal_uid = new byte[24];
        //终端数据
        private char[] char_terminal_data = new char[64];
        private byte[] byte_terminal_data = new byte[64];
        
        private APEncapsulator apEncapsulator;

        private int iCounter_invalid = 0;//invalid card data
        private DateTime time_receive;//time receiving the guard data
        private DateTime time_last;//the last receiving time
        private int[] index=new int[3]{-1,-1,-1};
        private string[] resultFile = new string[3];
        private int iPk_id=0;
        //门禁数据
        private char[] char_guard_uid = new char[24];
        private byte[] byte_guard_uid = new byte[24];
        private byte[] byte_guard_data = new byte[800];

        public AlarmHandler(UDPSocket udpSocket, List<APParser> alarmList, DB_AlarmHandler database, VideoFilesManager videoFilesManager)
        {
            this.udpSocket = udpSocket;
            this.alarmList = alarmList;
            this.database = database;
            this.videoFilesManager = videoFilesManager;
            time_last = new DateTime(1, 1, 1, 0, 0, 0);
            apEncapsulator = new APEncapsulator();
            
            AlarmHandlerThread = new Thread(new ThreadStart(AlarmHandleFunc));
        }

        public void Start()
        {
            bStop = false;
            AlarmHandlerThread.Start();
        }

        public void Stop()
        {
            bStop = true;
            resumeEvent.Set();
        }

        public void Resume(object sender,EventArgs e)
        {
            Log.write("AlarmHandle-Resume");
            resumeEvent.Set();
        }

        private void AlarmHandleFunc()
        {
            while(!bStop)
            {
                AlarmHandleJob();
                Thread.Sleep(10);
            }
            Log.write("AlarmHandle-ThreadEnd");
        }

        private void AlarmHandleJob()
        {
            try
            {
                if (alarmList.Count == 0)
                {
                    Log.write("AlarmHandle-WaitOne");
                    resumeEvent.Reset();
                    resumeEvent.WaitOne();
                }

                APParser apParser;
                EnumOrder order;
                lock (alarmList)
                {
                    if (alarmList.Count != 0)
                    {
                        apParser = alarmList[0];
                        order = apParser.GetOrder();
                        alarmList.RemoveAt(0);
                    }//处理发送数据
                    else
                    {
                        return;
                    }
                }//锁存alarmList


                switch (order)
                {
                    case EnumOrder.ENRANCE_GUARD_UP://门卫
                        {
                            Log.write("AlarmHandler " + " --Job_Entrance_Guard_Validate:" + apParser.GetParam_Origin_Hex(), 2);
                            time_receive = DateTime.Now;
                            Job_Entrance_Guard_Validate(apParser);
                            time_last = time_receive;
                            //sleep 1s to void the same name;
                            Thread.Sleep(1000);
                        }
                        break;
                    case EnumOrder.READER_DATA_UP:
                        {
                            Log.write("AlarmHandler " + " --TerminalHandleJob:" + apParser.GetParam_Origin_Hex(), 2);
                            Job_Terminal_data_upload(apParser);
                        }
                        break;
                    default:
                        break;
                }

                apParser.Dispose();//自销毁
            }
            catch (Exception ex)
            {
                Log.write("AlarmHandle-ThreadError:" + ex.Message);
            }
        }


        //工作-处理终端数据
        private void Job_Terminal_data_upload(APParser apPaser)
        {
            int size = 0;
            if (!apPaser.GetParam_Terminal_data_upload(ref byte_ap_uid, ref byte_terminal_uid, ref byte_terminal_data, ref size))
            {
                return;
            }

            TerminalHandleJob(size);
            udpSocket.Send(apEncapsulator.Response_Terminal_DataUpload(byte_ap_uid));
            string tmp1 = System.Text.Encoding.Default.GetString(byte_ap_uid);
            database.CmdLog("05020600", tmp1, apPaser.GetParam_Origin_Hex());
            tmp1 = null;
        }

        private void TerminalHandleJob(int size)
        {
            int cmd = byte_terminal_data[14];
            char_terminal_data = Encoding.ASCII.GetChars(byte_terminal_data);
            switch (cmd)
            {
                case 0x8B:
                    Process_Terminal_Alarm();
                    break;
                default:
                    break;
            }
        }

        //handle guard validate
        private void Job_Entrance_Guard_Validate(APParser apPaser)
        {
            
            int num = 0;
            apPaser.GetParam_Entrance_Guard(ref byte_guard_uid, ref byte_guard_data, ref num);

            string tmp1 = System.Text.Encoding.Default.GetString(byte_guard_uid);
            string tmp2 = ByteToHexString(byte_guard_data, 0, byte_guard_data.Length, 8, "");

           /* TimeSpan span = time_last.Subtract( time_receive);
            if(span.Minutes > 2)
            {
                
            }
            else
            {

            }*/


            iCounter_invalid = database.Guard_Validate(tmp1, tmp2, num, ref index[0], ref index[1], ref index[2], ref iPk_id);
            if (iCounter_invalid == 0)
            {
                udpSocket.Send(apEncapsulator.Response_Guard_Validate(byte_guard_uid, true));
            }
            else
            {
                udpSocket.Send(apEncapsulator.Response_Guard_Validate(byte_guard_uid, false));
                //stop cut threading 
                //VideoHandler handler = new VideoHandler(iPk_id, time_receive, database, videoFilesManager, index[0], index[1], index[2]);
                //Process_Alarm();
            }

            database.CmdLog("05010700", tmp1, apPaser.GetParam_Origin_Hex());
            tmp1 = null;
            tmp2 = null;
        }   

        private void Process_Terminal_Alarm()
        {
            byte type_warning = byte_terminal_data[16];
            byte no_index = byte_terminal_data[17];
            byte flag_warning = byte_terminal_data[18];
            byte[] warning = new byte[20];
            Array.Copy(byte_terminal_data, warning, 20);
            string strTerminalUID = ByteToHexString(byte_terminal_uid, 0, 12);

            if (database.Terminal_Coil_Alarm(strTerminalUID, flag_warning, no_index))
            {
                udpSocket.Send(apEncapsulator.Response_Terminal_Coil_Alarm(byte_ap_uid, byte_terminal_uid,
                    type_warning, no_index, flag_warning));
            }
        }


        #region deprecated
        private void Process_Alarm()
        {
            int acc = 0;

            for (int i = 0; i < 3;i++ )
            {
                if (index[i]==-1)
                {
                    continue;
                }

                if (videoFilesManager.PathExist(time_receive, index[i]))
                {
                    videoFilesManager.GetFile(time_receive, index[i], ref resultFile[i]);
                    acc++;
                }//get the files
            }// max:2

           
            if(acc>0)
            {
                string filename=System.IO.Path.GetFileName(resultFile[0]);
                string path = resultFile[0].Substring(0, resultFile[0].Length - filename.Length);

                

                for (int i = 0; i < resultFile.Length;i++ )
                {
                    if (resultFile[i] == null)
                    {
                        resultFile[i] = "";
                    }
                }

                if (database.SetVideoFile(iPk_id, path, resultFile[0], resultFile[1], resultFile[2], ""))
                {

                }
            }
        }
        #endregion
        private string ByteToHexString(byte[] b, int index = 0, int length = 0, int divid = 1, string seperator = "")
        {
            //byte[] b = encode.GetBytes(s);//按照指定编码将string编程字节数组
            string result = string.Empty;
            if (length == 0)
            {
                length = b.Length;
            }
            if (index + length > b.Length)
            {
                length = b.Length - index;
            }
            for (int i = index; i < index + length; i++)//逐字节变为16进制字符，以%隔开
            {
                if (i == index)
                {
                    string tmp = Convert.ToString(b[i], 16);
                    if (tmp.Length == 1)
                    {
                        tmp = "0" + tmp;
                    }
                    result += tmp;
                }
                else if (i % divid == 0)
                {
                    string tmp = Convert.ToString(b[i], 16);
                    if (tmp.Length == 1)
                    {
                        tmp = "0" + tmp;
                    }
                    result += seperator + tmp;
                }
                else
                {
                    string tmp = Convert.ToString(b[i], 16);
                    if (tmp.Length == 1)
                    {
                        tmp = "0" + tmp;
                    }
                    result += tmp;
                }
            }
            return result;
        }
    }
}

