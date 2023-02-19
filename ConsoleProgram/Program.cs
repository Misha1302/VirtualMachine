using ConsoleProgram;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

const string code = """
PrintLn(2 + 3) # 5
PrintLn(5 / 3) # 1.(6)
PrintLn('Привет!' + ' Мир?') # Привет! Мир?

var x = 5 + 2
var y = 6
var z = StringToNumber(Input())
PrintLn(y / (x + y) * x + z) # 3.23076923076923076923077 + z
""";

Parser parser = new();
List<Token> tokens = parser.Tokenize(code, Constants.MainLibraryPath, out AssemblyManager assemblyManager);
VmCompiler.VmCompiler compiler = new(assemblyManager);
VmImage vmImage = compiler.Compile(tokens);

VirtualMachine.VirtualMachine.RunAndWait(vmImage);

#if !DEBUG
Console.Read();
#endif