using ConsoleProgram;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

const string code = """
PrintLn((StringToNumber('23')) + (StringToNumber('32')))

var a = []
0 setElem of a to 234
1 setElem of a to 234

PrintLn(0 elemOf a)
PrintLn(1 elemOf a)
PrintLn((0 elemOf a) == (1 elemOf a))
""";

Parser parser = new();
List<Token> tokens = parser.Tokenize(code, Constants.MainLibraryPath, out AssemblyManager assemblyManager);
VmCompiler.VmCompiler compiler = new(assemblyManager);
VmImage vmImage = compiler.Compile(tokens);

VirtualMachine.VirtualMachine.RunAndWait(vmImage);

#if !DEBUG
Console.Read();
#endif