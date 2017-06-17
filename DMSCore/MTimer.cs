using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security; 

namespace MonitorCore
{
    class MTimer
    {
        #region private members
        private long ticksPerSecond = 0;
        private long elapsedTime = 0;
        private long baseTime = 0;
        #endregion

        #region windows API
        /// <summary>  
        /// 获取时间的精度  
        /// </summary>  
        /// <param name="PerformanceFrequency"></param>  
        /// <returns></returns>  
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32")]
        static private extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);
        /// <summary>  
        /// 获取时间计数  
        /// </summary>  
        /// <param name="PerformanceCount"></param>  
        /// <returns></returns>  
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32")]
        static private extern bool QueryPerformanceCounter(ref long PerformanceCount);
        [DllImport("kernel32")]
        static extern IntPtr GetCurrentThread();
        [DllImport("kernel32")]
        static extern IntPtr SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);

        #endregion

        #region constructors
        /// <summary>  
        /// new  
        /// </summary>  
        public MTimer()
        {
            // Use QueryPerformanceFrequency to get frequency of the timer  
            if (!QueryPerformanceFrequency(ref ticksPerSecond))
                throw new ApplicationException("Timer: Performance Frequency Unavailable");
            Reset();
        }
        #endregion

        #region public methods
        /// <summary>  
        /// 重置时间相关计数器  
        /// </summary>  
        public void Reset()
        {
            long time = 0;
            IntPtr threadId = GetCurrentThread();
            IntPtr previous = SetThreadAffinityMask(threadId, new IntPtr(1));
            QueryPerformanceCounter(ref time);
            SetThreadAffinityMask(threadId, previous);
            baseTime = time;
            elapsedTime = 0;
        }
        /// <summary>  
        /// 获取当前与最近一次 reset 时间差,单位：秒  
        /// </summary>  
        /// <returns>The time since last reset.</returns>  
        public double GetTime()
        {
            long time = 0;
            IntPtr threadId = GetCurrentThread();
            IntPtr previous = SetThreadAffinityMask(threadId, new IntPtr(1));
            QueryPerformanceCounter(ref time);
            SetThreadAffinityMask(threadId, previous);

            return (double)(time - baseTime) / (double)ticksPerSecond;
        }
        /// <summary>  
        /// 获取当前系统的时间 ticks 数  
        /// </summary>  
        /// <returns>The current time in seconds.</returns>  
        public double GetAbsoluteTime()
        {
            long time = 0;
            IntPtr threadId = GetCurrentThread();
            IntPtr previous = SetThreadAffinityMask(threadId, new IntPtr(1));
            QueryPerformanceCounter(ref time);
            SetThreadAffinityMask(threadId, previous);
            return (double)time / (double)ticksPerSecond;
        }
        /// <summary>  
        /// 获取此次与上次调用此方法的两次时间差  
        /// </summary>  
        /// <returns>The number of seconds since last call of this function.</returns>  
        public double GetElapsedTime()
        {
            long time = 0;
            IntPtr threadId = GetCurrentThread();
            IntPtr previous = SetThreadAffinityMask(threadId, new IntPtr(1));
            QueryPerformanceCounter(ref time);
            SetThreadAffinityMask(threadId, previous);
            double absoluteTime = (double)(time - elapsedTime) / (double)ticksPerSecond;
            elapsedTime = time;
            return absoluteTime;
        }
        #endregion  
    }
}
