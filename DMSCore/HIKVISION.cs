using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NVRCMonitor;
using System.Runtime.InteropServices;
using System.Timers;
using System.IO;
using System.Threading;

namespace MonitorCore
{
    public class HIKVISION
    {
        private bool m_bInitSDK = false;
        private Int32 m_lUserID = -1;
        private uint iLastErr = 0;

        private bool m_bRecord = false;//正在录像

        private Int32 m_lRealHandle = -1;//实时预览句柄
        private Int32 m_lPlayHandle = -1;//回放句柄
        private Int32 m_lFindHandle = -1;//查找句柄
        private Int32 m_lDownHandle = -1;//下载句柄

        private int[] iIPDevID = new int[96];
        private int[] iChannelNum = new int[96];
        private Int32 m_lTree = 0;
        private uint dwAChanTotalNum = 0;
        private uint dwDChanTotalNum = 0;

        private List<string[]> List_IPChannel=new List<string[]>();
        private List<string[]> List_AnalogChannel=new List<string[]>();
        private List<string[]> List_FileName = new List<string[]>();

        private Int32 m_lPort = -1;
        private IntPtr m_ptrRealHandle;
        private CHCNetSDK.REALDATACALLBACK RealData = null;
        private PlayCtrl.DECCBFUN m_fDisplayFun = null;
        bool bCallback = false;
        public delegate void MyDebugInfo(string str);//callback delegate

        public CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo;
        public CHCNetSDK.NET_DVR_IPPARACFG_V40 m_struIpParaCfgV40;
        public CHCNetSDK.NET_DVR_STREAM_MODE m_struStreamMode;
        public CHCNetSDK.NET_DVR_IPCHANINFO m_struChanInfo;
        public CHCNetSDK.NET_DVR_IPCHANINFO_V40 m_struChanInfoV40;

        private System.Timers.Timer timer_Download;
        int iPos_download=0;


        private System.Timers.Timer timer_Playback;
        int iPos_Playback = 0;

        

        public HIKVISION()
        {
            timer_Download = new System.Timers.Timer();
            timer_Download.Interval = 500;


            timer_Download.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Download);//到达时间的时候执行事件；

            timer_Download.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；

            timer_Download.Enabled = false;//是否执行System.Timers.Timer.Elapsed事件；


            timer_Playback = new System.Timers.Timer();
            timer_Playback.Interval = 500;


            timer_Playback.Elapsed += new System.Timers.ElapsedEventHandler(Timer_PlayBack);//到达时间的时候执行事件；

            timer_Playback.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；

            timer_Playback.Enabled = false;//是否执行System.Timers.Timer.Elapsed事件；

        }

        



        #region Init & Close
        public bool Init()
        {
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool Close()
        {
            //停止预览
            if (m_lRealHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
                m_lRealHandle = -1;
            }

            //停止回放 Stop playback
            if (m_lPlayHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopPlayBack(m_lPlayHandle);
                m_lPlayHandle = -1;
            }

            //停止下载 Stop download
            if (m_lDownHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopGetFile(m_lDownHandle);
                m_lDownHandle = -1;
            }

            //注销登录
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
                m_lUserID = -1;
            }

            CHCNetSDK.NET_DVR_Cleanup();

            return true;
        }
        #endregion
        public bool isLogin()
        {
            if(m_lUserID<0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #region login&logout
        public bool login_IP(string IP,Int16 portNum,string username,string password,bool HiDDNS=false)
        {
            if (m_lUserID < 0)
            {
                string DVRIPAddress = IP; //设备IP地址或者域名 Device IP
                Int16 DVRPortNumber = portNum;//设备服务端口号 Device Port
                string DVRUserName = username;//设备登录用户名 User name to login
                string DVRPassword = password;//设备登录密码 Password to login

                if (HiDDNS)
                {
                    byte[] HiDDNSName = System.Text.Encoding.Default.GetBytes(DVRIPAddress);
                    byte[] GetIPAddress = new byte[16];
                    uint dwPort = 0;
                    if (!CHCNetSDK.NET_DVR_GetDVRIPByResolveSvr_EX("www.hik-online.com", (ushort)80, HiDDNSName, (ushort)HiDDNSName.Length, null, 0, GetIPAddress, ref dwPort))
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        return false;
                    }
                    else
                    {
                        DVRIPAddress = System.Text.Encoding.UTF8.GetString(GetIPAddress).TrimEnd('\0');
                        DVRPortNumber = (Int16)dwPort;
                    }
                }

                //登录设备 Login the device
                m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    return false;
                }
                else
                {
                    //登录成功

                    dwAChanTotalNum = (uint)DeviceInfo.byChanNum;
                    dwDChanTotalNum = (uint)DeviceInfo.byIPChanNum + 256 * (uint)DeviceInfo.byHighDChanNum;
                    if (dwDChanTotalNum > 0)
                    {
                        InfoIPChannel();
                    }
                    else
                    {
                        for (int i = 0; i < dwAChanTotalNum; i++)
                        {
                            ListAnalogChannel(i + 1, 1);
                            iChannelNum[i] = i + (int)DeviceInfo.byStartChan;
                        }

                        //comboBoxView.SelectedItem = 1;
                        // MessageBox.Show("This device has no IP channel!");
                    }
                    return true;
                }

            }
            else
            {
                return false;
            }
        }


        public bool logout()
        {
            if (m_lRealHandle >= 0)
            {
                //DebugInfo("Please stop live view firstly"); //登出前先停止预览 Stop live view before logout
                return false;
            }

            if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_Logout failed, error code= " + iLastErr;
                //DebugInfo(str);
                return false ;
            }
            //DebugInfo("NET_DVR_Logout succ!");
            //listViewIPChannel.Items.Clear();//清空通道列表 Clean up the channel list
            m_lUserID = -1;
            //btnLogin.Text = "Login";
            return true;
        }
        #endregion
        public void InfoIPChannel()
        {
            List_IPChannel.Clear();
            List_AnalogChannel.Clear();
            uint dwSize = (uint)Marshal.SizeOf(m_struIpParaCfgV40);

            IntPtr ptrIpParaCfgV40 = Marshal.AllocHGlobal((Int32)dwSize);
            Marshal.StructureToPtr(m_struIpParaCfgV40, ptrIpParaCfgV40, false);

            uint dwReturn = 0;
            int iGroupNo = 0;  //该Demo仅获取第一组64个通道，如果设备IP通道大于64路，需要按组号0~i多次调用NET_DVR_GET_IPPARACFG_V40获取

            if (!CHCNetSDK.NET_DVR_GetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, dwSize, ref dwReturn))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_GET_IPPARACFG_V40 failed, error code= " + iLastErr;
                //获取IP资源配置信息失败，输出错误号 Failed to get configuration of IP channels and output the error code
                //DebugInfo(str);
            }
            else
            {
                //DebugInfo("NET_DVR_GET_IPPARACFG_V40 succ!");

                m_struIpParaCfgV40 = (CHCNetSDK.NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIpParaCfgV40, typeof(CHCNetSDK.NET_DVR_IPPARACFG_V40));

                for (int i = 0; i < dwAChanTotalNum; i++)
                {
                    ListAnalogChannel(i + 1, m_struIpParaCfgV40.byAnalogChanEnable[i]);
                    iChannelNum[i] = i + (int)DeviceInfo.byStartChan;
                }

                byte byStreamType = 0;
                uint iDChanNum = 64;

                if (dwDChanTotalNum < 64)
                {
                    iDChanNum = dwDChanTotalNum; //如果设备IP通道小于64路，按实际路数获取
                }

                for (int i = 0; i < iDChanNum; i++)
                {
                    iChannelNum[i + dwAChanTotalNum] = i + (int)m_struIpParaCfgV40.dwStartDChan;
                    byStreamType = m_struIpParaCfgV40.struStreamMode[i].byGetStreamType;

                    dwSize = (uint)Marshal.SizeOf(m_struIpParaCfgV40.struStreamMode[i].uGetStream);
                    switch (byStreamType)
                    {
                        //目前NVR仅支持直接从设备取流 NVR supports only the mode: get stream from device directly
                        case 0:
                            IntPtr ptrChanInfo = Marshal.AllocHGlobal((Int32)dwSize);
                            Marshal.StructureToPtr(m_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfo, false);
                            m_struChanInfo = (CHCNetSDK.NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(CHCNetSDK.NET_DVR_IPCHANINFO));

                            //列出IP通道 List the IP channel
                            ListIPChannel(i + 1, m_struChanInfo.byEnable, m_struChanInfo.byIPID);
                            iIPDevID[i] = m_struChanInfo.byIPID + m_struChanInfo.byIPIDHigh * 256 - iGroupNo * 64 - 1;

                            Marshal.FreeHGlobal(ptrChanInfo);
                            break;
                        case 6:
                            IntPtr ptrChanInfoV40 = Marshal.AllocHGlobal((Int32)dwSize);
                            Marshal.StructureToPtr(m_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfoV40, false);
                            m_struChanInfoV40 = (CHCNetSDK.NET_DVR_IPCHANINFO_V40)Marshal.PtrToStructure(ptrChanInfoV40, typeof(CHCNetSDK.NET_DVR_IPCHANINFO_V40));

                            //列出IP通道 List the IP channel
                            ListIPChannel(i + 1, m_struChanInfoV40.byEnable, m_struChanInfoV40.wIPID);
                            iIPDevID[i] = m_struChanInfoV40.wIPID - iGroupNo * 64 - 1;

                            Marshal.FreeHGlobal(ptrChanInfoV40);
                            break;
                        default:
                            break;
                    }
                }
            }
            Marshal.FreeHGlobal(ptrIpParaCfgV40);

        }

        #region list all device
        private void ListIPChannel(Int32 iChanNo, byte byOnline, int byIPID)
        {
            string str1 = "";
            string str2 = "";
            str1 = String.Format("IPCamera {0}", iChanNo);
            m_lTree++;

            if (byIPID == 0)
            {
                str2 = "X"; //通道空闲，没有添加前端设备 the channel is idle                  
            }
            else
            {
                if (byOnline == 0)
                {
                    str2 = "offline"; //通道不在线 the channel is off-line
                }
                else
                    str2 = "online"; //通道在线 The channel is on-line
            }

            //listViewIPChannel.Items.Add(new ListViewItem(new string[] { str1, str2 }));//将通道添加到列表中 add the channel to the list
            List_IPChannel.Add(new string[] { str1, str2 });
        }

        private void ListAnalogChannel(Int32 iChanNo, byte byEnable)
        {
            string str1 = "";
            string str2 = "";
            str1 = String.Format("Camera {0}", iChanNo);
            m_lTree++;

            if (byEnable == 0)
            {
                str2 = "Disabled"; //通道已被禁用 This channel has been disabled               
            }
            else
            {
                str2 = "Enabled"; //通道处于启用状态 This channel has been enabled
            }

            //listViewIPChannel.Items.Add(new ListViewItem(new string[] { str1, str2 }));//将通道添加到列表中 add the channel to the list
            List_AnalogChannel.Add(new string[] { str1, str2 });
        }
        #endregion

        #region get all type of channel
        public List<string[]> Get_List_IPChannel()
        {
            return List_IPChannel;
        }

        public List<string[]> Get_List_AnalogChannel()
        {
            return List_AnalogChannel;
        }
        #endregion

        #region download
        public bool Download_By_Time(int iChannelNum, string fileName, DateTime dateTimeStart, DateTime dateTimeEnd)
        {
           
            if (m_lDownHandle >= 0)
            {
                //MessageBox.Show("Downloading, please stop firstly!");//正在下载，请先停止下载
                return false;
            }
            
            if (!fileName.Contains(".mp4"))
            {
                return false;
            }

            CHCNetSDK.NET_DVR_PLAYCOND struDownPara = new CHCNetSDK.NET_DVR_PLAYCOND();
            struDownPara.dwChannel = (uint)iChannelNum; //通道号 Channel number  
            //设置下载的开始时间 Set the starting time
            struDownPara.struStartTime.dwYear = (uint)dateTimeStart.Year;
            struDownPara.struStartTime.dwMonth = (uint)dateTimeStart.Month;
            struDownPara.struStartTime.dwDay = (uint)dateTimeStart.Day;
            struDownPara.struStartTime.dwHour = (uint)dateTimeStart.Hour;
            struDownPara.struStartTime.dwMinute = (uint)dateTimeStart.Minute;
            struDownPara.struStartTime.dwSecond = (uint)dateTimeStart.Second;

            //设置下载的结束时间 Set the stopping time
            struDownPara.struStopTime.dwYear = (uint)dateTimeEnd.Year;
            struDownPara.struStopTime.dwMonth = (uint)dateTimeEnd.Month;
            struDownPara.struStopTime.dwDay = (uint)dateTimeEnd.Day;
            struDownPara.struStopTime.dwHour = (uint)dateTimeEnd.Hour;
            struDownPara.struStopTime.dwMinute = (uint)dateTimeEnd.Minute;
            struDownPara.struStopTime.dwSecond = (uint)dateTimeEnd.Second;

            string sVideoFileName;  //录像文件保存路径和文件名 the path and file name to save      
            sVideoFileName = fileName;

            //按时间下载 Download by time
            m_lDownHandle = CHCNetSDK.NET_DVR_GetFileByTime_V40(m_lUserID, sVideoFileName, ref struDownPara);
            if (m_lDownHandle < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_GetFileByTime_V40 failed, error code= " + iLastErr;
                //MessageBox.Show(str);
                return false;
            }

            uint iOutValue = 0;
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lDownHandle, CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_PLAYSTART failed, error code= " + iLastErr; //下载控制失败，输出错误号
                //MessageBox.Show(str);
                return false;
            }
            timer_Download.Interval = 1000;
            timer_Download.Enabled = true;
            return true;
        }

        public bool Download_By_Name(int iChannelNum, string srcFile, string dstFile)
        {
            if (m_lDownHandle >= 0)
            {
                //MessageBox.Show("Downloading, please stop firstly!");//正在下载，请先停止下载
                return false;
            }

            if (!dstFile.Contains(".mp4"))
            {
                return false;
            }

            if (m_lDownHandle >= 0)
            {
                //MessageBox.Show("Downloading, please stop firstly!");//正在下载，请先停止下载
                return false;
            }


            //按文件名下载 Download by file name
            m_lDownHandle = CHCNetSDK.NET_DVR_GetFileByName(m_lUserID, srcFile, dstFile);
            if (m_lDownHandle < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_GetFileByName failed, error code= " + iLastErr;
                //MessageBox.Show(str);
                return false;
            }

            uint iOutValue = 0;
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lDownHandle, CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_PLAYSTART failed, error code= " + iLastErr; //下载控制失败，输出错误号
                //MessageBox.Show(str);
                return false;
            }
            timer_Download.Interval = 1000;
            timer_Download.Enabled = true;
            //btnStopDownload.Enabled = true;
            return true;
        }

        public int Download_Progress()
        {
            return iPos_download;
        }


        private void Timer_Download(object sender, ElapsedEventArgs e)
        {
            iPos_download = 0;

            //获取下载进度
            iPos_download = CHCNetSDK.NET_DVR_GetDownloadPos(m_lDownHandle);


            if (iPos_download == 100)  //下载完成
            {
                if (!CHCNetSDK.NET_DVR_StopGetFile(m_lDownHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    //str = "NET_DVR_StopGetFile failed, error code= " + iLastErr; //下载控制失败，输出错误号
                    //MessageBox.Show(str);
                    return;
                }
                m_lDownHandle = -1;
                timer_Download.Stop();
            }

            if (iPos_download == 200) //网络异常，下载失败
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //MessageBox.Show("The downloading is abnormal for the abnormal network!");
                m_lDownHandle = -1;
                timer_Download.Stop();
            }
        }

        public bool Download_Stop()
        {
            if (m_lDownHandle < 0)
            {
                return true;
            }

            if (!CHCNetSDK.NET_DVR_StopGetFile(m_lDownHandle))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_StopGetFile failed, error code= " + iLastErr; //下载控制失败，输出错误号
                //MessageBox.Show(str);
                return false;
            }

            timer_Download.Stop();

            //MessageBox.Show("The downloading has been stopped succesfully!");
            m_lDownHandle = -1;
            iPos_download = 0;
            //DownloadProgressBar.Value = 0;
            // btnStopDownload.Enabled = true;
            return true;
        }
        #endregion

        #region playback
        public bool PlayBack_By_Time(int iChannelNum, DateTime dateTimeStart, DateTime dateTimeEnd,
                            IntPtr Handle, bool m_bReverse=false,bool m_bPause=false)
        {
            if (m_lPlayHandle >= 0)
            {
                //如果已经正在回放，先停止回放
                if (!CHCNetSDK.NET_DVR_StopPlayBack(m_lPlayHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    //str = "NET_DVR_StopPlayBack failed, error code= " + iLastErr;
                    //MessageBox.Show(str);
                    return false;
                }

                m_bReverse = false;


                m_bPause = false;


                m_lPlayHandle = -1;
            }

            CHCNetSDK.NET_DVR_VOD_PARA struVodPara = new CHCNetSDK.NET_DVR_VOD_PARA();
            struVodPara.dwSize = (uint)Marshal.SizeOf(struVodPara);
            struVodPara.struIDInfo.dwChannel = (uint)iChannelNum; //通道号 Channel number  
            struVodPara.hWnd = Handle;//回放窗口句柄

            //设置回放的开始时间 Set the starting time to search video files
            struVodPara.struBeginTime.dwYear = (uint)dateTimeStart.Year;
            struVodPara.struBeginTime.dwMonth = (uint)dateTimeStart.Month;
            struVodPara.struBeginTime.dwDay = (uint)dateTimeStart.Day;
            struVodPara.struBeginTime.dwHour = (uint)dateTimeStart.Hour;
            struVodPara.struBeginTime.dwMinute = (uint)dateTimeStart.Minute;
            struVodPara.struBeginTime.dwSecond = (uint)dateTimeStart.Second;

            //设置回放的结束时间 Set the stopping time to search video files
            struVodPara.struEndTime.dwYear = (uint)dateTimeEnd.Year;
            struVodPara.struEndTime.dwMonth = (uint)dateTimeEnd.Month;
            struVodPara.struEndTime.dwDay = (uint)dateTimeEnd.Day;
            struVodPara.struEndTime.dwHour = (uint)dateTimeEnd.Hour;
            struVodPara.struEndTime.dwMinute = (uint)dateTimeEnd.Minute;
            struVodPara.struEndTime.dwSecond = (uint)dateTimeEnd.Second;

            //按时间回放 Playback by time
            m_lPlayHandle = CHCNetSDK.NET_DVR_PlayBackByTime_V40(m_lUserID, ref struVodPara);
            if (m_lPlayHandle < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_PlayBackByTime_V40 failed, error code= " + iLastErr;
                //MessageBox.Show(str);
                return false;
            }

            uint iOutValue = 0;
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_PLAYSTART failed, error code= " + iLastErr; //回放控制失败，输出错误号
                //MessageBox.Show(str);
                return false;
            }
            timer_Playback.Interval = 1000;
            timer_Playback.Enabled = true;
            return true;
        }

        public bool PlayBack_By_Name(string srcFile, IntPtr Handle, bool m_bReverse = false, bool m_bPause = false)
        {
            if (srcFile == null)
            {
                //MessageBox.Show("Please select one file firstly!");//先选择回放的文件
                return false;
            }

            if (m_lPlayHandle >= 0)
            {
                //如果已经正在回放，先停止回放
                if (!CHCNetSDK.NET_DVR_StopPlayBack(m_lPlayHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    //str = "NET_DVR_StopPlayBack failed, error code= " + iLastErr; //停止回放失败，输出错误号
                    //MessageBox.Show(str);
                    return false;
                }

                m_bReverse = false;
                //btnReverse.Text = "Reverse";
                //labelReverse.Text = "切换为倒放";

                m_bPause = false;
                //btnPause.Text = "||";
                //labelPause.Text = "暂停";

                m_lPlayHandle = -1;
                //PlaybackprogressBar.Value = 0;
            }

            //按文件名回放
            m_lPlayHandle = CHCNetSDK.NET_DVR_PlayBackByName(m_lUserID, srcFile, Handle);
            if (m_lPlayHandle < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_PlayBackByName failed, error code= " + iLastErr;
                //MessageBox.Show(str);
                return false;
            }

            uint iOutValue = 0;
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_PLAYSTART failed, error code= " + iLastErr; //回放控制失败，输出错误号
                //MessageBox.Show(str);
                return false;
            }
            timer_Playback.Interval = 1000;
            timer_Playback.Enabled = true;
            //btnStopPlayback.Enabled = true;
            return true;
        }
        private void Timer_PlayBack(object sender, ElapsedEventArgs e)
        {
            uint iOutValue = 0;
            iPos_Playback = 0;

            IntPtr lpOutBuffer = Marshal.AllocHGlobal(4);

            //获取回放进度
            CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYGETPOS, IntPtr.Zero, 0, lpOutBuffer, ref iOutValue);

            iPos_Playback = (int)Marshal.PtrToStructure(lpOutBuffer, typeof(int));


            if (iPos_Playback == 100)  //回放结束
            {
                if (!CHCNetSDK.NET_DVR_StopPlayBack(m_lPlayHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    //str = "NET_DVR_StopPlayBack failed, error code= " + iLastErr; //回放控制失败，输出错误号
                    //MessageBox.Show(str);
                    return;
                }
                m_lPlayHandle = -1;
                timer_Playback.Stop();
            }

            if (iPos_Playback == 200) //网络异常，回放失败
            {
                //MessageBox.Show("The playback is abnormal for the abnormal network!");
                timer_Playback.Stop();
            }
            Marshal.FreeHGlobal(lpOutBuffer);
        }

        public bool PlayBack_Stop()
        {
            if (m_lPlayHandle < 0)
            {
                return true;
            }

            //停止回放
            if (!CHCNetSDK.NET_DVR_StopPlayBack(m_lPlayHandle))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_StopPlayBack failed, error code= " + iLastErr;
                //MessageBox.Show(str);
                return false;
            }

            //PlaybackprogressBar.Value = 0;
            //timerPlayback.Stop();

            //m_bReverse = false;
            //btnReverse.Text = "Reverse";
            //labelReverse.Text = "切换为倒放";

            //m_bPause = false;
            //btnPause.Text = "||";
            //labelPause.Text = "暂停";
            timer_Playback.Enabled = false;
            iPos_Playback = 0;
            m_lPlayHandle = -1;
            //VideoPlayWnd.Invalidate();//刷新窗口    
            //btnStopPlayback.Enabled = false;
            return true;
        }

        public bool PlayBack_Capture_BMP(string dstFile)
        {
            if (m_lPlayHandle < 0)
            {
                //MessageBox.Show("Please start playback firstly!"); //BMP抓图需要先打开预览
                return false;
            }

            if(!dstFile.Contains(".bmp"))
            {
                return false;
            }

            string sBmpPicFileName;
            //图片保存路径和文件名 the path and file name to save
            sBmpPicFileName = dstFile;

            //BMP抓图 Capture a BMP picture
            if (!CHCNetSDK.NET_DVR_PlayBackCaptureFile(m_lPlayHandle, sBmpPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_PlayBackCaptureFile failed, error code= " + iLastErr;
                //MessageBox.Show(str);
                return false;
            }
            else
            {
                //str = "Successful to capture the BMP file and the saved file is " + sBmpPicFileName;
                //MessageBox.Show(str);
            }
            return true;
        }

        public bool PlayBack_Direction(bool m_bReverse)
        {
            uint iOutValue = 0;
            if (!m_bReverse)
            {
                if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAY_REVERSE, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    //str = "NET_DVR_PLAY_REVERSE failed, error code= " + iLastErr; //回放控制失败，输出错误号
                    //MessageBox.Show(str);
                    return false;
                }
                //m_bReverse = true;
                //btnReverse.Text = "Forward";
                //labelReverse.Text = "切换为正放";
                return true;
            }
            else
            {
                if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAY_FORWARD, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    //str = "NET_DVR_PLAY_FORWARD failed, error code= " + iLastErr; //回放控制失败，输出错误号
                    //MessageBox.Show(str);
                    return false;
                }
                //m_bReverse = false;
                //btnReverse.Text = "Reverse";
                //labelReverse.Text = "切换为倒放";
                return true;
            }
        }


        public bool PlayBack_Pause()
        {
            uint iOutValue = 0;
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYPAUSE, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_PLAYPAUSE failed, error code= " + iLastErr; //回放控制失败，输出错误号
                //MessageBox.Show(str);
                return false;
            }
            //m_bPause = true;
            //btnPause.Text = ">";
            //labelPause.Text = "播放";
            return true;
        }

        public bool PlayBack_Resume()
        {
            uint iOutValue = 0;
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYRESTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_PLAYRESTART failed, error code= " + iLastErr; //回放控制失败，输出错误号
                //MessageBox.Show(str);
                return false;
            }
            return true;
        }

        public bool PlayBack_Normal()
        {
            uint iOutValue = 0;

            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYNORMAL, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_PLAYNORMAL failed, error code= " + iLastErr; //回放控制失败，输出错误号
                //MessageBox.Show(str);
                return false;
            }
            return true;
        }

        public bool PlayBack_Fast()
        {
            uint iOutValue = 0;

            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYFAST, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_PLAYFAST failed, error code= " + iLastErr; //回放控制失败，输出错误号
                //MessageBox.Show(str);
                return false;
            }
            return true;
        }

        public bool PlayBack_Slow()
        {
            uint iOutValue = 0;

            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYSLOW, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_PLAYSLOW failed, error code= " + iLastErr; //回放控制失败，输出错误号
                //MessageBox.Show(str);
                return false;
            }
            return true;
        }

        public bool PlayBack_Frame()
        {
            uint iOutValue = 0;

            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYFRAME, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_PLAYFRAME failed, error code= " + iLastErr; //回放控制失败，输出错误号
                //MessageBox.Show(str);
                return false;
            }
            return true;
        }
        #endregion

        #region Search File
        public bool SearchFile(int channel, DateTime dateTimeStart, DateTime dateTimeEnd)
        {
            List_FileName.Clear();//清空文件列表 
            string str1 = "";
            string str2 = "";
            string str3 = "";
            CHCNetSDK.NET_DVR_FILECOND_V40 struFileCond_V40 = new CHCNetSDK.NET_DVR_FILECOND_V40();

            struFileCond_V40.lChannel = channel; //通道号 Channel number
            struFileCond_V40.dwFileType = 0xff; //0xff-全部，0-定时录像，1-移动侦测，2-报警触发，...
            struFileCond_V40.dwIsLocked = 0xff; //0-未锁定文件，1-锁定文件，0xff表示所有文件（包括锁定和未锁定）

            //设置录像查找的开始时间 Set the starting time to search video files
            struFileCond_V40.struStartTime.dwYear = (uint)dateTimeStart.Year;
            struFileCond_V40.struStartTime.dwMonth = (uint)dateTimeStart.Month;
            struFileCond_V40.struStartTime.dwDay = (uint)dateTimeStart.Day;
            struFileCond_V40.struStartTime.dwHour = (uint)dateTimeStart.Hour;
            struFileCond_V40.struStartTime.dwMinute = (uint)dateTimeStart.Minute;
            struFileCond_V40.struStartTime.dwSecond = (uint)dateTimeStart.Second;

            //设置录像查找的结束时间 Set the stopping time to search video files
            struFileCond_V40.struStopTime.dwYear = (uint)dateTimeEnd.Year;
            struFileCond_V40.struStopTime.dwMonth = (uint)dateTimeEnd.Month;
            struFileCond_V40.struStopTime.dwDay = (uint)dateTimeEnd.Day;
            struFileCond_V40.struStopTime.dwHour = (uint)dateTimeEnd.Hour;
            struFileCond_V40.struStopTime.dwMinute = (uint)dateTimeEnd.Minute;
            struFileCond_V40.struStopTime.dwSecond = (uint)dateTimeEnd.Second;

            //开始录像文件查找 Start to search video files 
            m_lFindHandle = CHCNetSDK.NET_DVR_FindFile_V40(m_lUserID, ref struFileCond_V40);

            if (m_lFindHandle < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_FindFile_V40 failed, error code= " + iLastErr; //预览失败，输出错误号
               // MessageBox.Show(str);
                return false;
            }
            else
            {
                CHCNetSDK.NET_DVR_FINDDATA_V30 struFileData = new CHCNetSDK.NET_DVR_FINDDATA_V30(); ;
                while (true)
                {
                    //逐个获取查找到的文件信息 Get file information one by one.
                    int result = CHCNetSDK.NET_DVR_FindNextFile_V30(m_lFindHandle, ref struFileData);

                    if (result == CHCNetSDK.NET_DVR_ISFINDING)  //正在查找请等待 Searching, please wait
                    {
                        continue;
                    }
                    else if (result == CHCNetSDK.NET_DVR_FILE_SUCCESS) //获取文件信息成功 Get the file information successfully
                    {
                        str1 = struFileData.sFileName;

                        str2 = Convert.ToString(struFileData.struStartTime.dwYear) + "-" +
                            Convert.ToString(struFileData.struStartTime.dwMonth) + "-" +
                            Convert.ToString(struFileData.struStartTime.dwDay) + " " +
                            Convert.ToString(struFileData.struStartTime.dwHour) + ":" +
                            Convert.ToString(struFileData.struStartTime.dwMinute) + ":" +
                            Convert.ToString(struFileData.struStartTime.dwSecond);

                        str3 = Convert.ToString(struFileData.struStopTime.dwYear) + "-" +
                            Convert.ToString(struFileData.struStopTime.dwMonth) + "-" +
                            Convert.ToString(struFileData.struStopTime.dwDay) + " " +
                            Convert.ToString(struFileData.struStopTime.dwHour) + ":" +
                            Convert.ToString(struFileData.struStopTime.dwMinute) + ":" +
                            Convert.ToString(struFileData.struStopTime.dwSecond);

                        List_FileName.Add(new string[] { str1, str2, str3 });//将查找的录像文件添加到列表中

                    }
                    else if (result == CHCNetSDK.NET_DVR_FILE_NOFIND || result == CHCNetSDK.NET_DVR_NOMOREFILE)
                    {
                        break; //未查找到文件或者查找结束，退出   No file found or no more file found, search is finished 
                    }
                    else
                    {
                        break;
                    }
                }
                return true;
            }
        }

        public List<string[]> Get_List_File()
        {
            return List_FileName;
        }
        #endregion

        #region live view
        public bool LiveView_start(int iChanel, IntPtr Handle,bool bCallback=false)
        {
            this.bCallback = bCallback;
            if (m_lUserID < 0)
            {
                //MessageBox.Show("Please login the device firstly!");
                return false;
            }

            if (m_bRecord)
            {
                //MessageBox.Show("Please stop recording firstly!");
                return false;
            }

            if (m_lRealHandle < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = Handle;//预览窗口 live view window
                lpPreviewInfo.lChannel = iChanel;//预览的设备通道 the device channel number
                lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流
                lpPreviewInfo.dwDisplayBufNum = 15; //播放库显示缓冲区最大帧数

                IntPtr pUser = IntPtr.Zero;//用户数据 user data 

                if (!bCallback)
                {
                    //打开预览 Start live view 
                    m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);
                }
                else
                {
                    lpPreviewInfo.hPlayWnd = IntPtr.Zero;//预览窗口 live view window
                    m_ptrRealHandle = Handle;
                    RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);//预览实时流回调函数 real-time stream callback function 
                    m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, RealData, pUser);
                }

                if (m_lRealHandle < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    //str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //预览失败，输出错误号 failed to start live view, and output the error code.
                    //DebugInfo(str);
                    return false;
                }
                else
                {
                    //预览成功
                    //DebugInfo("NET_DVR_RealPlay_V40 succ!");
                    //btnPreview.Text = "Stop View";
                    return true;
                }
            }

            return false;
        }

        public bool LiveView_stop()
        {
            //停止预览 Stop live view 
            if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                //DebugInfo(str);
                return false;
            }

            if (bCallback && (m_lPort >= 0))
            {
                if (!PlayCtrl.PlayM4_Stop(m_lPort))
                {
                    iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                    //str = "PlayM4_Stop failed, error code= " + iLastErr;
                    //DebugInfo(str);
                }
                if (!PlayCtrl.PlayM4_CloseStream(m_lPort))
                {
                    iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                    //str = "PlayM4_CloseStream failed, error code= " + iLastErr;
                    //DebugInfo(str);
                }
                if (!PlayCtrl.PlayM4_FreePort(m_lPort))
                {
                    iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                    //str = "PlayM4_FreePort failed, error code= " + iLastErr;
                    //DebugInfo(str);
                }
                m_lPort = -1;
            }

            //DebugInfo("NET_DVR_StopRealPlay succ!");
            m_lRealHandle = -1;
            //btnPreview.Text = "Live View";
            //RealPlayWnd.Invalidate();//刷新窗口 refresh the window
            return true;
        }

        public bool LiveView_CaptureBMP(string dstFile)
        {
            if (m_lRealHandle < 0)
            {
                //DebugInfo("Please start live view firstly!"); //BMP抓图需要先打开预览
                return false;
            }

            string sBmpPicFileName;
            //图片保存路径和文件名 the path and file name to save
            sBmpPicFileName = dstFile;

            if (!bCallback)
            {
                //BMP抓图 Capture a BMP picture
                if (!CHCNetSDK.NET_DVR_CapturePicture(m_lRealHandle, sBmpPicFileName))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    //str = "NET_DVR_CapturePicture failed, error code= " + iLastErr;
                    //DebugInfo(str);
                    return false;
                }
                else
                {
                    //str = "NET_DVR_CapturePicture succ and the saved file is " + sBmpPicFileName;
                    //DebugInfo(str);
                    return true;
                }
            }
            else
            {
                int iWidth = 0, iHeight = 0;
                uint iActualSize = 0;

                if (!PlayCtrl.PlayM4_GetPictureSize(m_lPort, ref iWidth, ref iHeight))
                {
                    iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                    //str = "PlayM4_GetPictureSize failed, error code= " + iLastErr;
                    //DebugInfo(str);
                    return false;
                }

                uint nBufSize = (uint)(iWidth * iHeight) * 5;

                byte[] pBitmap = new byte[nBufSize];

                if (!PlayCtrl.PlayM4_GetBMP(m_lPort, pBitmap, nBufSize, ref iActualSize))
                {
                    iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                    //str = "PlayM4_GetBMP failed, error code= " + iLastErr;
                    //DebugInfo(str);
                    return false;
                }
                else
                {
                    FileStream fs = new FileStream(sBmpPicFileName, FileMode.Create);
                    fs.Write(pBitmap, 0, (int)iActualSize);
                    fs.Close();
                    //str = "PlayM4_GetBMP succ and the saved file is " + sBmpPicFileName;
                    //DebugInfo(str);
                    return true;
                }
            }
        }

        public bool LiveView_CaptureJPEG(int iChannel,string dstFile)
        {
            int lChannel = iChannel; //通道号 Channel number

            CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //图像质量 Image quality
            lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 0xff-Auto(使用当前码流分辨率) 
            //抓图分辨率需要设备支持，更多取值请参考SDK文档

            //JPEG抓图保存成文件 Capture a JPEG picture
            string sJpegPicFileName;
            sJpegPicFileName = dstFile;//图片保存路径和文件名 the path and file name to save

            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserID, lChannel, ref lpJpegPara, sJpegPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                //DebugInfo(str);
                return false;
            }
            else
            {
                //str = "NET_DVR_CaptureJPEGPicture succ and the saved file is " + sJpegPicFileName;
                //DebugInfo(str);
                return true;
            }

            //JEPG抓图，数据保存在缓冲区中 Capture a JPEG picture and save in the buffer
            /*
            uint iBuffSize = 400000; //缓冲区大小需要不小于一张图片数据的大小 The buffer size should not be less than the picture size
            byte[] byJpegPicBuffer = new byte[iBuffSize];
            uint dwSizeReturned = 0;

            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture_NEW(m_lUserID, lChannel, ref lpJpegPara, byJpegPicBuffer, iBuffSize, ref dwSizeReturned))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_CaptureJPEGPicture_NEW failed, error code= " + iLastErr;
                //DebugInfo(str);
                return false;
            }
            else
            {
                //将缓冲区里的JPEG图片数据写入文件 save the data into a file
                string str = "buffertest.jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)dwSizeReturned;
                fs.Write(byJpegPicBuffer, 0, iLen);
                fs.Close();

                //str = "NET_DVR_CaptureJPEGPicture_NEW succ and save the data in buffer to 'buffertest.jpg'.";
                //DebugInfo(str);
                return true;
            }*/

           
        }


        private bool LiveView_Record_Start(int iChannel,string dstFile)
        {
            //录像保存路径和文件名 the path and file name to save
            string sVideoFileName;
            sVideoFileName = dstFile;

            if (m_bRecord == false)
            {
                //强制I帧 Make one key frame
                int lChannel = iChannel; //通道号 Channel number
                CHCNetSDK.NET_DVR_MakeKeyFrame(m_lUserID, lChannel);

                //开始录像 Start recording
                if (!CHCNetSDK.NET_DVR_SaveRealData(m_lRealHandle, sVideoFileName))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    //str = "NET_DVR_SaveRealData failed, error code= " + iLastErr;
                    //DebugInfo(str);
                    return false;
                }
                else
                {
                    //DebugInfo("NET_DVR_SaveRealData succ!");
                    //btnRecord.Text = "Stop";
                    m_bRecord = true;
                    return true;
                }
            }
            else
            {
                return true;
            }
            
        }


        private bool LiveView_Record_end()
        {
            if(!m_bRecord)
            {
                return true;
            }
            //停止录像 Stop recording
            if (!CHCNetSDK.NET_DVR_StopSaveRealData(m_lRealHandle))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //str = "NET_DVR_StopSaveRealData failed, error code= " + iLastErr;
                //DebugInfo(str);
                return false;
            }
            else
            {
                //str = "NET_DVR_StopSaveRealData succ and the saved file is " + sVideoFileName;
                //DebugInfo(str);
                //btnRecord.Text = "Record";
                m_bRecord = false;
                return true;
            }
        }
        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
            //下面数据处理建议使用委托的方式
            string str = "";
            //MyDebugInfo AlarmInfo = new MyDebugInfo(DebugInfo);
            switch (dwDataType)
            {
                case CHCNetSDK.NET_DVR_SYSHEAD:     // sys head
                    if (dwBufSize > 0)
                    {
                        if (m_lPort >= 0)
                        {
                            return; //同一路码流不需要多次调用开流接口
                        }

                        //获取播放句柄 Get the port to play
                        if (!PlayCtrl.PlayM4_GetPort(ref m_lPort))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_GetPort failed, error code= " + iLastErr;
                            //this.BeginInvoke(AlarmInfo, str);
                            break;
                        }

                        //设置流播放模式 Set the stream mode: real-time stream mode
                        if (!PlayCtrl.PlayM4_SetStreamOpenMode(m_lPort, PlayCtrl.STREAME_REALTIME))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "Set STREAME_REALTIME mode failed, error code= " + iLastErr;
                            //this.BeginInvoke(AlarmInfo, str);
                        }

                        //打开码流，送入头数据 Open stream
                        if (!PlayCtrl.PlayM4_OpenStream(m_lPort, pBuffer, dwBufSize, 2 * 1024 * 1024))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_OpenStream failed, error code= " + iLastErr;
                            //this.BeginInvoke(AlarmInfo, str);
                            break;
                        }


                        //设置显示缓冲区个数 Set the display buffer number
                        if (!PlayCtrl.PlayM4_SetDisplayBuf(m_lPort, 15))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_SetDisplayBuf failed, error code= " + iLastErr;
                            //this.BeginInvoke(AlarmInfo, str);
                        }

                        //设置显示模式 Set the display mode
                        if (!PlayCtrl.PlayM4_SetOverlayMode(m_lPort, 0, 0/* COLORREF(0)*/)) //play off screen 
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_SetOverlayMode failed, error code= " + iLastErr;
                            //this.BeginInvoke(AlarmInfo, str);
                        }

                        //设置解码回调函数，获取解码后音视频原始数据 Set callback function of decoded data
                        m_fDisplayFun = new PlayCtrl.DECCBFUN(DecCallbackFUN);
                        if (!PlayCtrl.PlayM4_SetDecCallBackEx(m_lPort, m_fDisplayFun, IntPtr.Zero, 0))
                        {
                            //this.BeginInvoke(AlarmInfo, "PlayM4_SetDisplayCallBack fail");
                        }

                        //开始解码 Start to play                       
                        if (!PlayCtrl.PlayM4_Play(m_lPort, m_ptrRealHandle))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_Play failed, error code= " + iLastErr;
                            //this.BeginInvoke(AlarmInfo, str);
                            break;
                        }
                    }
                    break;
                case CHCNetSDK.NET_DVR_STREAMDATA:     // video stream data
                    if (dwBufSize > 0 && m_lPort != -1)
                    {
                        for (int i = 0; i < 999; i++)
                        {
                            //送入码流数据进行解码 Input the stream data to decode
                            if (!PlayCtrl.PlayM4_InputData(m_lPort, pBuffer, dwBufSize))
                            {
                                iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                                str = "PlayM4_InputData failed, error code= " + iLastErr;
                                Thread.Sleep(2);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
                default:
                    if (dwBufSize > 0 && m_lPort != -1)
                    {
                        //送入其他数据 Input the other data
                        for (int i = 0; i < 999; i++)
                        {
                            if (!PlayCtrl.PlayM4_InputData(m_lPort, pBuffer, dwBufSize))
                            {
                                iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                                str = "PlayM4_InputData failed, error code= " + iLastErr;
                                Thread.Sleep(2);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
            }
        }

        private void DecCallbackFUN(int nPort, IntPtr pBuf, int nSize, ref PlayCtrl.FRAME_INFO pFrameInfo, int nReserved1, int nReserved2)
        {
            // 将pBuf解码后视频输入写入文件中（解码后YUV数据量极大，尤其是高清码流，不建议在回调函数中处理）
            if (pFrameInfo.nType == 3) //#define T_YV12	3
            {
                //    FileStream fs = null;
                //    BinaryWriter bw = null;
                //    try
                //    {
                //        fs = new FileStream("DecodedVideo.yuv", FileMode.Append);
                //        bw = new BinaryWriter(fs);
                //        byte[] byteBuf = new byte[nSize];
                //        Marshal.Copy(pBuf, byteBuf, 0, nSize);
                //        bw.Write(byteBuf);
                //        bw.Flush();
                //    }
                //    catch (System.Exception ex)
                //    {
                //        MessageBox.Show(ex.ToString());
                //    }
                //    finally
                //    {
                //        bw.Close();
                //        fs.Close();
                //    }
            }
        }
        #endregion
    }
}
