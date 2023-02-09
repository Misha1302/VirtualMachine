// ReSharper disable once CheckNamespace

namespace Library;

using System.Globalization;
using VirtualMachine.VmRuntime;

public static class Library
{
    public static void Print(VmRuntime vmRuntime)
    {
        object? obj = vmRuntime.Memory.Stack.Pop();
        switch (obj)
        {
            case null:
                Console.WriteLine("null");
                break;
            case decimal i:
                Console.WriteLine(i.ToString(CultureInfo.InvariantCulture).Replace(',', '.'));
                break;
            default:
                Console.WriteLine(obj);
                break;
        }
    }

    public static void Input(VmRuntime vmRuntime)
    {
        vmRuntime.Memory.Stack.Push(Console.ReadLine());
    }

    public static void ToNumber(VmRuntime vmRuntime)
    {
        object memoryARegister = vmRuntime.Memory.Stack.Pop() ?? throw new NullReferenceException();

        decimal value = memoryARegister is string s
            ? Convert.ToDecimal(s.Replace('.', ','))
            : Convert.ToDecimal(memoryARegister);

        vmRuntime.Memory.Stack.Push(value);
    }

    public static void PrintState(VmRuntime vmRuntime)
    {
        vmRuntime.PrintState();
    }

    public static void RandomInteger(VmRuntime vmRuntime)
    {
        int max = (int)(decimal)(vmRuntime.Memory.Stack.Pop() ?? throw new NullReferenceException());
        int min = (int)(decimal)(vmRuntime.Memory.Stack.Pop() ?? throw new NullReferenceException());

        vmRuntime.Memory.Stack.Push((decimal)Random.Shared.Next(min, max + 1));
    }
}