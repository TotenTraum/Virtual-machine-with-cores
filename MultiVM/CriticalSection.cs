using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualProcessor
{
    class CriticalSection
    {
        Random r;
        Mutex mutex;
        public int Counter;
        Thread lockerThread;
        bool captured = false;

        public Thread Thread 
        {
            get 
            {
                return lockerThread;
            }
        }

        public CriticalSection() 
        {
            lockerThread = Thread.CurrentThread;
            r = new();
            mutex = new();
        }

        public void capture() 
        {
            while ((!Thread.CurrentThread.Equals(lockerThread)) && captured) 
                Thread.Sleep(r.Next(1,30));
            mutex.WaitOne();
            Counter++;
            mutex.ReleaseMutex();
        }

        public void release()
        {
            mutex.WaitOne();
            Counter--;
            mutex.ReleaseMutex();
        }

        public void enter() 
        {
            while ((!Thread.CurrentThread.Equals(lockerThread)) && captured)
                Thread.Sleep(r.Next(1, 30));
            lockerThread = Thread.CurrentThread;
            captured = true;
        }

        public void leave() 
        {
            while ((!Thread.CurrentThread.Equals(lockerThread)) && captured)
                Thread.Sleep(r.Next(1, 30));
            captured = false;
            Thread.Sleep(25);
        }
    }
}
