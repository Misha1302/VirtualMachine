namespace VirtualMachine.Variables;

public record VmVariable(string Name, bool IsConst = false)
{
    public readonly int Id = IdManager.GetNewId(Name);
    public readonly bool IsConst = IsConst;
    public string Name = Name;
    public object? VariableValue;


    public void ChangeValue(object? value)
    {
        if (IsConst) throw new Exception("Variable is const");
        if (value is ICloneable cloneable)
            VariableValue = cloneable.Clone();
        else VariableValue = value;
    }
}