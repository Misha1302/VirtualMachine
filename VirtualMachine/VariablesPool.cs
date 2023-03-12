namespace VirtualMachine;

using global::VirtualMachine.Variable;

public class VariablesPool
{
    public Dictionary<int, VmVariable> Variables = new(128);
}