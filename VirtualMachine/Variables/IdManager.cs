namespace VirtualMachine.Variables;

using System.Runtime.CompilerServices;

public static class IdManager
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNewId(string name)
    {
        return MakeHashCode(name);
    }

    public static int MakeHashCode(string s)
    {
        int sum = 0;
        for (int i = 0; i < s.Length; i++)
            sum += s[i] * (i + 1) + sum;
        return sum;
    }
}