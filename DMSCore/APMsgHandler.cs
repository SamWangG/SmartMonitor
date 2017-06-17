using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MonitorCore
{
    class APMsgHandler
    {
        private UDPSocket udpSocket;
        private DB_APMsgHandler database;
        private List<APParser> apOrderList;

        private Thread apHandlerThread = null;
        ManualResetEvent resumeEvent = new ManualResetEvent(false);
        private bool bStop = false;

        private char[] char_ap_uid = new char[24];
        private byte[] byte_ap_uid = new byte[24];
        private char[] char_terminal_uid = new char[24];
        private byte[] byte_terminal_uid = new byte[24];
        //终端数据
        private char[] char_terminal_data = new char[64];
        private byte[] byte_terminal_data = new byte[64];

        //门禁数据
        private char[] char_guard_uid = new char[24];
        private byte[] byte_guard_uid = new byte[24];
        private byte[] byte_guard_data = new byte[800];

        private APEncapsulator apEncapsulator;

        private bool isWaiting;//check if thread is running
        private int identity;//ID
        /*
        public void Job_Timeout(object sender, EventArgs e)
        {
            bool IsConnected = (bool)sender;
            database.APServer_Timeout(IsConnected);
            Log.write("APMsgHandler-Job_APServer_Timeout");
        }
        */

        public APMsgHandler(UDPSocket udpSocket, DB_APMsgHandler database, List<APParser> apOrderList,int identity)
        {
            this.udpSocket = udpSocket;
            this.database = database;
            this.apOrderList = apOrderList;
            this.identity = identity;
            isWaiting = true;

            apEncapsulator = new APEncapsulator();

            apHandlerThread = new Thread(new ThreadStart(APHandleFunc));
        }

        ~APMsgHandler()
        {
        }

        public void Start()
        {
            bStop = false;
            apHandlerThread.Start();
        }

        public void Stop()
        {
            bStop = true;
            //int threadTimeout = 0;
            resumeEvent.Set();


        }

        public void Resume(Object sender, EventArgs e)
        {
            if(!isWaiting)
            {
                return;
            }
            Log.write("APMsgHandler "+identity.ToString()+" -Resume");
            isWaiting = false;
            resumeEvent.Set();
        }

        private void APHandleFunc()
        {
            while (!bStop)
            {
                APHandleJob();
                //Thread.Sleep(10000);
            }
            Log.write("APMsgHandler " + identity.ToString() + " -ThreadEnd");
        }
        //AP工作分配
        private void APHandleJob()
        {
            APParser apParser;
            EnumOrder order;
            int count = 0;
            lock (apOrderList)
            {
                count=apOrderList.Count;
            }

            if (count == 0)
            {
                Log.write("APMsgHandler-WaitOne");
                isWaiting = true;
                resumeEvent.Reset();
                resumeEvent.WaitOne();
            }

            lock (apOrderList)
            {
                count = apOrderList.Count;
                if (count != 0)
                {
                    apParser = apOrderList[0];
                    order = apParser.GetOrder();
                }
                else
                {
                    return;
                }

                apOrderList.RemoveAt(0);
            }
                
                Log.write("APMsgHandler remain count:" + apOrderList.Count.ToString());
            //reset timeout
            switch (order)
            {
                case EnumOrder.AP_VALIDATION_UP://AP validation
                    {
                        Log.write("APMsgHandler " + identity.ToString() + " --Job_AP_Validate:" + apParser.GetParam_Origin_Hex(), 2);
                        Job_AP_Validate(apParser);
                    }
                    break;
                case EnumOrder.AP_STATUS_UP://Update AP status
                    {
                        Log.write("APMsgHandler " + identity.ToString() + " --Job_AP_StatusUpdate:" + apParser.GetParam_Origin_Hex(), 2);
                        Job_AP_StatusUpdate(apParser);
                        
                    }
                    break;
                case EnumOrder.READER_STATUS_UP://Update Terminal status
                    {
                        Log.write("APMsgHandler " + identity.ToString() + " --Job_Terminal_StatusUpdate:" + apParser.GetParam_Origin_Hex(), 2);
                        Job_Terminal_StatusUpdate(apParser);
                        
                    }
                    break;
                case EnumOrder.READER_DATA_UP://reader status upload
                    {
                        Log.write("APMsgHandler " + identity.ToString() + " --Job_Terminal_data_upload:" + apParser.GetParam_Origin_Hex(), 2);
                        Job_Terminal_data_upload(apParser);
                    }
                    break;
                case EnumOrder.ENRANCE_GUARD_UP:
                    {
                        //this section implement at class AlarmHandler
                        //Log.write("APMsgHandler " + identity.ToString() + " --Job_Entrance_Guard_Validate:" + apParser.GetParam_Origin_Hex(), 2);
                        //Job_Entrance_Guard_Validate(apParser);
                    }                   
                    break;
                case EnumOrder.HEARTBEAT_UP:

                    break;
                default:
                    break;
            }

            apParser.Dispose();//自销毁
            
        }
        //工作-处理AP校验
        private void Job_AP_Validate(APParser apPaser)
        {
            int Count = 0;
            apPaser.GetParam_Validate_Order(ref byte_ap_uid, ref Count);
            string tmp = System.Text.Encoding.Default.GetString(byte_ap_uid);
            if (database.AP_Validate(tmp, Count.ToString()))
            {
                udpSocket.Send(apEncapsulator.Response_AP_Validate(byte_ap_uid, 0x31));
            }
            else
            {
                udpSocket.Send(apEncapsulator.Response_AP_Validate(byte_ap_uid, 0x32));
            }

            database.CmdLog("05010100", tmp, apPaser.GetParam_Origin_Hex());

            tmp = null;
        }

        //工作-处理AP状态更新
        private void Job_AP_StatusUpdate(APParser apPaser)
        {
            int result = 0;
            apPaser.GetParam_AP_Status(ref byte_ap_uid, ref result);

            string tmp = System.Text.Encoding.Default.GetString(byte_ap_uid);

            if (result == 2)
            {
                result = 0;
            }
            else
            {
                result = 1;
            }
            if (database.AP_Status_Update(tmp, result))
            {

            }
            else
            {

            }
            //udpSocket.Send(apEncapsulator.Response_AP_StatusUpdate(char_ap_uid), 37);

            database.CmdLog("05010300", tmp, apPaser.GetParam_Origin_Hex());

            tmp = null;
        }


        //工作-处理终端状态更新
        private void Job_Terminal_StatusUpdate(APParser apPaser)
        {
            int result = 0;
            apPaser.GetParam_Terminal_Status(ref byte_ap_uid, ref byte_terminal_uid, ref result);

            string tmp1 = System.Text.Encoding.Default.GetString(byte_ap_uid);
            string tmp2 = System.Text.Encoding.Default.GetString(byte_terminal_uid); 
            if (result == 2)
            {
                result = 0;
            }
            else
            {
                result = 1;
            }

            if (database.Terminal_Status_Update(tmp1, tmp2, result))
            {

            }
            else
            {

            }

            //udpSocket.Send(apEncapsulator.Response_Terminal_StatusUpdate(byte_ap_uid, byte_terminal_uid));
            
            database.CmdLog("05010500", tmp1, apPaser.GetParam_Origin_Hex());

            tmp1 = null;
            tmp2 = null;
        }

        private void Job_Entrance_Guard_Validate(APParser apPaser)
        {
            int num = 0;
            apPaser.GetParam_Entrance_Guard(ref byte_guard_uid, ref byte_guard_data, ref num);

            string tmp1 = System.Text.Encoding.Default.GetString(byte_guard_uid);
            string tmp2 = ByteToHexString(byte_guard_data,0,byte_guard_data.Length,8,"");

            if (database.Guard_Validate(tmp1, tmp2, num))
            {
                udpSocket.Send(apEncapsulator.Response_Guard_Validate(byte_guard_uid, true));
            }
            else
            {
                udpSocket.Send(apEncapsulator.Response_Guard_Validate(byte_guard_uid, false));
            }
            database.CmdLog("05010700", tmp1, apPaser.GetParam_Origin_Hex());
            tmp1 = null;
            tmp2 = null;
        }

        //工作-处理终端数据
        private void Job_Terminal_data_upload(APParser apPaser)
        {
            int size = 0;
            if (!apPaser.GetParam_Terminal_data_upload(ref byte_ap_uid, ref byte_terminal_uid,ref byte_terminal_data, ref size))
            {    
                return;
            }

            TerminalHandleJob(size);
            udpSocket.Send(apEncapsulator.Response_Terminal_DataUpload(byte_ap_uid));
            string tmp1 = System.Text.Encoding.Default.GetString(byte_ap_uid);
            database.CmdLog("05020600", tmp1, apPaser.GetParam_Origin_Hex());
            tmp1 = null;
        }


        //终端数据分配
        private void TerminalHandleJob(int size)
        {
            int cmd = byte_terminal_data[14];
            char_terminal_data = Encoding.ASCII.GetChars(byte_terminal_data);
            switch (cmd)
            {
                case 0x81:
                    Process_Terminal_Label();
                    break;
                case 0x82:
                    Process_Terminal_Enter();
                    break;
                case 0x83:
                    Process_Terminal_Exit();
                    break;
                case 0x88:
                    Process_Terminal_Inventory();
                    break;
                case 0x8B:
                    Process_Terminal_Alarm();
                    break;
                default:
                    break;
            }
        }

        //工作流程-Process上传标签数据
        private void Process_Terminal_Label()
        {
            
            int length = byte_terminal_data[15];
            byte tmp1 = byte_terminal_data[16];
            int label_num = ((tmp1>>7) & 0x01) * 4 + ((tmp1>>6) & 0x01) * 2 + ((tmp1>>5) & 0x01);//前三位是label_num，后5位是差異數據
            Log.write(label_num.ToString());
            //差異數據
            int dif_label1 = ((tmp1 >> 4) & 0x01);
            int dif_label2 = ((tmp1 >> 3) & 0x01);
            int dif_label3 = ((tmp1 >> 2) & 0x01);
            int dif_label4 = ((tmp1 >> 1) & 0x01);
            int dif_label5 = ((tmp1 >> 0) & 0x01);
            //標籤卡的線圈索引
            int[] coilIndex = new int[5];
            coilIndex[0] = byte_terminal_data[17];
            coilIndex[1] = byte_terminal_data[18];
            coilIndex[2] = byte_terminal_data[19];
            coilIndex[3] = byte_terminal_data[20];
            coilIndex[4] = byte_terminal_data[21];
            //標籤卡數據位置
            int pos_label1 = 22;
            int pos_label2 = 30;
            int pos_label3 = 38;
            int pos_label4 = 46;
            int pos_label5 = 54;
            if (label_num == 0 || label_num > 5)
            {
                return;
            }

            string strLabel1 = "null";
            string strLabel2 = "null";
            string strLabel3 = "null";
            string strLabel4 = "null";
            string strLabel5 = "null";
            string tmp = ByteToHexString(byte_terminal_uid, 0, 12); 
            tmp = tmp.Trim();
            switch (label_num)
            {
                case 1:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, pos_label1, 8);
                        
                    }
                    break;
                case 2:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, pos_label1, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, pos_label2, 8);
                    }

                    break;
                case 3:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, pos_label1, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, pos_label2, 8);
                        strLabel3 = ByteToHexString(byte_terminal_data, pos_label3, 8);
                    }
                    break;
                case 4:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, pos_label1, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, pos_label2, 8);
                        strLabel3 = ByteToHexString(byte_terminal_data, pos_label3, 8);
                        strLabel4 = ByteToHexString(byte_terminal_data, pos_label4, 8);
                    }
                    break;
                case 5:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, pos_label1, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, pos_label2, 8);
                        strLabel3 = ByteToHexString(byte_terminal_data, pos_label3, 8);
                        strLabel4 = ByteToHexString(byte_terminal_data, pos_label4, 8);
                        strLabel5 = ByteToHexString(byte_terminal_data, pos_label5, 8);
                    }
                    break;
                default:
                    break;
            }

            GUIEvent.SendMsg(tmp, strLabel1, strLabel2, strLabel3, strLabel4, strLabel5);

            if (database.Terminal_Labels_Pos_Update(tmp, strLabel1, strLabel2, strLabel3, strLabel4, strLabel5, coilIndex) == 5)
            {

            }
            else
            {

            }
            strLabel1 = null;
            strLabel2 = null;
            strLabel3 = null;
            strLabel4 = null;
            strLabel5 = null;
        }

        //工作流程-Process登入数据
        private void Process_Terminal_Enter()
        {
            int length = byte_terminal_data[15];
            int label_num = byte_terminal_data[16];

            if (label_num == 0 || label_num > 5)
            {
                return;
            }

            string strLabel1 = null;
            string strLabel2 = null;
            string strLabel3 = null;
            string strLabel4 = null;
            string strLabel5 = null;

            string tmp = System.Text.Encoding.Default.GetString(byte_terminal_uid); 
            switch (label_num)
            {
                case 1:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                    }
                    break;
                case 2:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, 25, 8);
                    }
                    break;
                case 3:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, 25, 8);
                        strLabel3 = ByteToHexString(byte_terminal_data, 33, 8);
                    }
                    break;
                case 4:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, 25, 8);
                        strLabel3 = ByteToHexString(byte_terminal_data, 33, 8);
                        strLabel4 = ByteToHexString(byte_terminal_data, 41, 8);
                    }
                    break;
                case 5:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, 25, 8);
                        strLabel3 = ByteToHexString(byte_terminal_data, 33, 8);
                        strLabel4 = ByteToHexString(byte_terminal_data, 41, 8);
                        strLabel5 = ByteToHexString(byte_terminal_data, 49, 8);
                    }
                    break;
                default:
                    break;
            }

            if (database.Terminal_Labels_Pos_Update(tmp, strLabel1, strLabel2, strLabel3, strLabel4, strLabel5) == 5)
            {

            }
            else
            {

            }
        }

        //工作流程-Process登出数据
        private void Process_Terminal_Exit()
        {
            int length = byte_terminal_data[15];
            int label_num = byte_terminal_data[16];

            if (label_num == 0 || label_num > 5)
            {
                return;
            }

            string strLabel1 = null;
            string strLabel2 = null;
            string strLabel3 = null;
            string strLabel4 = null;
            string strLabel5 = null;

            string tmp = System.Text.Encoding.Default.GetString(byte_terminal_uid); 
            switch (label_num)
            {
                case 1:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                    }
                    break;
                case 2:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, 25, 8);
                    }
                    break;
                case 3:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, 25, 8);
                        strLabel3 = ByteToHexString(byte_terminal_data, 33, 8);
                    }
                    break;
                case 4:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, 25, 8);
                        strLabel3 = ByteToHexString(byte_terminal_data, 33, 8);
                        strLabel4 = ByteToHexString(byte_terminal_data, 41, 8);
                    }
                    break;
                case 5:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, 25, 8);
                        strLabel3 = ByteToHexString(byte_terminal_data, 33, 8);
                        strLabel4 = ByteToHexString(byte_terminal_data, 41, 8);
                        strLabel5 = ByteToHexString(byte_terminal_data, 49, 8);
                    }
                    break;
                default:
                    break;
            }

            if (database.Terminal_Labels_Pos_Update("", strLabel1, strLabel2, strLabel3, strLabel4, strLabel5) == 5)
            {

            }
            else
            {

            }
        }

        private void Process_Terminal_Inventory()
        {
            int length = byte_terminal_data[15];
            int label_num = byte_terminal_data[16];
            int label_flag = byte_terminal_data[17];

            if (label_num == 0 || label_num > 5)
            {
                return;
            }

            string strLabel1 = null;
            string strLabel2 = null;
            string strLabel3 = null;
            string strLabel4 = null;
            string strLabel5 = null;
            string tmp = ByteToHexString(byte_terminal_uid,0,12); 
            tmp = tmp.Trim();
            switch (label_num)
            {
                case 1:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                    }
                    break;
                case 2:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, 25, 8);
                    }
                    break;
                case 3:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, 25, 8);
                        strLabel3 = ByteToHexString(byte_terminal_data, 33, 8);
                    }
                    break;
                case 4:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, 25, 8);
                        strLabel3 = ByteToHexString(byte_terminal_data, 33, 8);
                        strLabel4 = ByteToHexString(byte_terminal_data, 41, 8);
                    }
                    break;
                case 5:
                    {
                        strLabel1 = ByteToHexString(byte_terminal_data, 17, 8);
                        strLabel2 = ByteToHexString(byte_terminal_data, 25, 8);
                        strLabel3 = ByteToHexString(byte_terminal_data, 33, 8);
                        strLabel4 = ByteToHexString(byte_terminal_data, 41, 8);
                        strLabel5 = ByteToHexString(byte_terminal_data, 49, 8);
                    }
                    break;
                default:
                    break;
            }

            if (database.Terminal_Labels_Pos_Update(tmp, strLabel1, strLabel2, strLabel3, strLabel4, strLabel5) == 5)
            {

            }
            else
            {

            }
            strLabel1 = null;
            strLabel2 = null;
            strLabel3 = null;
            strLabel4 = null;
            strLabel5 = null;
        }

        //工作流程-Process警告
        private void Process_Terminal_Alarm()
        {
            /*byte type_warning = byte_terminal_data[16];
            byte no_index = byte_terminal_data[17];
            byte flag_warning = byte_terminal_data[18];
            byte[] warning=new byte[20];
            Array.Copy(byte_terminal_data,warning,20);
            string strTerminalUID = ByteToHexString(byte_terminal_uid, 0, 12); 

            if (database.Terminal_Coil_Alarm(strTerminalUID, flag_warning, no_index))
            {
                udpSocket.Send(APEncapsulator.Response_Terminal_Coil_Alarm(byte_ap_uid, byte_terminal_uid, 
                    type_warning, no_index, flag_warning));
            }*/
        }

        private string ByteToHexString(byte[] b, int index = 0, int length = 0,int divid=1,string seperator="")
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
                    if(tmp.Length==1)
                    {
                        tmp = "0" + tmp;
                    }
                    result += tmp;
                }
                else if(i%divid ==0)
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
