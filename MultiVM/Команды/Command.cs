using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualProcessor
{
    interface ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs, 
                         ref Stack stack, ref PSW_t psw, ref Memory mem, ref CriticalSection section);
    }

    class EmptyCommand :ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                         ref Stack stack, ref PSW_t psw, ref Memory mem, ref CriticalSection section)
        { throw new Exception(); }
    }

    class StopCommand:ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                         ref Stack stack, ref PSW_t psw, ref Memory mem, ref CriticalSection section)
        {}
    }

    class STF : ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                         ref Stack stack, ref PSW_t psw, ref Memory mem, ref CriticalSection section)
        {
            psw.IP++;
            psw.trapFlag = true;
        }
    }

    class CTF : ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                         ref Stack stack, ref PSW_t psw, ref Memory mem, ref CriticalSection section)
        {
            psw.IP++;
            psw.trapFlag = false;
        }
    }

    class SLEEP : ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                         ref Stack stack, ref PSW_t psw, ref Memory mem, ref CriticalSection section)
        {
            psw.IP++;
            long time = 0;
            switch (data.arg1_state)
            {
                case Instruction.arg_states.reg:
                    time = regs[data.arg1.Value()];
                    break;
                case Instruction.arg_states.mem:
                    time = mem[(int)data.arg1.Value()];
                    break;
                case Instruction.arg_states.val:
                    time = data.arg1.Value();
                    break;
            }
            Thread.Sleep((int)time);
        }
    }

    class BegCritSection : ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                         ref Stack stack, ref PSW_t psw, ref Memory mem, ref CriticalSection section)
        {
            psw.IP++;
            section.Counter--;
            section.enter();
            section.Counter++;
            Console.WriteLine($"# {Thread.CurrentThread.Name}, начало крит. секции");
        }
    }

    class EndCritSection : ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                         ref Stack stack, ref PSW_t psw, ref Memory mem, ref CriticalSection section)
        {
            psw.IP++;
            Console.WriteLine($"# {Thread.CurrentThread.Name}, конец крит. секции");
            section.leave();
        }
    }
}
