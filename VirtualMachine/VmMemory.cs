namespace VirtualMachine;

public record VmMemory
{
    private const int MaxStackSize = 100_000;
    private readonly Stack<object?> _stack = new(0xFFF);
    public Dictionary<int, object?> Constants = new(16);
    public readonly Stack<int> RecursionStack = new(0xFFF);

    public int Ip;

    public byte[] MemoryArray = Array.Empty<byte>();

    public Stack<object?> GetStack()
    {
        return _stack;
    }

    public void Push(object? obj)
    {
        if (_stack.Count >= MaxStackSize) throw new Exception("Stack overflow");
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