using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace MonitorCore
{
    class APLotHandler
    {

        private UDPSocket udpSocket;
        private DB_APLotHandler database;
        private List<APParser> apLotList;

        private APEncapsulator apEncapsulator;

        private Thread apHandlerThread = null;
        ManualResetEvent resumeEvent = new ManualResetEvent(false);
        private bool bStop = false;

        private bool isWaiting;//check if thread is running
        private int identity;//ID

        private byte[] byte_ap_uid = new byte[24];
        private char[] char_ap_data = new char[256];
        private byte[] byte_ap_data = new byte[256];

        private System.Timers.Timer timeout = new System.Timers.Timer(1000);//Timer
        private int timeout_acc = 0;//accumulator
        private int iTimeout = 10000;//timeout time

        private int timeout_send_acc = 0;
        private int iTimeout_send = 2000;
        private bool bEnable_send = false;
        private void Enable_timeout_send()
        {
            if(!bEnable_send)
            {
                bEnable_send = true;
                timeout_send_acc = 0;
            }
            
        }
        public enum AP_LOT_STATUS
        {
            RFID_CARD_MSG,
            SELECT_MODE,
            LOT_OPERATION
        }

        public APLotHandler(UDPSocket udpSocket, DB_APLotHandler database, List<APParser> apLotList, int identity)
        {
            this.udpSocket = udpSocket;
            this.database = database;
            this.apLotList = apLotList;
            this.identity = identity;
            isWaiting = true;

            apEncapsulator = new APEncapsulator();

            apHandlerThread = new Thread(new ThreadStart(APHandleFunc));

            timeout.Elapsed += new System.Timers.ElapsedEventHandler(Job_Timer);//到达时间的时候执行事件；

            timeout.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；

            timeout.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；

            timeout.Start();
        }

        ~APLotHandler()
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
            /*while (apHandlerThread.ThreadState != ThreadState.Stopped)
            {
                Thread.Sleep(100);
                threadTimeout += 100;
                if (threadTimeout > 1000)
                {
                    apHandlerThread.Abort();
                    Thread.Sleep(100);
                }
            }*/

        }

        public void Resume(Object sender, EventArgs e)
        {
            if(!isWaiting)
            {
                return;
            }
            Log.write("APLotHandler " + identity.ToString() + " -Resume");
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
            Log.write("APLotHandler " + identity.ToString() + " -ThreadEnd");
        }
        //AP工作分配
        private void APHandleJob()
        {
            APParser apParser;
            EnumOrder order;
            int count = 0;
            lock (apLotList)
            {
                count = apLotList.Count;
            }

            if (count == 0)
            {
                Log.write("APLotHandler-WaitOne");
                isWaiting = true;
                resumeEvent.Reset();
                resumeEvent.WaitOne();
            }

            lock (apLotList)
            {
                count = apLotList.Count;
                if (count != 0)
                {
                    apParser = apLotList[0];
                    order = apParser.GetOrder();
                }
                else
                {
                    return;
                }

                apLotList.RemoveAt(0);
            }

            Log.write("APLotHandler remain count:" + apLotList.Count.ToString());
            //reset timeout

            switch (order)
            {
                case EnumOrder.AP_DATA_UPDOWN://AP Lot 
                    {
                        Log.write("APLotHandler " + identity.ToString() + " --AP_DATA_UP:" + apParser.GetParam_Origin_Hex(), 2);
                        timeout_acc = 0;
                        Job_AP_data_upload(apParser);
                        
                    }
                    break;
               case EnumOrder.AP_ORDER_UPDOWN://AP Lot 
                   {
                       Log.write("APLotHandler " + identity.ToString() + " --AP_ORDER_UP:" + apParser.GetParam_Origin_Hex(), 2);
                       timeout_acc = 0;
                       Job_AP_order_upload(apParser);
                   }
                    break;
                default:
                    break;
            }

            apParser.Dispose();//自销毁
            
        }

        private void Job_AP_data_upload(APParser apPaser)
        {
            int size = 0;
            if (!apPaser.GetParam_AP_data_upload(ref byte_ap_uid, ref byte_ap_data, ref size))
            {
                return;
            }

            APProcessJob();
            string tmp1 = System.Text.Encoding.Default.GetString(byte_ap_uid);
            database.CmdLog("05010500", tmp1, apPaser.GetParam_Origin_Hex());
        }

        private void Job_AP_order_upload(APParser apPaser)
        {
            int size = 0;
            if (!apPaser.GetParam_AP_data_upload(ref byte_ap_uid, ref byte_ap_data, ref size))
            {
                return;
            }

            APProcessJob1();
            string tmp1 = System.Text.Encoding.Default.GetString(byte_ap_uid);
            database.CmdLog("05010400", tmp1, apPaser.GetParam_Origin_Hex());
        }

        private void APProcessJob()
        {
            int cmd = byte_ap_data[2];
            char_ap_data = Encoding.ASCII.GetChars(byte_ap_data);
            switch(cmd)
            {
                case 0x81:
                    Process_AP_Show_Msg();
                    break;
                case 0x83:
                    Process_AP_Lot_Add();
                    break;
                case 0x85:
                    Process_AP_Lot_Decrease();
                    break;
                case 0x87:
                    Process_Button_Type();
                    break;
                case 0x89:
                    Process_Msg_Report();
                    break;
                case 0x93:
                    Process_Mobile_SO_NO();
                    break;
                case 0x95:
                    Process_Mobile_Label();
                    break;
                case 0x97:
                    Process_Mobile_Lot_Modify();
                    break;
            }
        }

        private void APProcessJob1()
        {
            int cmd = byte_ap_data[2];
            char_ap_data = Encoding.ASCII.GetChars(byte_ap_data);
            switch (cmd)
            {
                case 0x92:
                    Process_Response_Msg_Show();
                    break;
            }
        }

        void Process_Response_Msg_Show()
        {
            bEnable_send = false;
        }

        private void Process_Mobile_SO_NO()
        {
            byte length = byte_ap_data[3];
            byte[] data = new byte[length];
            Array.Copy(byte_ap_data, 4, data, 0, length);
            string strData = System.Text.Encoding.Default.GetString(data);
            MobileSendMsg msgObject = JsonConvert.DeserializeObject<MobileSendMsg>(strData);

            string COLOR_NO="";
            string strPos="";
            string ClothName="";
            string Cloth_Kind="";
            if (database.Mobile_Info_Get(msgObject.card, ref COLOR_NO, ref strPos, ref ClothName, ref Cloth_Kind) == 1)
            {
                MobileRecMsg msg=new MobileRecMsg();
                msg.COL_NO = COLOR_NO;
                msg.ClothName = ClothName;
                msg.Cloth_Kind = Cloth_Kind;
                msg.LOC_REM = strPos;
                string strJson=JsonConvert.SerializeObject(msg);
                byte[] byte_send=Encoding.GetEncoding("GB2312").GetBytes(strJson);
                udpSocket.Send(apEncapsulator.Response_Mobile_Msg_Show(byte_ap_uid, byte_send, 0x01));
            }
            else
            {
                udpSocket.Send(apEncapsulator.Response_Mobile_Msg_Show(byte_ap_uid, null, 0x02));
            }
        }

        private void Process_Mobile_Label()
        {
            byte length = byte_ap_data[3];
            byte[] data = new byte[length];
            Array.Copy(byte_ap_data, 4, data, 0, length);
            string strData = System.Text.Encoding.Default.GetString(data);
            MobileSendMsg msgObject = JsonConvert.DeserializeObject<MobileSendMsg>(strData);

            string COLOR_NO = "";
            string strPos = "";
            string ClothName = "";
            string Cloth_Kind = "";
            string SO_NO = "";
            if (database.Mobile_Info_Get_By_RFID(msgObject.card, ref SO_NO,ref COLOR_NO, ref strPos, ref ClothName, ref Cloth_Kind) == 1)
            {
                MobileRecMsg1 msg = new MobileRecMsg1();
                msg.SO_NO = SO_NO;
                msg.COL_NO = COLOR_NO;
                msg.ClothName = ClothName;
                msg.Cloth_Kind = Cloth_Kind;
                msg.LOC_REM = strPos;
                string strJson = JsonConvert.SerializeObject(msg);
                byte[] byte_send = Encoding.GetEncoding("GB2312").GetBytes(strJson);
                udpSocket.Send(apEncapsulator.Response_Mobile_Msg_Show1(byte_ap_uid, byte_send, 0x01));
            }
            else
            {
                udpSocket.Send(apEncapsulator.Response_Mobile_Msg_Show1(byte_ap_uid, null, 0x02));
            }
        }

        private void Process_Mobile_Lot_Modify()
        {
            byte length = byte_ap_data[3];
            byte[] data = new byte[length];
            Array.Copy(byte_ap_data, 4, data, 0, length);
            string strData = System.Text.Encoding.Default.GetString(data);
            MobileSendMsg1 msgObject = JsonConvert.DeserializeObject<MobileSendMsg1>(strData);

            if(msgObject.status==1)
            {
                byte status = database.AP_Lot_Add(msgObject.card, msgObject.lot);
                udpSocket.Send(apEncapsulator.Response_Mobile_Lot_Operate(byte_ap_uid, data, status));
            }
            else if(msgObject.status==2)
            {
                byte status = database.AP_Lot_Decrease(msgObject.card, msgObject.lot);
                udpSocket.Send(apEncapsulator.Response_Mobile_Lot_Operate(byte_ap_uid, data, status));
            }
        }

        #region state machine
        private AP_LOT_STATUS status = AP_LOT_STATUS.RFID_CARD_MSG;
        private byte mode = 0;
        private byte nBtnIndex = 0;
        private byte[] LotNo;
        private string strLabel = "";
        private string strSO_NO = "";
        private string strBtn_Name = "";
        byte[] data1 = null;
        byte[] data2 = null;
        byte[] data3 = null;
        byte[] data4 = null;
        private void SetState(AP_LOT_STATUS status)
        {
            this.status = status;
        }


        private void Process_Button_Type()
        {
            udpSocket.Send(apEncapsulator.Response_Button_Report(byte_ap_uid));
            nBtnIndex = byte_ap_data[4];
            string btn_name = "";
            switch (nBtnIndex)
            {
                case 0x01:
                    break;
                case 0x02:
                    break;
                case 0x04:
                    btn_name = "缸号增加";
                    break;
                case 0x08:
                    break;
                case 0x10:
                    btn_name = "缸号删除";
                    break;
            }
            strBtn_Name = btn_name;
            data1 = null;
            data2 = null;
            data3 = Encoding.GetEncoding("GB2312").GetBytes(btn_name);
            data4 = null;

            switch (status)
            {
                case AP_LOT_STATUS.RFID_CARD_MSG:
                    {
                        udpSocket.Send(apEncapsulator.Response_Msg_Show(byte_ap_uid, data1, data2, data3, data4));
                        Enable_timeout_send();
                    }
                    break;
                case AP_LOT_STATUS.SELECT_MODE:
                    {
                        data1=System.Text.Encoding.Default.GetBytes(ChineseConvert.TraditionalToSimplified(strSO_NO));
                        udpSocket.Send(apEncapsulator.Response_Msg_Show(byte_ap_uid, data1, data2, data3, data4));
                        Enable_timeout_send();
                        SetState(AP_LOT_STATUS.LOT_OPERATION);
                    }
                    break;
                case AP_LOT_STATUS.LOT_OPERATION:
                    {
                        data1 = System.Text.Encoding.Default.GetBytes(ChineseConvert.TraditionalToSimplified(strSO_NO));
                        udpSocket.Send(apEncapsulator.Response_Msg_Show(byte_ap_uid, data1, data2, data3, data4));
                        Enable_timeout_send();
                        SetState(AP_LOT_STATUS.LOT_OPERATION);
                    }
                    break;
            }
        }

        private void Process_Msg_Report()
        {

            udpSocket.Send(apEncapsulator.Response_Msg_Report(byte_ap_uid));
            switch (status)
            {
                case AP_LOT_STATUS.RFID_CARD_MSG:
                    {
                        Get_RFID_Label_Msg();
                    }
                    break;
                case AP_LOT_STATUS.SELECT_MODE:
                    {
                        Get_RFID_Label_Msg();
                    }
                    break;
                case AP_LOT_STATUS.LOT_OPERATION:
                    {
                        Get_Lot_Msg();
                    }
                    break;
            }
        }

        private void Get_Lot_Msg()
        {
            int source = byte_ap_data[4];
            int msg_kind = byte_ap_data[5];
            int length = byte_ap_data[6];
            byte[] byte_data = new byte[length];
            Array.Copy(byte_ap_data, 7, byte_data, 0, length);

            if (source == 2)
            {
                LotNo = new byte[length];
                Array.Copy(byte_data, LotNo, length);

                string strLotNo = "缸号："+System.Text.Encoding.Default.GetString(LotNo);
                byte bRet = 0;
                switch (nBtnIndex)
                {
                    case 0x04:
                        {
                            bRet = database.AP_Lot_Add(strLabel, strLotNo);
                            data1 = System.Text.Encoding.Default.GetBytes(ChineseConvert.TraditionalToSimplified(strSO_NO));
                            data2 = System.Text.Encoding.Default.GetBytes(ChineseConvert.TraditionalToSimplified(strLotNo));
                            data3 = System.Text.Encoding.Default.GetBytes(ChineseConvert.TraditionalToSimplified(strBtn_Name)); 
                            data4 = null;
                            switch (bRet)
                            {
                                case 1:
                                    data4 = Encoding.GetEncoding("GB2312").GetBytes("缸号增加成功");
                                    break;
                                case 2:
                                    data4 = Encoding.GetEncoding("GB2312").GetBytes("标签号不存在");
                                    break;
                                case 3:
                                    data4 = Encoding.GetEncoding("GB2312").GetBytes("缸号已存在");
                                    break;
                                case 4:
                                    data4 = Encoding.GetEncoding("GB2312").GetBytes("数据库错误");
                                    break;
                            }
                            udpSocket.Send(apEncapsulator.Response_Msg_Show(byte_ap_uid, data1, data2, data3, data4));
                            Enable_timeout_send();
                        }
                        break;
                    case 0x10:
                        {
                            bRet = database.AP_Lot_Decrease(strLabel, strLotNo);
                            data1 = System.Text.Encoding.Default.GetBytes(ChineseConvert.TraditionalToSimplified(strSO_NO));
                            data2 = System.Text.Encoding.Default.GetBytes(ChineseConvert.TraditionalToSimplified(strLotNo));
                            data3 = System.Text.Encoding.Default.GetBytes(ChineseConvert.TraditionalToSimplified(strBtn_Name)); 
                            data4 = null;

                            switch (bRet)
                            {
                                case 1:
                                    data4 = Encoding.GetEncoding("GB2312").GetBytes("缸号删除成功");
                                    break;
                                case 2:
                                    data4 = Encoding.GetEncoding("GB2312").GetBytes("标签号不存在");
                                    break;
                                case 3:
                                    data4 = Encoding.GetEncoding("GB2312").GetBytes("缸号不存在");
                                    break;
                                case 4:
                                    data4 = Encoding.GetEncoding("GB2312").GetBytes("数据库错误");
                                    break;
                            }

                            udpSocket.Send(apEncapsulator.Response_Msg_Show(byte_ap_uid, data1, data2, data3, data4));
                            Enable_timeout_send();
                        }
                        break;
                }
            }
            else
            {
                Get_RFID_Label_Msg();
            }
        }

        private void Get_RFID_Label_Msg()
        {
            int source = byte_ap_data[4];
            int msg_kind = byte_ap_data[5];
            int length = byte_ap_data[6]-1;
            byte[] byte_data = new byte[length];
            Array.Copy(byte_ap_data, 7, byte_data, 0, length);

            if (source == 1)
            {
                LotNo = new byte[length];
                Array.Copy(byte_data, LotNo, length);

                string strLabel_local = ByteToHexString(byte_ap_data, 7, length);
                string SO_NO = "";
                string COLOR_NO = "";
                string Lot_NO = "";
                string ClothName = "";
                string Cloth_kind = "";

                if (database.AP_Show_Msg(strLabel_local, ref SO_NO, ref COLOR_NO, ref Lot_NO, ref ClothName, ref Cloth_kind))
                {
                    strLabel = strLabel_local;
                    strSO_NO = SO_NO;
                    data1 = System.Text.Encoding.Default.GetBytes(ChineseConvert.TraditionalToSimplified(SO_NO));
                    data2 = System.Text.Encoding.Default.GetBytes(ChineseConvert.TraditionalToSimplified(COLOR_NO));
                    data3 = Encoding.GetEncoding("GB2312").GetBytes(ChineseConvert.TraditionalToSimplified(Lot_NO));
                    data4 = Encoding.GetEncoding("GB2312").GetBytes(ChineseConvert.TraditionalToSimplified(ClothName));
                    udpSocket.Send(apEncapsulator.Response_Msg_Show(byte_ap_uid, data1, data2, data3, data4));
                    Enable_timeout_send();
                    SetState(AP_LOT_STATUS.SELECT_MODE);
                }
                else
                {
                    data1 = null;
                    data2 = null;
                    data3 = null;
                    data4 = Encoding.GetEncoding("GB2312").GetBytes("卡号不存在");
                    udpSocket.Send(apEncapsulator.Response_Msg_Show(byte_ap_uid, data1, data2, data3, data4));
                    Enable_timeout_send();
                }

            }
            else
            {
                
                data1 = null;
                data2 = null;
                data3 = null;
                data4 = null;
                switch(status)
                {
                    case AP_LOT_STATUS.SELECT_MODE:
                        data1=Encoding.GetEncoding("GB2312").GetBytes("请选择功能");
                        data2 = Encoding.GetEncoding("GB2312").GetBytes("F3：缸号增加");
                        data3 = Encoding.GetEncoding("GB2312").GetBytes("F5：缸号删除");
                        data4 = null;
                        break;
                    case AP_LOT_STATUS.RFID_CARD_MSG:
                        data4 = Encoding.GetEncoding("GB2312").GetBytes("请先刷标签卡");
                        break;
                }

                udpSocket.Send(apEncapsulator.Response_Msg_Show(byte_ap_uid, data1, data2, data3, data4));
                Enable_timeout_send();
            }
        }

        private void Job_Timer(object source, System.Timers.ElapsedEventArgs e)
        {
            timeout_acc += 1000;
            if (timeout_acc > iTimeout)
            {
                status = AP_LOT_STATUS.RFID_CARD_MSG;
            }
            if (bEnable_send)
            {
                timeout_send_acc += 1000;
                if (timeout_send_acc > iTimeout_send)
                {
                    udpSocket.Send(apEncapsulator.Response_Msg_Show(byte_ap_uid, data1, data2, data3, data4));
                    timeout_send_acc = 0;
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

        #region deprecated
        private void Process_AP_Lot_Add()
        {
            string tmp = System.Text.Encoding.Default.GetString(byte_ap_uid);


            byte length = byte_ap_data[12];
            byte[] byte_LotNo = new byte[length];
            Array.Copy(byte_ap_data, 13, byte_LotNo, 0, length);

            string strLabel = ByteToHexString(byte_ap_data, 4, 8);
            string strLotNo = System.Text.Encoding.Default.GetString(byte_LotNo);

            byte[] byte_label = System.Text.Encoding.Default.GetBytes(strLabel);

            byte status = database.AP_Lot_Add(strLabel, strLotNo);
            udpSocket.Send(apEncapsulator.Response_Lot_Add(byte_ap_uid, byte_label, status, length, byte_LotNo));

            tmp = null;
        }
        private void Process_AP_Lot_Decrease()
        {

            string tmp = System.Text.Encoding.Default.GetString(byte_ap_uid);


            byte length = byte_ap_data[12];
            byte[] byte_LotNo = new byte[length];
            Array.Copy(byte_ap_data, 13, byte_LotNo, 0, length);

            string strLabel = ByteToHexString(byte_ap_data, 4, 8);
            string strLotNo = System.Text.Encoding.Default.GetString(byte_LotNo);

            byte[] byte_label = System.Text.Encoding.Default.GetBytes(strLabel);

            byte status = database.AP_Lot_Decrease(strLabel, strLotNo);
            udpSocket.Send(apEncapsulator.Response_Lot_Decrease(byte_ap_uid, byte_label, status, length, byte_LotNo));

            tmp = null;
        }

        private void Process_AP_Show_Msg()
        {
            string strLabel = ByteToHexString(byte_ap_data, 4, 8);
            string SO_NO = "";
            string COLOR_NO = "";
            string Lot_NO = "";
            string ClothName = "";
            string Cloth_kind = "";
            byte flag = byte_ap_data[12];
            //
            int mark1 = ((flag >> 0) & 0x01);
            int mark2 = ((flag >> 1) & 0x01);
            int mark3 = ((flag >> 2) & 0x01);
            int mark4 = ((flag >> 3) & 0x01);
            int mark5 = ((flag >> 4) & 0x01);
            byte[] size = new byte[4];
            byte[] data1 = null;
            byte[] data2 = null;
            byte[] data3 = null;
            byte[] data4 = null;

            size[0] = 0;
            size[1] = 0;
            size[2] = 0;
            size[3] = 0;
            byte[] byte_label = System.Text.Encoding.Default.GetBytes(strLabel);

            if (database.AP_Show_Msg(strLabel, ref SO_NO, ref COLOR_NO, ref Lot_NO, ref ClothName, ref Cloth_kind))
            {
                switch (flag)
                {
                    case 1://入库单号
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);
                        }
                        break;
                    case 2://色号
                        {
                            //色号
                            size[0] = (byte)COLOR_NO.Length;
                            data1 = new byte[COLOR_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                        }
                        break;
                    case 3://入库单号+色号
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);
                            //色号
                            size[1] = (byte)COLOR_NO.Length;
                            data2 = new byte[COLOR_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                        }
                        break;
                    case 4://缸号
                        {
                            //缸号
                            size[0] = (byte)Lot_NO.Length;
                            data1 = new byte[Lot_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(Lot_NO);
                        }
                        break;
                    case 5://入库单号+缸号
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);

                            //缸号
                            size[1] = (byte)Lot_NO.Length;
                            data2 = new byte[Lot_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(Lot_NO);
                        }
                        break;
                    case 6://色号+缸号
                        {
                            //色号
                            size[0] = (byte)COLOR_NO.Length;
                            data1 = new byte[COLOR_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                            //缸号
                            size[1] = (byte)Lot_NO.Length;
                            data2 = new byte[Lot_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(Lot_NO);
                        }
                        break;
                    case 7://入库单号+色号+缸号
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);
                            //色号
                            size[1] = (byte)COLOR_NO.Length;
                            data2 = new byte[COLOR_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                            //缸号
                            size[2] = (byte)Lot_NO.Length;
                            data3 = new byte[Lot_NO.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(Lot_NO);
                        }
                        break;
                    case 8://布名
                        {
                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[0] = (byte)byte_ClothName.Length;
                            data1 = new byte[ClothName.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(ClothName);
                        }
                        break;
                    case 9://入库单号+布名
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);
                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);

                            size[1] = (byte)byte_ClothName.Length;
                            data2 = new byte[ClothName.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(ClothName);
                        }
                        break;
                    case 10://色号+布名
                        {
                            //色号
                            size[0] = (byte)COLOR_NO.Length;
                            data1 = new byte[COLOR_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[1] = (byte)byte_ClothName.Length;
                            data2 = new byte[ClothName.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(ClothName);
                        }
                        break;
                    case 11://入库单号+色号+布名
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);
                            //色号
                            size[1] = (byte)COLOR_NO.Length;
                            data2 = new byte[COLOR_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[2] = (byte)byte_ClothName.Length;
                            data3 = new byte[ClothName.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(ClothName);
                        }
                        break;
                    case 12://缸号+布名
                        {
                            //缸号
                            size[0] = (byte)Lot_NO.Length;
                            data1 = new byte[Lot_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(Lot_NO);
                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[1] = (byte)byte_ClothName.Length;
                            data2 = new byte[ClothName.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(ClothName);
                        }
                        break;
                    case 13://入库单号+缸号+布名
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);

                            //缸号
                            size[1] = (byte)Lot_NO.Length;
                            data2 = new byte[Lot_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(Lot_NO);

                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[2] = (byte)byte_ClothName.Length;
                            data3 = new byte[ClothName.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(ClothName);
                        }
                        break;
                    case 14://色号+缸号+布名
                        {
                            //色号
                            size[0] = (byte)COLOR_NO.Length;
                            data1 = new byte[COLOR_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(COLOR_NO);

                            //缸号
                            size[1] = (byte)Lot_NO.Length;
                            data2 = new byte[Lot_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(Lot_NO);

                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[2] = (byte)byte_ClothName.Length;
                            data3 = new byte[ClothName.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(ClothName);
                        }
                        break;
                    case 15://入库单号+色号+缸号+布名
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);
                            //色号
                            size[1] = (byte)COLOR_NO.Length;
                            data2 = new byte[COLOR_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                            //缸号
                            size[2] = (byte)Lot_NO.Length;
                            data3 = new byte[Lot_NO.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(Lot_NO);
                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[3] = (byte)byte_ClothName.Length;
                            data4 = new byte[ClothName.Length];
                            data4 = System.Text.Encoding.Default.GetBytes(ClothName);
                        }
                        break;
                    case 16://布种
                        {
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[0] = (byte)byte_Cloth.Length;
                            data1 = new byte[Cloth_kind.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 17://入库单号+布种
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[1] = (byte)byte_Cloth.Length;
                            data2 = new byte[Cloth_kind.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 18://色号+布种
                        {
                            //色号
                            size[0] = (byte)COLOR_NO.Length;
                            data1 = new byte[COLOR_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[1] = (byte)byte_Cloth.Length;
                            data2 = new byte[Cloth_kind.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 19://入库单号+色号+布种
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);
                            //色号
                            size[1] = (byte)COLOR_NO.Length;
                            data2 = new byte[COLOR_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[2] = (byte)byte_Cloth.Length;
                            data3 = new byte[Cloth_kind.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 20://缸号+布种
                        {
                            //缸号
                            size[0] = (byte)Lot_NO.Length;
                            data1 = new byte[Lot_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(Lot_NO);
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[1] = (byte)byte_Cloth.Length;
                            data2 = new byte[Cloth_kind.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 21://入库单号+缸号+布种
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);
                            //缸号
                            size[1] = (byte)Lot_NO.Length;
                            data2 = new byte[Lot_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(Lot_NO);
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[2] = (byte)byte_Cloth.Length;
                            data3 = new byte[Cloth_kind.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 22://色号+缸号+布种
                        {
                            //色号
                            size[0] = (byte)COLOR_NO.Length;
                            data1 = new byte[COLOR_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                            //缸号
                            size[1] = (byte)Lot_NO.Length;
                            data2 = new byte[Lot_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(Lot_NO);
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[2] = (byte)byte_Cloth.Length;
                            data3 = new byte[Cloth_kind.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 23://入库单号+色号+缸号+布种
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);
                            //色号
                            size[1] = (byte)COLOR_NO.Length;
                            data2 = new byte[COLOR_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                            //缸号
                            size[2] = (byte)Lot_NO.Length;
                            data3 = new byte[Lot_NO.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(Lot_NO);
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[3] = (byte)byte_Cloth.Length;
                            data4 = new byte[Cloth_kind.Length];
                            data4 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 24://布名+布种
                        {
                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[0] = (byte)byte_ClothName.Length;
                            data1 = new byte[ClothName.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(ClothName);
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[1] = (byte)byte_Cloth.Length;
                            data2 = new byte[Cloth_kind.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 25://入库单号+布名+布种
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);
                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[1] = (byte)byte_ClothName.Length;
                            data2 = new byte[ClothName.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(ClothName);
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[2] = (byte)byte_Cloth.Length;
                            data3 = new byte[Cloth_kind.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 26://色号+布名+布种
                        {
                            //色号
                            size[0] = (byte)COLOR_NO.Length;
                            data1 = new byte[COLOR_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[1] = (byte)byte_ClothName.Length;
                            data2 = new byte[ClothName.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(ClothName);
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[2] = (byte)byte_Cloth.Length;
                            data3 = new byte[Cloth_kind.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 27://入库单号+色号+布名+布种
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);
                            //色号
                            size[1] = (byte)COLOR_NO.Length;
                            data2 = new byte[COLOR_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[2] = (byte)byte_ClothName.Length;
                            data3 = new byte[ClothName.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(ClothName);
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[3] = (byte)byte_Cloth.Length;
                            data4 = new byte[Cloth_kind.Length];
                            data4 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 28://缸号+布名+布种
                        {
                            //缸号
                            size[0] = (byte)Lot_NO.Length;
                            data1 = new byte[Lot_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(Lot_NO);
                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[1] = (byte)byte_ClothName.Length;
                            data2 = new byte[ClothName.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(ClothName);
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[2] = (byte)byte_Cloth.Length;
                            data3 = new byte[Cloth_kind.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 29://入库单号+缸号+布名+布种
                        {
                            //入库单号
                            size[0] = (byte)SO_NO.Length;
                            data1 = new byte[SO_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(SO_NO);
                            //缸号
                            size[1] = (byte)Lot_NO.Length;
                            data2 = new byte[Lot_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(Lot_NO);
                            //布名
                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[2] = (byte)byte_ClothName.Length;
                            data3 = new byte[ClothName.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(ClothName);
                            //布种
                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[3] = (byte)byte_Cloth.Length;
                            data4 = new byte[Cloth_kind.Length];
                            data4 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                    case 30://色号+缸号+布名+布种
                        {
                            //色号
                            size[0] = (byte)COLOR_NO.Length;
                            data1 = new byte[COLOR_NO.Length];
                            data1 = System.Text.Encoding.Default.GetBytes(COLOR_NO);
                            //缸号
                            size[1] = (byte)Lot_NO.Length;
                            data2 = new byte[Lot_NO.Length];
                            data2 = System.Text.Encoding.Default.GetBytes(Lot_NO);
                            //布名

                            byte[] byte_ClothName = Encoding.GetEncoding("GB2312").GetBytes(ClothName);
                            size[2] = (byte)byte_ClothName.Length;
                            data3 = new byte[ClothName.Length];
                            data3 = System.Text.Encoding.Default.GetBytes(ClothName);
                            //布种

                            byte[] byte_Cloth = Encoding.GetEncoding("GB2312").GetBytes(Cloth_kind);
                            size[3] = (byte)byte_Cloth.Length;
                            data4 = new byte[Cloth_kind.Length];
                            data4 = System.Text.Encoding.Default.GetBytes(Cloth_kind);
                        }
                        break;
                }

                udpSocket.Send(apEncapsulator.Response_AP_Show_Msg(byte_ap_uid, byte_label, 1, size, data1, data2, data3, data4));
            }
            else
            {
                udpSocket.Send(apEncapsulator.Response_AP_Show_Msg(byte_ap_uid, byte_label, 1, size, data1, data2, data3, data4));
            }
        }
        #endregion

      

    }
}
