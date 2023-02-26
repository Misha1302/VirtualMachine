using VirtualMachine.Variables;

namespace VirtualMachine.VmRuntime;

public record FunctionFrame
{
    public readonly object?[] Stack = new object?[16];
    public readonly VmList<VmVariable> Variables = new(16);
    public string? FuncName;
    public int Sp = 0;
}