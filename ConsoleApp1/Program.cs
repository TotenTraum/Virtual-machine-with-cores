
using System.Runtime.InteropServices;

namespace VirtualProcessor
{
    internal class Program
    {
        static void thr()
        {
            Thread.Sleep(10000);
        }
        static void Main(string[] args)
        {
            Thread th = new Thread(thr);
            th.Start();
            Console.WriteLine(th.ThreadState.ToString());
        }
    }


}
