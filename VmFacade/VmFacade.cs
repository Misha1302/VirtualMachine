using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

namespace VmFacade;

public static class VmFacade
{
    public static void Run(string code)
    {
        VirtualMachine.VirtualMachine.RunAndWait(Compile(code));
    }

    public static void RunSeveralPrograms(IEnumerable<string> codes)
    {
        codes.Select(Compile).ToList()
            .ForEach(VirtualMachine.VirtualMachine.Run);
        
        VirtualMachine.VirtualMachine.WaitLast();
    }

    private static VmImage Compile(string code)
    {
        List<Token> tokens = Parser.Tokenize(code, VmConstants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler.VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);
        return vmImage;
    }
}