namespace VmFacade;

using System.Globalization;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;
using VmCompiler;

public static class VmFacade
{
    static VmFacade()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    }

    public static void Run(string code)
    {
        VirtualMachine.RunAndWait(Compile(code));
    }

    public static void RunSeveralPrograms(IEnumerable<string> codes)
    {
        codes.Select(Compile).ToList()
            .ForEach(VirtualMachine.Run);

        VirtualMachine.WaitLast();
    }

    private static VmImage Compile(string code)
    {
        List<Token> tokens = Parser.Tokenize(code, VmConstants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);
        return vmImage;
    }
}