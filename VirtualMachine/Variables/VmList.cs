using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace VirtualMachine.Variables;

public class VmList : IEnumerable<object?>, ICloneable
{
    private object?[] _array;
    private int _maxIndex;

    public VmList()
    {
        _array = new object[32];
        _maxIndex = -1;
    }

    public VmList(List<object?> toList)
    {
        _array = toList.ToArray();
        _maxIndex = -1;
    }

    public int Len => _maxIndex + 1;

    public object? this[int index] => GetElement(index);

    public object Clone()
    {
        return new VmList(((object?[])_array.Clone()).ToList());
    }

    public IEnumerator<object?> GetEnumerator()
    {
        return _array[..Len].Cast<object?>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void SetElement(int index, object? obj)
    {
        IncreaseIfItNeed(index);
        _array[index] = obj;
        _maxIndex = index;
    }

    public void AddToEnd(object? obj)
    {
        int index = _maxIndex + 1;
        IncreaseIfItNeed(index);
        _array[index] = obj;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetEnd()
    {
        return _array[_maxIndex];
    }

    private object? GetElement(int index)
    {
        if (index > _maxIndex)
            throw new UnreachableException(
                $"The element at index {index} does not exist. List length - {Len}");

        return _array[index];
    }

    private void IncreaseIfItNeed(int maxIndex)
    {
        int arrayLength = _array.Length;
        if (maxIndex < arrayLength) return;

        if (maxIndex / 2 >= arrayLength) Array.Resize(ref _array, maxIndex + 1);
        else Array.Resize(ref _array, arrayLength * 2);
    }

    public void RemoveEnd()
    {
        _maxIndex--;
    }
}

public class VmList<T> : IEnumerable<object?>, ICloneable
{
    private T[] _array;
    private int _maxIndex;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public VmList(T[] array)
    {
        _array = array;
        _maxIndex = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public VmList(int capacity)
    {
        _array = new T[capacity];
        _maxIndex = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public VmList() : this(32)
    {
    }

    public int Len => _maxIndex + 1;

    public T this[int index] => GetElement(index);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public object Clone()
    {
        return new VmList<T>((T[])_array.Clone());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public IEnumerator<object?> GetEnumerator()
    {
        return _array[..Len].Cast<object?>().GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public IEnumerable<T> GetEnumerable()
    {
        return _array[..Len];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetElement(int index, T obj)
    {
        IncreaseIfItNeed(index);
        _array[index] = obj;
        _maxIndex = index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void AddToEnd(T obj)
    {
        _maxIndex++;
        IncreaseIfItNeed(_maxIndex);
        _array[_maxIndex] = obj;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public T GetEnd()
    {
        return _array[_maxIndex];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private T GetElement(int index)
    {
        if (index > _maxIndex)
            throw new UnreachableException(
                $"The element at index {index} does not exist. List length - {Len}");

        return _array[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void IncreaseIfItNeed(int maxIndex)
    {
        int arrayLength = _array.Length;
        if (maxIndex < arrayLength) return;

        if (maxIndex / 2 >= arrayLength) Array.Resize(ref _array, maxIndex + 1);
        else Array.Resize(ref _array, arrayLength * 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void RemoveEnd()
    {
        _maxIndex--;
    }
}