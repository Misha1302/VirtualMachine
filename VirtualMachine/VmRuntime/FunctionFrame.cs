namespace VirtualMachine.VmRuntime;

using global::VirtualMachine.Variables;

public record FunctionFrame
{
    public string? FuncName;
    public int Sp = 0;
    public object?[] Stack = new object?[16];
    public VmList<VmVariable> Variables = new(16);
}