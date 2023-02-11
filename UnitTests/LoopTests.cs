namespace UnitTests;

using System.Collections;
using ConsoleProgram;
using VirtualMachine;

public class LoopTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test()
    {
        VmImage vmImage = new(Constants.MainLibraryPath);

        vmImage.CreateVariable("i");
        vmImage.WriteNextOperation(InstructionName.PushConstant, 5);
        vmImage.SetVariable("i");

        vmImage.Repeat(
            () => { vmImage.WriteNextOperation(InstructionName.PushConstant, 0); },
            varName => { vmImage.LoadVariable(varName); },
            () => { vmImage.LoadVariable("i"); }
        );

        VmMemory memory = VirtualMachine.RunDebug(vmImage);

        Assert.That((decimal)(memory.GetStack().Pop() ?? decimal.MinValue), Is.EqualTo(4));
        Assert.That((decimal)(memory.GetStack().Pop() ?? decimal.MinValue), Is.EqualTo(3));
        Assert.That((decimal)(memory.GetStack().Pop() ?? decimal.MinValue), Is.EqualTo(2));
        Assert.That((decimal)(memory.GetStack().Pop() ?? decimal.MinValue), Is.EqualTo(1));
        Assert.That((decimal)(memory.GetStack().Pop() ?? decimal.MinValue), Is.EqualTo(0));
    }

    [Test]
    public void Test0()
    {
        VmImage vmImage = new(Constants.MainLibraryPath);

        vmImage.CreateVariable("i");
        vmImage.WriteNextOperation(InstructionName.PushConstant, 5);
        vmImage.SetVariable("i");

        vmImage.Repeat(
            () => { vmImage.WriteNextOperation(InstructionName.PushConstant, -5); },
            varName =>
            {
                vmImage.LoadVariable(varName);
                vmImage.WriteNextOperation(InstructionName.PushConstant, 1);
                vmImage.WriteNextOperation(InstructionName.Add);
                vmImage.SetVariable(varName);
                vmImage.LoadVariable(varName);
            },
            () => { vmImage.LoadVariable("i"); }
        );

        VmMemory memory = VirtualMachine.RunDebug(vmImage);

        Stack stack = memory.GetStack();
        Assert.That((decimal)(stack.Pop() ?? decimal.MinValue), Is.EqualTo(4));
        Assert.That((decimal)(stack.Pop() ?? decimal.MinValue), Is.EqualTo(2));
        Assert.That((decimal)(stack.Pop() ?? decimal.MinValue), Is.EqualTo(0));
        Assert.That((decimal)(stack.Pop() ?? decimal.MinValue), Is.EqualTo(-2));
        Assert.That((decimal)(stack.Pop() ?? decimal.MinValue), Is.EqualTo(-4));
    }

    [Test]
    public void Test1()
    {
        VmImage vmImage = new(Constants.MainLibraryPath);

        const string varName = "i";

        vmImage.ForLoop(
            () =>
            {
                vmImage.CreateVariable(varName);
                vmImage.WriteNextOperation(InstructionName.PushConstant, -1);
                vmImage.SetVariable(varName);
            },
            () =>
            {
                vmImage.LoadVariable(varName);
                vmImage.WriteNextOperation(InstructionName.PushConstant, 3);
                vmImage.WriteNextOperation(InstructionName.LessThan);
            },
            () =>
            {
                vmImage.LoadVariable(varName);
                vmImage.WriteNextOperation(InstructionName.PushConstant, 1);
                vmImage.WriteNextOperation(InstructionName.Add);
                vmImage.SetVariable(varName);
            },
            () =>
            {
                vmImage.LoadVariable(varName);
            }
        );

        VmMemory memory = VirtualMachine.RunDebug(vmImage);

        Stack stack = memory.GetStack();
        Assert.That((decimal)(stack.Pop() ?? decimal.MinValue), Is.EqualTo(2));
        Assert.That((decimal)(stack.Pop() ?? decimal.MinValue), Is.EqualTo(1));
        Assert.That((decimal)(stack.Pop() ?? decimal.MinValue), Is.EqualTo(0));
        Assert.That((decimal)(stack.Pop() ?? decimal.MinValue), Is.EqualTo(-1));
    }
}