namespace VirtualMachine.Variables;

internal static class IdManager
{
    public static int GetNewId(string name)
    {
        return name.GetHashCode();
    }
}