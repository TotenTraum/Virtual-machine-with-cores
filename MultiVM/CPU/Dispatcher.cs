using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualProcessor
{
    partial class Processor 
    {
        class Dispatcher
        {
            Thread[] threads;
            Core_t[] cores;
            Queue<Context_t>[] ctxQueue;
            Mutex queueMtx;
            int tick;

            public Dispatcher(int _tick, Core_t[] _cores)
            {
                cores = _cores;
                tick = _tick;

                queueMtx = new();
                int Length = 4;// cores.Length;
                threads = new Thread[Length];
                for (int i = 0; i < Length; i++)
                    threads[i] = new Thread(new ThreadStart(cores[i].run));

                foreach (var thread in threads)
                    thread.Start();

                ctxQueue = new Queue<Context_t>[Length];
                for (int i = 0; i < Length; i++)
                    ctxQueue[i] = new();
            }

            public void addContext(Context_t ctx)
            {
                queueMtx.WaitOne();
                int min = ctxQueue[0].Count;
                Queue<Context_t> tmp = ctxQueue[0];
                foreach (var i in ctxQueue)
                    if (min > i.Count)
                        tmp = i;
                tmp.Enqueue(ctx);
                queueMtx.ReleaseMutex();
            }

            public void dispatch()
            {
                int sum = 0;
                while (true)
                {
                    int free = 0;
                    for (int i = 0; i < threads.Length; i++)
                    {
                        queueMtx.WaitOne();

                        if (ctxQueue[i].Count != 0)
                        {
                            var tmp = ctxQueue[i].Dequeue();
                            cores[i].swap_context(ref tmp);
                            if (tmp != null)
                                if (tmp.isDone == false)
                                    ctxQueue[i].Enqueue(tmp);
                        }
                        else
                        {
                            if (cores[i].notWorking != null) 
                                if(cores[i].notWorking == true)
                                    free++;
                        }
                        queueMtx.ReleaseMutex();
                    }
                    if (free == threads.Length)
                        sum++;
                    else
                        sum = 0;
                    if (sum == 50) 
                    {
                        foreach (var i in cores)
                            i.stop();
                        break;
                    }
                    Thread.Sleep(tick);
                }
            }
        }
    }
}
