namespace VirtualMachine;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

public class AssemblyManager
{
    public delegate void CallingDelegate(VmRuntime.VmRuntime vmRuntime, int argsCount);

    private readonly List<CallingDelegate> _methods = new();
    public readonly Dictionary<string, int> ImportedMethods = new();

    public CallingDelegate GetMethodByIndex(int index)
    {
        lock (_methods) return _methods[index];
    }

#pragma warning disable IL2075
    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require object access otherwise can break functionality when trimming application code",
        Justification = "<Pending>")]
    public void ImportMethodFromAssembly(string dllPath, string methodName)
    {
        string assemblyFile = Path.GetFullPath(dllPath);
        if (!File.Exists(assemblyFile)) assemblyFile = @"C:\VirtualMachine\Libs\" + dllPath;
        if (!File.Exists(assemblyFile)) throw new InvalidOperationException($"library {dllPath} was not found");

        Assembly assembly = Assembly.LoadFrom(assemblyFile);
        Type? @class = assembly.GetType("Library.Library");
        if (@class is null) throw new Exception("Class 'Library' in namespace 'Library' was not found");

        MethodInfo method = @class.GetMethod(methodName) ?? throw new Exception($"Method '{methodName}' was not found");
        lock (_methods)
        {
            int methodsCount = _methods.Count;
            _methods.Add(method.CreateDelegate<CallingDelegate>());
            ImportedMethods.Add(method.Name, methodsCount);
        }
    }
#pragma warning restore IL2075
}