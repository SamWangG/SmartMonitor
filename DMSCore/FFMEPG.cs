using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace MonitorCore
{
    class FFMEPG
    {
        public string Cut(string OriginFile, string DstFile, TimeSpan startTime, TimeSpan endTime)
        {

            TimeSpan interval = endTime.Subtract(startTime);
            string strCmd = "-ss 00:00:10 -i " + OriginFile + " -ss " +
                startTime.ToString() + " -t " + interval.ToString() + " -vcodec copy " + DstFile + " -y ";
            
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "ffmpeg.exe";//要执行的程序名称  
            p.StartInfo.Arguments = " " + strCmd;
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.RedirectStandardInput = false;//可能接受来自调用程序的输入信息  
            p.StartInfo.RedirectStandardOutput = false;//由调用程序获取输出信息   
            p.StartInfo.RedirectStandardError = false;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = false;//不显示程序窗口   

            p.Start();//启动程序   

            //p.StandardInput.WriteLine(strCmd);
           // p.StandardInput.AutoFlush = true;
            
            
            //向CMD窗口发送输入信息：   
           // p.StandardInput.Write("ipconfig");
            p.WaitForExit();//等待程序执行完退出进程
            //-ss表示搜索到指定的时间 -i表示输入的文件 -y表示覆盖输出 -f表示强制使用的格式  

             if (System.IO.File.Exists(DstFile))
            {
                return DstFile;
            }
            return "";
        }

        public string Combine(string File1, string File2, string DstFile)
        {
            string strTmp1=File1+".ts";
            string strTmp2 = File2 + ".ts";
            string strCmd1 = " -i " + File1 + " -c copy -bsf:v h264_mp4toannexb -f mpegts " + strTmp1 + " -y ";
            string strCmd2 = " -i " + File2 + " -c copy -bsf:v h264_mp4toannexb -f mpegts " + strTmp2 + " -y ";

            string strCmd = " -i \"concat:" + strTmp1 + "|" +
                strTmp2 + "\" -c copy -bsf:a aac_adtstoasc -movflags +faststart " + DstFile + " -y ";

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "ffmpeg.exe";//要执行的程序名称  
            p.StartInfo.Arguments = " " + strCmd1;
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.RedirectStandardInput = false;//可能接受来自调用程序的输入信息  
            p.StartInfo.RedirectStandardOutput = false;//由调用程序获取输出信息   
            p.StartInfo.RedirectStandardError = false;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = false;//不显示程序窗口   

            p.Start();//启动程序   
            p.WaitForExit();
            //p.StandardInput.WriteLine(strCmd);

            p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "ffmpeg.exe";//要执行的程序名称  
            p.StartInfo.Arguments = " " + strCmd2;
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.RedirectStandardInput = false;//可能接受来自调用程序的输入信息  
            p.StartInfo.RedirectStandardOutput = false;//由调用程序获取输出信息   
            p.StartInfo.RedirectStandardError = false;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = false;//不显示程序窗口   

            p.Start();//启动程序   
            p.WaitForExit();
            // p.StandardInput.AutoFlush = true;
            p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "ffmpeg.exe";//要执行的程序名称  
            p.StartInfo.Arguments = " " + strCmd;
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.RedirectStandardInput = false;//可能接受来自调用程序的输入信息  
            p.StartInfo.RedirectStandardOutput = false;//由调用程序获取输出信息   
            p.StartInfo.RedirectStandardError = false;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = false;//不显示程序窗口   

            p.Start();//启动程序   

            //向CMD窗口发送输入信息：   
            // p.StandardInput.Write("ipconfig");

            //string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();//等待程序执行完退出进程
            //-ss表示搜索到指定的时间 -i表示输入的文件 -y表示覆盖输出 -f表示强制使用的格式  


            if (File.Exists(strTmp1))
            {
                File.Delete(strTmp1);
            }

            if (File.Exists(strTmp2))
            {
                File.Delete(strTmp2);
            }
            if (System.IO.File.Exists(DstFile))
            {
                return DstFile;
            }
            return "";
        }
        /*
        public int play_time(string File)
        {
            string strCmd = "-i " + File + " 2>&1 | grep 'Duration' | cut -d ' ' -f 4 | sed s/,//";

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "ffmpeg.exe";//要执行的程序名称  
            p.StartInfo.Arguments = " " + strCmd;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = false;//可能接受来自调用程序的输入信息  
            p.StartInfo.RedirectStandardOutput = false;//由调用程序获取输出信息   
            p.StartInfo.RedirectStandardError = false;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = false;//不显示程序窗口   

            p.Start();//启动程序   
            
            //p.StandardInput.WriteLine(strCmd);
            // p.StandardInput.AutoFlush = true;


            //向CMD窗口发送输入信息：   
            // p.StandardInput.Write("ipconfig");
            p.WaitForExit();//等待程序执行完退出进程
            //-ss表示搜索到指定的时间 -i表示输入的文件 -y表示覆盖输出 -f表示强制使用的格式  

            return 0;
        }*/
    }
}
