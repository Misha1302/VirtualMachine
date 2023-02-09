namespace VirtualMachine;

using System.Collections;

[Serializable]
public record VmMemory
{
    public Dictionary<int, object?> Constants = new();

    public int InstructionPointer;
    public byte[] MemoryArray = Array.Empty<byte>();

    public Stack Stack = new();
}