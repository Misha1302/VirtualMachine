namespace VirtualMachine.VmRuntime;

using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using global::VirtualMachine.Variables;

public partial class VmRuntime
{
    private readonly Stack<int> _recursionStack = new();

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
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Equals()
    {
        ReadTwoNumbers(out decimal a, out decimal b);

        Memory.Push(a.IsEquals(b));
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
        }
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
                Memory.Push(ObjectToString(b) + str1);
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
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Exit(Exception? error)
    {
        OnProgramExit?.Invoke(this, error);
    }

    private void SetVariable()
    {
        if (!_pointersToInsertVariables.TryGetValue(Memory.InstructionPointer, out int varId))
            throw new InvalidOperationException($"variable with id {Memory.InstructionPointer} not found");

        VmVariable vmVariable = _variables.FindLast(x => x.Id == varId) ?? throw new InvalidOperationException();
        vmVariable.ChangeValue(Memory.Pop());
    }

    private void LoadVariable()
    {
        int ip = Memory.InstructionPointer;
        if (!_pointersToInsertVariables.TryGetValue(ip, out int varId))
            throw new InvalidOperationException($"variable with id {ip} not found");

        VmVariable value = _variables.FindLast(x => x.Id == varId) ?? throw new InvalidOperationException();
        Memory.Push(value.Value);
    }

    private void CallMethod()
    {
        object obj = Memory.Constants[Memory.InstructionPointer] ?? throw new InvalidOperationException();

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
                Memory.InstructionPointer = (int)(Memory.Pop() ?? throw new InvalidOperationException());
                break;
        }
    }

    private void JumpIfZero()
    {
        int ip = (int)(Memory.Pop() ?? throw new InvalidOperationException());
        object obj = Memory.Pop() ?? throw new InvalidOperationException();
        switch (obj)
        {
            case decimal d when d.IsEquals(0):
            case false:
                Memory.InstructionPointer = ip;
                break;
        }
    }

    private void LessThan()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a < b);
    }

    private void PushAddress()
    {
        _recursionStack.Push(Memory.InstructionPointer + 2);
    }

    private void Ret()
    {
        Memory.InstructionPointer = _recursionStack.Pop();
    }

    private void Halt()
    {
        Memory.InstructionPointer = int.MaxValue;
    }

    private void Jump()
    {
        object reg = Memory.Pop() ?? throw new InvalidOperationException();
        Memory.InstructionPointer = reg is int i ? i : (int)(decimal)reg;
    }

    private void CreateVariable()
    {
        VmVariable var =
            (VmVariable)(Memory.Constants[Memory.InstructionPointer] ?? throw new InvalidOperationException());
        _variables.Add(var with { Name = GetNextName(var.Name) });
    }

    private static string GetNextName(string varName)
    {
        if (IsNumber().IsMatch(varName[^1].ToString()))
            return varName[..^1] + (int.Parse(varName[^1].ToString()) + 1);
        return varName + '0';
    }

    private void PushConstant()
    {
        object? var = Memory.Constants[Memory.InstructionPointer];
        Memory.Push(var);
    }

    private void Duplicate()
    {
        Memory.Push(Memory.Peek());
    }

    private void CopyVariable()
    {
        int varId = (int)(decimal)(Memory.Constants[Memory.InstructionPointer] ??
                                   throw new InvalidOperationException());
        VmVariable var = _variables.FindLast(x => x.Id == varId) ?? throw new InvalidOperationException();

        _variables.Add(var with { Name = GetNextName(var.Name) });
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex IsNumber();

    private void DeleteVariable()
    {
        int varId = (int)(decimal)(Memory.Constants[Memory.InstructionPointer] ??
                                   throw new InvalidOperationException());

        int index = _variables.FindLastIndex(x => x.Id == varId);
        _variables.RemoveAt(index);
    }

    private static void NoOperation()
    {
        // no operation
    }

    private void GetByIndex()
    {
        ReadTwoValues(out object? list, out object? index);
        IList obj0 = (List<object?>)(list ?? throw new InvalidOperationException());
        object? value = obj0[(int)(decimal)(index ?? throw new InvalidOperationException())];
        Memory.Push(value);
    }
}