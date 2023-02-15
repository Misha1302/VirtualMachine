namespace VirtualMachine.VmRuntime;

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using global::VirtualMachine.Variables;

public partial class VmRuntime
{
    private readonly List<VmVariable> _variables = new(64);
    private AssemblyManager _assemblyManager;

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
        List<Instruction> instructions = GenerateDictToExecute();

        Execute(instructions);
    }

    private void Execute(IReadOnlyList<Instruction> instructions)
    {
        int instructionsCount = instructions.Count - 1;
#if !DEBUG
        try
#endif
        {
            Memory.Ip = -1;
            do
            {
                Memory.Ip++;
                Instruction inst = instructions[Memory.Ip];
                // Console.WriteLine(inst.Method.Name);
                inst();
            } while (Memory.Ip < instructionsCount);
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
        byte operation = Memory.MemoryArray[ip];

        List<Instruction> instructions = new();

        while (operation != (byte)InstructionName.End)
        {
            Instruction instr = operation switch
            {
                (byte)InstructionName.Add => Add,
                (byte)InstructionName.Sub => Sub,
                (byte)InstructionName.Multiply => Mul,
                (byte)InstructionName.Divide => Div,
                (byte)InstructionName.Equals => Equals,
                (byte)InstructionName.Not => Not,
                (byte)InstructionName.JumpIfZero => JumpIfZero,
                (byte)InstructionName.JumpIfNotZero => JumpIfNotZero,
                (byte)InstructionName.SetVariable => SetVariable,
                (byte)InstructionName.LoadVariable => LoadVariable,
                (byte)InstructionName.CallMethod => CallMethod,
                (byte)InstructionName.Duplicate => Duplicate,
                (byte)InstructionName.LessThan => LessThan,
                (byte)InstructionName.GreatThan => GreatThan,
                (byte)InstructionName.Halt => Halt,
                (byte)InstructionName.Ret => Ret,
                (byte)InstructionName.Drop => Drop,
                (byte)InstructionName.PushAddress => PushAddress,
                (byte)InstructionName.Jump => Jump,
                (byte)InstructionName.CreateVariable => CreateVariable,
                (byte)InstructionName.CopyVariable => CopyVariable,
                (byte)InstructionName.PushConstant => PushConstant,
                (byte)InstructionName.DeleteVariable => DeleteVariable,
                (byte)InstructionName.NoOperation => NoOperation,
                (byte)InstructionName.GetByIndex => GetByIndex,
                _ or 0 => throw new InvalidOperationException($"unknown instruction - {(InstructionName)operation}")
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
        outputStringBuilder.Append(string.Join(",", _variables.Select(x =>
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