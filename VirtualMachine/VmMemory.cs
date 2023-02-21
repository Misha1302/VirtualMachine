namespace VirtualMachine;

using global::VirtualMachine.Variables;

public record VmMemory
{
    private const int StackSize = 0xFFFF;
    private const int RecursionSize = 0xFFFF;

    private readonly object?[] _stack = new object?[StackSize];
    private readonly List<VmVariable> _variables = new(64);
    public readonly Stack<int> RecursionStack = new(RecursionSize);

    private int _sp; // stack pointer
    public Dictionary<int, object?> Constants = new(64);
    public InstructionName[] InstructionsArray = Array.Empty<InstructionName>();

    public int Ip; // instruction pointer


    public Stack<object?> GetStack()
    {
        return new Stack<object?>(_stack);
    }

    public void Push(object? obj)
    {
        _stack[_sp] = obj;
        _sp++;
    }

    public object? Pop()
    {
        _sp--;
        return _stack[_sp];
    }

    public object? Peek()
    {
        return _stack[_sp - 1];
    }

    public void CreateVariable(VmVariable vmVariable)
    {
        vmVariable.ChangeValue(null);
        vmVariable.SetIndex(_variables.Count);
        _variables.Add(vmVariable);
    }

    public List<VmVariable> GetAllVariables()
    {
        return _variables;
    }

    public VmVariable FindVariableById(int id)
    {
        VmVariable? var = _variables.FindLast(x => x.Id == id);
        if (var is null) throw new Exception("Variable not found");
        return var;
    }

    public void DeleteVariable(int varId)
    {
        _variables.RemoveAt(_variables.FindLastIndex(x => x.Id == varId));
    }
}