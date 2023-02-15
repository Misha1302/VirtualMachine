namespace UnitTests;

using ConsoleProgram;
using VirtualMachine;

public class MathOperationsTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test()
    {
        VmImage vmImage = new(Constants.MainLibraryPath);

        vmImage.Repeat(
            () => { vmImage.WriteNextOperation(InstructionName.PushConstant, 0); },
            varName =>
            {
                vmImage.WriteNextOperation(InstructionName.PushConstant, "Hello, human number ");
                vmImage.LoadVariable(varName);
                vmImage.WriteNextOperation(InstructionName.Add);

                vmImage.WriteNextOperation(InstructionName.PushConstant, " ");
                vmImage.WriteNextOperation(InstructionName.Divide);

                vmImage.WriteNextOperation(InstructionName.PushConstant, "Added!");
                vmImage.WriteNextOperation(InstructionName.Add);

                vmImage.CreateVariable("list");
                vmImage.SetVariable("list");

                vmImage.LoadVariable("list");
                vmImage.WriteNextOperation(InstructionName.PushConstant, 3);
                vmImage.WriteNextOperation(InstructionName.GetByIndex);
                vmImage.WriteNextOperation(InstructionName.PushConstant, " ");
                vmImage.WriteNextOperation(InstructionName.Add);
                vmImage.LoadVariable("list");
                vmImage.WriteNextOperation(InstructionName.PushConstant, 4);
                vmImage.WriteNextOperation(InstructionName.GetByIndex);
                vmImage.WriteNextOperation(InstructionName.Add);
            },
            () => { vmImage.WriteNextOperation(InstructionName.PushConstant, 5); }
        );

        VmMemory memory = VirtualMachine.RunDebug(vmImage);

        Stack<object?> stack = memory.GetStack();
        Assert.That(stack.Pop(), Is.EqualTo("4 Added!"));
        Assert.That(stack.Pop(), Is.EqualTo("3 Added!"));
        Assert.That(stack.Pop(), Is.EqualTo("2 Added!"));
        Assert.That(stack.Pop(), Is.EqualTo("1 Added!"));
        Assert.That(stack.Pop(), Is.EqualTo("0 Added!"));
    }

    [Test]
    public void Test0()
    {
        Test(1, 3, InstructionName.Divide, 1 / 3m);
    }

    [Test]
    public void Test1()
    {
        Test(-3, 1, InstructionName.Divide, -3m / 1);
    }

    [Test]
    public void Test2()
    {
        Test("Hello!", "Hi!", InstructionName.Add, "Hello!" + "Hi!");
    }

    [Test]
    public void Test3()
    {
        Test("He Ha He!", " ", InstructionName.Divide, new List<object> { "He", "Ha", "He!" });
    }

    [Test]
    public void Test4()
    {
        Test("2", 5, InstructionName.Multiply, "22222");
    }

    [Test]
    public void Test5()
    {
        Test("Hello!", 5, InstructionName.Add, "Hello!" + 5);
    }

    [Test]
    public void Test6()
    {
        Test(5, "Hi", InstructionName.Add, 5 + "Hi");
    }

    private static void Test(object? a, object? b, InstructionName instruction, object expected)
    {
        VmImage vmImage = new(Constants.MainLibraryPath);

        vmImage.WriteNextOperation(InstructionName.PushConstant, a);
        vmImage.WriteNextOperation(InstructionName.PushConstant, b);
        vmImage.WriteNextOperation(instruction);

        VmMemory memory = VirtualMachine.RunDebug(vmImage);

        Stack<object?> stack = memory.GetStack();
        Assert.That(stack.Pop(), Is.EqualTo(expected));
    }
}