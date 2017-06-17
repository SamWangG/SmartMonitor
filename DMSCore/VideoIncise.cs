using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
namespace MonitorCore
{
    class VideoIncise
    {
        private string filename_origin1;
        private string filename_origin2;
        private string filename_dest;

        private VideoCapture cap1;
        private VideoCapture cap2;

        private VideoWriter writer;


        private double fps1;
        private double fps2;

        private double fourCC1;
        private double fourCC2;
        private int mode = 0;
        public VideoIncise()
        {
            

            
            //get video type
            
        }

        public bool open(string filename_origin1, string filename_origin2, string filename_dest)
        {
            mode = 1;
            this.filename_origin1 = filename_origin1;
            this.filename_origin2 = filename_origin2;
            this.filename_dest = filename_dest;
            cap1 = new VideoCapture(filename_origin1);
            cap2 = new VideoCapture(filename_origin2);

            if(!cap1.IsOpened || !cap2.IsOpened)
            {
                return false;
            }
            //get fps
            fps1 = cap1.GetCaptureProperty(CapProp.Fps);
            fps2 = cap2.GetCaptureProperty(CapProp.Fps);

            fourCC1 = cap1.GetCaptureProperty(CapProp.FourCC);
            fourCC2 = cap2.GetCaptureProperty(CapProp.FourCC);

            //double count1=cap1.GetCaptureProperty(CapProp.FrameCount);

            if((cap1.Width==cap2.Width) && (cap1.Height==cap2.Height))
            {
                return true;
            }
            return false;
        }

        public bool open(string filename_origin1, string filename_dest)
        {
            mode = 2;
            this.filename_origin1 = filename_origin1;
            this.filename_dest = filename_dest;
            cap1 = new VideoCapture(filename_origin1);

            if (!cap1.IsOpened)
            {
                return false;
            }
            //get fps
            fps1 = cap1.GetCaptureProperty(CapProp.Fps);

            fourCC1 = cap1.GetCaptureProperty(CapProp.FourCC);
            return true;
        }
        public void Combine(double origin1_start, int origin1_count, int origin2_start, int origin2_count)
        {
            if(mode!=1)
            {
                return;
            }
            cap1.SetCaptureProperty(CapProp.PosFrames, origin1_start);
            cap2.SetCaptureProperty(CapProp.PosFrames, origin2_start);

            Mat mat=new Mat();

            writer = new VideoWriter(filename_dest, (int)fps1, new Size(cap1.Width, cap1.Height), true);

            if (!writer.IsOpened)
            {
                return;
            }

            //第一个文件
            for(int i=0;i<origin1_count;i++)
            {
                mat= cap1.QueryFrame();
                if(mat.IsEmpty)
                {
                    break;
                }
                writer.Write(mat);
            }

            //第二个文件
            for (int i = 0; i < origin2_count; i++)
            {
                mat = cap2.QueryFrame();
                if (mat.IsEmpty)
                {
                    break;
                }
                writer.Write(mat);
                
            }
            mat.Dispose();
            writer.Dispose();
        }

        public void Combine(int origin1_start, int origin1_count)
        {
            if (mode != 2)
            {
                return;
            }
            cap1.SetCaptureProperty(CapProp.PosFrames, origin1_start);

            Mat mat = new Mat();

            writer = new VideoWriter(filename_dest,-1, (int)fps1, new Size(cap1.Width, cap1.Height),true);
            //第一个文件
            if(!writer.IsOpened)
            {
                return;
            }
            
            for (int i = 0; i < origin1_count; i++)
            {
                mat = cap1.QueryFrame();
                if (mat.IsEmpty)
                {
                    break;
                }
                writer.Write(mat);
                
                mat.Dispose();
                
            }
           
            
            writer.Dispose();
        }



        public double FrameSize(int index=0)
        {
            if(index>=2)
            {
                return -1;
            }
            if(index==0)
            {
                return cap1.GetCaptureProperty(CapProp.Fps);
            }
            else
            {
                if (mode==1)
                {
                    return cap2.GetCaptureProperty(CapProp.Fps);
                }
                else
                {
                    return -1;
                }
            }
        }

        public static double FrameSize(string file)
        {
            VideoCapture cap_tmp = new VideoCapture(file);

            if (!cap_tmp.IsOpened)
            {
                return -1.0;
            }
            //get fps
            return cap_tmp.GetCaptureProperty(CapProp.Fps);

        }

        public static double FrameCount(string file)
        {
            VideoCapture cap_tmp = new VideoCapture(file);

            if (!cap_tmp.IsOpened)
            {
                return -1.0;
            }
            //get fps
            return cap_tmp.GetCaptureProperty(CapProp.FrameCount);

        }
    }
}
