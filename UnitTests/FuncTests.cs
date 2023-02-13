namespace UnitTests;

using ConsoleProgram;
using VirtualMachine;

public class FuncTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test()
    {
        VmImage vmImage = new(Constants.MainLibraryPath);

        vmImage.Call("SuperFunc!");
        vmImage.Call("SuperFunc!");
        vmImage.Call("SuperFunc!");

        vmImage.CreateFunction("SuperFunc!", Array.Empty<string>(),
            () => { vmImage.WriteNextOperation(InstructionName.PushConstant, 5); });

        VmMemory memory = VirtualMachine.RunDebug(vmImage);

        Stack<object?> stack = memory.GetStack();
        Assert.That(stack.Pop(), Is.EqualTo(5));
        Assert.That(stack.Pop(), Is.EqualTo(5));
        Assert.That(stack.Pop(), Is.EqualTo(5));
    }

    [Test]
    public void Test0()
    {
        VmImage vmImage = new(Constants.MainLibraryPath);

        decimal[] arrayOfValues = { 2, -5, 25 };

        vmImage.WriteNextOperation(InstructionName.PushConstant, arrayOfValues[0]);
        vmImage.Call("SuperFunc!");
        vmImage.WriteNextOperation(InstructionName.PushConstant, arrayOfValues[1]);
        vmImage.Call("SuperFunc!");
        vmImage.WriteNextOperation(InstructionName.PushConstant, arrayOfValues[2]);
        vmImage.Call("SuperFunc!");

        vmImage.CreateFunction("SuperFunc!", new[] { "a" },
            () =>
            {
                vmImage.LoadVariable("a");
                vmImage.LoadVariable("a");
                vmImage.WriteNextOperation(InstructionName.Multiply);
            });

        VmMemory memory = VirtualMachine.RunDebug(vmImage);

        Stack<object?> stack = memory.GetStack();
        Assert.That(stack.Pop(),
            Is.EqualTo(arrayOfValues[2] * arrayOfValues[2]));
        Assert.That(stack.Pop(),
            Is.EqualTo(arrayOfValues[1] * arrayOfValues[1]));
        Assert.That(stack.Pop(),
            Is.EqualTo(arrayOfValues[0] * arrayOfValues[0]));
    }

    [Test]
    public void Test1()
    {
        VmImage vmImage = new(Constants.MainLibraryPath);

        decimal[] arrayOfValues = { 2, -5, 25 };
        decimal[] arrayOfValues0 = { -2, 6, -1 };

        vmImage.WriteNextOperation(InstructionName.PushConstant, arrayOfValues[0]);
        vmImage.WriteNextOperation(InstructionName.PushConstant, arrayOfValues0[0]);
        vmImage.Call("SuperFunc!");
        vmImage.WriteNextOperation(InstructionName.PushConstant, arrayOfValues[1]);
        vmImage.WriteNextOperation(InstructionName.PushConstant, arrayOfValues0[1]);
        vmImage.Call("SuperFunc!");
        vmImage.WriteNextOperation(InstructionName.PushConstant, arrayOfValues[2]);
        vmImage.WriteNextOperation(InstructionName.PushConstant, arrayOfValues0[2]);
        vmImage.Call("SuperFunc!");

        vmImage.CreateFunction("SuperFunc!", new[] { "a", "b" },
            () =>
            {
                vmImage.LoadVariable("a");
                vmImage.LoadVariable("b");
                vmImage.WriteNextOperation(InstructionName.Add);
            });

        VmMemory memory = VirtualMachine.RunDebug(vmImage);

        Stack<object?> stack = memory.GetStack();
        Assert.That(stack.Pop(),
            Is.EqualTo(arrayOfValues[2] + arrayOfValues0[2]));
        Assert.That(stack.Pop(),
            Is.EqualTo(arrayOfValues[1] + arrayOfValues0[1]));
        Assert.That(stack.Pop(),
            Is.EqualTo(arrayOfValues[0] + arrayOfValues0[0]));
    }
}