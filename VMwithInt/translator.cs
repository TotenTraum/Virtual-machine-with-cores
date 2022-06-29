using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;



namespace VirtualProcessor
{
    class Translator
    {
        static CommandMap map = getMapOfCommand();  //Словарь команд
        static IndexMap indexesOfVars = new();      //Словарь адресов переменных
        static int address = 0;                     //Абсолютный адрес
        static int offset = 0;                      //Сдвиг адреса


        protected static List<string> readLines(string filename)
        {
            StreamReader reader = new(filename);
            string? str;
            List<string> lines = new();
            while ((str = reader.ReadLine()) != null)
                lines.Add(str);
            return lines;
        }
        //Находит точку входа
        protected static int? findEntryPoint(ref List<string> lines)
        {
            int? entryPoint = null;
            int cmdCounter = 0;
            int i = 0;
            while (i < lines.Count)
            {
                string str = lines[i].Trim();
                if (str.Length > 0)
                    str = str.Substring(0, 1).ToLower();
                if (str == "c")
                    cmdCounter++;
                else if (str == "l" && lines[i].Contains("__start")) 
                {
                    if (entryPoint == null)
                        entryPoint = cmdCounter;
                    else
                        throw new Exception("Неоднократное объявление точки вхождения");
                }
                i++;
            }
            return entryPoint;
        }
        //Возвращает словарь команд
        protected static CommandMap getMapOfCommand()
        {
            CommandMap map1 = new();
            //Arifmetic commands
            map1.Add("inci", new Inc());
            map1.Add("incf", new IncF());
            map1.Add("deci", new Dec());
            map1.Add("decf", new DecF());
            map1.Add("addi", new Add());
            map1.Add("addf", new AddF());
            map1.Add("subi", new Sub());
            map1.Add("subf", new SubF());
            map1.Add("muli", new Mul());
            map1.Add("mulf", new MulF());
            map1.Add("mod", new Mod());
            map1.Add("divi", new Div());
            map1.Add("divf", new DivF());
            map1.Add("cmpi", new Cmp());
            map1.Add("cmpf", new CmpF());
            map1.Add("not", new Not());
            // Transfer command
            map1.Add("read", new Read());
            map1.Add("write", new Write());
            map1.Add("mov", new Mov());
            map1.Add("lea", new Lea());
            map1.Add("clr", new Clr());
            map1.Add("toi", new toInt());
            map1.Add("tof", new toFloat());

            // stack command
            map1.Add("push", new Push());
            map1.Add("pop", new Pop());
            map1.Add("top", new Top());
            map1.Add("si", new StackIndex());
            // jump command
            map1.Add("call", new Call());
            map1.Add("rtn", new Rtn());
            map1.Add("jump", new Jump());
            map1.Add("je", new JE());
            map1.Add("jne", new JNE());
            map1.Add("jl", new JL());
            map1.Add("jle", new JLE());
            map1.Add("jg", new JG());
            map1.Add("jge", new JGE());

            map1.Add("stop", new StopCommand());
            map1.Add("stf", new STF());
            map1.Add("ctf", new CTF());
            return map1;
        }
        //Разделяет аргумент команды
        protected static string[] separateArg(string str)
        {
            List<string> out_str = new();
            if (str.IndexOf("[") != -1)
            {
                out_str.Add(str.Substring(0, str.IndexOf("[")).ToLower().Trim());
                str = str.Remove(0, str.IndexOf("[") + 1);
                if (str.IndexOf("]") == -1)
                    throw new Exception();
                str = str.Remove(str.IndexOf("]"));
            }
            else
                out_str.Add("val");
            // index : offset
            var indexAndOffset = str.Split(":");
            foreach (var val in indexAndOffset)
            {
                out_str.Add(val.Trim());
            }
            if (indexAndOffset.Length == 1)
                out_str.Add("");

            return out_str.ToArray();
        }

        //Делит команду на слова
        protected static List<string> separateCmd(string str)
        {
            List<string> strs = new();

            //Убираем комментарий
            str = str.Split(";")[0].Trim();
            //Убираем лишние пробелы
            str = Regex.Replace(str, @"\s+", " ");
            //Удаляем тег
            str = str.Remove(0, 2);
            //Команда

            if (str.IndexOf(" ") > 0)
            {
                strs.Add(str.Substring(0, str.IndexOf(" ")).ToLower());
                str = str.Remove(0, str.IndexOf(" ") + 1);
                // Аргументы
                string[] splitedArgs = str.Split(",");
                if (splitedArgs.Length > 2)
                    throw new Exception("Количество аргументов больше двух");
                strs.Insert(0, splitedArgs.Length.ToString());
                if (splitedArgs.Length >= 1)
                    strs.AddRange(separateArg(splitedArgs[0]));
                if (splitedArgs.Length == 2)
                    strs.AddRange(separateArg(splitedArgs[1]));
            }
            else if (str.IndexOf(" ") == -1)
            {
                strs.Add(str.ToLower());
                strs.Insert(0, "0");
            }
            return strs;
        }

        //Возвращает разделённые на слова команды с тегом 'c' 
        protected static List<List<string>> getSeparatedCmds(List<string> lines)
        {
            List<List<string>> cmds = new();
            for (int i = 0; i < lines.Count; i++)
            {
                string str = lines[i].Trim();
                if (str.Length > 0)
                    if (str.Substring(0, 1) == "c")
                        cmds.Add(separateCmd(str));
            }
            return cmds;
        }
        //Чтение адреса положения в памяти
        protected static void readOrg(string str)
        {
            //Убираем комментарий
            str = str.Split(";")[0];
            //Убираем лишние пробелы
            str = Regex.Replace(str, @"\s+", " ");
            var strs = str.Split(" ");
            if (strs.Length == 2)
            {
                address = (int)long.Parse(strs[1]);
                offset = 0;
            }
            else
                throw new Exception($"Неправильное объявление адреса положения в строке {str}");
        }
        //Чтение целочисленной переменной
        protected static void readInt(string str, ref Memory mem)
        {
            //Убираем комментарий
            str = str.Split(";")[0];
            //Убираем лишние пробелы
            str = Regex.Replace(str, @"\s+", " ");
            var strs = str.Split(" ");
            if (strs.Length == 3)
            {
                indexesOfVars.Add(strs[1], address + offset);
                mem[address + offset] = long.Parse(strs[2]);
                offset++;
            }
            else if (strs.Length == 2)
            {
                mem[address + offset] = long.Parse(strs[1]);
                offset++;
            }
            else
                throw new Exception($"Неправильное объявление целого числа в строке {str}");
        }
        //Чтение вещественной переменной
        protected static void readFloat(string str, ref Memory mem)
        {
            //Убираем комментарий
            str = str.Split(";")[0];
            //Убираем лишние пробелы
            str = Regex.Replace(str, @"\s+", " ");
            var strs = str.Split(" ");
            if (strs.Length == 3)
            {
                indexesOfVars.Add(strs[1], address + offset);
                mem[address + offset] = float.Parse(strs[2]);
                offset++;
            }
            else if (strs.Length == 2)
            {
                mem[address + offset] = float.Parse(strs[1]);
                offset++;
            }
            else
                throw new Exception($"Неправильное объявление вещественного числа в строке {str}");
        }

        //Считывание переменных
        protected static void readVars(ref List<string> lines, ref Memory mem)
        {
            indexesOfVars.Clear();

            int i = 0;
            while (i < lines.Count)
            {
                string str = lines[i].Trim();
                if (str.Length > 0)
                    str = str.Substring(0, 1).ToLower();

                if (str == "i")
                    readInt(lines[i], ref mem);
                else if (str == "f")
                    readFloat(lines[i], ref mem);
                else if (str == "o")
                    readOrg(lines[i]);
                i++;
            }
        }


        //Смена имен переменных на адреса в памяти
        protected static void replaceAddressOfVars(ref List<List<string>> cmds)
        {
            foreach (var Var in indexesOfVars)
                foreach (var inst in cmds)
                {
                    int? countArg = null;
                    if (inst.Count > 0)
                        countArg = (int)long.Parse(inst[0]);
                    if (countArg != null)
                        for (int i = 0; i < countArg; i++)
                        {
                            if (inst[3 + i * 3] == Var.Key)
                                inst[3 + i * 3] = Var.Value.ToString();
                            if (inst[4 + i * 3] == Var.Key)
                                inst[4 + i * 3] = Var.Value.ToString();
                        }
                }

        }

        //Преобразование в инструкцию
        protected static Instruction toInstruction(List<string> cmd)
        {
            // cmd == | количество аргументов | команда | arg 1 | неиспользуемая область | arg 2 | offset
            Instruction inst;
            Instruction.data_t data = new();

            int countOfArgs = (int)long.Parse(cmd[0]);
            //Начало создания нового объекта команды
            object? obj = Activator.CreateInstance(map[cmd[1]].GetType());
            ICommand opcode;

            if (obj == null)
                throw new Exception();

            opcode = (ICommand)obj;
            inst = new(opcode);
            //Конец создания нового объекта команды

            //Первый аргумент
            if (countOfArgs >= 1)
            {
                if (cmd[2] == "r")
                    data.arg1_state = Instruction.arg_states.reg;
                else if (cmd[2] == "mem")
                    data.arg1_state = Instruction.arg_states.mem;
                else
                    data.arg1_state = Instruction.arg_states.val;
                data.arg1 = (register_t)long.Parse(cmd[3]);
            }
            //Второй аргумент
            if (countOfArgs == 2)
            {
                if (cmd[5] == "r")
                    data.arg2_state = Instruction.arg_states.reg;
                else if (cmd[5] == "mem")
                    data.arg2_state = Instruction.arg_states.mem;
                else
                    data.arg2_state = Instruction.arg_states.val;
                data.arg2 = (register_t)long.Parse(cmd[6]);
                if (cmd[7] != "")
                    data.offset = (register_t)long.Parse(cmd[7]);
            }
            inst.data = data;
            return inst;
        }

        //Прочтение метки
        protected static string? readLabel(string str)
        {
            //Убираем комментарий
            str = str.Split(";")[0];
            //Убираем лишние пробелы
            str = Regex.Replace(str, @"\s+", " ");
            var strs = str.Trim().Split(" ");
            if (strs.Length == 2)
                return strs[1].Split(":")[0].Trim();
            else
                throw new Exception($"Неправильное объявление метки {str}");
        }

        //Поиск меток и замена в строчках комманд их названия на местоположение
        protected static void replaceLabelsNames(ref List<string> lines, ref List<List<string>> cmds)
        {
            int cmdCounter = 0;
            int i = 0;
            IndexMap map = new();
            while (i < lines.Count)
            {
                string? str = lines[i].Trim();
                if (str.Length > 0)
                    str = str.Substring(0, 1).ToLower();
                if (str == "c")
                    cmdCounter++;
                else if (str == "l")
                {
                    str = readLabel(lines[i]);
                    if (str != null && str != "__start")
                    {
                        if (!map.ContainsKey(str))
                            map.Add(str, cmdCounter);
                        else
                            throw new Exception($"Повторное объявление метки {str}");
                    }    
                }
                i++;
            }
            foreach (var item in map)
            {
                foreach (var inst in cmds)
                {
                    int? countArg = null;
                    if (inst.Count > 0)
                        countArg = (int)long.Parse(inst[0]);
                    if (countArg >= 1)
                        if (inst[3] == item.Key)
                            inst[3] = item.Value.ToString();
                }
            }

        }

        protected static void insertIncludes(ref List<string> lines) 
        {
            List<string> included = new();
            for(int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim().Contains("#include") )
                {
                    string lineToRemove = lines[i].Trim();
                    string[] str = Regex.Replace(lines[i], @"\s+", " ").Trim().Split(" ");
                    if (str.Length == 2) 
                    {
                        if (!included.Contains(str[1]))
                        {
                            lines.InsertRange(lines.IndexOf(lines[i]) + 1, readLines(str[1]));
                            included.Add(str[1]);
                        }
                        else
                            throw new Exception($"Повторное включение {str[1]}");
                    }
                    else
                        throw new Exception();
                    lines.RemoveAt(i);
                    i--;
                }
            }
            
        }

        public static VMProgram? translate(string filename, ref Memory mem)
        {
            address = 0;
            VMProgram program = new VMProgram { };
            var lines = readLines(filename);

            //Возможно добавить аля инклуды других файлов
            insertIncludes(ref lines);

            var textCmds = getSeparatedCmds(lines);

            //Ищем точку входа в программу
            int? entry = findEntryPoint(ref lines);
            if (entry == null)
                return null;
            program.entry_point = entry;

            //Записываем переменные в память и заменяем в textCmds
            readVars(ref lines, ref mem);
            replaceAddressOfVars(ref textCmds);

            //Записываем label-ы и удаляем из lines
            replaceLabelsNames(ref lines, ref textCmds);

            //Добавляем готовые инструкции
            foreach (var textCmd in textCmds)
            {
                program.Add(toInstruction(textCmd));
            }

            return program;
        }
    }
}
