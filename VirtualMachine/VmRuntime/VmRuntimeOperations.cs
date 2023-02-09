namespace VirtualMachine.VmRuntime;

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using global::VirtualMachine.Variables;

public partial class VmRuntime
{
    private readonly Stack<int> _recursionStack = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void NotNumber()
    {
        ReadNumber(out decimal a);
        Memory.Stack.Push(a.IsEquals(0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EqualsNumber()
    {
        ReadTwoNumbers(out decimal a, out decimal b);

        Memory.Stack.Push(a.IsEquals(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DivNumber()
    {
        ReadTwoNumbers(out decimal a, out decimal b);

        Memory.Stack.Push(a / b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MulNumber()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Stack.Push(a * b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SubNumber()
    {
        ReadTwoNumbers(out decimal a, out decimal b);

        Memory.Stack.Push(a - b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddNumber()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Stack.Push(a + b);
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
        vmVariable.ChangeValue(Memory.Stack.Pop());
    }

    private void LoadVariable()
    {
        int ip = Memory.InstructionPointer;
        if (!_pointersToInsertVariables.TryGetValue(ip, out int varId))
            throw new InvalidOperationException($"variable with id {ip} not found");

        VmVariable value = _variables.FindLast(x => x.Id == varId) ?? throw new InvalidOperationException();
        Memory.Stack.Push(value.Value);
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
        object obj = Memory.Stack.Pop() ?? throw new InvalidOperationException();
        switch (obj)
        {
            case decimal d when !d.IsEquals(0):
            case true:
                Memory.InstructionPointer = (int)(Memory.Stack.Pop() ?? throw new InvalidOperationException());
                break;
        }
    }

    private void JumpIfZero()
    {
        int ip = (int)(Memory.Stack.Pop() ?? throw new InvalidOperationException());
        object obj = Memory.Stack.Pop() ?? throw new InvalidOperationException();
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
        Memory.Stack.Push(a < b);
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
        object reg = Memory.Stack.Pop() ?? throw new InvalidOperationException();
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
        Memory.Stack.Push(var);
    }

    private void Duplicate()
    {
        Memory.Stack.Push(Memory.Stack.Peek());
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
}