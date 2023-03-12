namespace VirtualMachine.VmRuntime;

using global::VirtualMachine.Variable;

public record FunctionFrame(VariablesPool VariablesPool)
{
    private static int _frameId;

    private readonly int _currentFrameId = _frameId++;
    public readonly object?[] Stack = new object?[32];
    public string? FuncName;
    public int Sp;

    public void AddVariable(VmVariable variable)
    {
        VariablesPool.Variables[variable.Id] = variable;
    }
}