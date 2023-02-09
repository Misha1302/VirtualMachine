namespace VirtualMachine.Variables;

internal static class IdManager
{
    private static int _maxId;

    public static int GetNewId()
    {
        return _maxId++;
    }
}