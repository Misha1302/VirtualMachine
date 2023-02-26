namespace VirtualMachine.VmRuntime;

public class FunctionsPool
{
    private readonly List<FunctionFrame> _functionFramesTrace = new();
    private readonly FunctionFrame[] _pool;
    private int _pointer;

    public FunctionsPool(int maxRecursion = 8192)
    {
        List<FunctionFrame> poolList = new();
        for (int i = 0; i < maxRecursion; i++) poolList.Add(new FunctionFrame());
        _pool = poolList.ToArray();
    }

    public FunctionFrame GetNewFunction(string name)
    {
        FunctionFrame functionFrame = _pool[_pointer++];
        functionFrame.FuncName = name;
        _functionFramesTrace.Add(functionFrame);
        return functionFrame;
    }

    public void FreeFunction()
    {
        _functionFramesTrace.RemoveAt(_functionFramesTrace.Count - 1);
        _pointer--;
    }

    public FunctionFrame GetTopOfTrace()
    {
        return _functionFramesTrace[^1];
    }

    public List<FunctionFrame> GetTrace()
    {
        return _functionFramesTrace;
    }
}