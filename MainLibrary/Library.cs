using VirtualMachine.Variables;
using VirtualMachine.VmRuntime;

// ReSharper disable once CheckNamespace
namespace Library;

public static class Library
{
    // AddToEnd memory and pointers
    // AddToEnd dynamically create a new instance of external class
    public static void PrintLn(VmRuntime vmRuntime)
    {
        object? obj = vmRuntime.Memory.Pop();
        Console.WriteLine(VmRuntime.ObjectToString(obj));
    }

    public static void Print(VmRuntime vmRuntime)
    {
        object? obj = vmRuntime.Memory.Pop();
        Console.Write(VmRuntime.ObjectToString(obj));
    }

    public static void LenOf(VmRuntime vmRuntime)
    {
        object? obj = vmRuntime.Memory.Pop();
        decimal dec = obj switch
        {
            string s => s.Length,
            VmList list => list.Len,
            _ => throw new InvalidOperationException(
                $"Unable to find length from object {VmRuntime.ObjectToString(obj)}")
        };

        vmRuntime.Memory.Push(dec);
    }

    public static void MaxNumber(VmRuntime vmRuntime)
    {
        vmRuntime.Memory.Push(decimal.MaxValue);
    }

    public static void MinNumber(VmRuntime vmRuntime)
    {
        vmRuntime.Memory.Push(decimal.MinValue);
    }

    public static void ValueToString(VmRuntime vmRuntime)
    {
        object? obj = vmRuntime.Memory.Pop();
        vmRuntime.Memory.Push(VmRuntime.ObjectToString(obj));
    }

    public static void ToCharArray(VmRuntime vmRuntime)
    {
        object? obj = vmRuntime.Memory.Pop();
        string str = (string)(obj ?? throw new InvalidOperationException());
        vmRuntime.Memory.Push(str.ToCharArray().Select(x => (object)x).ToList());
    }

    public static void Reverse(VmRuntime vmRuntime)
    {
        object? obj = vmRuntime.Memory.Pop();
        if (obj is VmList list)
        {
            VmList newList = new();
            if (list.Len == 0)
            {
                vmRuntime.Memory.Push(newList);
                return;
            }

            if (list[0] is ICloneable)
                foreach (object? item in list)
                    newList.AddToEnd(item is not null ? ((ICloneable)item).Clone() : null);
            else
                foreach (object? item in list)
                    newList.AddToEnd(item);

            newList = new VmList(newList.Reverse().ToList());
            vmRuntime.Memory.Push(newList);
        }
        else
        {
            vmRuntime.Memory.Push(((string)(obj ?? throw new InvalidOperationException())).Reverse());
        }
    }

    public static void Input(VmRuntime vmRuntime)
    {
        vmRuntime.Memory.Push(Console.ReadLine());
    }

    public static void StringToNumber(VmRuntime vmRuntime)
    {
        object memoryARegister = vmRuntime.Memory.Pop() ?? throw new NullReferenceException();

        decimal value = memoryARegister is string s
            ? Convert.ToDecimal(s.Replace("_", ""))
            : Convert.ToDecimal(memoryARegister);

        vmRuntime.Memory.Push(value);
    }

    public static void PrintState(VmRuntime vmRuntime)
    {
        Console.WriteLine(vmRuntime.GetStateAsString());
    }

    public static void RandomInteger(VmRuntime vmRuntime)
    {
        int max = (int)(decimal)(vmRuntime.Memory.Pop() ?? throw new NullReferenceException());
        int min = (int)(decimal)(vmRuntime.Memory.Pop() ?? throw new NullReferenceException());

        vmRuntime.Memory.Push((decimal)Random.Shared.Next(min, max + 1));
    }
}