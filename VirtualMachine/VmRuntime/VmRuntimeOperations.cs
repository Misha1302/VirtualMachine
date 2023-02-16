namespace VirtualMachine.VmRuntime;

using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using global::VirtualMachine.Variables;

public partial class VmRuntime
{
    private static int _varId;
    private readonly Predicate<VmVariable> _predicate = x => x.Id == _varId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Not()
    {
        object obj = Memory.Pop() ?? throw new InvalidOperationException();
        switch (obj)
        {
            case decimal d when !d.IsEquals(0):
                Memory.Push(d == 0);
                break;
            case bool b:
                Memory.Push(!b);
                break;
            default:
                throw new StrongTypingException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Equals()
    {
        object? a = Memory.Pop();
        object? b = Memory.Pop();

        Memory.Push(a?.Equals(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Div()
    {
        ReadTwoValues(out object? a, out object? b);

        switch (a)
        {
            case decimal m:
                Memory.Push(m / (decimal)(b ?? throw new InvalidOperationException()));
                break;
            case string s:
                List<object?> strings = s.Split((string)(b ?? string.Empty)).Select(x => (object?)x).ToList();
                Memory.Push(strings);
                break;
            default:
                throw new StrongTypingException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Mul()
    {
        ReadTwoValues(out object? a, out object? b);

        StringBuilder stringBuilder;
        switch (a)
        {
            case decimal m:
                Memory.Push(m * (decimal)(b ?? throw new InvalidOperationException()));
                break;
            case string s:
                stringBuilder = new StringBuilder();
                for (int i = 0; i < (decimal)(b ?? throw new InvalidOperationException()); i++)
                    stringBuilder.Append(s);
                Memory.Push(stringBuilder.ToString());
                break;
            case char c:
                stringBuilder = new StringBuilder();
                for (int i = 0; i < (decimal)(b ?? throw new InvalidOperationException()); i++)
                    stringBuilder.Append(c);
                Memory.Push(stringBuilder.ToString());
                break;
            default:
                throw new StrongTypingException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Modulo()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a % b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Add()
    {
        ReadTwoValues(out object? a, out object? b);

        if (a is string str0)
        {
            if (b is List<object?> list)
            {
                list.Add(str0);
                Memory.Push(list);
            }
            else
            {
                Memory.Push(str0 + ObjectToString(b));
            }
        }
        else if (b is string str1)
        {
            if (a is List<object?> list)
            {
                list.Add(str1);
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
                case char c:
                    Memory.Push($"{c}{ObjectToString(b)}");
                    break;
                case List<object?> collection:
                    collection.Add(b);
                    Memory.Push(collection);
                    break;
                default:
                    throw new StrongTypingException();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Exit(Exception? error)
    {
        OnProgramExit?.Invoke(this, error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetVariable()
    {
        if (!_pointersToInsertVariables.TryGetValue(Memory.Ip, out _varId))
            throw new InvalidOperationException($"variable with id {Memory.Ip} not found");

        List<VmVariable> allVariables = Memory.GetAllVariables();
        VmVariable vmVariable = allVariables.FindLast(_predicate) ?? throw new InvalidOperationException();
        vmVariable.ChangeValue(Memory.Pop());
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LoadVariable()
    {
        int ip = Memory.Ip;
        if (!_pointersToInsertVariables.TryGetValue(ip, out _varId))
            throw new InvalidOperationException($"variable with id {ip} not found");

        VmVariable value = Memory.GetAllVariables().FindLast(_predicate) ?? throw new InvalidOperationException();
        Memory.Push(value.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void JumpIfZero()
    {
        int ip = (int)(Memory.Pop() ?? throw new InvalidOperationException());
        object obj = Memory.Pop() ?? throw new InvalidOperationException();
        switch (obj)
        {
            case decimal d when d.IsEquals(0):
            case false:
                Memory.Ip = ip;
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LessThan()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a < b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GreatThan()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a > b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PushAddress()
    {
        Memory.RecursionStack.Push(Memory.Ip + 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Ret()
    {
        Memory.Ip = Memory.RecursionStack.Pop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Halt()
    {
        Memory.Ip = int.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Jump()
    {
        object reg = Memory.Pop() ?? throw new InvalidOperationException();
        Memory.Ip = reg is int i ? i : (int)(decimal)reg;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CreateVariable()
    {
        VmVariable var =
            (VmVariable)(Memory.Constants[Memory.Ip] ?? throw new InvalidOperationException());
        Memory.CreateVariable(var with { Name = GetNextName(var.Name) });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetNextName(string varName)
    {
        if (IsNumber().IsMatch(varName[^1].ToString()))
            return varName[..^1] + (int.Parse(varName[^1].ToString()) + 1);
        return varName + '0';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PushConstant()
    {
        object? var = Memory.Constants[Memory.Ip];
        Memory.Push(var);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Duplicate()
    {
        object? peek = Memory.Peek();
        Memory.Push(peek);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVariable()
    {
        int varId = (int)(decimal)(Memory.Constants[Memory.Ip] ??
                                   throw new InvalidOperationException());
        VmVariable var = Memory.GetAllVariables().FindLast(x => x.Id == varId) ?? throw new InvalidOperationException();

        Memory.CreateVariable(var with { Name = GetNextName(var.Name) });
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex IsNumber();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DeleteVariable()
    {
        int varId = (int)(decimal)(Memory.Constants[Memory.Ip] ??
                                   throw new InvalidOperationException());

        Memory.DeleteVariable(varId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NoOperation()
    {
        // no operation
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetByIndex()
    {
        ReadTwoValues(out object? list, out object? index);
        List<object?> obj0 = (List<object?>)(list ?? throw new InvalidOperationException());
        object? value = obj0[(int)(decimal)(index ?? throw new InvalidOperationException())];
        Memory.Push(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Drop()
    {
        _ = Memory.Pop();
    }

    private void GetPtr()
    {
        int id = (int)(decimal)(Memory.Pop() ?? throw new InvalidOperationException());
        ulong offset = Memory.GetAllVariables().FindLast(x => x.Id == id).OffsetPtr;
        Memory.Push((decimal)offset);
    }
}