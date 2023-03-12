namespace VirtualMachine.VmRuntime;

public class FunctionsPool
{
    private readonly FunctionFrame[] _functionFramesTrace;
    private readonly FunctionFrame[] _pool;
    public readonly VariablesPool VariablesPool = new();
    private int _framesTracePointer;
    private int _poolPointer;

    public FunctionsPool(int maxRecursion)
    {
        _pool = new FunctionFrame[maxRecursion];
        for (int i = 0; i < maxRecursion; i++) _pool[i] = new FunctionFrame(VariablesPool);

        _functionFramesTrace = new FunctionFrame[maxRecursion];
    }

    public FunctionFrame GetNewFunction(string name)
    {
        FunctionFrame functionFrame = _pool[_poolPointer++];
        functionFrame.FuncName = name;
        _functionFramesTrace[_framesTracePointer++] = functionFrame;
        return functionFrame;
    }

    public FunctionFrame FreeFunction()
    {
        _poolPointer--;
        _framesTracePointer--;
        return _functionFramesTrace[_framesTracePointer - 1];
    }

    public List<FunctionFrame> GetTrace()
    {
        return _functionFramesTrace[.._framesTracePointer].ToList();
    }
}