using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualProcessor
{
    abstract class TransferCommand: ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                        ref Stack stack, ref PSW_t psw, ref Memory mem, ref CriticalSection section)
        {
            psw.IP++; 
            register_t? Address = calcAddress(data,ref regs,ref mem);
            if (!data.arg1.HasValue())
                throw new Exception();
            if (data.arg1_state == Instruction.arg_states.reg)
                run(ref regs[data.arg1.Value()], ref Address, ref stack, ref mem,ref psw);
            else if (data.arg1_state == Instruction.arg_states.mem)
            {
                register_t left_reg = 0;
                run(ref left_reg, ref Address, ref stack, ref mem, ref psw);
                mem[(int)data.arg1.Value().integer] = left_reg;
            }


        }

        private register_t? calcAddress(Instruction.data_t data,ref register_t[] regs, ref Memory mem) 
        {
            register_t Address;
            //Вычисление значения адреса
            if (data.arg2.HasValue() && data.offset.HasValue())
                Address = data.arg2.Value().integer + data.offset.Value().integer;
            else if (data.arg2.HasValue())
                Address = data.arg2.Value();
            else
                return null;

            //Если аргумент это регистр
            if (data.arg2_state == Instruction.arg_states.reg)
                return regs[Address.integer];
            //Если аргумент это участок памяти
            else if (data.arg2_state == Instruction.arg_states.mem)
                return mem[(int)Address.integer];
            else 
                return Address;
        }
        abstract protected void run(ref register_t out_reg, ref register_t? reg,
                                    ref Stack stack,ref Memory mem,ref PSW_t psw);
    }
    // address         = null <=> stdin 
    //                 >=   0 <=> адрес в памяти
    // stack[top]      =    0 <=> int
    //                      1 <=> float
    class Read : TransferCommand
    {
        protected override void run(ref register_t out_reg, ref register_t? address,
                                    ref Stack stack, ref Memory mem, ref PSW_t psw)
        {
            if (address == null)
            {
                register_t type = stack.pop(ref psw.SP);
                if (type == 0)
                    out_reg.integer = Convert.ToInt32(Console.ReadLine());
                else if (type == 1)
                    out_reg.Float = (float)Convert.ToDouble(Console.ReadLine());
                else
                    throw new Exception();
            }
            else if (address.integer >= 0)
                out_reg = mem[(int)address.integer];
            else
                throw new Exception();
        }
    }
    // address         = null <=> stdin 
    //                 >=   0 <=> адрес в памяти
    // stack[top]      =    0 <=> int
    //                      1 <=> float
    //                      2 <=> int с переходом на следующую строку
    //                      3 <=> float с переходом на следующую строку
    class Write : TransferCommand
    {
        protected override void run(ref register_t out_reg, ref register_t? address,
                                    ref Stack stack, ref Memory mem, ref PSW_t psw)
        {
            if (address == null) 
            {
                register_t type = stack.pop(ref psw.SP);
                switch (type)
                {
                    case 0:
                        Console.Write(out_reg.integer.ToString());
                        break;
                    case 1:
                        Console.Write(out_reg.Float.ToString());
                        break;
                    case 2:
                        Console.WriteLine(out_reg.integer.ToString());
                        break;
                    case 3:
                        Console.WriteLine(out_reg.Float.ToString());
                        break;
                    default:
                        throw new Exception();
                }
            }
            else if (address.integer >= 0)
                mem[(int)address.integer] = new(out_reg);
            else
                throw new Exception();
        }
    }

    class Output : TransferCommand
    {
        protected override void run(ref register_t out_reg, ref register_t? address,
                                    ref Stack stack, ref Memory mem, ref PSW_t psw)
        {
            Console.WriteLine(out_reg.integer.ToString());
        }
    }

    class Mov : TransferCommand
    {
        protected override void run(ref register_t out_reg, ref register_t? address,
                                    ref Stack stack, ref Memory mem, ref PSW_t psw)
        {
            if (address != null)
                out_reg = new(address);
            else
                throw new Exception();
        }
    }

    class loadMemory : TransferCommand
    {
        protected override void run(ref register_t out_reg, ref register_t? address,
                                    ref Stack stack, ref Memory mem, ref PSW_t psw)
        {
            if (address != null)
                out_reg = new( mem[(int)address]);
            else
                throw new Exception();
        }
    }

    class Lea : TransferCommand
    {
        protected override void run(ref register_t out_reg, ref register_t? address,
                                    ref Stack stack, ref Memory mem, ref PSW_t psw)
        {
            if (address != null)
                out_reg = new(address);
            else
                throw new Exception();
        }
    }

    class Clr : TransferCommand
    {
        protected override void run(ref register_t out_reg, ref register_t? address,
                                    ref Stack stack, ref Memory mem, ref PSW_t psw)
        {
            out_reg.integer = 0;
        }
    }

    class toInt : TransferCommand
    {
        protected override void run(ref register_t out_reg, ref register_t? address,
                                    ref Stack stack, ref Memory mem, ref PSW_t psw)
        {
            out_reg.integer = (long)out_reg.Float;
        }
    }

    class toFloat : TransferCommand
    {
        protected override void run(ref register_t out_reg, ref register_t? address,
                                    ref Stack stack, ref Memory mem, ref PSW_t psw)
        {
            out_reg.Float = out_reg.integer;
        }
    }
}
