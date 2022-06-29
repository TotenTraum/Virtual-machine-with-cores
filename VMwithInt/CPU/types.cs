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
        public Memory(int size) 
        {
            this.AddRange(Enumerable.Repeat<register_t>(0, size));
            
        }
    }
    class Stack<T>
    {
        T[] arr;
        int currentIndex;
        int size;
        public Stack(int _size)
        {
            size = _size;
            arr = new T[size];
        }
        public void push(T val)
        {
            int sizeCheck = size - currentIndex - 1;
            if (sizeCheck > 0)
            {
                currentIndex++;
                arr[sizeCheck] = val;
            }
            else
                throw new Exception();
        }

        public T top()
        {
            int sizeCheck = size - currentIndex;
            if (sizeCheck > 0)
                return arr[sizeCheck];
            else
                throw new Exception();
        }

        public T pop()
        {
            int sizeCheck = size - currentIndex ;
            if (sizeCheck > 0)
            {
                currentIndex--;
                return arr[sizeCheck];
            }
            else
                throw new Exception();
        }

        public T this[int index]
        {
            get
            {
                int sizeCheck = size - index - 1;
                if (sizeCheck > 0 && index <= currentIndex)
                    return arr[size - index - 1];
                else
                    throw new Exception();
            }
        }

        public int index
        {
            get { return currentIndex; }
        }
    }

    class Stack : Stack<register_t>
    {
        public Stack(int _size) : base(_size)
        {
        }
    }
    
}
