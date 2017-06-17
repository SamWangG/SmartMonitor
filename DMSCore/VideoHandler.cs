using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MonitorCore
{
    class VideoHandler
    {
        const int iMinute=5;
        private DB_AlarmHandler database;
        private Thread VideoHandlerThread = null;
        VideoFilesManager videoFilesManager;
        private bool bStop = false;

        private int[] index = new int[3] { -1, -1, -1 };
        private string[] resultFile = new string[3];
        private int pk_id = 0;
        private DateTime time;
        public VideoHandler(int pk_id, DateTime time, DB_AlarmHandler database,VideoFilesManager videoFilesManager, int index1, int index2, int index3)
        {
            index[0] = index1;
            index[1] = index2;
            index[2] = index3;
            this.pk_id = pk_id;
            this.time = time;
            this.database = database;
            this.videoFilesManager = videoFilesManager;
            VideoHandlerThread = new Thread(new ThreadStart(VideoHandleFunc));
            this.Start();            
        }

         public void Start()
        {
            bStop = false;
            VideoHandlerThread.Start();
        }

        public void Stop()
        {
            bStop = true;
        }

        public void Resume(object sender,EventArgs e)
        {
            Log.write("AlarmHandle-Resume");
            //resumeEvent.Set();
        }

        private void VideoHandleFunc()
        {
            AlarmHandleJob(); 
        }

        private void AlarmHandleJob()
        {
            for(int i=0;i<iMinute;i++)
            {
                Thread.Sleep(60000);
            }//cut after 5 minutes
            
            Process_Alarm();
        }

        private void Process_Alarm()
        {
            try
            {
                int acc = 0;

                for (int i = 0; i < 3; i++)
                {
                    if (index[i] == -1)
                    {
                        continue;
                    }

                    if (videoFilesManager.PathExist(time, index[i]))
                    {
                        if (videoFilesManager.GetFile(time, index[i], ref resultFile[i]))
                        {
                            acc++;
                        }

                    }//get the files
                }// max:2

                Log.write("VideoHandler Process_Alarm processing:",1);
                if (acc > 0)
                {
                    string filename = System.IO.Path.GetFileName(resultFile[0]);
                    string path = resultFile[0].Substring(0, resultFile[0].Length - filename.Length);



                    for (int i = 0; i < resultFile.Length; i++)
                    {
                        if (resultFile[i] == null)
                        {
                            resultFile[i] = "";
                        }
                    }

                    if (database.SetVideoFile(pk_id, path, resultFile[0], resultFile[1], resultFile[2], ""))
                    {

                    }
                }
            }
            catch(Exception ex)
            {
                Log.write("VideoHandler Process_Alarm-Error:" + ex.Message, 0);
            }
        }
    }
}

