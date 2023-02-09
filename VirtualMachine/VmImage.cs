namespace VirtualMachine;

using System.Collections;
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
    public readonly Dictionary<string, int> ImportedMethodsIndexes;

    private int _index;
    private int _ip;

    public VmImage()
    {
        ImportedMethodsIndexes = new Dictionary<string, int>();
        AssemblyManager = new AssemblyManager();
        _variables = new List<VmVariable>();
        _pointersToInsertVariables = new Dictionary<int, int>();
        _labels = new Dictionary<string, int>();
        _goto = new List<(string, int)>();

        _memory = new VmMemory
        {
            MemoryArray = new byte[BaseProgramSize],
            InstructionPointer = 0
        };

        _ip = 0;
        _index = 0;
    }

    public void WriteNextOperation(InstructionName operation)
    {
        _memory.MemoryArray[_ip] = (byte)operation;
        _ip++;

        IncreaseMemArrayIfItNeed();
    }

    public void WriteNextOperation(InstructionName operation, object? arg)
    {
        _memory.MemoryArray[_ip] = (byte)operation;
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
        byte[] memArray = (byte[])_memory.MemoryArray.Clone();

        WriteNextOperation(InstructionName.End);
        _ip--;
        ReplaceGoto();

        VmMemory memToReturn = new()
        {
            Constants = _memory.Constants,
            Stack = new Stack(),
            InstructionPointer = 0,
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
        VmVariable? var = _variables.FindLast(x => x.Name == varName);

        if (var is not null)
        {
            WriteNextOperation(InstructionName.CopyVariable, var.Id);
        }
        else
        {
            var = new VmVariable(varName);
            WriteNextOperation(InstructionName.CreateVariable, var);
        }

        _variables.Add(var);
    }

    public void SetVariable(string varName)
    {
        WriteNextOperation(InstructionName.SetVariable);

        VmVariable keyValuePair = _variables.First(x => x.Name == varName);
        _pointersToInsertVariables.Add(_ip - 1, keyValuePair.Id);
    }

    public void LoadVariable(string varName)
    {
        WriteNextOperation(InstructionName.LoadVariable);

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

    public void ImportMethodFromAssembly(string dllPath, string methodName)
    {
        if (ImportedMethodsIndexes.ContainsKey(methodName)) return;

        AssemblyManager.ImportMethodFromAssembly(dllPath, methodName);
        ImportedMethodsIndexes.Add(methodName, _index);
        _index++;
    }

    public void CreateFunction(string name)
    {
        WriteNextOperation(InstructionName.Halt);

        SetLabel(name);
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
        int varId = (_variables.FindLast(x => x.Name == varName) ?? throw new InvalidOperationException()).Id;
        WriteNextOperation(InstructionName.DeleteVariable, varId);
    }
}