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
        public int SP;
        public bool zeroFlag;
        public bool signFlag;
        public bool OverflowFlag;
        public bool trapFlag;
    }

    partial class Processor
    {
        //Контекст ядра
        class Context_t 
        {
            public register_t[] r;
            public PSW_t PSW;
            public VMProgram prog;
            public Instruction? currentInst;
            public bool isDone = false;

            public Context_t(VMProgram _prog, int nRegisters) 
            {

                PSW = new ();
                prog = _prog;

                PSW.IP = (int)prog.entry_point;
                
                r = new register_t[nRegisters];
                for (int i = 0; i < nRegisters; i++)
                    r[i] = new register_t(0);
            }
        };

        int nRegisters;
        Memory mem;
        Stack stack;

        Core_t[] cores;

        Dispatcher dispatcher;
        public Processor(int nCores,int tick,int _nRegisters,ref Memory memory)
        {
            mem = memory;
            stack = new(ref mem);

            cores = new Core_t[nCores];
            for (int i = 0; i < nCores; i++)
                cores[i] = new(mem,stack);

            nRegisters = _nRegisters;
            dispatcher = new(tick, cores);
            Thread thread = new Thread(new ThreadStart(dispatcher.dispatch));
            thread.Start();
        }

        public void run(VMProgram prog)
        {
            if (prog.entry_point == null) 
                throw new Exception();
                Context_t ctx = new(prog,nRegisters);

            //Отправка в диспетчера
            dispatcher.addContext(ctx);
        }
    }
}
