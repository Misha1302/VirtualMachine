namespace VirtualMachine.VmRuntime;

using System.Runtime.InteropServices;
using global::VirtualMachine.Variables;

public class Structure
{
    private readonly Dictionary<int, Field> _values = new();

    public Structure(List<string> fieldsNames)
    {
        foreach (string fieldName in fieldsNames)
            _values.Add(IdManager.MakeHashCode(fieldName), new Field(fieldName));
    }

    public void SetValue(int id, object? obj)
    {
        CollectionsMarshal.GetValueRefOrNullRef(_values, id).FieldValue = obj;
    }

    public object? GetValue(int id)
    {
        return CollectionsMarshal.GetValueRefOrNullRef(_values, id).FieldValue;
    }

    public override string ToString()
    {
        return "{ " + string.Join(", ", _values.Select(x => x.Value.ToString())) + " }";
    }


    private record Field(string Name)
    {
        public readonly string Name = Name;
        public object? FieldValue;

        public override string ToString()
        {
            return $"{Name}={VmRuntime.ObjectToString(FieldValue)}";
        }
    }
}