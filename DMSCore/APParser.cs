using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonitorCore
{
    enum EnumOrder
    {
        AP_VALIDATION_UP,
        HEARTBEAT_UP,
        AP_STATUS_UP,
        AP_DATA_UPDOWN,
        AP_ORDER_UPDOWN,
        READER_STATUS_UP,
        READER_DATA_UP,
        ENRANCE_GUARD_UP,
        READER_SERACH_DOWN,
        READER_CONFIG_DOWN,
        READER_DELETE_DOWN,
        READER_ORDER_DOWN,
        ERROR,
    }
    class APParser : IDisposable
    {
        private byte[] data;
        private int size=0;
        private int pk_id = 0;//For databse result return 
        public APParser(byte[] data,int size,int pk_id=0)
        {
            this.data = data;
            this.size = size;
            this.pk_id = pk_id;
        }

        ~APParser()
        {
            //Dispose();
        }
        //命令类型--DB数据回复或者AP上报
        /// <summary>
        ///1:AP命令，2：DBOrder response ，3:Alarm,4:AP Lot
        /// </summary>
        public int MsgKind()
        {
            int cmd = data[5] * 0x1000000 + data[6] * 0x10000 + data[7] * 0x100 + data[8];
            int iRet = 0;
            switch(cmd)
            {
                case 0x05010100:
                    iRet = 1;
                    break;
                case 0x05010200:
                    iRet = 1;
                    break;
                case 0x05010300:
                    iRet = 1;
                    break;
                case 0x05010400:
                    if (data[35]==0xA1 || data[35]==0xA3 ||data[35]==0xA5 )
                    {
                        iRet = 2;
                    }
                    else
                    {
                        iRet = 4;
                    }
                    
                    break;
                case 0x05010500:
                    iRet = 4;
                    break;
                case 0x05020100:
                    iRet = 2;
                    break;
                case 0x05020200:
                    iRet = 2;
                    break;
                case 0x05020300:
                    iRet = 2;
                    break;
                case 0x05020400:
                    iRet = 1;
                    break;
                case 0x05020500:
                    iRet = 1;
                    break;
                case 0x05020600://终端数据
                    {
                        if (data[47] == 0x81 || data[47] == 0x82
                            || data[47] == 0x83 || data[47] == 0x88
                            )
                        {
                            iRet = 1;
                        }
                        else if(data[47] == 0x8B)
                        {
                            iRet = 3;
                        }
                        else
                        {
                            iRet = 2;
                        }
                    }
                    
                    break;
                case 0x05020700:
                    iRet = 3;
                    break;
                default:
                    break;
            }
            return iRet;
        }

        public byte GetAPOrder()
        {
            if(data.Length>35)
            {
                return data[35];
            }
            else
            {
                return 0;
            }
        }


        public byte GetTerminalOrder()
        {
            if (data.Length > 47)
            {
                return data[47];
            }
            else
            {
                return 0;
            }
        }

        //获取命令
        public EnumOrder GetOrder()
        {
            int cmd = data[5] * 0x1000000 + data[6] * 0x10000 + data[7] * 0x100 + data[8];
            EnumOrder order;
            switch(cmd)
            {
                case 0x05010100:
                    order=EnumOrder.AP_VALIDATION_UP;
                    break;
                case 0x05010200:
                    order = EnumOrder.HEARTBEAT_UP;
                    break;
                case 0x05010300:
                    order = EnumOrder.AP_STATUS_UP;
                    break;
                case 0x05010400:
                    order = EnumOrder.AP_ORDER_UPDOWN;
                    break;
                case 0x05010500:
                    order = EnumOrder.AP_DATA_UPDOWN;
                    break;
                case 0x05020100:
                    order = EnumOrder.READER_SERACH_DOWN;
                    break;
                case 0x05020200:
                    order = EnumOrder.READER_CONFIG_DOWN;
                    break;
                case 0x05020300:
                    order = EnumOrder.READER_DELETE_DOWN;
                    break;
                case 0x05020400:
                    order = EnumOrder.READER_ORDER_DOWN;
                    break;
                case 0x05020500:
                    order = EnumOrder.READER_STATUS_UP;
                    break;
                case 0x05020600:
                    order = EnumOrder.READER_DATA_UP;
                    break;
                case 0x05020700:
                    order = EnumOrder.ENRANCE_GUARD_UP;
                    break;
                default:
                    order = EnumOrder.ERROR;
                    break;
            }
            return order;
        }

        public void GetParam_Validate_Order(ref byte[] apuid,ref int channelCount)
        {
            Array.Copy(data, 9, apuid, 0, 24);
            channelCount = data[33]; 
        }

        public void GetParam_AP_Status(ref byte[] apuid, ref int isOnline)
        {
            Array.Copy(data, 9, apuid, 0, 24);
            isOnline = data[33]; 
        }

        public void GetParam_Terminal_Status(ref byte[] apuid, ref byte[] terminal_uid, ref int isOnline)
        {
            Array.Copy(data, 9, apuid, 0, 24);
            Array.Copy(data, 33, terminal_uid, 0, 24);
            isOnline = data[57];
        }

        public void GetParam_Entrance_Guard(ref byte[] guard_uid,ref byte[] guard_data,ref int guard_data_size)
        {
            Array.Copy(data, 9, guard_uid, 0, 24);

            guard_data_size = data[33];

            Array.Copy(data, 34, guard_data, 0, guard_data_size * 8);
        }

        public bool GetParam_Terminal_data_upload(ref byte[] ap_uid,ref byte[] terminal_uid,ref byte[] terminal_data,ref int size)
        {
            //ap_uid = Encoding.ASCII.GetChars(data, 9, 24);
            Array.Copy(data, 9, ap_uid, 0, 24);

            size = data[2] * 0x100 + data[3]-9-24;

            //terminal_data = Encoding.ASCII.GetChars(data, 33, size);
            //terminal_uid = Encoding.ASCII.GetChars(data, 35, 12);
            Array.Copy(data, 35, terminal_uid, 0, 12);

            Array.Copy(data, 33, terminal_data, 0, size);

            if(terminal_data[0]==42 && terminal_data[1]==42)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool GetParam_AP_data_upload(ref byte[] ap_uid, ref byte[] AP_data, ref int size)
        {
            //ap_uid = Encoding.ASCII.GetChars(data, 9, 24);
            Array.Copy(data, 9, ap_uid, 0, 24);

            size = data[2] * 0x100 + data[3] - 9 - 24;

            //terminal_data = Encoding.ASCII.GetChars(data, 33, size);
            //terminal_uid = Encoding.ASCII.GetChars(data, 35, 12);

            Array.Copy(data, 33, AP_data, 0, size);

            if (AP_data[0] == 38 && AP_data[1] == 38)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void GetParam_Terminal_Search(ref string ap_uid, ref string terminal_uid)
        {
            ap_uid = System.Text.Encoding.Default.GetString(data, 9, 24);

            terminal_uid = System.Text.Encoding.Default.GetString(data, 33, 24);
        }

        public void GetParam_AP_UID(ref string ap_uid)
        {
            ap_uid = System.Text.Encoding.Default.GetString(data, 9, 24);
        }

        public string GetParam_Origin()
        {
            string str = System.Text.Encoding.Default.GetString(data);
            return str;
        }//for DB Send data

        public byte[] GetParam_Origin_byte()
        {
            return data;
        }

        public string GetParam_Origin_Hex()
        {
            string str = ByteToHexString(data,0,0,1,",");
            return str;
        }

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

 
        public string GetParam_Result()
        {
            int length_data = data[2] * 0x100 + data[3]-9;
            string str = ByteToHexString(data, 9, length_data,1,",");
            //string str = data[57].ToString();
            return str;
        }

        public int GetParam_Cmd_SendOrNot()
        {
            if(data[33]==1)
            {
                return 1;
            }
            else
            {
                return -1;
            }
            
        }

        //有效数据检测
        public static bool IsAvaible(byte[] data, int size)
        {
            if(data[4]!=0x06)
            {
                return false;
            }

            int len = data[2] * 0x100 + data[3]+4;

            if(len==size)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        public static bool IsTimeout(byte[] data, int size)
        {
            if (data[4] != 0x06)
            {
                return false;
            }

            int len = data[2] * 0x100 + data[3] + 4;

            if (len == size)
            {
                if(data[5]==5 && data[6]==1 && data[7]==2 && data[8]==0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        //DB 数据结果类似
        public static int RelatingMsg(APParser SendMsg,APParser ReceiveMsg)
        {
            EnumOrder order = SendMsg.GetOrder();
            string ap_uid_send = null;
            string terminal_uid_send = null;
            string ap_uid_receive = null;
            string terminal_uid_receive = null;

            if(order==EnumOrder.READER_SERACH_DOWN || order==EnumOrder.READER_CONFIG_DOWN || order==EnumOrder.READER_DELETE_DOWN)
            {
                if(SendMsg.GetOrder()!=ReceiveMsg.GetOrder())
                {
                    return -1;
                }

                SendMsg.GetParam_Terminal_Search(ref ap_uid_send, ref terminal_uid_send);
                ReceiveMsg.GetParam_Terminal_Search(ref ap_uid_receive, ref terminal_uid_receive);
                if (ap_uid_send.Equals(ap_uid_receive) && terminal_uid_send.Equals(terminal_uid_receive))
                {

                    return 1;
                }
                else
                {
                    return -1;
                }
            }//终端搜索，配置，删除配置
            else if(order==EnumOrder.READER_ORDER_DOWN)
            {
                SendMsg.GetParam_AP_UID(ref ap_uid_send);
                ReceiveMsg.GetParam_AP_UID(ref ap_uid_receive);

                if (SendMsg.GetOrder() == ReceiveMsg.GetOrder())
                {
                    
                    if (ap_uid_send.Equals(ap_uid_receive))
                    {
                        return 0;
                    }
                }//终端命令响应
                else if (ReceiveMsg.GetOrder()==EnumOrder.READER_DATA_UP)
                {
                    if (ap_uid_send.Equals(ap_uid_receive))
                    {
                        if ((SendMsg.GetTerminalOrder() == 0x84 && ReceiveMsg.GetTerminalOrder() == 0x85)
                            ||(SendMsg.GetTerminalOrder() == 0x86 && ReceiveMsg.GetTerminalOrder() == 0x87)
                            || (SendMsg.GetTerminalOrder() == 0x89 && ReceiveMsg.GetTerminalOrder() == 0x8A)
                            || (SendMsg.GetTerminalOrder() == 0x8B && ReceiveMsg.GetTerminalOrder() == 0x8C)
                            || (SendMsg.GetTerminalOrder() == 0x8D && ReceiveMsg.GetTerminalOrder() == 0x8E)
                            || (SendMsg.GetTerminalOrder() == 0x8F && ReceiveMsg.GetTerminalOrder() == 0x90)
                            || (SendMsg.GetTerminalOrder() == 0x91 && ReceiveMsg.GetTerminalOrder() == 0x92)
                            || (SendMsg.GetTerminalOrder() == 0x93 && ReceiveMsg.GetTerminalOrder() == 0x94)
                            || (SendMsg.GetTerminalOrder() == 0x95 && ReceiveMsg.GetTerminalOrder() == 0x96)
                            || (SendMsg.GetTerminalOrder() == 0x97 && ReceiveMsg.GetTerminalOrder() == 0x98))
                        {
                            return 1;
                        }
                    }
                }//终端数据返回
            }//终端命令 || 终端数据
            else if (order == EnumOrder.AP_ORDER_UPDOWN)
            {
                SendMsg.GetParam_AP_UID(ref ap_uid_send);
                ReceiveMsg.GetParam_AP_UID(ref ap_uid_receive);

                if((SendMsg.GetAPOrder()==0xA0 && ReceiveMsg.GetAPOrder()==0xA1)
                    || (SendMsg.GetAPOrder() == 0xA2 && ReceiveMsg.GetAPOrder() == 0xA3)
                    || (SendMsg.GetAPOrder() == 0xA4 && ReceiveMsg.GetAPOrder() == 0xA5))
                {
                    if (ap_uid_send.Equals(ap_uid_receive))
                    {
                        if ((SendMsg.GetAPOrder() == 0xA0 && ReceiveMsg.GetAPOrder() == 0xA1)
                            || (SendMsg.GetAPOrder() == 0xA2 && ReceiveMsg.GetAPOrder() == 0xA3)
                            || (SendMsg.GetAPOrder() == 0xA4 && ReceiveMsg.GetAPOrder() == 0xA5))
                        {
                            return 1;
                        }
                    }
                }
            }//AP命令
            return -1;
        }

        public int GetID()
        {
            return pk_id;
        }
        public void Dispose()
        {
            //Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
