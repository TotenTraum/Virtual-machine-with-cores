﻿namespace VirtualProcessor
{
    abstract class stackCommands : ICommand
    {
        public void exec(Instruction.data_t data, ref register_t[] regs,
                        ref Stack stack, ref PSW_t psw, ref Memory mem)
        {
            psw.IP++;

            //Работает только со вторым аргументом и не обращаем внимание на сдвиг
            //Второй аргумент должен быть регистром 
            //if (data.arg1_state != Instruction.arg_states.reg || !data.arg1.HasValue())
            //    throw new Exception("Стековые команды взаимодействуют только с регистрами");

            register_t? arg2 = calcSecondArg(data, ref regs, ref mem);

            if (data.arg1_state == Instruction.arg_states.reg)
                run(data.arg1_state, ref regs[data.arg1.Value()], arg2, ref stack);
            else if (data.arg1_state == Instruction.arg_states.mem)
            {
                register_t reg = mem[(int)data.arg1.Value()];
                run(data.arg1_state, ref reg, arg2, ref stack);
                mem[(int)data.arg1.Value()] = reg;
            }
            else
            {
                register_t reg = data.arg1.Value();
                run(data.arg1_state, ref reg, arg2, ref stack);
            }

        }
        protected static register_t? calcSecondArg(Instruction.data_t data, ref register_t[] regs, ref Memory mem)
        {
            register_t Address;
            //Вычисление значения адреса
            if (data.arg2.HasValue() && data.offset.HasValue())
                Address = data.arg2.Value().integer + data.offset.Value().integer;
            else if (data.arg2.HasValue())
                Address = data.arg2.Value();
            else
                return null;

            //Если аргумент это регистр
            if (data.arg2_state == Instruction.arg_states.reg)
                return regs[data.arg2.Value().integer] -
                        (data.offset.HasValue() ? data.offset.Value().integer : 0);
            //Если аргумент это участок памяти
            else if (data.arg2_state == Instruction.arg_states.mem)
                return mem[(int)Address.integer];
            else
                return Address;
        }

        abstract protected void run(Instruction.arg_states state,ref register_t reg1,
                                    register_t? reg2, ref Stack stack);
    }

    class Push : stackCommands
    {
        protected override void run(Instruction.arg_states state, ref register_t reg, 
                                    register_t? reg2, ref Stack stack)
        {
            stack.push(reg);
        }
    }

    class Pop : stackCommands
    {
        protected override void run(Instruction.arg_states state, ref register_t reg,
                                    register_t? reg2, ref Stack stack)
        {
            if (state == Instruction.arg_states.reg || state == Instruction.arg_states.mem)
                reg = stack.pop();
            else
                throw new Exception();
        }
    }

    class Top : stackCommands
    {
        protected override void run(Instruction.arg_states state, ref register_t reg,
                                    register_t? reg2, ref Stack stack)
        {
            if (state == Instruction.arg_states.reg || state == Instruction.arg_states.mem)
                reg = (reg2 == null) ? new(stack.top()) : new( stack[(int)reg2]);
            else
                throw new Exception();
        }
    }

    class StackIndex : stackCommands
    {
        protected override void run(Instruction.arg_states state, ref register_t reg, 
                                    register_t? reg2, ref Stack stack)
        {
            reg = stack.index - 1;
        }
    }
}
