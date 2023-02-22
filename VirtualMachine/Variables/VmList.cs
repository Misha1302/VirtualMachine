namespace VirtualMachine.Variables;

using System.Collections;
using System.Diagnostics;

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

    public object? GetEnd()
    {
        return _array[_maxIndex];
    }

    public object? GetElement(int index)
    {
        if (index > _maxIndex)
            throw new UnreachableException(
                $"The element at index {index} does not exist. List length - {_maxIndex + 1}");

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

    public VmList()
    {
        _array = new T[32];
        _maxIndex = -1;
    }

    public VmList(List<T> toList)
    {
        _array = toList.ToArray();
        _maxIndex = -1;
    }

    public int Len => _maxIndex + 1;

    public object? this[int index] => GetElement(index);

    public object Clone()
    {
        return new VmList<T>(((T[])_array.Clone()).ToList());
    }

    public IEnumerator<object?> GetEnumerator()
    {
        return _array[..Len].Cast<object?>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void SetElement(int index, T obj)
    {
        IncreaseIfItNeed(index);
        _array[index] = obj;
        _maxIndex = index;
    }

    public void AddToEnd(T obj)
    {
        _maxIndex++;
        IncreaseIfItNeed(_maxIndex);
        _array[_maxIndex] = obj;
    }

    public T GetEnd()
    {
        return _array[_maxIndex];
    }

    public T GetElement(int index)
    {
        if (index > _maxIndex)
            throw new UnreachableException(
                $"The element at index {index} does not exist. List length - {_maxIndex + 1}");

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