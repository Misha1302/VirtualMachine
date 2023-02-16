using ConsoleProgram;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

// save variables in memory and get ptr to it
const string code = """
loop var i = 0, i 1_000_000 <, i = i 1 +
    # PrintLn(i)
    # PrintLn(ptr i)
end
""";

Parser parser = new();
List<Token> tokens = parser.Tokenize(code, Constants.MainLibraryPath, out AssemblyManager assemblyManager);
VmCompiler.VmCompiler compiler = new(assemblyManager);
VmImage vmImage = compiler.Compile(tokens);

VirtualMachine.VirtualMachine.RunAndWait(vmImage, assemblyManager);
#if !DEBUG
Console.Read();
#endif