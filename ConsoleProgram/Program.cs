using ConsoleProgram;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

const string code = """
myFunction1()
myFunction2()

func myFunction1()
    PrintLn(StringToNumber('23') + StringToNumber('32') + 2)
end

func myFunction3(var a)
    return 2 + a
end

func myFunction2()
    var q = StringToNumber(Input()) + myFunction3(5)
    PrintLn(q * q / 2)
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