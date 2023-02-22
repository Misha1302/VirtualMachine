namespace VirtualMachine;

using global::VirtualMachine.Variables;

public record VmMemory
{
    private const int RecursionSize = 0xFFFF;
    private readonly VmList<FunctionFrame> _functionFrames = new();
    public readonly Stack<int> RecursionStack = new(RecursionSize);
    private FunctionFrame _currentFunctionFrame;
    public Dictionary<int, object?> Constants = new(64);
    public InstructionName[] InstructionsArray = Array.Empty<InstructionName>();

    public int Ip; // instruction pointer

    public VmMemory()
    {
        _functionFrames.AddToEnd(new FunctionFrame("main"));
        _currentFunctionFrame = _functionFrames.GetEnd();
    }

    public Stack<object?> GetStack()
    {
        return new Stack<object?>(_currentFunctionFrame.Stack);
    }

    public void Push(object? obj)
    {
        int temp = _currentFunctionFrame.Sp++;
        _currentFunctionFrame.Stack[temp] = obj;
    }

    public object? Pop()
    {
        int temp = --_currentFunctionFrame.Sp;
        return _currentFunctionFrame.Stack[temp];
    }

    public object? Peek()
    {
        return _currentFunctionFrame.Stack[_currentFunctionFrame.Sp - 1];
    }

    public void CreateVariable(VmVariable vmVariable)
    {
        vmVariable.ChangeValue(null);
        _currentFunctionFrame.Variables.Add(vmVariable);
    }

    public List<VmVariable> GetAllVariables()
    {
        return _currentFunctionFrame.Variables;
    }

    public VmVariable FindVariableById(int id)
    {
        VmVariable? var = _currentFunctionFrame.Variables.FindLast(x => x.Id == id);
        if (var is null) throw new Exception("Variable not found");
        return var;
    }

    public void DeleteVariable(int varId)
    {
        _currentFunctionFrame.Variables.RemoveAt(_currentFunctionFrame.Variables.FindLastIndex(x => x.Id == varId));
    }

    public void OnCallingFunction(string funcName, int paramsCount)
    {
        RecursionStack.Push(Ip + 2);
        _functionFrames.AddToEnd(new FunctionFrame(funcName));

        object?[] @params = new object?[32];
        for (int i = 0; i < paramsCount; i++) @params[i] = Pop();

        _currentFunctionFrame = _functionFrames.GetEnd();
        for (int i = 0; i < paramsCount; i++) Push(@params[i]);
    }

    public void OnFunctionExit()
    {
        Ip = RecursionStack.Pop();

        object? returnObject = _currentFunctionFrame.Sp != 0 ? Pop() : null;

        _functionFrames.RemoveEnd();
        _currentFunctionFrame = _functionFrames.GetEnd();

        Push(returnObject);
    }
}

internal struct FunctionFrame
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