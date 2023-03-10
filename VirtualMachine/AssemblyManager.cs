namespace VirtualMachine;

using System.Reflection;

public class AssemblyManager
{
    public delegate void CallingDelegate(VmRuntime.VmRuntime vmRuntime, int argsCount);

    private readonly List<CallingDelegate> _methods = new();
    public readonly Dictionary<string, int> ImportedMethods = new();

    public CallingDelegate GetMethodByIndex(int index)
    {
        return _methods[index];
    }


    public void ImportMethod(MethodInfo method)
    {
        int methodsCount = _methods.Count;
        _methods.Add(method.CreateDelegate<CallingDelegate>());
        ImportedMethods.Add(method.Name, methodsCount);
    }
}