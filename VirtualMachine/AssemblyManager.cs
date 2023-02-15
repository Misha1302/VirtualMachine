namespace VirtualMachine;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

public class AssemblyManager
{
    public delegate void CallingDelegate(VmRuntime.VmRuntime vmRuntime);

    private readonly List<CallingDelegate> _methods = new();
    public readonly List<string?> ImportedMethods = new();

    public CallingDelegate GetMethodByIndex(int index)
    {
        lock (_methods) return _methods[index];
    }

#pragma warning disable IL2075
    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require object access otherwise can break functionality when trimming application code",
        Justification = "<Pending>")]
    public void ImportMethodFromAssembly(string dllPath, string? methodName)
    {
        Assembly assembly = Assembly.LoadFrom(Path.GetFullPath(dllPath));
        Type? @class = assembly.GetType("Library.Library");
        if (@class is null) throw new Exception("Class 'Library' in namespace 'Library' not found");

        MethodInfo method = @class.GetMethod(methodName) ?? throw new Exception($"Method '{methodName}' not found");
        lock (_methods)
        {
            _methods.Add(method.CreateDelegate<CallingDelegate>());
            ImportedMethods.Add(method.Name);
        }
    }
#pragma warning restore IL2075
}