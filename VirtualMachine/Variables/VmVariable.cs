namespace VirtualMachine.Variables;

public record VmVariable(string Name)
{
    public readonly int Id = IdManager.GetNewId(Name);
    public readonly string Name = Name;
    public object? VariableValue;


    public void ChangeValue(object? value)
    {
        if (value is ICloneable cloneable)
            VariableValue = cloneable.Clone();
        else VariableValue = value;
    }
}