using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualProcessor
{
    class Instruction
    {
        public ICommand cmd;
        public data_t data = new data_t();
        public enum arg_states
        {
            reg, // Регистр
            mem, // Память
            val  // По значению
        };
        public struct data_t
        {
            public arg_states arg1_state;
            public arg_states arg2_state;

            public Optional<register_t> arg1 = new Optional<register_t> { };
            public Optional<register_t> arg2 = new Optional<register_t> { };
            public Optional<register_t> offset = new Optional<register_t> { };
        }

        // cmd | dest  | src | offset
        public Instruction(ICommand _cmd, arg_states state1, register_t _arg1,
                    arg_states state2, register_t _arg2, Optional<register_t> _offset)
        {

            data.arg1_state = state1;

            data.arg2_state = state2;

            cmd = _cmd;
            data.arg1 = _arg1;
            data.arg2 = _arg2;
            data.offset = _offset;
        }

        // inc | r[i]
        public Instruction(ICommand _cmd, register_t _arg1)
        {
            cmd = _cmd;
            data.arg1_state = arg_states.reg;
            data.arg1 = _arg1;
        }
        // end | r0
        public Instruction(ICommand _cmd)
        {
            cmd = _cmd;
        }

        
        public Instruction(ICommand _cmd, data_t _data)
        {
            cmd = _cmd;
            data = _data;
        }
    }
}
