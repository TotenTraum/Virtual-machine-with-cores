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
                         ref Stack stack, ref PSW_t psw, ref Memory mem);
    }

    class EmptyCommand :ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                         ref Stack stack, ref PSW_t psw, ref Memory mem)
        { throw new Exception(); }
    }

    class StopCommand:ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                         ref Stack stack, ref PSW_t psw, ref Memory mem)
        {}
    }

    class STF : ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                         ref Stack stack, ref PSW_t psw, ref Memory mem)
        {
            psw.IP++;
            psw.trapFlag = true;
        }
    }

    class CTF : ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                         ref Stack stack, ref PSW_t psw, ref Memory mem)
        {
            psw.IP++;
            psw.trapFlag = false;
        }
    }
}
