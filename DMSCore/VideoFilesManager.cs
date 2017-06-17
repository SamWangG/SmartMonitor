using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using System.Diagnostics;

namespace MonitorCore
{
    public class FileComparer : IComparer
    {
        int IComparer.Compare(Object o1, Object o2)
        {
            FileInfo fi1 = o1 as FileInfo;
            FileInfo fi2 = o2 as FileInfo;
            return fi2.CreationTime.CompareTo(fi1.CreationTime);
        }
    }
    struct VideoFile
    {
        public string fileName;
        public DateTime startTime;
        public DateTime endTime;
        public bool bRecording;
    }
    class VideoFilesManager
    {
        private const int CONST_INTERVAL = 300;
        private VideoFile[] videoFile;
        private VideoFile[] videoFile_Yesterday;
        private string folderPath;
        private string IP;
        private string Port;
        private string dstPath;
        private bool bTodayExist = false;
        private bool bYesterdayExist=false;

        //for thread
        private Thread apHandlerThread = null;
        ManualResetEvent resumeEvent = new ManualResetEvent(false);
        private bool bStop = false;
        bool isExist = true;
        bool isStatusUpdate = false;
        //database
        private DB_Core database;
        public VideoFilesManager(DB_Core database, string folderPath, string dstPath, string IP, int Port)
        {
            this.database = database;
            this.folderPath = folderPath + "\\RecordFile";
            this.IP = IP;
            this.Port = Port.ToString();
            this.dstPath = dstPath;

            apHandlerThread = new Thread(new ThreadStart(VideoMonitorFunc));
        }

        public bool PathExist(DateTime time,int cameraIndex)
        {
            string strDate=time.ToString("yyyyMMdd");
            string datePath = folderPath + "\\" + strDate;

            string dstPath = datePath + "\\" + IP + "_" + Port + "_" + cameraIndex.ToString() + "_" + cameraIndex.ToString() + "_-1";

            if (!Directory.Exists(dstPath))
            {
                return false;
            }
            return true;
        }
        //Get all videoFiles' time message
        private bool UpdateAll(DateTime time,int cameraIndex)
        {
            string strDate=time.ToString("yyyyMMdd");

            string datePath = folderPath + "\\" + strDate;
            string datePatth_yesterday = folderPath + "\\" + time.AddDays(-1).ToString("yyyyMMdd");

            string dstPath = datePath + "\\" + IP + "_" + Port + "_" + cameraIndex.ToString() + "_" + cameraIndex.ToString() + "_-1";
            string dstPath_yesterday = datePatth_yesterday + "\\" + IP + "_" + Port + "_" + cameraIndex.ToString() + "_" + cameraIndex.ToString() + "_-1";

            if (!Directory.Exists(dstPath))
            {
                bTodayExist = false;
            }
            else
            {
                bTodayExist = true;
            }

            if (!Directory.Exists(dstPath_yesterday))
            {
                bYesterdayExist = false;
            }
            else
            {
                bYesterdayExist = true;
            }

            if(bTodayExist)
            {
                DirectoryInfo Folder = new DirectoryInfo(dstPath);

                int i = 0;

                FileInfo[] fileInfo = Folder.GetFiles("*.mp4");

                FileComparer fc = new FileComparer();
                Array.Sort(fileInfo, fc);//排序

                videoFile = new VideoFile[fileInfo.Length];

                foreach (FileInfo NextFile in fileInfo)
                {
                    if (NextFile.Extension != ".mp4")
                    {
                        continue;
                    }
                    videoFile[i].fileName = dstPath + "\\" + NextFile.Name;

                    if (NextFile.Name.Length > 30)
                    {
                        int year_start = Convert.ToInt32(NextFile.Name.Substring(9, 4));
                        int month_start = Convert.ToInt32(NextFile.Name.Substring(13, 2));
                        int day_start = Convert.ToInt32(NextFile.Name.Substring(15, 2));
                        int hour_start = Convert.ToInt32(NextFile.Name.Substring(17, 2));
                        int minute_start = Convert.ToInt32(NextFile.Name.Substring(19, 2));
                        int second_start = Convert.ToInt32(NextFile.Name.Substring(21, 2));

                        int year_end = Convert.ToInt32(NextFile.Name.Substring(24, 4));
                        int month_end = Convert.ToInt32(NextFile.Name.Substring(28, 2));
                        int day_end = Convert.ToInt32(NextFile.Name.Substring(30, 2));
                        int hour_end = Convert.ToInt32(NextFile.Name.Substring(32, 2));
                        int minute_end = Convert.ToInt32(NextFile.Name.Substring(34, 2));
                        int second_end = Convert.ToInt32(NextFile.Name.Substring(36, 2));
                        double Framesize = VideoIncise.FrameSize(videoFile[i].fileName);
                        double FrameCount = VideoIncise.FrameCount(videoFile[i].fileName);
                        double second_add = FrameCount / Framesize ;
                        videoFile[i].startTime = new DateTime(year_start, month_start, day_start, hour_start, minute_start, second_start);
                        videoFile[i].endTime = new DateTime(year_end, month_end, day_end, hour_end, minute_end, second_end);
                        videoFile[i].bRecording = false;
                    }//已经录好的文件
                    else
                    {
                        int year_start = Convert.ToInt32(NextFile.Name.Substring(0, 4));
                        int month_start = Convert.ToInt32(NextFile.Name.Substring(4, 2));
                        int day_start = Convert.ToInt32(NextFile.Name.Substring(6, 2));
                        int hour_start = Convert.ToInt32(NextFile.Name.Substring(9, 2));
                        int minute_start = Convert.ToInt32(NextFile.Name.Substring(11, 2));
                        int second_start = Convert.ToInt32(NextFile.Name.Substring(13, 2));
                        videoFile[i].startTime = new DateTime(year_start, month_start, day_start, hour_start, minute_start, second_start);
                        videoFile[i].bRecording = true;
                        double Framesize = VideoIncise.FrameSize(videoFile[i].fileName);
                        double FrameCount = VideoIncise.FrameCount(videoFile[i].fileName);


                        if (File.Exists(videoFile[i].fileName))
                        {
                            FileInfo fileInfo1 = new FileInfo(videoFile[i].fileName);

                            videoFile[i].endTime = fileInfo1.LastWriteTime;
                        }
                        
                        //videoFile[i].startTime.AddSeconds(second_add);
                    }//正在录取
                    i++;
                }
            }

            if(bYesterdayExist)
            {
                DirectoryInfo Folder = new DirectoryInfo(dstPath_yesterday);

                int i = 0;

                FileInfo[] fileInfo = Folder.GetFiles("*.mp4");
                FileComparer fc = new FileComparer();
                Array.Sort(fileInfo, fc);//排序
                videoFile_Yesterday = new VideoFile[fileInfo.Length];

                foreach (FileInfo NextFile in fileInfo)
                {
                    if (NextFile.Extension != ".mp4")
                    {
                        continue;
                    }
                    videoFile_Yesterday[i].fileName = dstPath_yesterday + "\\" + NextFile.Name;

                    if (NextFile.Name.Length > 30)
                    {
                        int year_start = Convert.ToInt32(NextFile.Name.Substring(9, 4));
                        int month_start = Convert.ToInt32(NextFile.Name.Substring(13, 2));
                        int day_start = Convert.ToInt32(NextFile.Name.Substring(15, 2));
                        int hour_start = Convert.ToInt32(NextFile.Name.Substring(17, 2));
                        int minute_start = Convert.ToInt32(NextFile.Name.Substring(19, 2));
                        int second_start = Convert.ToInt32(NextFile.Name.Substring(21, 2));

                        int year_end = Convert.ToInt32(NextFile.Name.Substring(24, 4));
                        int month_end = Convert.ToInt32(NextFile.Name.Substring(28, 2));
                        int day_end = Convert.ToInt32(NextFile.Name.Substring(30, 2));
                        int hour_end = Convert.ToInt32(NextFile.Name.Substring(32, 2));
                        int minute_end = Convert.ToInt32(NextFile.Name.Substring(34, 2));
                        int second_end = Convert.ToInt32(NextFile.Name.Substring(36, 2));
                        videoFile_Yesterday[i].startTime = new DateTime(year_start, month_start, day_start, hour_start, minute_start, second_start);
                        videoFile_Yesterday[i].endTime = new DateTime(year_end, month_end, day_end, hour_end, minute_end, second_end);
                        videoFile_Yesterday[i].bRecording = false;
                    }//已经录好的文件
                    else
                    {
                        int year_start = Convert.ToInt32(NextFile.Name.Substring(0, 4));
                        int month_start = Convert.ToInt32(NextFile.Name.Substring(4, 2));
                        int day_start = Convert.ToInt32(NextFile.Name.Substring(6, 2));
                        int hour_start = Convert.ToInt32(NextFile.Name.Substring(9, 2));
                        int minute_start = Convert.ToInt32(NextFile.Name.Substring(11, 2));
                        int second_start = Convert.ToInt32(NextFile.Name.Substring(13, 2));
                        videoFile_Yesterday[i].startTime = new DateTime(year_start, month_start, day_start, hour_start, minute_start, second_start);
                        videoFile_Yesterday[i].bRecording = true;
                        if(File.Exists(videoFile_Yesterday[i].fileName))
                        {
                            FileInfo fileInfo2 = new FileInfo(videoFile_Yesterday[i].fileName);

                            videoFile_Yesterday[i].endTime = fileInfo2.LastWriteTime;
                        }
                        
                        //videoFile_Yesterday[i].endTime = videoFile_Yesterday[i].startTime.AddSeconds(second_add);
                    }//正在录取
                    i++;
                }
            }
            if(bTodayExist ||bYesterdayExist)
            {
                return true;
            }
            return false;
        }

        //Get needed file according to time&camera index.
        public bool GetFile(DateTime currentTime, int cameraIndex, ref string file_ResultName)
        {
            try 
            {
                if (!UpdateAll(currentTime, cameraIndex))
                {
                    return false;
                }

                int[] file_index_today = new int[] { -1, -1, -1 };
                int[] nStartPt_today = new int[] { -1, -1, -1 };
                int[] nLength_today = new int[] { -1, -1, -1 };
                int index_today = 0;

                int[] file_index_yesterday = new int[] { -1, -1, -1 };
                int[] nStartPt_yesterday = new int[] { -1, -1, -1 };
                int[] nLength_yesterday = new int[] { -1, -1, -1 };
                int index_yesterday = 0;

                bool bFirstFile = false;
                bool bSecondFile = false;
                VideoIncise videoIncise = new VideoIncise();
                string dstFolder = dstPath + "\\" + currentTime.ToString("yyyyMMdd");
                if (!Directory.Exists(dstFolder))
                {
                    Directory.CreateDirectory(dstFolder);
                }

                string dstFile = dstFolder + "\\" + currentTime.ToString("yyyyMMddHHmmss") + "_" + cameraIndex.ToString() + ".mp4";
                string dstFile_tmp1 = dstFolder + "\\" + currentTime.ToString("mmss") + cameraIndex.ToString() + "tmp1.mp4";
                string dstFile_tmp2 = dstFolder + "\\" + currentTime.ToString("mmss") + cameraIndex.ToString() + "tmp2.mp4";


                if (bYesterdayExist)
                {
                    for (int i = 0; i < videoFile_Yesterday.Length; i++)
                    {
                        int nStartPt = 0;
                        int nLength = 0;
                        if (IsValidFile(videoFile_Yesterday[i], currentTime, ref nStartPt, ref nLength))
                        {
                            file_index_yesterday[index_yesterday] = i;
                            nStartPt_yesterday[index_yesterday] = nStartPt;
                            nLength_yesterday[index_yesterday++] = nLength;
                            if (index_yesterday > file_index_yesterday.Length)
                            {
                                break;
                            }
                        }

                    }
                }

                if (bTodayExist)
                {
                    for (int i = 0; i < videoFile.Length; i++)
                    {
                        int nStartPt = 0;
                        int nLength = 0;
                        if (IsValidFile(videoFile[i], currentTime, ref nStartPt, ref nLength))
                        {
                            file_index_today[index_today] = i;
                            nStartPt_today[index_today] = nStartPt;
                            nLength_today[index_today++] = nLength;
                            if (index_today > file_index_today.Length)
                            {
                                break;
                            }
                        }

                    }
                }


                for (int i = 0; i < file_index_yesterday.Length; i++)
                {
                    if (file_index_yesterday[i] == -1)
                    {
                        break;
                    }

                    if (!bFirstFile)
                    {
                        /*if (videoIncise.open(videoFile_Yesterday[i].fileName, dstFile_tmp1))
                        {
                            int FrameSize = (int)videoIncise.FrameSize();
                            videoIncise.Combine(nStartPt_yesterday[i] * FrameSize, nLength_yesterday[i] * FrameSize);
                            bFirstFile = true;
                        }*/
                        FFMEPG ffmpeg = new FFMEPG();
                        TimeSpan sp1 = new TimeSpan(0, 0, nStartPt_yesterday[i]);
                        TimeSpan sp2 = new TimeSpan(0, 0, nStartPt_yesterday[i] + nLength_yesterday[i]);
                        ffmpeg.Cut(videoFile_Yesterday[i].fileName, dstFile_tmp1, sp1, sp2);
                        bFirstFile = true;
                    }//first File
                    else
                    {
                        /*if (videoIncise.open(dstFile_tmp1, videoFile_Yesterday[i].fileName, dstFile_tmp2))
                        {
                            int frameCount=(int)VideoIncise.FrameCount(dstFile);
                            int FrameSize = (int)videoIncise.FrameSize();
                            videoIncise.Combine(0, frameCount, nStartPt_yesterday[i] * FrameSize, nLength_yesterday[i] * FrameSize);
                        }*/
                        FFMEPG ffmpeg = new FFMEPG();
                        TimeSpan sp1 = new TimeSpan(0, 0, nStartPt_yesterday[i]);
                        TimeSpan sp2 = new TimeSpan(0, 0, nStartPt_yesterday[i] + nLength_yesterday[i]);
                        ffmpeg.Cut(videoFile_Yesterday[file_index_yesterday[i]].fileName, dstFile_tmp2, sp1, sp2);
                        ffmpeg.Combine(dstFile_tmp2, dstFile_tmp1, dstFile_tmp2);
                        bSecondFile = true;
                    }
                }

                for (int i = 0; i < file_index_today.Length; i++)
                {
                    if (file_index_today[i] == -1)
                    {
                        break;
                    }

                    if (!bFirstFile)
                    {
                        /*if (videoIncise.open(videoFile[i].fileName, dstFile_tmp1))
                        {
                            int FrameSize = (int)videoIncise.FrameSize();
                            videoIncise.Combine(nStartPt_today[i] * FrameSize, nLength_today[i] * FrameSize);
                            bFirstFile = true;
                        }*/
                        TimeSpan sp1 = new TimeSpan(0, 0, nStartPt_today[i]);
                        TimeSpan sp2 = new TimeSpan(0, 0, nStartPt_today[i] + nLength_today[i]);
                        FFMEPG ffmpeg = new FFMEPG();

                        ffmpeg.Cut(videoFile[file_index_today[i]].fileName, dstFile_tmp1, sp1, sp2);
                        bFirstFile = true;
                    }//first File
                    else
                    {
                        /*if (videoIncise.open(dstFile_tmp1, videoFile[i].fileName, dstFile_tmp2))
                        {
                            int frameCount = (int)VideoIncise.FrameCount(dstFile);
                            int FrameSize = (int)videoIncise.FrameSize();
                            videoIncise.Combine(0, frameCount, nStartPt_today[i] * FrameSize, nLength_today[i] * FrameSize);
                        }*/
                        FFMEPG ffmpeg = new FFMEPG();
                        TimeSpan sp1 = new TimeSpan(0, 0, nStartPt_today[i]);
                        TimeSpan sp2 = new TimeSpan(0, 0, nStartPt_today[i] + nLength_today[i]);
                        ffmpeg.Cut(videoFile[file_index_today[i]].fileName, dstFile_tmp2, sp1, sp2);
                        ffmpeg.Combine(dstFile_tmp2, dstFile_tmp1, dstFile_tmp2);
                        bSecondFile = true;
                    }
                }


                if (!bFirstFile)
                {
                    return false;
                }

                if (bSecondFile)
                {
                    File.Copy(dstFile_tmp2, dstFile, true);

                    File.Delete(dstFile_tmp1);
                    File.Delete(dstFile_tmp2);
                }
                else
                {
                    if (!File.Exists(dstFile_tmp1))
                    {
                        return false;
                    }
                    File.Copy(dstFile_tmp1, dstFile, true);
                    File.Delete(dstFile_tmp1);
                }

                file_ResultName = dstFile;
                return true;
            }
            catch
            {
                return false;
            }
        }

        //check if the file is valid.
        public bool IsValidFile(VideoFile videoFile, DateTime currentTime, ref int nStartPt, ref int nLength)
        {
            int nStartPt_file2 = 0;
            int nLength_file2 = 0;

            long nRealTime = currentTime.Ticks / 10000000;

            long nStart = videoFile.startTime.Ticks / 10000000;
            long nEnd = videoFile.endTime.Ticks / 10000000;

            long nInterval_real_start = nStart - nRealTime;
            long nInterval_real_end = nEnd - nRealTime;
            long kk=nStart - nRealTime;
            if (nStart - nRealTime >= CONST_INTERVAL)
            {
                return false;
            }//useless file----creating time of the file is bigger than  5min;    
            else if (nInterval_real_start >= 0 && nInterval_real_start < CONST_INTERVAL)
            {
                if (nInterval_real_end > CONST_INTERVAL)
                {
                    nStartPt_file2 = (int)nInterval_real_start;
                    nLength_file2 = CONST_INTERVAL - (int)nInterval_real_start;
                }//modified time of the file is beyond  5min.
                else
                {
                    nStartPt_file2 = (int)nInterval_real_start;
                    nLength_file2 = (int)(nEnd - nRealTime - nInterval_real_start);
                }//modified time of the file is whithin 5min
            }//creating future file--creating time of the file is within 5min.
            else if (nInterval_real_start < 0 && nInterval_real_start >= -CONST_INTERVAL)
            {
                if (nInterval_real_end > CONST_INTERVAL)
                {
                    nStartPt_file2 = 0;
                    nLength_file2 = CONST_INTERVAL + (int)System.Math.Abs(nInterval_real_start);
                }//modified time of the file is beyond  5min.
                else if (nInterval_real_end >= 0)
                {
                    nStartPt_file2 = 0;
                    nLength_file2 = (int)nInterval_real_end + (int)System.Math.Abs(nInterval_real_start);
                }//modified time of the file is whithin 5min
                else
                {
                    nStartPt_file2 = 0;
                    nLength_file2 = (int)System.Math.Abs(nInterval_real_start) - (int)System.Math.Abs(nInterval_real_end);
                }
            }
            else if (nInterval_real_start < -CONST_INTERVAL)
            {
                if (nInterval_real_end > CONST_INTERVAL)
                {
                    nStartPt_file2 = (int)System.Math.Abs(nInterval_real_start) - CONST_INTERVAL;
                    nLength_file2 = CONST_INTERVAL * 2;
                }//modified time of the file is beyond  5min.
                else if (nInterval_real_end >= 0)
                {
                    nStartPt_file2 = (int)System.Math.Abs(nInterval_real_start) - CONST_INTERVAL;
                    nLength_file2 = (int)nInterval_real_end + CONST_INTERVAL;
                }//modified time of the file is whithin 5min
                else if (nInterval_real_end < 0 && nInterval_real_end >= -CONST_INTERVAL)
                {
                    nStartPt_file2 = (int)System.Math.Abs(nInterval_real_start) - CONST_INTERVAL;
                    nLength_file2 = CONST_INTERVAL - (int)System.Math.Abs(nInterval_real_end);
                }
                else if (nInterval_real_end < -CONST_INTERVAL)
                {
                    return false;
                }
            }
            nStartPt = nStartPt_file2;
            nLength = nLength_file2;
            return true;
        }
       
        public void RemoveOldFiles()
        {
            if(!Directory.Exists(folderPath))
            {
                return;
            }
            string[] folders = Directory.GetDirectories(folderPath); //获取某个目录下的所有目录，即文件夹
           

            DirectoryInfo folder_oldest = new DirectoryInfo(folders[0]);

            DateTime t_oldest=folder_oldest.CreationTime;

            for(int i=1;i<folders.Length;i++)
            {
                DirectoryInfo folder_temp = new DirectoryInfo(folders[i]);
                if(folder_temp.CreationTime<t_oldest)
                {
                    t_oldest = folder_temp.CreationTime;
                    folder_oldest=folder_temp;
                }
            }//get the oldest folder

            Directory.Delete(folder_oldest.FullName,true);
        }

         public void Start()
        {
            bStop = false;
            apHandlerThread.Start();
        }

        public void Stop()
        {
            bStop = true;

        }


        private void VideoMonitorFunc()
        {
            while (!bStop)
            {
                VideoMonitorJob();
                Thread.Sleep(1000);
            }
            Log.write("APMsgHandler "  + " -ThreadEnd");
        }
        
        private void VideoMonitorJob()
        {
            //video process monitor
            Process[] proc = Process.GetProcesses();
            bool isExist_tmp = false;
            for(int i=0;i<proc.Length;i++)
            {
                if (String.Compare(proc[i].ProcessName, "iVMS-4200", true) == 0)
                {
                    isExist_tmp = true;
                    break;
                }
            }

            if (isStatusUpdate)
            {
                if (isExist_tmp)
                {
                    if (!isExist)
                    {
                        database.VideoServer_Status(true);
                    }

                }
                else
                {
                    if (isExist)
                    {
                        database.VideoServer_Status(false);
                    }
                }
            }//update status once changing.
            else
            {
                database.VideoServer_Status(isExist_tmp);
                isStatusUpdate = true;
            }//the first time,update i-VMS status
            

            isExist = isExist_tmp;
            //hardware space detection.
            long freespace=HardDisk.GetHardDiskFreeSpace(folderPath.Substring(0, 1));
            if (freespace < 5*(long)1073741824/*1GB*/)
            {
                RemoveOldFiles();
            }
        }
        

    }
}
/*long nStart = videoFile[i].startTime.Ticks / 10000000;
               long nEnd = videoFile[i].endTime.Ticks / 10000000;

               long nInterval_real_start = nStart - nRealTime;
               long nInterval_real_end = nEnd - nRealTime;
               if (nStart - nRealTime >= CONST_INTERVAL)
               {
                   continue;
               }//useless file----creating time of the file is bigger than  5min;    
               else if (nInterval_real_start >= 0 && nInterval_real_start < CONST_INTERVAL)
               {
                   if (nInterval_real_end > CONST_INTERVAL)
                   {
                       nStartPt_file2 = (int)nInterval_real_start;
                       nLength_file2 = CONST_INTERVAL - (int)nInterval_real_start;
                   }//modified time of the file is beyond  5min.
                   else
                   {
                       nStartPt_file2 = (int)nInterval_real_start;
                       nLength_file2 = (int)(nEnd - nRealTime - nInterval_real_start);
                   }//modified time of the file is whithin 5min
               }//creating future file--creating time of the file is within 5min.
               else if (nInterval_real_start < 0 && nInterval_real_start >=-CONST_INTERVAL  )
               {
                   if (nInterval_real_end > CONST_INTERVAL)
                   {
                       nStartPt_file2 = 0;
                       nLength_file2 = CONST_INTERVAL + (int)System.Math.Abs(nInterval_real_start);
                   }//modified time of the file is beyond  5min.
                   else if(nInterval_real_end >=0)
                   {
                       nStartPt_file2 = 0;
                       nLength_file2 = (int)nInterval_real_end + (int)System.Math.Abs(nInterval_real_start);
                   }//modified time of the file is whithin 5min
                   else
                   {
                       nStartPt_file2 = 0;
                       nLength_file2 = (int)System.Math.Abs(nInterval_real_start)-(int)System.Math.Abs(nInterval_real_end);
                   }
               }
               else if (nInterval_real_start < -CONST_INTERVAL)
               {
                   if (nInterval_real_end > CONST_INTERVAL)
                   {
                       nStartPt_file2 = (int)System.Math.Abs(nInterval_real_start) - CONST_INTERVAL;
                       nLength_file2 = CONST_INTERVAL*2;
                   }//modified time of the file is beyond  5min.
                   else if (nInterval_real_end >= 0)
                   {
                       nStartPt_file2 = (int)System.Math.Abs(nInterval_real_start) - CONST_INTERVAL;
                       nLength_file2 = (int)nInterval_real_end + CONST_INTERVAL;
                   }//modified time of the file is whithin 5min
                   else if (nInterval_real_end < 0 && nInterval_real_end >= -CONST_INTERVAL)
                   {
                       nStartPt_file2 = (int)System.Math.Abs(nInterval_real_start) - CONST_INTERVAL;
                       nLength_file2 = CONST_INTERVAL - (int)System.Math.Abs(nInterval_real_end);
                   }
                   else if(nInterval_real_end <-CONST_INTERVAL)
                   {
                       //invalid
                   }
               }*/