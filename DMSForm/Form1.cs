using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MonitorCore;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections;


namespace MonitorForm
{
    public partial class Form1 : Form
    {
        private static Core core;

        private bool isDownloading = false;
        
        delegate void GetMsg(byte[] data);

        DateTime time;

        int iAcc = 0;
        //
        public Form1()
        {
            InitializeComponent();
            Log.Event_ShowLog += ShowLog;
            GUIEvent.Event_GUI_Get_Msg += Show_RFID_Msg;
            core = new Core();
            
            //创建log--测试用
            
        }
        private List<string> list_data=new List<string>();
        public delegate void MsgDelegate(string RFID, string label1, string label2, string label3, string label4, string label5);
        private void Show_RFID_Msg(string RFID, string label1, string label2, string label3, string label4, string label5)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MsgDelegate(Show_RFID_Msg), RFID, label1, label2, label3, label4, label5);
                return;
            }

            string[] labels = new string[5] { label1, label2, label3, label4, label5 };


            //////////上卡/////////////////
            time = DateTime.Now;

            

            
            if (isDownloading)
            {
                return;
            }

            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }
            

            isDownloading = true;
            string jpgName = time.ToString("yyyymmddhhmmss") + ".jpg";
            core.hikvision.LiveView_CaptureJPEG(1, jpgName);

            pictureBox2.ImageLocation = jpgName;

            if(checkBox2.Checked)
            {
                core.hikvision.LiveView_start(1, pictureBox1.Handle);
            }

            if(!checkBox1.Checked)
            {
                label6.Text = "截图完成";
                isDownloading = false;
                return;
            }
            
            label6.Text = "截图完成,90s后开始录制视频";

            timer2.Interval = 1000;

            timer2.Start();


            textBox3.Text = RFID;

            if (textBox2.Text == RFID)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (labels[i] != "null")
                    {
                        if (radioButton1.Checked)
                        {
                            if (!list_data.Contains(labels[i]))
                            {
                                listView1.Items.Add(labels[i]);
                                list_data.Add(labels[i]);
                            }
                        }
                        else
                        {
                            if (list_data.Contains(labels[i]))
                            {
                                //listView1.Items.Add(labels[i]);

                                //list_data.Add(labels[i]);
                                list_data.Remove(labels[i]);
                                listView1.Items.Clear();
                                for (int k = 0; k < list_data.Count; k++)
                                {
                                    listView1.Items.Add(list_data[k]);
                                }
                            }
                        }
                    }
                }

                LABEL_CardNum.Text = listView1.Items.Count.ToString();
            }
            
            if(textBox1.Text==RFID)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (labels[i] != "null")
                    {

                        if (list_data.Contains(labels[i]))
                        {
                            //告警
                            DoGuardJob(labels[i]);
                        }
                    }
                }
            }

        }

        private void DoGuardJob(string label)
        {
            DateTime t = DateTime.Now;
            string name = label + t.ToString("yyyymmddhhmmss") + ".bmp";
            core.hikvision.LiveView_CaptureJPEG(1, name);
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.Event_ShowLog -= ShowLog;
            core.Close();
        }
        private void button_Start_Click(object sender, EventArgs e)
        {
            
        }

        private void button_Stop_Click(object sender, EventArgs e)
        {
            
        }

        private void ShowLog(object sender, EventArgs e)
        {
            if (richTextBox1.InvokeRequired)
            {
                // 当一个控件的InvokeRequired属性值为真时，说明有一个创建它以外的线程想访问它
                Action<string> actionDelegate = (x) =>
                {
                    richTextBox1.AppendText(sender.ToString() + "\r\n");
                    richTextBox1.ScrollToCaret();
                    if (richTextBox1.Lines.Length == 1000)
                    {
                        richTextBox1.Text = "";
                    }
                };
                // 或者
                // Action<string> actionDelegate = delegate(string txt) { this.label2.Text = txt; };
                this.richTextBox1.Invoke(actionDelegate, sender);
            }
            else
            {
                richTextBox1.AppendText(sender.ToString() + "\r\n");
                richTextBox1.ScrollToCaret();
                if (richTextBox1.Lines.Length == 1000)
                {
                    richTextBox1.Text = "";
                }
            }

        }

        

        private void Form1_Shown(object sender, EventArgs e)
        {
            /*if (!core.Init())
            {
                //core.Close();
                MessageBox.Show("初始化失败！");
                //this.Close();
            }*/
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void BTN_PlayBackByTime_Click(object sender, EventArgs e)
        {
            if(core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }

            DateTime start = DateTime.Parse(DTP_Start.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            DateTime end = DateTime.Parse(DTP_End.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            core.hikvision.PlayBack_By_Time(1, start, end, pictureBox1.Handle);

        }

        private void BTN_DownloadByTime_Click(object sender, EventArgs e)
        {
            
            if (core.hikvision==null || !core.hikvision.isLogin())
            {
                return;
            }

            DateTime start = DateTime.Parse(DTP_Start.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            DateTime end = DateTime.Parse(DTP_End.Value.ToString("yyyy-MM-dd HH:mm:ss")); 
            
            if(core.hikvision.Download_By_Time(1, "D:\\test.mp4", start, end))
            {
                timer1.Interval = 500;
                timer1.Enabled = true;
            }
            else
            {
                MessageBox.Show("Download error");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int iRet=core.hikvision.Download_Progress();
            progressBar1.Value = iRet;

            label6.Text = "视频下载中";

            if (iRet>=100)
            {
                timer1.Enabled = false;
                label6.Text = "视频下载完成";
                isDownloading = false;
            }
        }

        private void BTN_GetList_Click(object sender, EventArgs e)
        {

            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }

            listViewIPChannel.Items.Clear();
            
            List<string[]> a=core.hikvision.Get_List_IPChannel();
            List<string[]> b=core.hikvision.Get_List_AnalogChannel();
            
            for (int i=0;i<a.Count;i++)
            {
                listViewIPChannel.Items.Add(new ListViewItem(a[i]));
            }

            for (int i = 0; i < b.Count; i++)
            {
                listViewIPChannel.Items.Add(new ListViewItem(b[i]));
            }



            DateTime start = DateTime.Parse(DTP_Start.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            DateTime end = DateTime.Parse(DTP_End.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            core.hikvision.SearchFile(1, start, end);
            List<string[]> fileList = core.hikvision.Get_List_File();
            for (int i = 0; i < fileList.Count; i++)
            {
                listViewFile.Items.Add(new ListViewItem(fileList[i]));
            }
        }

        private void BTN_StopDownload_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }
            core.hikvision.Download_Stop();
        }

        private void BTN_StopPlayback_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }
            core.hikvision.PlayBack_Stop();
            pictureBox1.Invalidate();
           
        }

        private void BTN_Pause_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }
            core.hikvision.PlayBack_Pause();
        }

        private void BTN_Resume_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }
            core.hikvision.PlayBack_Resume();
        }

        private void BTN_Slow_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }
            core.hikvision.PlayBack_Slow();
        }

        private void BTN_Fast_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }
            core.hikvision.PlayBack_Fast();
        }

        private void BTN_Normal_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }
            core.hikvision.PlayBack_Normal();
        }

        private void BTN_LiveView_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }
            core.hikvision.LiveView_start(1,pictureBox1.Handle,true);
        }

        private void BTN_STOPLiveView_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }
            core.hikvision.LiveView_stop();
            pictureBox1.Invalidate();
        }

        private void BTN_PlayBackCapture_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }

            core.hikvision.PlayBack_Capture_BMP("D:\\haha.bmp");
        }

        private void BTN_LiveViewCapture_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }

            core.hikvision.LiveView_CaptureBMP("D:\\haha_liveView.bmp");
        }

        private void BTN_LiveViewCaptureJPG_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }

            core.hikvision.LiveView_CaptureJPEG(1,"D:\\haha_liveView.jpg");
        }

        private void BTN_Reverse_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }

            core.hikvision.PlayBack_Direction(false);
        }

        private void BTN_DownloadByName_Click(object sender, EventArgs e)
        {
            if (core.hikvision == null || !core.hikvision.isLogin())
            {
                return;
            }

            if (core.hikvision.Download_By_Name(1, listViewFile.FocusedItem.SubItems[0].Text, "D://DownloadByName.mp4"))
            {
                timer1.Interval = 500;
                timer1.Enabled = true;
            }
            else
            {
                MessageBox.Show("Download error");
            }
        }

        private void BTN_Clear_Click(object sender, EventArgs e)
        {
            list_data.Clear();
            listView1.Items.Clear();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {

            if(iAcc<90)
            {
                iAcc++;
                label6.Text = "检索视频中,"+(90- iAcc).ToString() + "s后开始下载视频";
                return;
            }
            DateTime start = time.AddSeconds(-Convert.ToInt16(textBox4.Text));
            DateTime end = time.AddSeconds(Convert.ToInt16(textBox4.Text));


            string mp4Name = time.ToString("yyyymmddhhmmss") + ".mp4";
            if (core.hikvision.Download_By_Time(1, mp4Name, start, end))
            {
                timer1.Interval = 500;
                timer1.Enabled = true;
            }
            else
            {
                //MessageBox.Show("Download error");
                isDownloading = false;
            }

            timer2.Enabled = false;
            iAcc = 0;
            ///////////////////////////
        }

        private void label8_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = null;
            pictureBox2.Invalidate();
        }
    }
}
