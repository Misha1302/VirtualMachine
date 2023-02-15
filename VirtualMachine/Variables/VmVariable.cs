namespace VirtualMachine.Variables;

[Serializable]
public record VmVariable(string? Name, bool IsConst = false)
{
    public readonly int Id = IdManager.GetNewId();
    public readonly bool IsConst = IsConst;
    public string? Name = Name;
    public object? Value;

    public void ChangeValue(object? value)
    {
        if (IsConst) return;
        Value = value;
    }
}