namespace VirtualMachine;

using global::VirtualMachine.Variables;

public class VmImage
{
    private const int BaseProgramSize = 128;

    private readonly List<(string, int)> _goto;
    private readonly Dictionary<string, int> _labels;
    private readonly VmMemory _memory;
    private readonly Dictionary<int, int> _pointersToInsertVariables;
    public readonly List<VmVariable> Variables;
    public readonly AssemblyManager AssemblyManager;

    private int _ip;

    private string _labelName = "label0";
    private string _varName = "var0";

    public VmImage(AssemblyManager? assemblyManager = null)
    {
        AssemblyManager = new AssemblyManager();
        Variables = new List<VmVariable>();
        _pointersToInsertVariables = new Dictionary<int, int>();
        _labels = new Dictionary<string, int>();
        _goto = new List<(string, int)>();

        _memory = new VmMemory
        {
            MemoryArray = new InstructionName[BaseProgramSize],
            Ip = 0
        };

        _ip = 0;

        if (assemblyManager is not null) AssemblyManager = assemblyManager;
    }

    public void WriteNextOperation(InstructionName operation)
    {
        _memory.MemoryArray[_ip] = operation;
        _ip++;

        IncreaseMemArrayIfItNeed();
    }

    public void WriteNextOperation(InstructionName operation, object? arg)
    {
        _memory.MemoryArray[_ip] = operation;
        _ip++;

        IncreaseMemArrayIfItNeed();

        arg = arg switch
        {
            int i => (decimal)i,
            long l => (decimal)l,
            float f => (decimal)f,
            double d => (decimal)d,
            _ => arg
        };

        WriteNextConstant(arg);
    }

    private void IncreaseMemArrayIfItNeed()
    {
        int len = _memory.MemoryArray.Length;
        if (_ip < len) return;
        Array.Resize(ref _memory.MemoryArray, len << 1);
    }


    private void WriteNextConstant(object? word, int? position = null)
    {
        int pos = position ?? _ip - 1;
        _memory.Constants.Add(pos, word);
    }

    public VmMemory GetMemory()
    {
        Dictionary<int, object?> constants = _memory.Constants.ToDictionary(entry => entry.Key, entry => entry.Value);
        InstructionName[] memArray = (InstructionName[])_memory.MemoryArray.Clone();

        WriteNextOperation(InstructionName.End);
        _ip--;
        ReplaceGoto();

        VmMemory memToReturn = new()
        {
            Constants = _memory.Constants,
            Ip = 0,
            MemoryArray = _memory.MemoryArray
        };

        _memory.Constants = constants;
        _memory.MemoryArray = memArray;

        return memToReturn;
    }

    private void ReplaceGoto()
    {
        foreach ((string key, int position) in _goto)
        {
            int value = _labels[key];
            WriteNextConstant(value, position);
        }
    }

    public void CreateVariable(string varName)
    {
        VmVariable? var = Variables.FindLast(x => x.Name == varName);

        if (var is not null)
        {
            WriteNextOperation(InstructionName.CopyVariable, var.Id);
        }
        else
        {
            var = new VmVariable(varName);
            WriteNextOperation(InstructionName.CreateVariable, var);
        }

        Variables.Add(var);
    }

    public void SetVariable(string varName)
    {
        WriteNextOperation(InstructionName.SetVariable);

        VmVariable keyValuePair = Variables.First(x => x.Name == varName);
        _pointersToInsertVariables.Add(_ip - 1, keyValuePair.Id);
    }

    public void LoadVariable(string varName)
    {
        WriteNextOperation(InstructionName.LoadVariable);

        VmVariable keyValuePair = Variables.First(x => x.Name == varName);
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

    public void ImportMethodFromAssembly(string dllPath, string methodName)
    {
        if (AssemblyManager.ImportedMethods.ContainsKey(methodName)) return;

        AssemblyManager.ImportMethodFromAssembly(dllPath, methodName);
    }


    public void CreateFunction(string name, string[] parameters, Action body)
    {
        WriteNextOperation(InstructionName.Halt);

        SetLabel(name);

        parameters = parameters.Reverse().ToArray();
        foreach (string parameter in parameters)
        {
            CreateVariable(parameter);
            SetVariable(parameter);
        }

        body();

        foreach (string parameter in parameters)
            DeleteVariable(parameter);

        WriteNextOperation(InstructionName.Ret);
    }

    public void Call(string funcName)
    {
        // 1. push return address
        // 2. goto to label
        WriteNextOperation(InstructionName.PushAddress);
        Goto(funcName, InstructionName.Jump);
    }

    public void DeleteVariable(string varName)
    {
        int varId = (Variables.FindLast(x => x.Name == varName) ?? throw new InvalidOperationException()).Id;
        WriteNextOperation(InstructionName.DeleteVariable, varId);
    }

    public void ForLoop(Action start, Action condition, Action end, Action body)
    {
        string loopLabel = GetNextLabelName();
        string endOfLoopLabel = GetNextLabelName();

        start();
        SetLabel(loopLabel);
        condition();
        Goto(endOfLoopLabel, InstructionName.JumpIfZero);
        body();
        end();
        Goto(loopLabel, InstructionName.Jump);
        SetLabel(endOfLoopLabel);
    }

    public void Repeat(Action start, Action<string> body, Action upperBound)
    {
        string varName = GenerateNextVarName();

        ForLoop(
            () =>
            {
                // i = 0
                CreateVariable(varName);
                start();
                SetVariable(varName);
            },
            () =>
            {
                // i < count
                LoadVariable(varName);
                upperBound();
                WriteNextOperation(InstructionName.LessThan);
            },
            () =>
            {
                // i++
                LoadVariable(varName);
                WriteNextOperation(InstructionName.PushConstant, 1);
                WriteNextOperation(InstructionName.Add);
                SetVariable(varName);
            },
            () => { body(varName); }
        );
    }

    private string GenerateNextVarName()
    {
        return GenerateName(ref _varName);
    }

    private string GetNextLabelName()
    {
        return GenerateName(ref _labelName);
    }

    private static string GenerateName(ref string name)
    {
        int number = Convert.ToInt32(name[^1].ToString());
        int next = number + 1;

        if (next == 10) name += '0';
        else name = name[..^1] + next;

        return name;
    }

    public void CallForeignMethod(string name)
    {
        WriteNextOperation(InstructionName.CallMethod, AssemblyManager.ImportedMethods[name]);
    }
}