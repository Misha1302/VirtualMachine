using ConsoleProgram;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

const string code = """
PrintLn(3 + 5 * (((3 - 4) / 3)) / 2.21 * 1 * 2 * 5 / 33) # approximately 2.771470359705654
loop var i = 0, i < 5, i = i + 1
    PrintLn('Multiplication equals ' + (StringToNumber(Input()) * StringToNumber(Input())))
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