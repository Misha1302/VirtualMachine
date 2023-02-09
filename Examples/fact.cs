using ConsoleProgram;
using VirtualMachine;

VmImage vmImage = new();

vmImage.ImportMethodFromAssembly(Constants.MainLibraryPath, "Print");
vmImage.ImportMethodFromAssembly(Constants.MainLibraryPath, "Input");
vmImage.ImportMethodFromAssembly(Constants.MainLibraryPath, "ToNumber");
vmImage.ImportMethodFromAssembly(Constants.MainLibraryPath, "PrintState");
vmImage.ImportMethodFromAssembly(Constants.MainLibraryPath, "RandomInteger");


vmImage.WriteNextOperation(InstructionName.PushConstant, 27);
vmImage.Call("fact");
vmImage.WriteNextOperation(InstructionName.CallMethod, vmImage.ImportedMethodsIndexes["Print"]);

vmImage.CreateFunction("fact");
    vmImage.CreateVariable("n");
    vmImage.SetVariable("n");

    // if n == 1 than return 1
    vmImage.LoadVariable("n");
    vmImage.WriteNextOperation(InstructionName.PushConstant, 1);
    vmImage.WriteNextOperation(InstructionName.EqualsNumber);
        vmImage.Goto("not", InstructionName.JumpIfZero);
    vmImage.DeleteVariable("n");
    vmImage.WriteNextOperation(InstructionName.PushConstant, 1);
vmImage.WriteNextOperation(InstructionName.Ret);

    vmImage.SetLabel("not");
    // fact(n - 1)
    vmImage.LoadVariable("n");
    vmImage.WriteNextOperation(InstructionName.PushConstant, 1);
    vmImage.WriteNextOperation(InstructionName.SubNumber);
    vmImage.Call("fact");
    // * n
    vmImage.LoadVariable("n");
    vmImage.DeleteVariable("n");
    vmImage.WriteNextOperation(InstructionName.MultiplyNumber);
vmImage.WriteNextOperation(InstructionName.Ret);


VirtualMachine.VirtualMachine.Run(vmImage);
VirtualMachine.VirtualMachine.WaitLast();