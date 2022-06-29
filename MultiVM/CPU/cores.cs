using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualProcessor
{
    partial class Processor
    {
        class Core_t
        {
            Context_t? ctx;
            Memory memory;
            Stack stack;
            bool running = true;

            public bool? notWorking
            {
                get 
                {
                    if (ctx != null)
                        return ctx.isDone;
                    else
                        return null;
                }
            }

            static CriticalSection section = new();
            Mutex coreMtx;

            public Core_t(Memory mem, Stack _stack)
            {
                coreMtx = new();
                memory = mem;
                stack = _stack;
            }

            public void swap_context(ref Context_t? _ctx) 
            {
                section.enter();
                var tmp = ctx;
                ctx = _ctx;
                _ctx = tmp;
                section.leave();
            }

            public void stop() 
            {
                section.enter();
                running = false;
                section.leave();
            }

            public void run()
            {
                while (true)
                    if (ctx != null)
                        if (ctx.isDone != true)
                        {

                            section.capture(); // Захватывает поток
                            Thread.CurrentThread.Name = $"name {ctx.prog.name}, thread.id {Thread.CurrentThread.ManagedThreadId.ToString()}";

                            ctx.currentInst = ctx.prog.ElementAtOrDefault(ctx.PSW.IP);

                            //Если инструкция не была прочитана
                            if (ctx.currentInst != null)
                            {
                                if (ctx.PSW.trapFlag)
                                    trapHandler(ref ctx.currentInst, ref memory);

                                //Если конец программы
                                if (ctx.currentInst.cmd.GetType().Equals(typeof(StopCommand)))
                                    ctx.isDone = true;

                                ctx.currentInst.cmd.exec(ctx.currentInst.data, ref ctx.r, ref stack,
                                                        ref ctx.PSW, ref memory, ref section);
                            }
                            section.release();// освобождает поток
                        }
                        else 
                        {
                            if (running)
                                Thread.Sleep(100);
                            else
                                break;
                        } 
            }
            private void trapHandler(ref Instruction inst, ref Memory mem)
            {
                if (ctx == null)
                    throw new Exception();

                section.capture();

                var key = Console.ReadKey(true);
                while (char.ToLower(key.KeyChar) != 'n')
                {
                    switch (char.ToLower(key.KeyChar))
                    {
                        case 'h':
                            Console.WriteLine($@"n - перейти к след. команде,
                                              p - вывод r[i] и psw, 
                                              m - вывод учатка памяти [l,{mem.Count - 1}],
                                              c - вывод названия команды и аргументов. {Thread.CurrentThread.Name}");
                            break;
                        case 'p':
                            for (int i = 0; i < 8; i++)
                                Console.WriteLine("r{0}.int = {1} , .float = {2}", i, ctx.r[i].integer, ctx.r[i].Float);
                            Console.WriteLine("PSW.IP = {0}, PSW.SP = {4} , .ZF = {1}, .SF = {2}, .OF = {3}", ctx.PSW.IP,
                                                            ctx.PSW.zeroFlag, ctx.PSW.signFlag, ctx.PSW.OverflowFlag,ctx.PSW.SP);


                            break;
                        case 'c':
                            Console.WriteLine("cmd = {0},", inst.cmd.ToString());
                            if (inst.data.arg1.HasValue())
                                Console.WriteLine("arg1.int = {0}, .float = {1}", inst.data.arg1.Value().integer,
                                                                                  inst.data.arg1.Value().Float);
                            if (inst.data.arg2.HasValue())
                                Console.WriteLine("arg2.int = {0}, .float = {1}", inst.data.arg2.Value().integer,
                                                                                  inst.data.arg2.Value().Float);
                            if (inst.data.offset.HasValue())
                                Console.WriteLine("offset.int = {0}, .float = {1}", inst.data.offset.Value().integer,
                                                                                  inst.data.offset.Value().Float);
                            break;
                        case 'm':
                            {
                                Console.Write("Введите левую границу:");
                                int l = Convert.ToInt32(Console.ReadLine());
                                Console.Write("Введите правую границу:");
                                int r = Convert.ToInt32(Console.ReadLine());
                                if (r > mem.Count - 1)
                                    break;
                                if (l > r)
                                {
                                    int tmp = l;
                                    l = r;
                                    r = tmp;
                                }
                                
                                for (; l <= r; l++)
                                    Console.WriteLine("mem[{0}].int = {1} , .float = {2}", l, mem[l].integer, mem[l].Float);
                            }
                            break;
                        default:
                            break;
                    }
                    key = Console.ReadKey(true);
                }
                section.release();
            }
        }
    }
}
