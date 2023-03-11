namespace VirtualMachine.VmRuntime;

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using global::VirtualMachine.Variables;

public partial class VmRuntime
{
    private const int Decimals = 23;
    private static readonly string _numberFormat = "0." + new string('#', 20);
    private readonly Stack<int> _failedStack = new();

    private List<Instruction>? _instructions;
    private Dictionary<int, int> _pointersToInsertVariables = new(64);
    public VmMemory Memory = default!;

    public Action<VmRuntime, Exception?>? OnProgramExit;

    public void Run()
    {
        Execute(_instructions ?? throw new InvalidOperationException());
    }

    private void Execute(IEnumerable<Instruction> instructionsEnumerable)
    {
#pragma warning disable CS8321
        void LogExtraInfo(ReadOnlySpan<Instruction> instructions)
        {
            Console.Write(instructions[Memory.Ip].Method.Name);
            if (Memory.Constants.TryGetValue(Memory.Ip, out object? value))
                Console.Write($" - {ObjectToString(value)}");
            Console.WriteLine();
        }
#pragma warning restore CS8321

        ReadOnlySpan<Instruction> instructions = new(instructionsEnumerable.ToArray());
        for (Memory.Ip = 0; Memory.Ip >= 0; Memory.Ip++)
            try
            {
                // LogExtraInfo(instructions);
                instructions[Memory.Ip]();
            }
            catch (Exception ex)
            {
                if (_failedStack.TryPop(out int pointer))
                {
                    Memory.Ip = pointer;
                    Memory.Push(ex.Message);
                }
                else
                {
                    Exit(ex);
                    return;
                }
            }

        Exit(null);
    }

    private List<Instruction> GenerateDictToExecute()
    {
        int ip = 0;
        InstructionName operation = Memory.InstructionsArray[ip];

        List<Instruction> instructions = new();

        while (operation != InstructionName.EndOfProgram)
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
                InstructionName.PushAddress => PushAddress,
                InstructionName.Or => Or,
                InstructionName.And => And,
                InstructionName.Jump => Jump,
                InstructionName.CreateVariable => CreateVariable,
                InstructionName.PushConstant => PushConstant,
                InstructionName.NoOperation => NoOperation,
                InstructionName.PushField => PushField,
                InstructionName.SetField => SetField,
                InstructionName.Increase => Increase,
                InstructionName.Decrease => Decrease,
                InstructionName.NotEquals => NotEquals,
                InstructionName.PushFailed => PushFailed,
                InstructionName.JumpToFuncMethod => JumpToFuncMethod,
                _ or 0 => throw new InvalidOperationException($"unknown instruction - {operation}")
            };
            instructions.Add(instr);

            ip++;

            operation = Memory.InstructionsArray[ip];
        }

        return instructions;
    }

    public void SetImage(VmImage image)
    {
        Memory = image.GetMemory();

        _pointersToInsertVariables = image.GetPointersToInsertVariables();

        _instructions = GenerateDictToExecute();
    }

    public string GetStateAsString()
    {
        StringBuilder outputStringBuilder = new();

        outputStringBuilder.Append("Stack={ ");
        outputStringBuilder.Append(string.Join(",", Memory.GetStack().ToArray()[..Memory.CurrentFunctionFrame.Sp]
            .ToArray().Select(x => x switch
            {
                string s => $"\"{s}\"",
                char c => $"\'{c}\'",
                decimal m => m.ToString(CultureInfo.InvariantCulture).Replace(',', '.'),
                _ => x?.ToString()
            })));
        outputStringBuilder.AppendLine("}");


        outputStringBuilder.Append("Variables={");
        outputStringBuilder.Append(string.Join(",", Memory.CurrentFunctionFrame.Variables.Select(x =>
        {
            string obj = x.Value.VariableValue switch
            {
                string s => $"\"{s}\"",
                char c => $"\'{c}\'",
                decimal m => m.ToString(CultureInfo.InvariantCulture).Replace(',', '.'),
                _ => x.Value.VariableValue?.ToString()
            } ?? string.Empty;

            return x.Value.Name + "=" + obj;
        })));
        outputStringBuilder.AppendLine("}");


        outputStringBuilder.Append("StackTrace={");
        outputStringBuilder.Append(string.Join("->", Memory.GetFunctionsFramesTrace().Select(x =>
            (x ?? throw new ArgumentNullException(nameof(x))).FuncName)));
        outputStringBuilder.AppendLine("}");

        return outputStringBuilder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadTwoValues(out object? obj0, out object? obj1)
    {
        obj1 = Memory.Pop();
        obj0 = Memory.Pop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadTwoNumbers(out decimal a, out decimal b)
    {
        ReadTwoValues(out object? obj0, out object? obj1);
        // if comparisons occur during subsequent actions, then 0.(9) will be equal to 1
        // 0.(9) will not be less than/greater than 1
        a = decimal.Round((decimal)(obj0 ?? throw new InvalidOperationException()), Decimals);
        b = decimal.Round((decimal)(obj1 ?? throw new InvalidOperationException()), Decimals);
    }

    private static string NumberToString(decimal m)
    {
        return m.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
    }

    public static string ObjectToString(object? obj)
    {
        const string empty = "empty";
        switch (obj)
        {
            case null:
                return empty;
            case decimal i:
                decimal round = decimal.Round(i, 18);
                string objectToString = round.ToString(_numberFormat, CultureInfo.InvariantCulture);
                return string.IsNullOrWhiteSpace(objectToString) ? "0" : objectToString;
            case string s:
                return s;
            case VmList list when list.Len != 0 && list.All(x => x is char):
                return new string(list.Select(x => (char)x!).ToArray());
            case VmList list:
                StringBuilder stringBuilder = new();
                stringBuilder.Append('[');

                foreach (object? item in list)
                {
                    string str = item switch
                    {
                        null => $"{empty}, ",
                        decimal m => $"{NumberToString(m)}, ",
                        string s => $"\"{s}\", ",
                        _ => $"{item}, "
                    };
                    stringBuilder.Append(str);
                }

                if (list.Len > 0) stringBuilder.Append("\b\b");
                stringBuilder.Append(']');
                return stringBuilder.ToString();
        }

        if (obj is IFormattable f)
            return f.ToString(null, null);
        return obj.ToString() ?? string.Empty;
    }

    private delegate void Instruction();
}