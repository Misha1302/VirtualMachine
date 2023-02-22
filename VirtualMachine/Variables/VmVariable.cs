namespace VirtualMachine.Variables;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4 + 1 + 8 + 8 /*21*/)]
public record VmVariable(string Name, bool IsConst = false)
{
    public readonly int Id = IdManager.GetNewId(Name);
    public readonly bool IsConst = IsConst;
    public string Name = Name;
    public object? Value;

    public void ChangeValue(object? value)
    {
        if (IsConst) throw new Exception("Variable is const");
        Value = value;
    }
}