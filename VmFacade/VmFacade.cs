using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

namespace VmFacade;

public static class VmFacade
{
    public static void Run(string code)
    {
        List<Token> tokens = Parser.Tokenize(code, VmConstants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler.VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);

        VirtualMachine.VirtualMachine.RunAndWait(vmImage);
    }

    public static void RunSeveralPrograms(IEnumerable<string> codes)
    {
        foreach (string code in codes)
        {
            List<Token> tokens =
                Parser.Tokenize(code, VmConstants.MainLibraryPath, out AssemblyManager assemblyManager);
            VmCompiler.VmCompiler compiler = new(assemblyManager);
            VmImage vmImage = compiler.Compile(tokens);
            VirtualMachine.VirtualMachine.Run(vmImage);
        }

        VirtualMachine.VirtualMachine.WaitLast();
    }
}