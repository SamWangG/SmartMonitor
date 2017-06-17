using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonitorCore
{
    public class GUIEvent
    {
        public delegate void MsgDelegate(string RFID,string label1,string label2,string label3,string label4,string label5);
        public static event MsgDelegate Event_GUI_Get_Msg;

        public static void SendMsg(string RFID, string label1, string label2, string label3, string label4, string label5)
        {
            if(Event_GUI_Get_Msg!=null)
            {
                Event_GUI_Get_Msg(RFID, label1, label2, label3, label4, label5);
            }
        }
    }
}
