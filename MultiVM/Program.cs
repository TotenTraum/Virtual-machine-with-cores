
using System.Runtime.InteropServices;

namespace VirtualProcessor
{

    class VMProgram : List<Instruction>
    {
        public int? entry_point = null;
        public string name = "untitled";
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            string file = "main.txt";

            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.InputEncoding = System.Text.Encoding.Unicode;
          
            Memory mem = new(65536);
            //var inst = Translator.translate(file, ref mem);
            Processor processor = new Processor(4,25,8,ref mem);

            //if (inst == null)
            //    throw new Exception();
            /*
            */
            processor.run(Translator.translate("e1.txt", ref mem));
            processor.run(Translator.translate("e2.txt", ref mem));
            processor.run(Translator.translate("e3.txt", ref mem));
            processor.run(Translator.translate("e4.txt", ref mem));
            processor.run(Translator.translate("e5.txt", ref mem));
            processor.run(Translator.translate("e6.txt", ref mem));
            processor.run(Translator.translate("e7.txt", ref mem));
            processor.run(Translator.translate("e8.txt", ref mem));
            /*
            processor.run(Translator.translate("e1K.txt", ref mem));
            processor.run(Translator.translate("e2K.txt", ref mem));
            processor.run(Translator.translate("e3K.txt", ref mem));
            processor.run(Translator.translate("e4K.txt", ref mem));
            processor.run(Translator.translate("e5K.txt", ref mem));
            processor.run(Translator.translate("e6K.txt", ref mem));
            processor.run(Translator.translate("e7K.txt", ref mem));
            processor.run(Translator.translate("e8K.txt", ref mem));
            */
        }
    }

    
}
