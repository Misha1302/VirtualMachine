namespace UnitTests;

using ConsoleProgram;
using VirtualMachine;

public class StackTests
{
    private readonly object?[] _arrayOfValues =
    {
        (decimal)-31, (decimal)323, (decimal)int.MaxValue, (decimal)(Random.Shared.NextSingle() * 100_000_000),
        decimal.MinValue, null, (decimal)float.Epsilon,
        "Hello, World!", 'h', 'i', true
    };

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test()
    {
        VmImage vmImage = new(Constants.MainLibraryPath);

        decimal value = (decimal)Random.Shared.NextDouble() * 1_000_000m;

        vmImage.WriteNextOperation(InstructionName.PushConstant, value);

        VmMemory memory = VirtualMachine.RunDebug(vmImage);

        Assert.That((decimal)(memory.GetStack().Pop() ?? -1), Is.EqualTo(value));
    }

    [Test]
    public void Test0()
    {
        StackTest(_arrayOfValues[0]);
    }

    [Test]
    public void Test1()
    {
        StackTest(_arrayOfValues[1]);
    }

    [Test]
    public void Test2()
    {
        StackTest(_arrayOfValues[2]);
    }

    [Test]
    public void Test3()
    {
        StackTest(_arrayOfValues[3]);
    }

    [Test]
    public void Test4()
    {
        StackTest(_arrayOfValues[4]);
    }

    [Test]
    public void Test5()
    {
        StackTest(_arrayOfValues[5]);
    }

    [Test]
    public void Test6()
    {
        StackTest(_arrayOfValues[6]);
    }

    [Test]
    public void Test7()
    {
        StackTest(_arrayOfValues[7]);
    }

    [Test]
    public void Test8()
    {
        StackTest(_arrayOfValues[8]);
    }

    [Test]
    public void Test9()
    {
        StackTest(_arrayOfValues[9]);
    }

    [Test]
    public void Test_10()
    {
        StackTest(_arrayOfValues[10]);
    }

    private static void StackTest(object? item)
    {
        VmImage vmImage = new(Constants.MainLibraryPath);

        vmImage.WriteNextOperation(InstructionName.PushConstant, item);
        VmMemory memory = VirtualMachine.RunDebug(vmImage);

        Assert.That(memory.GetStack().Pop(), Is.EqualTo(item));
    }

    [Test]
    public void Test_11()
    {
        VmImage vmImage = new(Constants.MainLibraryPath);

        vmImage.WriteNextOperation(InstructionName.PushConstant, 4);
        VmMemory memory = VirtualMachine.RunDebug(vmImage);

        Assert.That(memory.GetStack().Pop(), !Is.EqualTo(5));
    }
}