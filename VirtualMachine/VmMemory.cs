namespace VirtualMachine;

using System.Runtime.CompilerServices;
using global::VirtualMachine.Variables;

public record VmMemory
{
    private const int StackSize = 0xFFFF;
    private const int RecursionSize = 0xFFFF;
    private const int MemorySize = 0xFFFF;
    private readonly object?[] _stack = new object?[StackSize];
    private readonly List<VmVariable> _variables = new(64);

    public readonly byte[] ArrayOfMemory = GC.AllocateArray<byte>(sizeof(byte) * MemorySize, true);
    public readonly Stack<int> RecursionStack = new(RecursionSize);


    private ulong _mp; // memory pointer
    private int _sp; // stack pointer
    public Dictionary<int, object?> Constants = new(64);

    public int Ip; // instruction pointer
    public InstructionName[] MemoryArray = Array.Empty<InstructionName>();


    public Stack<object?> GetStack()
    {
        return new Stack<object?>(_stack);
    }

    public void Push(object? obj)
    {
        _stack[_sp] = obj;
        Console.Write("@@!!");
        Console.Write(_stack[_sp].GetType());
        Console.WriteLine("@@!!");
        _sp++;
    }

    public object? Pop()
    {
        _sp--;
        object? pop = _stack[_sp];
        Console.Write("@@!");
        Console.Write(pop.GetType());
        Console.WriteLine("@@!");
        return pop;
    }

    public object? Peek()
    {
        object? peek = _stack[_sp - 1];
        Console.Write("@@");
        Console.Write(peek.GetType());
        Console.WriteLine("@@");
        return peek;
    }

    public void CreateVariable(VmVariable vmVariable)
    {
        WriteToMemory(_mp, int.MinValue);
        vmVariable.SetOffsetPtr(_mp, this);

        _variables.Add(vmVariable);
        _mp += 32;
    }

    public List<VmVariable> GetAllVariables()
    {
        return _variables;
    }

    public void DeleteVariable(int varId)
    {
        _variables.RemoveAt(_variables.FindLastIndex(x => x.Id == varId));
    }

    public unsafe void WriteToMemory(ulong offset, object? obj)
    {
        fixed (byte* ptr = ArrayOfMemory)
            Unsafe.Write(ptr + offset, obj);
    }

    public unsafe object? ReadFromMemory(ulong offset)
    {
        fixed (byte* ptr = ArrayOfMemory)
            return Unsafe.Read<object?>(ptr + offset);
    }
}