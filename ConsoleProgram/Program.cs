using ConsoleProgram;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

// add reverse polish notation
const string code = """
var iPtr
var somePtr

loop var i = 0, i 100_000 <, i = i 1 +
    PrintLn(i) # print i
    iPtr = ptr i # get a pointer to variable i
    iPtr -> i 2 + # set iPtr by it's pointer
    PrintLn(i) # print i (increased by two)

    somePtr = 2 # i pointer
    somePtr -> ref iPtr 3 + # get value from pointer iPtr, add 2, set it by somePtr pointer
    PrintLn(i) # print i (increased by three)
    
    PrintLn('') # new line
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