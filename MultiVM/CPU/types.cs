using System.Runtime.InteropServices;

namespace VirtualProcessor
{
    [StructLayout(LayoutKind.Explicit)]
    class register_t
    {
        [FieldOffset(0)]
        public long integer;
        [FieldOffset(0)]
        public float Float;

        public register_t(long val)
        {
            integer = val;
        }
        public register_t(float val)
        {
            Float = val;
        }

        public static implicit operator register_t(long value) => new register_t(value);
        public static implicit operator register_t(float value) => new register_t(value);

        public static implicit operator float(register_t value) => value.Float;
        public static implicit operator long(register_t value) => value.integer;
    }

    class CommandMap : Dictionary<string, ICommand>
    {
    }

    class IndexMap : Dictionary<string, int>
    {
    }

    class Memory : List<register_t>
    {
        private Mutex memMutex;
        public Memory(int size) 
        {
            this.AddRange(Enumerable.Repeat<register_t>(0, size));
            memMutex = new();
        }

        new public register_t this[int index]
        {
            get
            {
                memMutex.WaitOne();
                register_t tmp;
                if (0 <= index && index < Count)
                    tmp = base[index];
                else
                    throw new OverflowException("Memory overflow");
                memMutex.ReleaseMutex();
                return tmp;
            }
            set 
            {
                memMutex.WaitOne();
                if (0 <= index && index < Count)
                    base[index] = value;
                else
                    throw new OverflowException("Memory overflow");
                memMutex.ReleaseMutex();
            }
        }
    }
    class Stack
    {
        Memory mem;
        public Stack(ref Memory _mem)
        {
            mem = _mem;
        }

        public void push(ref int SP,register_t val)
        {
            try
            {
                mem[SP] = val;
                SP++;
            }
            catch(OverflowException ex) 
            {
                throw new OverflowException("Stack overflow");
            }
        }

        public register_t top(ref int SP)
        {
            try
            {
                var tmp = mem[SP-1];
                return tmp;
            }
            catch (OverflowException ex)
            {
                throw new OverflowException("Stack overflow");
            }
        }

        public register_t pop(ref int SP)
        {
            try
            {
                var tmp = mem[SP-1];
                SP--;
                return tmp;
            }
            catch (OverflowException ex)
            {
                throw new OverflowException("Stack overflow");
            }
        }

        public register_t this[int index]
        {
            get
            {
                try
                {
                    register_t tmp = mem[index];
                    return tmp;
                }
                catch (OverflowException ex)
                {
                    throw new OverflowException("Stack overflow");
                }
            }
        }
    }
}
