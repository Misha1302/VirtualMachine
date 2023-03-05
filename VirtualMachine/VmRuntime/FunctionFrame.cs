namespace VirtualMachine.VmRuntime;

using System.Runtime.CompilerServices;
using global::VirtualMachine.Variables;

public record FunctionFrame
{
    public readonly object?[] Stack = new object?[16];

    private VmVariable[] _variables = new VmVariable[16];

    public string? FuncName;
    public int Sp = 0;
    public int Vp { get; private set; }

    public void AddVariable(VmVariable variable)
    {
        _variables[Vp++] = variable;
        if (Vp >= _variables.Length) Array.Resize(ref _variables, _variables.Length << 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VmVariable[] GetVariables()
    {
        return _variables;
    }
}