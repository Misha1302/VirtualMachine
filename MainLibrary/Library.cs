// ReSharper disable once CheckNamespace

namespace Library;

using System.Runtime.CompilerServices;
using VirtualMachine.VmRuntime;

public static class Library
{
    // add memory and pointers
    // add dynamically create a new instance of external class
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

    public static unsafe void WriteToMemory(VmRuntime vmRuntime)
    {
        ulong offset = (ulong)(decimal)(vmRuntime.Memory.Pop() ?? throw new InvalidOperationException());

        fixed (byte* ptr = vmRuntime.Memory.ArrayOfMemory)
        {
            object? obj0 = vmRuntime.Memory.Pop();
            Unsafe.Write(ptr + offset, obj0);
        }
    }

    public static unsafe void ReadFromMemory(VmRuntime vmRuntime)
    {
        fixed (byte* ptr = vmRuntime.Memory.ArrayOfMemory)
        {
            ulong offset = (ulong)(decimal)(vmRuntime.Memory.Pop() ?? throw new InvalidOperationException());
            vmRuntime.Memory.Push(Unsafe.Read<object?>(ptr + offset));
        }
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
        if (obj is List<object?> list)
        {
            List<object?> newList = new(list.Count);
            if (list.Count == 0)
            {
                vmRuntime.Memory.Push(newList);
                return;
            }

            if (list[0] is ICloneable)
                list.ForEach(item =>
                {
                    if (item is null)
                    {
                        newList.Add(null);
                    }
                    else
                    {
                        object clone = ((ICloneable)item).Clone();
                        newList.Add(clone);
                    }
                });
            else
                list.ForEach(item => newList.Add(item));

            newList.Reverse();
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
            ? Convert.ToDecimal(s.Replace('.', ','))
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