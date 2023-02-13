namespace VirtualMachine;

[Serializable]
public record VmMemory
{
    private Stack<object?> _stack = new(16);
    public Dictionary<int, object?> Constants = new(16);

    public int InstructionPointer;

    public int MaxStackSize = 1_000_000;
    public byte[] MemoryArray = Array.Empty<byte>();

    public Stack<object?> GetStack()
    {
        return _stack;
    }

    public void Push(object? obj)
    {
        if (_stack.Count > MaxStackSize) throw new Exception("Stack overflow");
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