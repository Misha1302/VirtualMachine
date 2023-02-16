namespace VirtualMachine.Variables;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4 + 1 + 8 + 8 /*21*/)]
public record VmVariable(string Name, bool IsConst = false)
{
    public readonly int Id = IdManager.GetNewId();
    public readonly bool IsConst = IsConst;
    private VmMemory _memory;
    public string Name = Name;
    public ulong OffsetPtr { get; private set; }

    public object? Value
    {
        get => _memory.ReadFromMemory(OffsetPtr);
        private set => _memory.WriteToMemory(OffsetPtr, value);
    }

    public void ChangeValue(object? value)
    {
        if (IsConst) throw new Exception("Variable is const");
        Value = value;
    }

    public void SetOffsetPtr(ulong offsetPtr, VmMemory memory)
    {
        _memory = memory;
        OffsetPtr = offsetPtr;
    }
}