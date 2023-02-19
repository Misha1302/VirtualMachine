using ConsoleProgram;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

const string code = """
var a = []

loop var i = 0, i lt 100, i = i + 1
    setElem i in a to i * i
    Print(5 + i / 2 + ' ')
end
PrintLn('\n')

PrintLn(a)

var v = 2 elemOf a
PrintLn(v is 4)
PrintLn(v is 5)
PrintLn(v is not 5)
PrintLn(v is 5 or v is 4)
PrintLn(v is 5 and v is 4)
""";

Parser parser = new();
List<Token> tokens = parser.Tokenize(code, Constants.MainLibraryPath, out AssemblyManager assemblyManager);
VmCompiler.VmCompiler compiler = new(assemblyManager);
VmImage vmImage = compiler.Compile(tokens);

VirtualMachine.VirtualMachine.RunAndWait(vmImage);

#if !DEBUG
Console.Read();
#endif