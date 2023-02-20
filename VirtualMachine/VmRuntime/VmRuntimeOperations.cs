namespace VirtualMachine.VmRuntime;

using System.Data;
using System.Text;
using System.Text.RegularExpressions;
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
        if (!_pointersToInsertVariables.TryGetValue(Memory.Ip, out int varId))
            throw new InvalidOperationException($"variable with id {Memory.Ip} was not found");

        VmVariable vmVariable = Memory.FindVariableById(varId);
        vmVariable.ChangeValue(Memory.Pop());
    }


    private void LoadVariable()
    {
        if (!_pointersToInsertVariables.TryGetValue(Memory.Ip, out int varId))
            throw new InvalidOperationException($"variable with id {Memory.Ip} was not found");

        VmVariable vmVariable = Memory.FindVariableById(varId);
        Memory.Push(vmVariable.Value);
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
        object obj = Memory.Pop() ?? throw new InvalidOperationException();
        switch (obj)
        {
            case decimal d when !d.IsEquals(0):
            case true:
                Memory.Ip = (int)(Memory.Pop() ?? throw new InvalidOperationException());
                break;
        }
    }


    private void JumpIfZero()
    {
        int ip = (int)(decimal)(Memory.Pop() ?? throw new InvalidOperationException());
        object obj = Memory.Pop() ?? throw new InvalidOperationException();
        switch (obj)
        {
            case decimal d when d.IsEquals(0):
            case false:
                Memory.Ip = ip;
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
        Memory.RecursionStack.Push(Memory.Ip + 2);
    }


    private void Ret()
    {
        Memory.Ip = Memory.RecursionStack.Pop();
    }


    private void Halt()
    {
        Memory.Ip = int.MaxValue;
    }


    private void Jump()
    {
        object reg = Memory.Pop() ?? throw new InvalidOperationException();
        Memory.Ip = reg is int i ? i : (int)(decimal)reg;
    }


    private void CreateVariable()
    {
        VmVariable var =
            (VmVariable)(Memory.Constants[Memory.Ip] ?? throw new InvalidOperationException());
        Memory.CreateVariable(var with { Name = GetNextName(var.Name) });
    }


    private static string GetNextName(string varName)
    {
        if (IsNumber().IsMatch(varName[^1].ToString()))
            return varName[..^1] + (int.Parse(varName[^1].ToString()) + 1);
        return varName + '0';
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


    private void CopyVariable()
    {
        int varId = (int)(decimal)(Memory.Constants[Memory.Ip] ??
                                   throw new InvalidOperationException());
        VmVariable var = Memory.FindVariableById(varId);
        if (var.Id == 0) throw new Exception("Variable not found");

        Memory.CreateVariable(var with { Name = GetNextName(var.Name) });
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex IsNumber();


    private void DeleteVariable()
    {
        int varId = (int)(decimal)(Memory.Constants[Memory.Ip] ??
                                   throw new InvalidOperationException());

        Memory.DeleteVariable(varId);
    }


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
        _ = Memory.Pop();
    }

    private void GetPtr()
    {
        int varId = (int)(decimal)(Memory.Pop() ?? throw new InvalidOperationException());
        int index  = Memory.FindVariableById(varId).Index;
        Memory.Push((decimal)index);
    }

    private void SetToPtr()
    {
        List<VmVariable> allVariables = Memory.GetAllVariables();
        int varId = (int)(decimal)(Memory.Pop() ?? throw new InvalidOperationException());
        int index = Memory.FindVariableById(varId).Index;
        allVariables[index].ChangeValue(Memory.Pop());
    }

    private void PushByPtr()
    {
        List<VmVariable> allVariables = Memory.GetAllVariables();
        int varId = (int)(decimal)(Memory.Pop() ?? throw new InvalidOperationException());
        int index = Memory.FindVariableById(varId).Index;
        Memory.Push(allVariables[index].Value);
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