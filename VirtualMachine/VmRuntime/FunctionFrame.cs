namespace VirtualMachine.VmRuntime;

using global::VirtualMachine.Variables;

public record FunctionFrame
{
    public readonly object?[] Stack = new object?[32];

    public readonly Dictionary<int, VmVariable> Variables = new(32);

    public string? FuncName;
    public int Sp = 0;

    public void AddVariable(VmVariable variable)
    {
        Variables.Add(variable.Id, variable);
    }
}