using ConsoleProgram;
using VirtualMachine;

VmImage vmImage = new();

vmImage.ImportMethodFromAssembly(Constants.MainLibraryPath, "Print");

// number i = 0
vmImage.CreateVariable("i");
vmImage.WriteNextOperation(InstructionName.PushConstant, 0);
vmImage.SetVariable("i");

vmImage.SetLabel("label");

// body
vmImage.WriteNextOperation(InstructionName.NoOperation);

// i++
vmImage.LoadVariable("i");
vmImage.WriteNextOperation(InstructionName.PushConstant, 1);
vmImage.WriteNextOperation(InstructionName.AddNumber);
vmImage.SetVariable("i");

// if i is 100_000_000 than goto label
vmImage.LoadVariable("i");
vmImage.WriteNextOperation(InstructionName.PushConstant, 100_000_000);
vmImage.WriteNextOperation(InstructionName.EqualsNumber);
vmImage.Goto("label", InstructionName.JumpIfZero);

VirtualMachine.VirtualMachine.Run(vmImage);
VirtualMachine.VirtualMachine.WaitLast();