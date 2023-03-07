namespace VirtualMachine.Variables;

using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class VmList : IEnumerable<object?>, ICloneable, IEquatable<VmList>
{
    private object?[] _array;

    public VmList()
    {
        _array = new object[32];
    }

    public VmList(List<object?> list)
    {
        _array = list.ToArray();
        Len = _array.Length;
    }

    public VmList(object?[] array)
    {
        _array = (object?[])array.Clone();
        Len = _array.Length;
    }

    public int Len { get; private set; }

    public object? this[int index]
    {
        get => GetElement(index);
        set => SetElement(index, value);
    }

    public object Clone()
    {
        VmList vmList = new(((object?[])_array.Clone()).ToList()) { Len = Len };
        return vmList;
    }

    public IEnumerator<object?> GetEnumerator()
    {
        return _array[..Len].Cast<object?>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Equals(VmList? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Len == other.Len && _array[Len..].SequenceEqual(other._array[other.Len..]);
    }

    public void SetElement(int index, object? obj)
    {
        if (index > Len) Len = index;
        index--;
        IncreaseIfItNeed(index);
        _array[index] = obj;
    }

    public void AddToEnd(object? obj)
    {
        int index = Len + 1;
        IncreaseIfItNeed(index);
        SetElement(index, obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetEnd()
    {
        return _array[Len];
    }

    private object? GetElement(int index)
    {
        index--;
        if (index > Len)
            throw new UnreachableException(
                $"The element at index {index + 1} does not exist. List length - {Len}");

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
        Len--;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((VmList)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_array, Len);
    }
}