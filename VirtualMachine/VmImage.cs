namespace VirtualMachine;

using global::VirtualMachine.Variable;

public class VmImage
{
    private readonly List<(string, int)> _goto;
    private readonly Dictionary<string, int> _labels;
    private readonly VmMemory _memory;
    private readonly Dictionary<int, int> _pointersToInsertVariables;
    private readonly List<VmVariable> _variables;
    public readonly AssemblyManager AssemblyManager;

    public VmImage(AssemblyManager? assemblyManager = null)
    {
        AssemblyManager = new AssemblyManager();
        _variables = new List<VmVariable>();
        _pointersToInsertVariables = new Dictionary<int, int>();
        _labels = new Dictionary<string, int>();
        _goto = new List<(string, int)>();
        _memory = new VmMemory(_labels);

        Ip = 0;

        if (assemblyManager is not null) AssemblyManager = assemblyManager;
    }

    public int Ip { get; private set; }

    public void WriteNextOperation(InstructionName operation)
    {
        _memory.InstructionsArray[Ip] = operation;
        Ip++;

        IncreaseMemArrayIfItNeed();
    }

    public void WriteNextOperation(InstructionName operation, params object?[] args)
    {
        _memory.InstructionsArray[Ip] = operation;
        Ip++;

        IncreaseMemArrayIfItNeed();

        if (args.Length != 1) WriteNextConstant(args);
        else
            WriteNextConstant(args[0] switch
            {
                int i => (decimal)i,
                long l => (decimal)l,
                float f => (decimal)f,
                double d => (decimal)d,
                _ => args[0]
            });
    }

    private void IncreaseMemArrayIfItNeed()
    {
        int len = _memory.InstructionsArray.Length;
        if (Ip < len) return;
        Array.Resize(ref _memory.InstructionsArray, len << 1);
    }


    public void WriteNextConstant(object? word, int? position = null)
    {
        int pos = position ?? Ip - 1;
        _memory.Constants.Add(pos, word);
    }

    public VmMemory GetMemory()
    {
        Dictionary<int, object?> constants = _memory.Constants.ToDictionary(entry => entry.Key, entry => entry.Value);
        InstructionName[] memArray = (InstructionName[])_memory.InstructionsArray.Clone();

        WriteNextOperation(InstructionName.Halt);
        WriteNextOperation(InstructionName.EndOfProgram);
        Ip--;
        ReplaceGoto();

        VmMemory memToReturn = new(_labels)
        {
            Constants = _memory.Constants,
            InstructionsArray = _memory.InstructionsArray
        };

        _memory.Constants = constants;
        _memory.InstructionsArray = memArray;

        return memToReturn;
    }

    private void ReplaceGoto()
    {
        foreach ((string key, int position) in _goto)
        {
            int value = _labels[key];
            WriteNextConstant((decimal)value, position);
        }
    }

    public void CreateVariable(string varName)
    {
        VmVariable var = new(varName);
        WriteNextOperation(InstructionName.CreateVariable, var);
        _variables.Add(var);
    }

    public void SetVariable(string varName)
    {
        WriteNextOperation(InstructionName.SetVariable, varName);

        VmVariable keyValuePair = _variables.First(x => x.Name == varName);
        _pointersToInsertVariables.Add(Ip - 1, keyValuePair.Id);
    }

    public void LoadVariable(string varName)
    {
        WriteNextOperation(InstructionName.LoadVariable, varName);

        VmVariable keyValuePair = _variables.First(x => x.Name == varName);
        _pointersToInsertVariables.Add(Ip - 1, keyValuePair.Id);
    }

    public void SetLabel(string label)
    {
        _labels.Add(label, Ip - 1);
    }

    public void Goto(string label, InstructionName jumpInstruction)
    {
        _goto.Add((label, Ip));
        WriteNextOperation(jumpInstruction);
    }

    public Dictionary<int, int> GetPointersToInsertVariables()
    {
        return _pointersToInsertVariables;
    }


    public void CreateFunction(string name, IEnumerable<string> parameters, Action body)
    {
        int constantPtr = Ip;
        WriteNextConstant((decimal)-1, constantPtr);
        WriteNextOperation(InstructionName.Jump);

        SetLabel(name);

        foreach (string parameter in parameters)
        {
            CreateVariable(parameter);
            SetVariable(parameter);
        }

        body();

        WriteNextOperation(InstructionName.Ret);
        _memory.Constants[constantPtr] = (decimal)Ip - 1;
    }

    public void CallFunction(string funcName, int paramsCount)
    {
        WriteNextOperation(InstructionName.PushAddress, funcName, paramsCount);
        Goto(funcName, InstructionName.Jump);
    }

    public void CallStructFunction(string funcNameFunc, int paramsCount)
    {
        WriteNextOperation(InstructionName.JumpToFuncMethod, funcNameFunc, paramsCount);
    }

    public void CallForeignMethod(string name, int argsCount)
    {
        AssemblyManager.CallingDelegate method =
            AssemblyManager.GetMethodByIndex(AssemblyManager.ImportedMethods[name]);

        WriteNextOperation(InstructionName.CallMethod, method, argsCount);
    }

    public void ReplaceConstant(int constantPtr, object? value)
    {
        _memory.Constants[constantPtr] = value;
    }
}