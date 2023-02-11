using ConsoleProgram;
using VirtualMachine;

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
        vmImage.CallForeignMethod("Print");
    },
    () => { vmImage.LoadVariable("i"); }
);


VirtualMachine.VirtualMachine.RunAndWait(vmImage);