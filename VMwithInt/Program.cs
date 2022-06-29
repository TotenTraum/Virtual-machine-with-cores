
using System.Runtime.InteropServices;

namespace VirtualProcessor
{

    class VMProgram : List<Instruction>
    {
        public int? entry_point = null;
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            string file = "main.txt";

            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.InputEncoding = System.Text.Encoding.Unicode;
          
            Memory mem = new(65536);
            Stack stack = new Stack(1024);
            Processor processor = new Processor(8);
            var inst = Translator.translate(file, ref mem);

            if (inst == null)
                throw new Exception();

            processor.run(inst, ref stack, ref mem);
        }
    }

    
}
