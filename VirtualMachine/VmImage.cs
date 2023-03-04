namespace VirtualMachine;

using global::VirtualMachine.Variables;

public class VmImage
{
    private const int BaseProgramSize = 128;

    private readonly List<(string, int)> _goto;
    private readonly Dictionary<string, int> _labels;
    private readonly VmMemory _memory;
    private readonly Dictionary<int, int> _pointersToInsertVariables;
    private readonly List<VmVariable> _variables;
    public readonly AssemblyManager AssemblyManager;

    private int _ip;

    public VmImage(AssemblyManager? assemblyManager = null)
    {
        AssemblyManager = new AssemblyManager();
        _variables = new List<VmVariable>();
        _pointersToInsertVariables = new Dictionary<int, int>();
        _labels = new Dictionary<string, int>();
        _goto = new List<(string, int)>();

        _memory = new VmMemory
        {
            InstructionsArray = new InstructionName[BaseProgramSize],
            Ip = 0
        };

        _ip = 0;

        if (assemblyManager is not null) AssemblyManager = assemblyManager;
    }

    public void WriteNextOperation(InstructionName operation)
    {
        _memory.InstructionsArray[_ip] = operation;
        _ip++;

        IncreaseMemArrayIfItNeed();
    }

    public void WriteNextOperation(InstructionName operation, params object?[] args)
    {
        _memory.InstructionsArray[_ip] = operation;
        _ip++;

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
        if (_ip < len) return;
        Array.Resize(ref _memory.InstructionsArray, len << 1);
    }


    private void WriteNextConstant(object? word, int? position = null)
    {
        int pos = position ?? _ip - 1;
        _memory.Constants.Add(pos, word);
    }

    public VmMemory GetMemory()
    {
        Dictionary<int, object?> constants = _memory.Constants.ToDictionary(entry => entry.Key, entry => entry.Value);
        InstructionName[] memArray = (InstructionName[])_memory.InstructionsArray.Clone();

        WriteNextOperation(InstructionName.EndOfProgram);
        _ip--;
        ReplaceGoto();

        VmMemory memToReturn = new()
        {
            Constants = _memory.Constants,
            Ip = 0,
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
        _pointersToInsertVariables.Add(_ip - 1, keyValuePair.Id);
    }

    public void LoadVariable(string varName)
    {
        WriteNextOperation(InstructionName.LoadVariable, varName);

        VmVariable keyValuePair = _variables.First(x => x.Name == varName);
        _pointersToInsertVariables.Add(_ip - 1, keyValuePair.Id);
    }

    public void SetLabel(string label)
    {
        _labels.Add(label, _ip - 1);
    }

    public void Goto(string label, InstructionName jumpInstruction)
    {
        _goto.Add((label, _ip));
        WriteNextOperation(InstructionName.PushConstant);
        WriteNextOperation(jumpInstruction);
    }

    public Dictionary<int, int> GetPointersToInsertVariables()
    {
        return _pointersToInsertVariables;
    }


    public void CreateFunction(string name, IEnumerable<string> parameters, Action body)
    {
        WriteNextOperation(InstructionName.Halt);

        SetLabel(name);

        foreach (string parameter in parameters)
        {
            CreateVariable(parameter);
            SetVariable(parameter);
        }

        body();

        WriteNextOperation(InstructionName.Ret);
    }

    public void Call(string funcName, int paramsCount)
    {
        WriteNextOperation(InstructionName.PushAddress, funcName, paramsCount);
        Goto(funcName, InstructionName.Jump);
    }

    public void CallForeignMethod(string name)
    {
        WriteNextOperation(InstructionName.CallMethod, AssemblyManager.ImportedMethods[name]);
    }
}