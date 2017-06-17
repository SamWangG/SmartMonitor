using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonitorCore
{
    
    class Scheduler
    {
        //List
        private List<APParser> apOrderList;//AP Order List
        //list APMSgHandler
        private List<APMsgHandler> apMsgHandlerList;
        private int size;
        public Scheduler(List<APMsgHandler> apMsgHandlerList, List<APParser> apOrderList)
        {
            this.apMsgHandlerList = apMsgHandlerList;
            this.apOrderList = apOrderList;
            
        }

        public void onWorking(object sender, EventArgs e)
        {
            int count = 0;
            size = apMsgHandlerList.Count;
            lock(apOrderList)
            {
                count = apOrderList.Count;
            }

            for (int i = 0; i < count; i++)
            {
                if (size > i)
                {
                    APMsgHandler apMsgHandler = apMsgHandlerList[i];
                    apMsgHandler.Resume(sender, e);
                }
            }
        }
    }
}
