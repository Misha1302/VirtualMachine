// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

namespace VirtualMachine;

using System.Runtime.CompilerServices;
using global::VirtualMachine.Variables;
using global::VirtualMachine.VmRuntime;

public record VmMemory
{
    private const int RecursionSize = 0xFFFF;
    private const string MainFuncName = "__main__";

    private readonly FunctionsPool _functionsPool = new();
    private readonly object?[] _params = new object?[32];
    private readonly Stack<int> _recursionStack = new(RecursionSize);

    public Dictionary<int, object?> Constants = new(64);
    public FunctionFrame CurrentFunctionFrame;
    public InstructionName[] InstructionsArray = Array.Empty<InstructionName>();

    public int Ip; // instruction pointer

    public VmMemory()
    {
        CurrentFunctionFrame = _functionsPool.GetNewFunction(MainFuncName);
    }

    public Stack<object?> GetStack()
    {
        return new Stack<object?>(CurrentFunctionFrame.Stack[..CurrentFunctionFrame.Sp].Reverse());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(object? obj)
    {
        CurrentFunctionFrame.Stack[CurrentFunctionFrame.Sp++] = obj;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? Pop()
    {
        return CurrentFunctionFrame.Stack[--CurrentFunctionFrame.Sp];
    }

    public object? Peek()
    {
        return CurrentFunctionFrame.Stack[CurrentFunctionFrame.Sp - 1];
    }

    public void CreateVariable(VmVariable vmVariable)
    {
        CurrentFunctionFrame.Variables.AddToEnd(vmVariable);
    }

    public VmVariable FindVariableById(int id)
    {
        VmList<VmVariable> vmList = CurrentFunctionFrame.Variables;

        for (int i = vmList.Len - 1; i >= 0; i--)
            if (vmList[i].Id == id)
                return vmList[i];

        throw new InvalidOperationException();
    }

    public void OnCallingFunction(string funcName, int paramsCount)
    {
        _recursionStack.Push(Ip + 2);

        for (int i = 0; i < paramsCount; i++) _params[i] = Pop();
        CurrentFunctionFrame = _functionsPool.GetNewFunction(funcName);
        for (int i = 0; i < paramsCount; i++) Push(_params[i]);
    }

    public void OnFunctionExit()
    {
        Ip = _recursionStack.Pop();

        object? returnObject = CurrentFunctionFrame.Sp != 0 ? Pop() : null;

        CurrentFunctionFrame = _functionsPool.FreeFunction();

        Push(returnObject);
    }

    public void Drop()
    {
        CurrentFunctionFrame.Sp--;
    }

    public List<FunctionFrame> GetFunctionsFramesTrace()
    {
        return _functionsPool.GetTrace();
    }
}