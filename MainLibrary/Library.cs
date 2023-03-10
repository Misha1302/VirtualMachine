// ReSharper disable once CheckNamespace

namespace Library;

using VirtualMachine.Variables;
using VirtualMachine.VmRuntime;

public static class Library
{
    // AddToEnd memory and pointers
    // AddToEnd dynamically create a new instance of external class
    public static void PrintLn(VmRuntime vmRuntime, int argsCount)
    {
        object? obj = vmRuntime.Memory.Pop();
        Console.WriteLine(VmRuntime.ObjectToString(obj));
    }

    public static void Print(VmRuntime vmRuntime, int argsCount)
    {
        object? obj = vmRuntime.Memory.Pop();
        Console.Write(VmRuntime.ObjectToString(obj));
    }

    public static void GetElement(VmRuntime vmRuntime, int argsCount)
    {
        int index = (int)(decimal)(vmRuntime.Memory.Pop() ?? throw new InvalidOperationException());
        object obj = vmRuntime.Memory.Pop() ?? throw new InvalidOperationException();

        vmRuntime.Memory.Push(
            obj is VmList list
                ? list[index]
                : ((string)obj)[index]
        );
    }

    public static void SetElement(VmRuntime vmRuntime, int argsCount)
    {
        object? value = vmRuntime.Memory.Pop();
        object obj = vmRuntime.Memory.Pop() ?? throw new InvalidOperationException();
        int index = (int)(decimal)(vmRuntime.Memory.Pop() ?? throw new InvalidOperationException());

        if (obj is VmList list)
            list[index] = value;
        else
            vmRuntime.Memory.Push(ReplaceAt((string)obj, index - 1,
                ((string)(value ?? throw new InvalidOperationException()))[0]));


        string ReplaceAt(string input, int charIndex, char newChar)
        {
            char[] chars = input.ToCharArray();
            chars[charIndex] = newChar;
            return new string(chars);
        }
    }

    public static void CreateArray(VmRuntime vmRuntime, int argsCount)
    {
        object?[] objects = new object?[argsCount];
        for (int i = argsCount - 1; i >= 0; i--) objects[i] = vmRuntime.Memory.Pop();

        VmList vmList = new(objects);
        vmRuntime.Memory.Push(vmList);
    }

    public static void LenOf(VmRuntime vmRuntime, int argsCount)
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

    public static void MaxNumber(VmRuntime vmRuntime, int argsCount)
    {
        vmRuntime.Memory.Push(decimal.MaxValue);
    }

    public static void MinNumber(VmRuntime vmRuntime, int argsCount)
    {
        vmRuntime.Memory.Push(decimal.MinValue);
    }

    public static void ValueToString(VmRuntime vmRuntime, int argsCount)
    {
        object? obj = vmRuntime.Memory.Pop();
        vmRuntime.Memory.Push(VmRuntime.ObjectToString(obj));
    }

    public static void ToCharArray(VmRuntime vmRuntime, int argsCount)
    {
        object? obj = vmRuntime.Memory.Pop();
        string str = (string)(obj ?? throw new InvalidOperationException());
        vmRuntime.Memory.Push(str.ToCharArray().Select(x => (object)x).ToList());
    }

    public static void Reverse(VmRuntime vmRuntime, int argsCount)
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

    public static void Input(VmRuntime vmRuntime, int argsCount)
    {
        vmRuntime.Memory.Push(Console.ReadLine());
    }

    public static void ToNumber(VmRuntime vmRuntime, int argsCount)
    {
        object memoryARegister = vmRuntime.Memory.Pop() ?? throw new NullReferenceException();

        decimal value = memoryARegister is string s
            ? Convert.ToDecimal(s.Replace("_", ""))
            : Convert.ToDecimal(memoryARegister);

        vmRuntime.Memory.Push(value);
    }

    public static void PrintState(VmRuntime vmRuntime, int argsCount)
    {
        Console.WriteLine(vmRuntime.GetStateAsString());
    }

    public static void RandomInteger(VmRuntime vmRuntime, int argsCount)
    {
        int max = (int)(decimal)(vmRuntime.Memory.Pop() ?? throw new NullReferenceException());
        int min = (int)(decimal)(vmRuntime.Memory.Pop() ?? throw new NullReferenceException());

        vmRuntime.Memory.Push((decimal)Random.Shared.Next(min, max + 1));
    }
}