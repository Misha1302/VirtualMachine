namespace VirtualMachine.VmRuntime;

public class VmException : Exception
{
    public VmException(string s) : base(s)
    {
    }

    public VmException()
    {
    }

    public VmException(string a, string b) : base($"{a}\nname: {b}")
    {
    }
}