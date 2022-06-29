using System;

namespace VirtualProcessor
{
    // Первый операнд всегда регистр
    abstract class ArifmeticCommand : ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                        ref Stack stack, ref PSW_t psw, ref Memory mem)
        {
            psw.IP++;

            if (!data.arg1.HasValue())
                throw new Exception();
            callFunc(regs[data.arg1.Value()], ref data, ref regs, ref mem);
            setFlags(ref psw);
            setRegister(ref regs[data.arg1.Value()]);
        }

        protected static register_t calcSecondArg(Instruction.data_t data, ref register_t[] regs, ref Memory mem)
        {
            register_t Address;
            //Вычисление значения адреса
            if (data.arg2.HasValue() && data.offset.HasValue())
                Address = data.arg2.Value().integer + data.offset.Value().integer;
            else if (data.arg2.HasValue())
                Address = data.arg2.Value();
            else
                throw new Exception("Отсутствует аргумент");

            //Если аргумент это регистр
            if (data.arg2_state == Instruction.arg_states.reg)
                return regs[Address.integer];
            //Если аргумент это участок памяти
            else if (data.arg2_state == Instruction.arg_states.mem)
                return mem[(int)Address.integer];
            else
                return Address;
        }

        abstract protected void callFunc(register_t out_reg, ref Instruction.data_t data,
                                         ref register_t[] regs, ref Memory mem);

        abstract protected void setFlags(ref PSW_t psw);
        abstract protected void setRegister(ref register_t reg);

        abstract protected void setZF(ref PSW_t psw);
        abstract protected void setOF(ref PSW_t psw);
        abstract protected void setSF(ref PSW_t psw);
    }

    abstract class intAC : ArifmeticCommand
    {
        protected static long result;
        protected static bool overflowed;

        protected override void setFlags(ref PSW_t psw)
        {
            setOF(ref psw);
            setSF(ref psw);
            setZF(ref psw);
        }

        protected override sealed void setZF(ref PSW_t psw)
        {
            psw.zeroFlag = (result == 0.0);
        }

        protected override sealed void setOF(ref PSW_t psw)
        {
            psw.OverflowFlag = overflowed;//(int.MinValue > result) || (result > int.MaxValue);
        }

        protected override sealed void setSF(ref PSW_t psw)
        {
            psw.signFlag = result < 0;
        }

        protected override void setRegister(ref register_t reg)
        {
            reg.integer = result;
        }
    }


    abstract class unarIntAC : intAC
    {
        abstract protected long function(long l_arg);
        protected override void callFunc(register_t out_reg, ref Instruction.data_t data,
                                         ref register_t[] regs, ref Memory mem)
        {
            overflowed = false;
            result = function(out_reg);
        }
    }
    abstract class binarIntAC : intAC 
    {
        abstract protected long function(long l_arg, long r_arg);
        protected override void callFunc(register_t out_reg, ref Instruction.data_t data,
                                         ref register_t[] regs, ref Memory mem) 
        {
            overflowed = false;
            register_t arg = calcSecondArg(data, ref regs, ref mem);
            result = function(out_reg, arg);
        }
    }

    class Inc : unarIntAC
    {
        protected override long function(long l_arg)
        {
            try
            {
                checked
                {
                    long tmp = l_arg + 1;
                }
            }
            catch (Exception)
            {
                overflowed = true;
            }
            return ++l_arg;
        }
    }


    class Dec : unarIntAC
    {
        protected override long function(long l_arg)
        {
            try
            {
                checked
                {
                    long tmp = l_arg - 1;
                }
            }
            catch (Exception)
            {
                overflowed = true;
            }
            return --l_arg;
        }
    }


    class Not : unarIntAC
    {
        protected override void setFlags(ref PSW_t psw){}
        protected override long function(long l_arg) => ~l_arg;
    }

    class Add : binarIntAC
    {
        protected override long function(long l_arg,long r_arg)
        {
            try
            {
                checked
                {
                    long tmp = l_arg + r_arg;
                }
            }
            catch (Exception)
            {
                overflowed = true;
            }
            return l_arg + r_arg;
        }
    }


    class Sub : binarIntAC
    {
        protected override long function(long l_arg, long r_arg)
        {
            try
            {
                checked
                {
                    long tmp = l_arg - r_arg;
                }
            }
            catch (Exception)
            {
                overflowed = true;
            }
            return l_arg - r_arg; 
        }
    }


    class Mul : binarIntAC
    {
        protected override long function(long l_arg, long r_arg)
        {
            try
            {
                checked
                {
                    return l_arg * r_arg;
                }
            }
            catch (Exception)
            {
                overflowed = true;
            }
            return l_arg * r_arg;
        }
    }

    class Mod : binarIntAC
    {
        protected override long function(long l_arg, long r_arg) => l_arg % r_arg;
    }

    class Div : binarIntAC
    {
        protected override long function(long l_arg, long r_arg) 
        {
            if (r_arg == 0)
                throw new Exception();
            try
            {
                checked
                {
                    long tmp = l_arg / r_arg;
                }
            }
            catch (Exception)
            {
                overflowed = true;
            }
            return l_arg / r_arg; 
        }
    }

    class Cmp : binarIntAC
    {
        protected override void setRegister(ref register_t reg) { }
        protected override long function(long l_arg, long r_arg) 
        {
            try
            {
                checked
                {
                    long tmp = l_arg - r_arg;
                }
            }
            catch (Exception) 
            {
                overflowed = true;
            }
           return l_arg - r_arg; 
        }
    }
   
    class DecF : unarFloatAC
    {
        protected override float function(float l_arg) => --l_arg;
    }

    abstract class unarFloatAC : floatAC
    {
        abstract protected float function(float l_arg);
        protected override void callFunc(register_t out_reg, ref Instruction.data_t data,
                                         ref register_t[] regs, ref Memory mem)
        {
            result = function(out_reg);
        }
    }
    abstract class binarFloatAC : floatAC
    {
        abstract protected float function(float l_arg, float r_arg);
        protected override void callFunc(register_t out_reg, ref Instruction.data_t data,
                                         ref register_t[] regs, ref Memory mem)
        {
            register_t arg = calcSecondArg(data, ref regs, ref mem);
            result = function(out_reg, arg);
        }
    }
    abstract class floatAC : ArifmeticCommand
    {
        protected static float result;

        protected override void setFlags(ref PSW_t psw)
        {
            setOF(ref psw);
            setSF(ref psw);
            setZF(ref psw);
        }

        protected override sealed void setZF(ref PSW_t psw)
        {
            psw.zeroFlag = (result == 0.0f);
        }

        protected override sealed void setOF(ref PSW_t psw)
        {
            psw.OverflowFlag = float.IsInfinity(result);
        }

        protected override sealed void setSF(ref PSW_t psw)
        {
            psw.signFlag = float.IsNegative(result);
        }

        protected override void setRegister(ref register_t reg) 
        {
            reg.Float = result;
        }
    }
    class CmpF : binarFloatAC
    {
        protected override void setRegister(ref register_t reg){}
        protected override float function(float l_arg, float r_arg) => l_arg - r_arg;
    }
    class IncF : unarFloatAC
    {
        protected override float function(float l_arg) => ++l_arg;
    }
    class AddF : binarFloatAC
    {
        protected override float function(float l_arg,float r_arg) => l_arg + r_arg;
    }
    class DivF : binarFloatAC
    {
        protected override float function(float l_arg, float r_arg)
        {
            if(MathF.Abs(r_arg)<float.Epsilon)
                throw new Exception();
            return l_arg / r_arg;
        }
    }
    class MulF : binarFloatAC
    {
        protected override float function(float l_arg, float r_arg) => l_arg * r_arg;
    }
    class SubF : binarFloatAC
    {
        protected override float function(float l_arg, float r_arg) => l_arg - r_arg;
    }
}
