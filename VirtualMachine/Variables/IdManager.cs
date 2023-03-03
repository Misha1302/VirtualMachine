namespace VirtualMachine.Variables;

using System.Runtime.CompilerServices;

internal static class IdManager
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNewId(string name)
    {
        return name.GetHashCode();
    }
}