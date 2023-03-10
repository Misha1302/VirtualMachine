namespace VirtualMachine;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using global::VirtualMachine.Variables;
using global::VirtualMachine.VmRuntime;

public record VmMemory
{
    private const int DefaultProgramSize = 128;
    private const int RecursionSize = 1024;
    private const string MainFuncName = "__main__";

    private readonly FunctionsPool _functionsPool = new();
    private readonly object?[] _params = new object?[32];
    private readonly int[] _recursionStack = new int[RecursionSize];

    public Dictionary<int, object?> Constants = new(64);
    public FunctionFrame CurrentFunctionFrame;
    public InstructionName[] InstructionsArray = new InstructionName[DefaultProgramSize];

    public int Ip; // instruction pointer
    public int Rp; // recursion pointer

    public VmMemory()
    {
        CurrentFunctionFrame = _functionsPool.GetNewFunction(MainFuncName);
    }

    public Stack<object?> GetStack()
    {
        return new Stack<object?>(CurrentFunctionFrame.Stack[..CurrentFunctionFrame.Sp].Reverse());
    }

    public void Push(object? obj)
    {
        CurrentFunctionFrame.Stack[CurrentFunctionFrame.Sp++] = obj;
    }

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
        CurrentFunctionFrame.AddVariable(vmVariable);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VmVariable FindVariableById(int id)
    {
        return CollectionsMarshal.GetValueRefOrNullRef(CurrentFunctionFrame.Variables, id);
    }

    public void OnCallingFunction(string funcName, int paramsCount)
    {
        _recursionStack[Rp++] = Ip + 2;

        for (int i = 0; i < paramsCount; i++) _params[i] = Pop();
        CurrentFunctionFrame = _functionsPool.GetNewFunction(funcName);
        for (int i = 0; i < paramsCount; i++) Push(_params[i]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnFunctionExit()
    {
        Ip = _recursionStack[--Rp];

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