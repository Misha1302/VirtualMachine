namespace VirtualMachine;

public record VmMemory
{
    private readonly Stack<object?> _stack = new(0xFFF);
    public readonly Stack<int> RecursionStack = new(0xFFF);
    public Dictionary<int, object?> Constants = new(16);

    public int Ip;

    public InstructionName[] MemoryArray = Array.Empty<InstructionName>();

    public Stack<object?> GetStack()
    {
        return _stack;
    }

    public void Push(object? obj)
    {
        _stack.Push(obj);
    }

    public object? Pop()
    {
        return _stack.Pop();
    }

    public object? Peek()
    {
        return _stack.Peek();
    }
}