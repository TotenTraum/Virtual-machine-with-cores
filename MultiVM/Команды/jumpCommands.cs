using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualProcessor
{
    abstract class JCommands:ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                        ref Stack stack, ref PSW_t psw, ref Memory mem, ref CriticalSection section)
        {
            psw.IP++;
            register_t Address;
            //Вычисление значения адреса
            //if (data.arg2.HasValue() && data.offset.HasValue())
                //Address = data.arg2.Value().integer + data.offset.Value().integer;
            if (data.arg1.HasValue())
                Address = data.arg1.Value();
            else
                throw new Exception("Отсутствует аргумент");
            //Если аргумент это регистр
            if (data.arg1_state == Instruction.arg_states.reg)
                Address = regs[Address.integer];
            //Если аргумент это участок памяти
            else if (data.arg1_state == Instruction.arg_states.mem)
                Address = mem[(int)Address.integer];

           run((int)Address,ref psw,ref stack);
        }

        abstract protected void run(int address, ref PSW_t psw, ref Stack stack);
    }

    class Call:JCommands
    {
        protected override void run(int address, ref PSW_t psw, ref Stack stack) 
        {
            stack.push(ref psw.SP,psw.IP);
            psw.IP = address;
        }
    }

    class Rtn:JCommands
    {
        protected override void run(int address, ref PSW_t psw, ref Stack stack)
        {
            //Возвращаем IP
            int IP_stack = (int)stack.pop(ref psw.SP);
            //Очистка стека от параметров функции
            while (address > 0) 
            {
                var tmp = stack.pop(ref psw.SP);
                address--;
            }

            psw.IP = IP_stack;
        }
    }

    class Jump : JCommands
    {
        protected override void run(int address, ref PSW_t psw, ref Stack stack)
        {
            psw.IP = address;
        }
    }

    class JE : JCommands
    {
        protected override void run(int address, ref PSW_t psw, ref Stack stack)
        {
            if (psw.zeroFlag)
                psw.IP = address;
        }
    }

    class JNE : JCommands
    {
        protected override void run(int address, ref PSW_t psw, ref Stack stack)
        {
            if (!psw.zeroFlag)
                psw.IP = address;
        }
    }

    class JL : JCommands
    {
        protected override void run(int address, ref PSW_t psw, ref Stack stack)
        {
            if (psw.signFlag != psw.OverflowFlag)
                psw.IP = address;
        }
    }

    class JLE : JCommands
    {
        protected override void run(int address, ref PSW_t psw, ref Stack stack)
        {
            if ((psw.signFlag != psw.OverflowFlag) || psw.zeroFlag)
                psw.IP = address;
        }
    }

    class JG : JCommands
    {
        protected override void run(int address, ref PSW_t psw, ref Stack stack)
        {
            if ((psw.signFlag == psw.OverflowFlag) && !psw.zeroFlag)
                psw.IP = address;
        }
    }

    class JGE : JCommands
    {
        protected override void run(int address, ref PSW_t psw, ref Stack stack)
        {
            if ((psw.signFlag == psw.OverflowFlag) || psw.zeroFlag)
                psw.IP = address;
        }
    }
}
