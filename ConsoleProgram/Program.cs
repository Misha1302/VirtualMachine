using ConsoleProgram;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;


// save variables in memory and get ptr to it
const string code = """
var q = 100_000
loop var i = 0, i q <, i = i 1 +
    PrintLn('value of i: ' i + '    ptr of i: ' + ptr i +)
    PrintLn('value of q: ' q + '    ptr of q: ' + ptr q +)
end
""";

Parser parser = new();
List<Token> tokens = parser.Tokenize(code, Constants.MainLibraryPath, out AssemblyManager assemblyManager);
VmCompiler.VmCompiler compiler = new(assemblyManager);
VmImage vmImage = compiler.Compile(tokens);

VirtualMachine.VirtualMachine.RunAndWait(vmImage);
#if !DEBUG
Console.Read();
#endif