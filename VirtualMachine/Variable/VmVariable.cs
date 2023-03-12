namespace VirtualMachine.Variable;

using System.Diagnostics;

[DebuggerDisplay("{Id}::{Name} = {VariableValue}")]
public record VmVariable(string Name)
{
    public readonly int Id = IdManager.GetNewId(Name);
    public readonly string Name = Name;
    public object? VariableValue;
}