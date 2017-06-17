using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace MonitorCore
{
    public class Log
    {
        private string logFile;
        private static StreamWriter writer;
        private static FileStream fileStream = null;
        private static object locker = new object();
        public static event EventHandler Event_ShowLog;
        public static int level = 1;
        public Log(string fileName)
        {
            logFile = fileName;
            CreateDirectory(logFile);
        }

        /*public void write(string info)
        {
            try
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(logFile);
                if (!fileInfo.Exists)
                {
                    fileStream = fileInfo.Create();
                    writer = new StreamWriter(fileStream);
                }
                else
                {
                    fileStream = fileInfo.Open(FileMode.Append, FileAccess.Write);
                    writer = new StreamWriter(fileStream);
                }
                writer.WriteLine(DateTime.Now + ": " + info);
                writer.WriteLine("----------------------------------");
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }
        }*/

        public static void write(string info,int level_local=1)
        {
            if (level_local>level)
            {
                return;
            }
            
            lock (locker)
            {
                try
                {
                    string FileName = Process.GetCurrentProcess().MainModule.FileName;
                    string FilePath = System.IO.Path.GetDirectoryName(FileName);
                    string sFilePath = FilePath + "\\log";

                    string fileName = sFilePath+"\\"+DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    DirectoryInfo directoryInfo = Directory.GetParent(fileName);
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }

                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);
                    if (!fileInfo.Exists)
                    {
                        fileStream = fileInfo.Create();
                        writer = new StreamWriter(fileStream);
                    }
                    else
                    {
                        fileStream = fileInfo.Open(FileMode.Append, FileAccess.Write);
                        writer = new StreamWriter(fileStream);
                    }

                    string strLog = DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss") + ":  " + info;
                    writer.WriteLine(strLog);

                    if(Event_ShowLog!=null)
                    {
                        Event_ShowLog(strLog, null);
                    }   
                    
                    //writer.WriteLine("----------------------------------");
                }
                catch(Exception ex)
                {
                    
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                        writer.Dispose();
                        fileStream.Close();
                        fileStream.Dispose();
                    }
                }
            }
            
        }

        public void CreateDirectory(string infoPath)
        {
            DirectoryInfo directoryInfo = Directory.GetParent(infoPath);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
        }
    }
}
