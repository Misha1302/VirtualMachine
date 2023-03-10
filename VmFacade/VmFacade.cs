namespace VmFacade;

using System.Globalization;
using System.Runtime.InteropServices;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;
using VmCompiler;

public static class VmFacade
{
    private const int AttachParentProcess = -1;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int dwProcessId);

    public static void Run(string code, bool useConsole = true)
    {
        if (useConsole)
        {
            AttachConsole(AttachParentProcess);
            AllocConsole();
        }

        VirtualMachine.RunAndWait(Compile(code));
    }

    public static void RunSeveralPrograms(IEnumerable<string> codes)
    {
        codes.Select(Compile).ToList().ForEach(VirtualMachine.Run);

        VirtualMachine.WaitLast();
    }

    private static VmImage Compile(string code)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        List<Token> tokens = Parser.Tokenize(code, VmConstants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);
        return vmImage;
    }

    public static double TakeMeasurements(string code)
    {
        VmImage image = Compile(code);

        // call to prepare all methods to be used
        VirtualMachine.RunDebug(image);

        const int count = 20;
        double sum = 0;
        for (int i = 0; i < count; i++) sum += VirtualMachine.RunDebug(image).ElapsedMilliseconds;
        return sum / count;
    }
}