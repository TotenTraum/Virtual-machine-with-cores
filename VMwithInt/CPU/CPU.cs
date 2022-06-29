using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualProcessor
{
    public struct PSW_t
    {
        public int IP;
        public bool zeroFlag;
        public bool signFlag;
        public bool OverflowFlag;
        public bool trapFlag;
    }

    class Processor
    {
        private register_t[] r;
        private PSW_t PSW;

        public Processor(int size)
        {
            r = new register_t[size];
            for (int i = 0; i < size; i++)
            {
                r[i] = new register_t(0);
            }
        }

        public int run(VMProgram prog, ref Stack stack, ref Memory memory)
        {
            // Установка указателя на следующую команду
            if (prog.entry_point != null)
                PSW.IP = (int)prog.entry_point;
            else
                throw new Exception();
            while (true)
            {
                Instruction? instruction = prog.ElementAtOrDefault(PSW.IP);
                //Если инструкция не была прочитана
                if (instruction == null)
                    break;
                if (PSW.trapFlag)
                    trapHandler(ref instruction, ref stack, ref memory);
                //Если конец программы
                if (instruction.cmd.GetType().Equals(typeof(StopCommand)))
                    break;
                instruction.cmd.exec(instruction.data, ref r, ref stack, ref PSW, ref memory);
            }
            return 0;
        }
        private void trapHandler(ref Instruction inst, ref Stack stack, ref Memory mem)
        {

            var key = Console.ReadKey(true);
            while (char.ToLower(key.KeyChar) != 'n')
            {
                switch (char.ToLower(key.KeyChar))
                {
                    case 'h':
                        Console.WriteLine("n - перейти к след. команде,");
                        Console.WriteLine("p - вывод r[i] и psw,");
                        Console.WriteLine("m - вывод учатка памяти [l,{0}],", mem.Count - 1);
                        Console.WriteLine("c - вывод названия команды и аргументов.");
                        break;
                    case 'p':
                        for (int i = 0; i < 8; i++)
                            Console.WriteLine("r{0}.int = {1} , .float = {2}", i, r[i].integer, r[i].Float);
                        Console.WriteLine("PSW.IP = {0} , .ZF = {1}, .SF = {2}, .OF = {3}", PSW.IP,
                                                        PSW.zeroFlag, PSW.signFlag, PSW.OverflowFlag);


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
                            {
                                Console.WriteLine("mem[{0}].int = {1} , .float = {2}", l, mem[l].integer, mem[l].Float);
                            }
                        }
                        break;
                    default:
                        break;
                }
                key = Console.ReadKey(true);
            }
        }
    }
}
