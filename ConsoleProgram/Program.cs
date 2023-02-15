using ConsoleProgram;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

// const string code = """
// repeat(0, 5, i)
//     # list = (('Hello, human number ' + i) / ' ') + 'Added!'
//     var list
//     list = 'Hello, human number ', i + ' ' / 'Added!' +
//     Print(list[3] + ' ' + list[4])
// end
// """;

const string code = """
var str = 'qwertyytrewq0'
var arrayOfChars = ToCharArray(str)

var reversed = Reverse(arrayOfChars)
var strr = ValueToString(reversed)

PrintLn(str ' ' + strr +)
PrintLn(str strr ==)
""";

Parser parser = new();
List<Token> tokens = parser.Tokenize(code, Constants.MainLibraryPath, out AssemblyManager assemblyManager);
VmCompiler.VmCompiler compiler = new(Constants.MainLibraryPath);
VmImage vmImage = compiler.Compile(tokens);

VirtualMachine.VirtualMachine.RunAndWait(vmImage);