namespace VirtualMachine.VmRuntime;

using global::VirtualMachine.Variables;

public record FunctionFrame(string FuncName)
{
    public readonly string FuncName = FuncName;
    public readonly object?[] Stack = new object?[0x80];
    public readonly List<VmVariable> Variables = new(64);
    public int Sp;
}