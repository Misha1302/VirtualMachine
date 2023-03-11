namespace VirtualMachine.VmRuntime;

using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using global::VirtualMachine.Variables;

public partial class VmRuntime
{
    private static readonly object _one = 1m;
    private static readonly object _zero = 0m;

    private void Not()
    {
        object obj = Memory.Pop() ?? throw new InvalidOperationException();
        if (obj is decimal d) Memory.Push(d == 0 ? _one : _zero);
        else throw new StrongTypingException();
    }


    private void Equals()
    {
        object? a = Memory.Pop();
        object? b = Memory.Pop();

        switch (a)
        {
            case decimal m:
                bool isEquals = decimal.Round(m, Decimals) ==
                                decimal.Round((decimal)(b ?? throw new InvalidOperationException()), Decimals);
                Memory.Push(isEquals ? _one : _zero);
                break;
            default:
                Memory.Push(a != null && a.Equals(b) ? _one : _zero);
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

        decimal obj0 = (decimal)(a ?? throw new InvalidOperationException());
        decimal obj1 = (decimal)(b ?? throw new InvalidOperationException());
        Memory.Push(obj0 - obj1);
    }


    private void Modulo()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a % b);
    }


    private void Increase()
    {
        VmVariable variable =
            Memory.FindVariableById((int)(decimal)(GetConstant() ?? throw new InvalidOperationException()));
        variable.VariableValue = (decimal)(variable.VariableValue ?? throw new InvalidOperationException()) + 1;
    }


    private void Decrease()
    {
        VmVariable variable =
            Memory.FindVariableById((int)(decimal)(GetConstant() ?? throw new InvalidOperationException()));
        variable.VariableValue = (decimal)(variable.VariableValue ?? throw new InvalidOperationException()) - 1;
    }


    private void Add()
    {
        ReadTwoValues(out object? a, out object? b);

        if (b is string)
        {
            Memory.Push(ObjectToString(a) + b);
            return;
        }

        switch (a)
        {
            case decimal m:
                Memory.Push(m + (decimal)(b ?? throw new InvalidOperationException()));
                break;
            case string str0:
                Memory.Push(str0 + ObjectToString(b));
                break;
            default:
                if (b is string str1) Memory.Push(ObjectToString(a) + str1);
                else throw new StrongTypingException();
                break;
        }
    }


    private void Exit(Exception? error)
    {
        OnProgramExit?.Invoke(this, error);
    }


    private void SetVariable()
    {
        GetVmVariable().VariableValue = Memory.Pop();
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
        object[] memoryConstant = (object[])(Memory.Constants[Memory.Ip] ?? throw new InvalidOperationException());
        AssemblyManager.CallingDelegate method = (AssemblyManager.CallingDelegate)memoryConstant[0];
        int countOfArgs = (int)memoryConstant[1];

        method.Invoke(this, countOfArgs);
    }

    private void JumpIfNotZero()
    {
        int ip = (int)(decimal)(GetConstant() ?? throw new InvalidOperationException());
        object obj = Memory.Pop() ?? throw new InvalidOperationException();
        switch (obj)
        {
            case decimal d when d != 0:
            case true:
                JumpInternal(ip);
                break;
        }
    }

    private void JumpInternal(int ip)
    {
        Memory.Ip = ip;
    }


    private void JumpIfZero()
    {
        int ip = (int)(decimal)(GetConstant() ?? throw new InvalidOperationException());
        object obj = Memory.Pop() ?? throw new InvalidOperationException();

        if ((decimal)obj == 0) JumpInternal(ip);
    }


    private void LessThan()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a < b ? _one : _zero);
    }


    private void GreatThan()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a > b ? _one : _zero);
    }


    private void PushAddress()
    {
        object?[] constants = (object?[])(Memory.Constants[Memory.Ip] ?? throw new InvalidOperationException());

        string funcName = (string)(constants[0] ?? throw new InvalidOperationException());
        int paramsCount = (int)(constants[1] ?? throw new InvalidOperationException());

        Memory.OnCallingFunction(funcName, paramsCount, Memory.Ip + 1);
    }


    private void Ret()
    {
        Memory.OnFunctionExit();
    }


    private void Halt()
    {
        Memory.Ip = int.MinValue;
    }


    private void Jump()
    {
        int ip = (int)(decimal)(GetConstant() ?? throw new InvalidOperationException());
        JumpInternal(ip);
    }


    private void CreateVariable()
    {
        VmVariable variable = (VmVariable)(Memory.Constants[Memory.Ip] ?? throw new InvalidOperationException());
        if (Memory.CurrentFunctionFrame.Variables.ContainsKey(variable.Id)) return;
        Memory.CreateVariable(new VmVariable(variable.Name));
    }


    private void PushConstant()
    {
        Memory.Push(GetConstant());
    }

    private object? GetConstant()
    {
        return CollectionsMarshal.GetValueRefOrNullRef(Memory.Constants, Memory.Ip);
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

    private void Or()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a == 1 || b == 1 ? _one : _zero);
    }

    private void And()
    {
        ReadTwoNumbers(out decimal a, out decimal b);
        Memory.Push(a == 1 && b == 1 ? _one : _zero);
    }

    private void PushField()
    {
        VmStruct structure = (VmStruct)(Memory.Pop() ?? throw new InvalidOperationException());
        int fieldId = (int)(decimal)(Memory.Constants[Memory.Ip] ?? throw new InvalidOperationException());

        Memory.Push(structure.GetValue(fieldId));
    }

    private void SetField()
    {
        VmStruct structure = (VmStruct)(Memory.Pop() ?? throw new InvalidOperationException());
        int fieldId = (int)(decimal)(Memory.Constants[Memory.Ip] ?? throw new InvalidOperationException());
        object? value = Memory.Pop();

        structure.SetValue(fieldId, value);
    }

    private void NotEquals()
    {
        object? a = Memory.Pop();
        object? b = Memory.Pop();

        switch (a)
        {
            case decimal m:
                bool isEquals = decimal.Round(m, Decimals) ==
                                decimal.Round((decimal)(b ?? throw new InvalidOperationException()), Decimals);
                Memory.Push(isEquals ? _zero : _one);
                break;
            default:
                Memory.Push(a != null && a.Equals(b) ? _zero : _one);
                break;
        }
    }

    private void PushFailed()
    {
        _failedStack.Push((int)(decimal)(GetConstant() ?? throw new InvalidOperationException()));
    }

    private void JumpToFuncMethod()
    {
        object[] constants = (object[])(GetConstant() ?? throw new InvalidOperationException());
        string funcName = (string)(constants[0] ?? throw new InvalidOperationException());
        int paramsCount = (int)(constants[1] ?? throw new InvalidOperationException());

        string labelName = ((VmStruct)(Memory.Pop() ?? throw new InvalidOperationException())).Name + funcName;
        Memory.OnCallingFunction(labelName, paramsCount, Memory.Ip);
        JumpInternal(Memory.GetLabelPointer(labelName));
    }
}