// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

namespace VirtualMachine.VmRuntime;

using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using global::VirtualMachine.Variables;

public partial class VmRuntime
{
    private void Not()
    {
        object obj = Memory.Pop() ?? throw new InvalidOperationException();
        switch (obj)
        {
            case decimal d:
                Memory.Push(d == 0 ? 1m : 0m);
                break;
            case bool b:
                Memory.Push(!b ? 1m : 0m);
                break;
            default:
                throw new StrongTypingException();
        }
    }


    private void Equals()
    {
        object? a = Memory.Pop();
        object? b = Memory.Pop();

        if (a is null && b is null)
        {
            Memory.Push(1m);
            return;
        }

        if (a is null || b is null)
        {
            Memory.Push(null);
            return;
        }

        switch (a)
        {
            case decimal m:
                decimal m1 = (decimal)b;
                Memory.Push(m1.IsEquals(m) ? 1m : 0m);
                break;
            case VmList list:
                VmList list1 = (VmList)b;
                Memory.Push(list1.SequenceEqual(list) ? 1m : 0m);
                break;
            default:
                Memory.Push(a.Equals(b) ? 1m : 0m);
                break;
        }
    }


    private void Div()
    {
        ReadTwoValues(out object? a, out object? b);

        switch (a)
        {
            case decimal m:
                Memory.Push(m / (decimal)(b ?? throw new InvalidOperationException()));
                break;
            case string s:
                VmList strings = new(s.Split((string)(b ?? string.Empty)).Select(x => (object?)x).ToList());
                Memory.Push(strings);
                break;
            default:
                throw new StrongTypingException();
        }
    }


    private void Mul()
    {
        ReadTwoValues(out object? a, out object? b);

        switch (a)
        {
            case decimal m:
                Memory.Push(m * (decimal)(b ?? throw new InvalidOperationException()));
                break;
            case string s:
                StringBuilder stringBuilder = new();
                for (int i = 0; i < (decimal)(b ?? throw new InvalidOperationException()); i++)
                    stringBuilder.Append(s);
                Memory.Push(stringBuilder.ToString());
                break;
            default:
                throw new StrongTypingException();
        }
    }


    private void Sub()
    {
        ReadTwoValues(out object? a, out object? b);

        switch (a)
        {
            case decimal m:
                Memory.Push(m - (decimal)(b ?? throw new InvalidOperationException()));
                break;
            case string s:
                Memory.Push(s.Replace((string)(b ?? string.Empty), ""));
                break;
            default:
                throw new StrongTypingException();
        }
    }


    private void Modulo()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a % b);
    }


    private void Add()
    {
        ReadTwoValues(out object? a, out object? b);

        if (a is string str0)
        {
            Memory.Push(str0 + ObjectToString(b));
        }
        else if (b is string str1)
        {
            if (a is VmList list)
            {
                list.AddToEnd(str1);
                Memory.Push(list);
            }
            else
            {
                Memory.Push(ObjectToString(a) + str1);
            }
        }
        else
        {
            switch (a)
            {
                case decimal m:
                    Memory.Push(m + (decimal)(b ?? throw new InvalidOperationException()));
                    break;
                case VmList collection:
                    collection.AddToEnd(b);
                    Memory.Push(collection);
                    break;
                default:
                    throw new StrongTypingException();
            }
        }
    }


    private void Exit(Exception? error)
    {
        OnProgramExit?.Invoke(this, error);
    }


    private void SetVariable()
    {
        GetVmVariable().ChangeValue(Memory.Pop());
    }

    private void LoadVariable()
    {
        Memory.Push(GetVmVariable().VariableValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private VmVariable GetVmVariable()
    {
        int varId = CollectionsMarshal.GetValueRefOrNullRef(_pointersToInsertVariables, Memory.Ip);
        return Memory.FindVariableById(varId);
    }


    private void CallMethod()
    {
        object obj = Memory.Constants[Memory.Ip] ?? throw new InvalidOperationException();

        int index = obj switch
        {
            decimal d => (int)d,
            int i => i,
            _ => throw new InvalidCastException(obj.GetType().ToString())
        };

        _assemblyManager.GetMethodByIndex(index).Invoke(this);
    }


    private void JumpIfNotZero()
    {
        int ip = (int)(decimal)(Memory.Pop() ?? throw new InvalidOperationException());
        object obj = Memory.Pop() ?? throw new InvalidOperationException();
        switch (obj)
        {
            case decimal d when !d.IsEquals(0):
            case true:
                Jump(ip);
                break;
        }
    }

    private void Jump(int ip)
    {
        Memory.Ip = ip;
    }


    private void JumpIfZero()
    {
        int ip = (int)(decimal)(Memory.Pop() ?? throw new InvalidOperationException());
        object obj = Memory.Pop() ?? throw new InvalidOperationException();

        switch (obj)
        {
            case decimal d when d.IsEquals(0):
            case false:
                Jump(ip);
                break;
        }
    }


    private void LessThan()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a < b ? 1m : 0m);
    }


    private void GreatThan()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a > b ? 1m : 0m);
    }


    private void PushAddress()
    {
        object?[] constants = (object?[])(Memory.Constants[Memory.Ip] ?? throw new InvalidOperationException());

        string funcName = (string)(constants[0] ?? throw new InvalidOperationException());
        int paramsCount = (int)(constants[1] ?? throw new InvalidOperationException());
        Memory.OnCallingFunction(funcName, paramsCount);
    }


    private void Ret()
    {
        Memory.OnFunctionExit();
    }


    private void Halt()
    {
        Memory.Ip = Memory.InstructionsArray.Length + 1;
    }


    private void Jump()
    {
        int ip = (int)(decimal)(Memory.Pop() ?? throw new InvalidOperationException());
        Jump(ip);
    }


    private void CreateVariable()
    {
        VmVariable variable = (VmVariable)(Memory.Constants[Memory.Ip] ?? throw new InvalidOperationException());
        int variableId = variable.Id;

        // ReSharper disable once ForCanBeConvertedToForeach
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (int index = 0; index < Memory.CurrentFunctionFrame.Variables.Len; index++)
            if (Memory.CurrentFunctionFrame.Variables[index].Id == variableId)
                return;

        Memory.CreateVariable(new VmVariable(variable.Name));
    }


    private void PushConstant()
    {
        Memory.Push(Memory.Constants[Memory.Ip]);
    }


    private void Duplicate()
    {
        object? peek = Memory.Peek();
        Memory.Push(peek);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NoOperation()
    {
        // no operation
    }


    private void ElemOf()
    {
        ReadTwoValues(out object? index, out object? list);
        VmList obj0 = (VmList)(list ?? throw new InvalidOperationException());
        object? value = obj0[(int)(decimal)(index ?? throw new InvalidOperationException())];
        Memory.Push(value);
    }

    private void Drop()
    {
        Memory.Drop();
    }

    private void SetElem()
    {
        object? obj = Memory.Pop();
        VmList array = (VmList)(Memory.Pop() ?? throw new InvalidOperationException());
        int index = (int)(decimal)(Memory.Pop() ?? throw new InvalidOperationException());

        array.SetElement(index, obj);
    }

    private void Or()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a == 1 || b == 1 ? 1m : 0m);
    }

    private void And()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a == 1 && b == 1 ? 1m : 0m);
    }
}