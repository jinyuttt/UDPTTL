#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：UDPTTL
* 项目描述 ：
* 类 名 称 ：QueuePool
* 类 描 述 ：
* 命名空间 ：UDPTTL
* CLR 版本 ：4.0.30319.42000
* 作    者 ：jinyu
* 创建时间 ：2019
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ jinyu 2019. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion


using UDPTTL.Package;
using System.Collections.Concurrent;
using System.Threading;
using WinTimer = System.Timers;

namespace UDPTTL
{

    /* ============================================================================== 
* 功能描述：QueuePool 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class QueuePool
    {
       /// <summary>
       /// 默认：50M
       /// </summary>
        public static int Min = 50;

        /// <summary>
        /// 默认：100M
        /// </summary>
        private static int Max = 100;

        /// <summary>
        /// 空闲不使用时间超时
        /// 单位：分钟
        /// 默认：5分钟
        /// </summary>
        public static int IldeTimeOut = 5;

        private static ConcurrentStack<BufferEntity> queue=null;

        private static int Count = 0;//缓存创建大小

        private static WinTimer.Timer timer = null;

        private static int CountTimeNum = 0;

        private static int MaxBufNum = 0;

        private static readonly object obj_lock = new object();

        /// <summary>
        /// 默认：100M
        /// 单位:M
        /// </summary>
        public static int MaxMemory
        {
            get { return Max; }
            set { Max = value;
               MaxBufNum=   Max * 1024 * 1024 / (PackageControl.PackSize - BasePackage.PackSum);
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <returns></returns>
        public static byte[] GetBuffers()
        {
            if(queue==null)
            {
                lock (obj_lock)
                {
                    Init();
                }
            }
            BufferEntity entity;
            if (queue.TryPop(out entity))
            {
                return entity.buf;
            }
            else
            {
                
                    byte[] buf = Create();
                    if (buf == null)
                    {
                        //不能创建了,只能获取
                      return  GetBufferEntity().buf;

                    }
                return buf;
              
            }
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="buf"></param>
        public static  void Roll(byte[]buf)
        {
            queue.Push(new BufferEntity() { buf = buf, LastUse = CountTimeNum });
        }


        /// <summary>
        /// 循环获取
        /// </summary>
        /// <returns></returns>
        private static BufferEntity GetBufferEntity()
        {
            do
            {
                BufferEntity entity = null;
                if (queue.TryPop(out entity))
                {
                    return entity;
                }
                Thread.Sleep(50);
            }
            while (true);
        }


        /// <summary>
        /// 增长创建
        /// </summary>
        /// <returns></returns>
        private static byte[] Create()
        {
            if (Interlocked.Increment(ref Count) < MaxBufNum)
            {
                byte[] buf = new byte[PackageControl.PackSize - BasePackage.PackSum];
                return buf;
            }
            return null;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private static void Init()
        {
            if (queue == null)
            {
                queue = new ConcurrentStack<BufferEntity>();
                int num = Min * 1024 * 1024 / (PackageControl.PackSize - BasePackage.PackSum);
                for (int i = 0; i < num; i++)
                {
                    byte[] buf = new byte[PackageControl.PackSize - BasePackage.PackSum];
                    queue.Push(new BufferEntity() { buf = buf, LastUse = CountTimeNum });
                    Count = num;
                }
                timer = new WinTimer.Timer();
                timer.Interval = 60000;//1分钟
                timer.Elapsed += Timer_Elapsed;
            }

        }

        private static void Timer_Elapsed(object sender, WinTimer.ElapsedEventArgs e)
        {
            //
             CountTimeNum++;
             int num = 0;
            if (!queue.IsEmpty)
            {
               var array=  queue.ToArray();
                foreach (var v in array)
                {
                    if(v.LastUse+IldeTimeOut<CountTimeNum)
                    {
                        num++;
                    }
                }
                int min=  Min * 1024 * 1024 / (PackageControl.PackSize - BasePackage.PackSum);
                num = Count - num > min ? num : Count - min;
                if(num>0)
                {
                    BufferEntity buffer;
                    for(int i=0;i<num;i++)
                    {
                        if(queue.TryPop(out buffer))
                        {
                            //丢弃，回收内存
                            Interlocked.Decrement(ref Count);
                        }
                    }
                }
            }
        }
    }
}
