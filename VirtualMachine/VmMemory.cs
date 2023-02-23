namespace VirtualMachine;

using System.Diagnostics;
using global::VirtualMachine.Variables;
using global::VirtualMachine.VmRuntime;

public record VmMemory
{
    private const int RecursionSize = 0xFFFF;

    private readonly object?[] _params = new object?[32];
    public readonly VmList<FunctionFrame> FunctionFrames = new();
    public readonly Stack<int> RecursionStack = new(RecursionSize);
    public Dictionary<int, object?> Constants = new(64);
    public FunctionFrame CurrentFunctionFrame;
    public InstructionName[] InstructionsArray = Array.Empty<InstructionName>();

    public int Ip; // instruction pointer

    public VmMemory()
    {
        FunctionFrames.AddToEnd(new FunctionFrame("main"));
        CurrentFunctionFrame = FunctionFrames.GetEnd();
    }

    public Stack<object?> GetStack()
    {
        return new Stack<object?>(CurrentFunctionFrame.Stack[..CurrentFunctionFrame.Sp].Reverse());
    }

    public void Push(object? obj)
    {
        int temp = CurrentFunctionFrame.Sp++;
        CurrentFunctionFrame.Stack[temp] = obj;
    }

    public object? Pop()
    {
        int temp = --CurrentFunctionFrame.Sp;
        return CurrentFunctionFrame.Stack[temp];
    }

    public object? Peek()
    {
        return CurrentFunctionFrame.Stack[CurrentFunctionFrame.Sp - 1];
    }

    public void CreateVariable(VmVariable vmVariable)
    {
        vmVariable.ChangeValue(null);
        CurrentFunctionFrame.Variables.Add(vmVariable);
    }

    public List<VmVariable> GetAllVariables()
    {
        return CurrentFunctionFrame.Variables;
    }

    public VmVariable FindVariableById(int id)
    {
        VmVariable? var = CurrentFunctionFrame.Variables.FindLast(x => x.Id == id);
        if (var is null) throw new Exception("Variable not found");
        return var;
    }

    public void DeleteVariable(int varId)
    {
        CurrentFunctionFrame.Variables.RemoveAt(CurrentFunctionFrame.Variables.FindLastIndex(x => x.Id == varId));
    }

    public void OnCallingFunction(string funcName, int paramsCount)
    {
        RecursionStack.Push(Ip + 2);
        FunctionFrames.AddToEnd(new FunctionFrame(funcName));

        for (int i = 0; i < paramsCount; i++) _params[i] = Pop();

        CurrentFunctionFrame = FunctionFrames.GetEnd();
        for (int i = 0; i < paramsCount; i++) Push(_params[i]);
    }

    public void OnFunctionExit()
    {
        Ip = RecursionStack.Pop();

        object? returnObject = CurrentFunctionFrame.Sp != 0 ? Pop() : null;

        FunctionFrames.RemoveEnd();
        CurrentFunctionFrame = FunctionFrames.GetEnd();

        Push(returnObject);
    }
}