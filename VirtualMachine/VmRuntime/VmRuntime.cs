namespace VirtualMachine.VmRuntime;

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using global::VirtualMachine.Variables;

public partial class VmRuntime
{
    private AssemblyManager _assemblyManager;
    private List<Instruction>? _instructions;

    private Dictionary<int, int> _pointersToInsertVariables = new(64);
    public VmMemory Memory;

    public Action<VmRuntime, Exception?>? OnProgramExit;

    public VmRuntime()
    {
        Memory = new VmMemory();
        _assemblyManager = new AssemblyManager();

        PrepareInstructionsForExecution();
    }

    private static void PrepareInstructionsForExecution()
    {
        MethodInfo[] handles = typeof(VmRuntime).GetMethods(
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (MethodInfo handle in handles)
            RuntimeHelpers.PrepareMethod(handle.MethodHandle);
    }

    public void Run()
    {
        Execute(_instructions ?? throw new InvalidOperationException());
    }

    private void Execute(IReadOnlyList<Instruction> instructions)
    {
#if !DEBUG
        try
#endif
        {
            int instructionsCount = instructions.Count;
            for (Memory.Ip = 0; Memory.Ip < instructionsCount; Memory.Ip++)
                instructions[Memory.Ip]();
        }
#if !DEBUG
        catch (Exception ex)
        {
            Exit(ex);
            return;
        }
#endif

        Exit(null);
    }

    private List<Instruction> GenerateDictToExecute()
    {
        int ip = 0;
        InstructionName operation = Memory.MemoryArray[ip];

        List<Instruction> instructions = new();

        while (operation != InstructionName.End)
        {
            Instruction instr = operation switch
            {
                InstructionName.Add => Add,
                InstructionName.Sub => Sub,
                InstructionName.Modulo => Modulo,
                InstructionName.Multiply => Mul,
                InstructionName.Divide => Div,
                InstructionName.Equals => Equals,
                InstructionName.Not => Not,
                InstructionName.JumpIfZero => JumpIfZero,
                InstructionName.JumpIfNotZero => JumpIfNotZero,
                InstructionName.SetVariable => SetVariable,
                InstructionName.LoadVariable => LoadVariable,
                InstructionName.CallMethod => CallMethod,
                InstructionName.Duplicate => Duplicate,
                InstructionName.LessThan => LessThan,
                InstructionName.GreatThan => GreatThan,
                InstructionName.Halt => Halt,
                InstructionName.Ret => Ret,
                InstructionName.Drop => Drop,
                InstructionName.PushAddress => PushAddress,
                InstructionName.Jump => Jump,
                InstructionName.CreateVariable => CreateVariable,
                InstructionName.CopyVariable => CopyVariable,
                InstructionName.PushConstant => PushConstant,
                InstructionName.DeleteVariable => DeleteVariable,
                InstructionName.NoOperation => NoOperation,
                InstructionName.GetByIndex => GetByIndex,
                InstructionName.GetPtr => GetPtr,
                _ or 0 => throw new InvalidOperationException($"unknown instruction - {operation}")
            };
            instructions.Add(instr);

            ip++;

            operation = Memory.MemoryArray[ip];
        }

        return instructions;
    }

    public void SetImage(VmImage image)
    {
        Memory = image.GetMemory();

        _pointersToInsertVariables = image.GetPointersToInsertVariables();
        _assemblyManager = image.AssemblyManager;

        _instructions = GenerateDictToExecute();
    }

    public void PrintState()
    {
        StringBuilder outputStringBuilder = new();

        outputStringBuilder.Append("Stack={ ");
        outputStringBuilder.Append(string.Join(",", Memory.GetStack().ToArray().Select(x => x switch
        {
            string s => $"\"{s}\"",
            char c => $"\'{c}\'",
            decimal m => m.ToString(CultureInfo.InvariantCulture).Replace(',', '.'),
            _ => x?.ToString()
        })));
        outputStringBuilder.AppendLine("}");


        outputStringBuilder.Append("Variables={");
        outputStringBuilder.Append(string.Join(",", Memory.GetAllVariables().Select(x =>
        {
            string obj = x.Value switch
            {
                string s => $"\"{s}\"",
                char c => $"\'{c}\'",
                decimal m => m.ToString(CultureInfo.InvariantCulture).Replace(',', '.'),
                _ => x.Value?.ToString()
            } ?? string.Empty;

            return x.Name + "=" + obj;
        })));
        outputStringBuilder.AppendLine("}");

        Console.WriteLine(outputStringBuilder);
    }

    private void ReadTwoValues(out object? obj0, out object? obj1)
    {
        obj1 = Memory.Pop();
        obj0 = Memory.Pop();
    }

    private void ReadTwoNumbers(out decimal a, out decimal b)
    {
        ReadTwoValues(out object? obj0, out object? obj1);
        a = (decimal)(obj0 ?? throw new InvalidOperationException());
        b = (decimal)(obj1 ?? throw new InvalidOperationException());
    }

    private static string NumberToString(decimal m)
    {
        return m.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
    }

    public static string ObjectToString(object? obj)
    {
        switch (obj)
        {
            case null:
                return "null";
            case decimal i:
                i = decimal.Round(i, 23);
                return i.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
            case string s:
                return s;
            case List<object> list when list.All(x => x is char):
                return new string(list.Select(x => (char)x).ToArray());
            case IEnumerable collection:
            {
                StringBuilder stringBuilder = new();
                stringBuilder.Append('[');
                foreach (object? item in collection)
                {
                    string str = item switch
                    {
                        null => "null, ",
                        decimal m => $"{NumberToString(m)}, ",
                        string s => $"\"{s}\", ",
                        _ => $"{item}, "
                    };
                    stringBuilder.Append(str);
                }

                stringBuilder.Remove(stringBuilder.Length - 2, 2);
                stringBuilder.Append(']');
                return stringBuilder.ToString();
            }
        }

        if (obj is IFormattable f)
            return f.ToString(null, null);
        return obj.ToString() ?? string.Empty;
    }

    private delegate void Instruction();
}