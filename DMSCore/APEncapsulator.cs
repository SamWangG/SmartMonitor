using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonitorCore
{
    class APEncapsulator:IDisposable
    {
        private byte[] cmd_APvalidate = { 36, 36/*头*/, 0, 48/*帧长*/, 2/*方向*/, 5, 1, 1, 0,/*命令码*/ 
                                                   1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 
                                                   25, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 
                                                   0,52/*校验和*/, 37, 37 /*尾*/};
        //private static char[] charRet_APvalidate = new char[52];

        private byte[] cmd_APStatusUpdate = { 36, 36/*头*/, 0, 33/*帧长*/, 2/*方向*/, 5, 1, 3, 0,/*命令码*/  
                                                       1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
                                                       0,37/*校验和*/, 37, 37 /*尾*/};
        private char[] charAPStatusUpdate = new char[37];

        private byte[] cmd_TerminalStatusUpdate = { 36, 36/*头*/, 0, 57/*帧长*/, 2/*方向*/, 5, 2, 5, 0,/*命令码*/ 
                                                       1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
                                                       1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
                                                       0,61/*校验和*/, 37, 37 /*尾*/};
        private char[] charTerminalStatusUpdate = new char[61];


        private byte[] cmd_TerminalDataUpload = { 36, 36/*头*/, 0, 33/*帧长*/, 2/*方向*/, 5, 2, 6, 0,/*命令码*/  
                                                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
                                                0,37/*校验和*/, 37, 37 /*尾*/};
        private char[] charTerminalDataUpload = new char[37];

        private byte[] cmd_GuardValidate = { 36, 36/*头*/, 0, 34/*帧长*/, 2/*方向*/, 5, 2, 7, 0,/*命令码*/  
                                                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
                                                2,0,38/*校验和*/, 37, 37 /*尾*/};
        private char[] charGuardValidate = new char[38];

        private byte[] cmd_HeartBeat = {36, 36/*头*/, 0, 9/*帧长*/, 2/*方向*/, 5, 1, 2, 0,/*命令码*/  
                                                0,13/*校验和*/, 37, 37 /*尾*/ };

        private byte[] cmd_GetapStatus ={ 36, 36/*头*/, 0, 33/*帧长*/, 2/*方向*/, 5, 1, 3, 0,/*命令码*/  
                                                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
                                                0,37/*校验和*/, 37, 37 /*尾*/};

        private byte[] cmd_MsgShow = { 36, 36/*头*/, 0, 119/*帧长*/, 2/*方向*/, 5, 1, 4, 0,/*命令码*/  
                                                       1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
                                                       38, 38/*头*/,0x91/*命令码*/,80/*长度*/,
                                                       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0, 0, 0,
                                                       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0, 0, 0,
                                                       0,37,/*校验和*/
                                                       0,37/*校验和*/, 37, 37 /*尾*/};

        private byte[] cmd_MsgReport = { 36, 36/*头*/, 0, 39/*帧长*/, 2/*方向*/, 5, 1, 5, 0,/*命令码*/  
                                                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
                                                38, 38/*头*/,0x90/*命令码*/,0/*长度*/,
                                                0,0xDE,/*校验和*/
                                                0,37/*校验和*/, 37, 37 /*尾*/};

        private byte[] cmd_BtnReport = { 36, 36/*头*/, 0, 39/*帧长*/, 2/*方向*/, 5, 1, 5, 0,/*命令码*/  
                                                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
                                                38, 38/*头*/,0x88/*命令码*/,0/*长度*/,
                                                0,0xDC,/*校验和*/
                                                0,37/*校验和*/, 37, 37 /*尾*/};

        private byte[] cmd_CoilAlarm = {36, 36/*头*/, 0, 54/*帧长*/, 2/*方向*/, 5, 2, 4, 0,/*命令码*/  
                                                       1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,/*AP UID*/
                                                       42, 42/*头*/,1,2,3,4,5,6,7,8,9,10,11,12/*终端UID*/,0x8C/*命令码*/,3/*长度*/,
                                                       0x01, 0x01, 0,/*告警标识*/
                                                       0xff,0xff,/*校验和*/
                                                       0xff,0xff/*校验和*/, 37, 37 /*尾*/  };

        public byte[] Response_AP_Validate(byte[] apuid, byte value)
        {
            //charRet_APvalidate = Encoding.ASCII.GetChars(cmd_APvalidate);
            for (int i = 0; i < 24; i++)
            {
                cmd_APvalidate[9 + i] = apuid[i];
            }
            cmd_APvalidate[33] = value;

           char[] time=DateTime.Now.ToString("yyyyMMddHHmmss").ToCharArray();
            for (int i = 0; i < 14;i++ )
            {
                cmd_APvalidate[34 + i] = System.Convert.ToByte(time[i]);
            }
 
            int valid_sum_recheck = 0;
            for (int i = 0; i < 47; i++)
            {
                valid_sum_recheck += cmd_APvalidate[i];
            }

            cmd_APvalidate[48] = (byte)(valid_sum_recheck >> 8);
            cmd_APvalidate[49] = (byte)valid_sum_recheck;
            return cmd_APvalidate;
        }

        /*public static char[] Response_AP_StatusUpdate(char[] apuid)
        {
            charAPStatusUpdate = Encoding.ASCII.GetChars(cmd_APStatusUpdate);
            for (int i = 0; i < 24; i++)
            {
                charAPStatusUpdate[9 + i] = apuid[i];
            }


            return charAPStatusUpdate;
        }*/

        public byte[] Response_Terminal_StatusUpdate(byte[] apuid,byte[] terminal_uid)
        {
            //charTerminalStatusUpdate = Encoding.ASCII.GetChars(cmd_TerminalStatusUpdate);
            for (int i = 0; i < 24; i++)
            {
                cmd_TerminalStatusUpdate[9 + i] = apuid[i];
            }

            for (int i = 0; i < 24; i++)
            {
                cmd_TerminalStatusUpdate[33 + i] = terminal_uid[i];
            }

            int valid_sum_recheck = 0;
            for (int i = 0; i < 56; i++)
            {
                valid_sum_recheck += cmd_TerminalStatusUpdate[i];
            }

            cmd_TerminalStatusUpdate[57] = (byte)(valid_sum_recheck >> 8);
            cmd_TerminalStatusUpdate[58] = (byte)valid_sum_recheck;

            return cmd_TerminalStatusUpdate;
        }

        public byte[] Response_Terminal_Coil_Alarm(byte[] apuid, byte[] terminal_uid,byte type_alarm,byte no_alarm,byte flag_alarm)
        {
            for (int i = 0; i < 24; i++)
            {
                cmd_CoilAlarm[9 + i] = apuid[i];
            }

            for (int i = 0; i < 12; i++)
            {
                cmd_CoilAlarm[35 + i] = terminal_uid[i];
            }

            cmd_CoilAlarm[49] = type_alarm;
            cmd_CoilAlarm[50] = no_alarm;
            cmd_CoilAlarm[51] = flag_alarm;


            int T_Check = 0;
            for(int i=0;i<19;i++)
            {
                T_Check+=cmd_CoilAlarm[33+i];
            }
            cmd_CoilAlarm[52] = (byte)(T_Check >> 8);
            cmd_CoilAlarm[53] = (byte)T_Check;

            int valid_sum_recheck = 0;
            for (int i = 0; i < 54; i++)
            {
                valid_sum_recheck += cmd_CoilAlarm[i];
            }

            cmd_CoilAlarm[54] = (byte)(valid_sum_recheck >> 8);
            cmd_CoilAlarm[55] = (byte)valid_sum_recheck;

            return cmd_CoilAlarm;
        }

        public byte[] Response_Terminal_DataUpload(byte[] apuid)
        {
            //charTerminalDataUpload = Encoding.ASCII.GetChars(cmd_TerminalDataUpload);
            for (int i = 0; i < 24; i++)
            {
                cmd_TerminalDataUpload[9 + i] = apuid[i];
            }

            int valid_sum_recheck = 0;
            for (int i = 0; i < 32; i++)
            {
                valid_sum_recheck += cmd_TerminalDataUpload[i];
            }

            cmd_TerminalDataUpload[33] = (byte)(valid_sum_recheck >> 8);
            cmd_TerminalDataUpload[34] = (byte)valid_sum_recheck;

            return cmd_TerminalDataUpload;
        }

        public byte[] Response_Guard_Validate(byte[] guarduid,bool IsAuthorized)
        {
            if(IsAuthorized)
            {
                cmd_GuardValidate[33] = 1;
            }
            else
            {
                cmd_GuardValidate[33] = 2;
            }
            //charGuardValidate = Encoding.ASCII.GetChars(cmd_GuardValidate);
            for (int i = 0; i < 24; i++)
            {
                cmd_GuardValidate[9 + i] = guarduid[i];
            }

            int valid_sum_recheck = 0;
            for (int i = 0; i < 33; i++)
            {
                valid_sum_recheck += cmd_GuardValidate[i];
            }

            cmd_GuardValidate[34] = (byte)(valid_sum_recheck >> 8);
            cmd_GuardValidate[35] = (byte)valid_sum_recheck;

            return cmd_GuardValidate;
        }

        public byte[] HeartBeat()
        {
            return cmd_HeartBeat;
        }

        public byte[] GetApStatus(byte[] apuid)
        {
            for (int i = 0; i < 24; i++)
            {
                cmd_GetapStatus[9 + i] = apuid[i];
            }

            int valid_sum_recheck = 0;
            for (int i = 0; i < 32; i++)
            {
                valid_sum_recheck += cmd_GetapStatus[i];
            }

            cmd_GetapStatus[33] = (byte)(valid_sum_recheck >> 8);
            cmd_GetapStatus[34] = (byte)valid_sum_recheck;

            return cmd_GetapStatus;
        }

        public byte[] Response_AP_Show_Msg(byte[] ap_id,byte[] label,byte status,byte[] size,byte[] data1,byte[] data2,byte[] data3,byte[] data4)
        {
            if(size.Length!=4)
            {
                return null;
            }
            //长度
            byte[] cmd_AP_Show_Msg = new byte[37 + 19 + size[0] + size[1] + size[2] + size[3]];
            //头
            cmd_AP_Show_Msg[0] = 36;
            cmd_AP_Show_Msg[1] = 36;
            //帧长
            int length=size[0] + size[1] + size[2] + size[3]+33+19;

            cmd_AP_Show_Msg[2] = (byte)(length>>8);
            cmd_AP_Show_Msg[3] = (byte)(length);

            //方向
            cmd_AP_Show_Msg[4] = 0x02;
            //命令码
            cmd_AP_Show_Msg[5] = 0x05;
            cmd_AP_Show_Msg[6] = 0x01;
            cmd_AP_Show_Msg[7] = 0x05;
            cmd_AP_Show_Msg[8] = 0x00;
            //AP UID
            for(int i=0;i<24;i++)
            {
                cmd_AP_Show_Msg[9 + i] = ap_id[i];
            }
            //头
            cmd_AP_Show_Msg[33] = 38;
            cmd_AP_Show_Msg[34] = 38;
            //命令
            cmd_AP_Show_Msg[35] = 0x82;
            //长度
            cmd_AP_Show_Msg[36] = (byte)(13 + size[0] + size[1] + size[2] + size[3]);
            //标签号
            for (int i = 0; i < 8; i++)
            {
                cmd_AP_Show_Msg[37 + i] = label[i];
            }

            cmd_AP_Show_Msg[45] = status;
            cmd_AP_Show_Msg[46] = size[0];
            cmd_AP_Show_Msg[47] = size[1];
            cmd_AP_Show_Msg[48] = size[2];
            cmd_AP_Show_Msg[49] = size[3];
            //内容
            for(int i=0;i<size[0];i++)
            {
                cmd_AP_Show_Msg[50 + i] = data1[i];
            }

            for (int i = 0; i < size[1]; i++)
            {
                cmd_AP_Show_Msg[50 + size[0] + i] = data2[i];
            }

            for (int i = 0; i < size[2]; i++)
            {
                cmd_AP_Show_Msg[50 + size[0] + size[1] + i] = data3[i];
            }

            for (int i = 0; i < size[3]; i++)
            {
                cmd_AP_Show_Msg[50 + size[0] + size[1] + size[2] + i] = data4[i];
            }
            //校验1
            int data_check = 0;
            for(int i=0;i<17 + size[0] + size[1] + size[2] + size[3];i++)
            {
                data_check += cmd_AP_Show_Msg[33 + i];
            }
            
            int T_datasize = cmd_AP_Show_Msg[36] + 33 + 4; ;
            cmd_AP_Show_Msg[T_datasize] =(byte) (data_check>>8);
            cmd_AP_Show_Msg[T_datasize+1] = (byte)(data_check);
            //校验2
            int data_check1 = 0;
            for(int i=0;i<length;i++)
            {
                data_check1 += cmd_AP_Show_Msg[i];
            }

            cmd_AP_Show_Msg[length] = (byte)(data_check1 >> 8);
            cmd_AP_Show_Msg[length + 1] = (byte)(data_check1);

            cmd_AP_Show_Msg[length + 2] =37;
            cmd_AP_Show_Msg[length + 3] = 37;

            return cmd_AP_Show_Msg;
        }

        public byte[] Response_Lot_Add(byte[] ap_id, byte[] label, byte status, byte size, byte[] data1)
        {
            //长度
            byte[] cmd_AP_Lot_Add = new byte[37 + 16 + size];
            //头
            cmd_AP_Lot_Add[0] = 36;
            cmd_AP_Lot_Add[1] = 36;
            //帧长
            int length = size + 33 + 15;

            cmd_AP_Lot_Add[2] = (byte)(length >> 8);
            cmd_AP_Lot_Add[3] = (byte)(length);

            //方向
            cmd_AP_Lot_Add[4] = 0x02;
            //命令码
            cmd_AP_Lot_Add[5] = 0x05;
            cmd_AP_Lot_Add[6] = 0x01;
            cmd_AP_Lot_Add[7] = 0x05;
            cmd_AP_Lot_Add[8] = 0x00;
            //AP UID
            for (int i = 0; i < 24; i++)
            {
                cmd_AP_Lot_Add[9 + i] = ap_id[i];
            }
            //头
            cmd_AP_Lot_Add[33] = 38;
            cmd_AP_Lot_Add[34] = 38;
            //命令
            cmd_AP_Lot_Add[35] = 0x84;
            //长度
            cmd_AP_Lot_Add[36] = (byte)(10+ size);
            //标签号
            for (int i = 0; i < 8; i++)
            {
                cmd_AP_Lot_Add[37 + i] = label[i];
            }

            cmd_AP_Lot_Add[45] = status;
            cmd_AP_Lot_Add[46] = size;

            //内容
            for (int i = 0; i < size; i++)
            {
                cmd_AP_Lot_Add[47 + i] = data1[i];
            }
            //校验1
            int data_check = 0;
            for (int i = 0; i < 14 + size; i++)
            {
                data_check += cmd_AP_Lot_Add[33 + i];
            }

            int T_datasize = cmd_AP_Lot_Add[36] + 33 + 4 ;
            cmd_AP_Lot_Add[T_datasize] = (byte)(data_check >> 8);
            cmd_AP_Lot_Add[T_datasize + 1] = (byte)(data_check);
            //校验2
            int data_check1 = 0;
            for (int i = 0; i < length; i++)
            {
                data_check1 += cmd_AP_Lot_Add[i];
            }

            cmd_AP_Lot_Add[length] = (byte)(data_check1 >> 8);
            cmd_AP_Lot_Add[length + 1] = (byte)(data_check1);
            cmd_AP_Lot_Add[length + 2] = 37;
            cmd_AP_Lot_Add[length + 3] = 37;

            return cmd_AP_Lot_Add;
        }

        public byte[] Response_Lot_Decrease(byte[] ap_id, byte[] label, byte status, byte size, byte[] data1)
        {
            //长度
            byte[] cmd_AP_Lot_Decrease = new byte[37 + 16 + size];
            //头
            cmd_AP_Lot_Decrease[0] = 36;
            cmd_AP_Lot_Decrease[1] = 36;
            //帧长
            int length = size + 33 + 15;

            cmd_AP_Lot_Decrease[2] = (byte)(length >> 8);
            cmd_AP_Lot_Decrease[3] = (byte)(length);

            //方向
            cmd_AP_Lot_Decrease[4] = 0x02;
            //命令码
            cmd_AP_Lot_Decrease[5] = 0x05;
            cmd_AP_Lot_Decrease[6] = 0x01;
            cmd_AP_Lot_Decrease[7] = 0x05;
            cmd_AP_Lot_Decrease[8] = 0x00;
            //AP UID
            for (int i = 0; i < 24; i++)
            {
                cmd_AP_Lot_Decrease[9 + i] = ap_id[i];
            }
            //头
            cmd_AP_Lot_Decrease[33] = 38;
            cmd_AP_Lot_Decrease[34] = 38;
            //命令
            cmd_AP_Lot_Decrease[35] = 0x86;
            //长度
            cmd_AP_Lot_Decrease[36] = (byte)(10 + size);
            //标签号
            for (int i = 0; i < 8; i++)
            {
                cmd_AP_Lot_Decrease[37 + i] = label[i];
            }

            cmd_AP_Lot_Decrease[45] = status;
            cmd_AP_Lot_Decrease[46] = size;

            //内容
            for (int i = 0; i < size; i++)
            {
                cmd_AP_Lot_Decrease[47+ i] = data1[i];
            }
            //校验1
            int data_check = 0;
            for (int i = 0; i < 14 + size; i++)
            {
                data_check += cmd_AP_Lot_Decrease[33 + i];
            }

            int T_datasize = cmd_AP_Lot_Decrease[36] + 33 + 4;
            cmd_AP_Lot_Decrease[T_datasize] = (byte)(data_check >> 8);
            cmd_AP_Lot_Decrease[T_datasize + 1] = (byte)(data_check);
            //校验2
            int data_check1 = 0;
            for (int i = 0; i < length; i++)
            {
                data_check1 += cmd_AP_Lot_Decrease[i];
            }
            cmd_AP_Lot_Decrease[length] = (byte)(data_check1 >> 8);
            cmd_AP_Lot_Decrease[length + 1] = (byte)(data_check1);
            cmd_AP_Lot_Decrease[length + 2] = 37;
            cmd_AP_Lot_Decrease[length + 3] = 37;

            return cmd_AP_Lot_Decrease;
        }


        public byte[] Response_Msg_Show(byte[] ap_uid, byte[] msg1,byte[] msg2,byte[] msg3,byte[] msg4)
        {

            for (int i = 0; i < 24; i++)
            {
                cmd_MsgShow[9 + i] = ap_uid[i];
            }

            for (int i = 0; i < 20; i++)
            {
                if(msg1==null || msg1.Length-1<i)
                {
                    cmd_MsgShow[37 + i] = 0x20;
                }
                else
                {
                    cmd_MsgShow[37 + i] = msg1[i];
                }
                
            }

            for (int i = 0; i < 20; i++)
            {
                if (msg2 == null || msg2.Length - 1 < i)
                {
                    cmd_MsgShow[57 + i] = 0x20;
                }
                else
                {
                    cmd_MsgShow[57 + i] = msg2[i];
                }
            }

            for (int i = 0; i < 20; i++)
            {
                if (msg3 == null || msg3.Length - 1 < i)
                {
                    cmd_MsgShow[77 + i] = 0x20;
                }
                else
                {
                    cmd_MsgShow[77 + i] = msg3[i];
                }
            }

            for (int i = 0; i < 20; i++)
            {
                if (msg4 == null || msg4.Length - 1 < i)
                {
                    cmd_MsgShow[97 + i] = 0x20;
                }
                else
                {
                    cmd_MsgShow[97 + i] = msg4[i];
                }
            }

            int valid_sum_recheck = 0;
            for (int i = 0; i < 84; i++)
            {
                valid_sum_recheck += cmd_MsgShow[33+i];
            }

            cmd_MsgShow[117] = (byte)(valid_sum_recheck >> 8);
            cmd_MsgShow[118] = (byte)valid_sum_recheck;


            valid_sum_recheck = 0;
            for (int i = 0; i < 119; i++)
            {
                valid_sum_recheck += cmd_MsgShow[i];
            }

            cmd_MsgShow[119] = (byte)(valid_sum_recheck >> 8);
            cmd_MsgShow[120] = (byte)valid_sum_recheck;
            return cmd_MsgShow;
        }

        public byte[] Response_Msg_Report(byte[] ap_uid)
        {
            for (int i = 0; i < 24; i++)
            {
                cmd_MsgReport[9 + i] = ap_uid[i];
            }

            int valid_sum_recheck = 0;
            for (int i = 0; i < 39; i++)
            {
                valid_sum_recheck += cmd_MsgReport[i];
            }

            cmd_MsgReport[39] = (byte)(valid_sum_recheck >> 8);
            cmd_MsgReport[40] = (byte)valid_sum_recheck;

            return cmd_MsgReport;
        }


        public byte[] Response_Button_Report(byte[] ap_uid)
        {
            for (int i = 0; i < 24; i++)
            {
                cmd_BtnReport[9 + i] = ap_uid[i];
            }

            int valid_sum_recheck = 0;
            for (int i = 0; i < 39; i++)
            {
                valid_sum_recheck += cmd_BtnReport[i];
            }

            cmd_BtnReport[39] = (byte)(valid_sum_recheck >> 8);
            cmd_BtnReport[40] = (byte)valid_sum_recheck;

            return cmd_BtnReport;
        }

        public byte[] Response_Mobile_Msg_Show(byte[] ap_uid,byte[] data,byte status)
        {
            int dataLength = 0;
            if (data == null)
            {
                dataLength = 0;
            }
            else
            {
                dataLength = data.Length;
            }
            //长度
            byte[] cmd_Mobile_Msg_Show = new byte[37 + 5 + dataLength];
            //头
            cmd_Mobile_Msg_Show[0] = 36;
            cmd_Mobile_Msg_Show[1] = 36;
            //帧长
            int length = dataLength + 33 + 5;

            cmd_Mobile_Msg_Show[2] = (byte)(length >> 8);
            cmd_Mobile_Msg_Show[3] = (byte)(length);

            //方向
            cmd_Mobile_Msg_Show[4] = 0x02;
            //命令码
            cmd_Mobile_Msg_Show[5] = 0x05;
            cmd_Mobile_Msg_Show[6] = 0x01;
            cmd_Mobile_Msg_Show[7] = 0x05;
            cmd_Mobile_Msg_Show[8] = 0x00;
            //AP UID
            for (int i = 0; i < 24; i++)
            {
                cmd_Mobile_Msg_Show[9 + i] = ap_uid[i];
            }
            //头
            cmd_Mobile_Msg_Show[33] = 38;
            cmd_Mobile_Msg_Show[34] = 38;
            //命令
            cmd_Mobile_Msg_Show[35] = 0x94;
            //长度
            cmd_Mobile_Msg_Show[36] = (byte)(1 + dataLength);
            //状态
            cmd_Mobile_Msg_Show[37] = status;

            for (int i = 0; i < dataLength; i++)
            {
                cmd_Mobile_Msg_Show[38 + i] = data[i];
            }
                
            //校验2
            int data_check1 = 0;
            for (int i = 0; i < length; i++)
            {
                data_check1 += cmd_Mobile_Msg_Show[i];
            }
            cmd_Mobile_Msg_Show[length] = (byte)(data_check1 >> 8);
            cmd_Mobile_Msg_Show[length + 1] = (byte)(data_check1);

            cmd_Mobile_Msg_Show[length + 2] = 37;
            cmd_Mobile_Msg_Show[length + 3] = 37;

            return cmd_Mobile_Msg_Show;
        }

        public byte[] Response_Mobile_Msg_Show1(byte[] ap_uid, byte[] data, byte status)
        {
            int dataLength = 0;
            if(data==null)
            {
                dataLength = 0;
            }
            else
            {
                dataLength = data.Length;
            }
            //长度
            byte[] cmd_Mobile_Msg_Show = new byte[37 + 5 + dataLength];
            //头
            cmd_Mobile_Msg_Show[0] = 36;
            cmd_Mobile_Msg_Show[1] = 36;
            //帧长
            int length = dataLength + 33 + 5;

            cmd_Mobile_Msg_Show[2] = (byte)(length >> 8);
            cmd_Mobile_Msg_Show[3] = (byte)(length);

            //方向
            cmd_Mobile_Msg_Show[4] = 0x02;
            //命令码
            cmd_Mobile_Msg_Show[5] = 0x05;
            cmd_Mobile_Msg_Show[6] = 0x01;
            cmd_Mobile_Msg_Show[7] = 0x05;
            cmd_Mobile_Msg_Show[8] = 0x00;
            //AP UID
            for (int i = 0; i < 24; i++)
            {
                cmd_Mobile_Msg_Show[9 + i] = ap_uid[i];
            }
            //头
            cmd_Mobile_Msg_Show[33] = 38;
            cmd_Mobile_Msg_Show[34] = 38;
            //命令
            cmd_Mobile_Msg_Show[35] = 0x96;
            //长度
            cmd_Mobile_Msg_Show[36] = (byte)(1 + dataLength);
            //状态
            cmd_Mobile_Msg_Show[37] = status;

            for (int i = 0; i < dataLength; i++)
            {
                cmd_Mobile_Msg_Show[38 + i] = data[i];
            }

            //校验2
            int data_check1 = 0;
            for (int i = 0; i < length; i++)
            {
                data_check1 += cmd_Mobile_Msg_Show[i];
            }
            cmd_Mobile_Msg_Show[length] = (byte)(data_check1 >> 8);
            cmd_Mobile_Msg_Show[length + 1] = (byte)(data_check1);

            cmd_Mobile_Msg_Show[length + 2] = 37;
            cmd_Mobile_Msg_Show[length + 3] = 37;

            return cmd_Mobile_Msg_Show;
        }


        public byte[] Response_Mobile_Lot_Operate(byte[] ap_uid, byte[] data, byte status)
        {
            int dataLength = 0;
            if (data == null)
            {
                dataLength = 0;
            }
            else
            {
                dataLength = data.Length;
            }
            //长度
            byte[] cmd_Mobile_Msg_Show = new byte[37 + 5 + dataLength];
            //头
            cmd_Mobile_Msg_Show[0] = 36;
            cmd_Mobile_Msg_Show[1] = 36;
            //帧长
            int length = dataLength + 33 + 5;

            cmd_Mobile_Msg_Show[2] = (byte)(length >> 8);
            cmd_Mobile_Msg_Show[3] = (byte)(length);

            //方向
            cmd_Mobile_Msg_Show[4] = 0x02;
            //命令码
            cmd_Mobile_Msg_Show[5] = 0x05;
            cmd_Mobile_Msg_Show[6] = 0x01;
            cmd_Mobile_Msg_Show[7] = 0x05;
            cmd_Mobile_Msg_Show[8] = 0x00;
            //AP UID
            for (int i = 0; i < 24; i++)
            {
                cmd_Mobile_Msg_Show[9 + i] = ap_uid[i];
            }
            //头
            cmd_Mobile_Msg_Show[33] = 38;
            cmd_Mobile_Msg_Show[34] = 38;
            //命令
            cmd_Mobile_Msg_Show[35] = 0x98;
            //长度
            cmd_Mobile_Msg_Show[36] = (byte)(1 + dataLength);
            //状态
            cmd_Mobile_Msg_Show[37] = status;

            for (int i = 0; i < dataLength; i++)
            {
                cmd_Mobile_Msg_Show[38 + i] = data[i];
            }

            //校验2
            int data_check1 = 0;
            for (int i = 0; i < length; i++)
            {
                data_check1 += cmd_Mobile_Msg_Show[i];
            }
            cmd_Mobile_Msg_Show[length] = (byte)(data_check1 >> 8);
            cmd_Mobile_Msg_Show[length + 1] = (byte)(data_check1);

            cmd_Mobile_Msg_Show[length + 2] = 37;
            cmd_Mobile_Msg_Show[length + 3] = 37;

            return cmd_Mobile_Msg_Show;
        }



        public void Dispose()
        {
            //Dispose(); 
            GC.SuppressFinalize(this);
        }
    }
}
