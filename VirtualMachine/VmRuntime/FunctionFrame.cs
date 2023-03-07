namespace VirtualMachine.VmRuntime;

using global::VirtualMachine.Variables;

public record FunctionFrame
{
    public readonly object?[] Stack = new object?[16];

    public string? FuncName;
    public int Sp = 0;

    public VmVariable[] Variables = new VmVariable[16];
    public int Vp { get; private set; }

    public void AddVariable(VmVariable variable)
    {
        Variables[Vp++] = variable;
        if (Vp >= Variables.Length) Array.Resize(ref Variables, Variables.Length << 2);
    }
}