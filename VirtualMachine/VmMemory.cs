namespace VirtualMachine;

using global::VirtualMachine.Variables;

public record VmMemory
{
    private const int RecursionSize = 0xFFFF;
    public readonly VmList<FunctionFrame> FunctionFrames = new();
    public FunctionFrame CurrentFunctionFrame;
    public readonly Stack<int> RecursionStack = new(RecursionSize);
    public Dictionary<int, object?> Constants = new(64);
    public InstructionName[] InstructionsArray = Array.Empty<InstructionName>();

    public int Ip; // instruction pointer

    public VmMemory()
    {
        FunctionFrames.AddToEnd(new FunctionFrame("main"));
        CurrentFunctionFrame = FunctionFrames.GetEnd();
    }

    public Stack<object?> GetStack()
    {
        return new Stack<object?>(CurrentFunctionFrame.Stack);
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

        object?[] @params = new object?[32];
        for (int i = 0; i < paramsCount; i++) @params[i] = Pop();

        CurrentFunctionFrame = FunctionFrames.GetEnd();
        for (int i = 0; i < paramsCount; i++) Push(@params[i]);
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

public struct FunctionFrame
{
    public readonly string FuncName;
    public int Sp = 0;
    public readonly object?[] Stack = new object?[0x80];
    public readonly List<VmVariable> Variables = new(64);

    public FunctionFrame(string funcName)
    {
        FuncName = funcName;
    }
}