namespace VirtualMachine.VmRuntime;

using System.Runtime.InteropServices;
using global::VirtualMachine.Variable;

public class VmStruct
{
    private readonly Dictionary<int, StructField> _structFields = new();

    public readonly string Name;

    public VmStruct(List<string> structFields, string name)
    {
        Name = name;
        foreach (string fieldName in structFields)
            _structFields.Add(IdManager.MakeHashCode(fieldName), new StructField(fieldName));
    }

    public void SetValue(int id, object? obj)
    {
        CollectionsMarshal.GetValueRefOrNullRef(_structFields, id).FieldValue = obj;
    }

    public object? GetValue(int id)
    {
        return CollectionsMarshal.GetValueRefOrNullRef(_structFields, id).FieldValue;
    }

    public override string ToString()
    {
        return "{ " + string.Join(", ", _structFields.Select(x => x.Value.ToString())) + " }";
    }


    private record StructField(string Name)
    {
        public readonly string Name = Name;
        public object? FieldValue;

        public override string ToString()
        {
            return $"{Name}={VmRuntime.ObjectToString(FieldValue)}";
        }
    }
}