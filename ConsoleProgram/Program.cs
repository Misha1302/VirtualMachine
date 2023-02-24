using ConsoleProgram;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

const string code = """
import 'MathLibrary\MathLibrary.dll' *

loop var i = 0, i < 2_000, i = i + 1
    var q = isOdd(i, 0)
end

func isOdd(var n, var odd)
    n = Abs(n)
    if n is 0
        return odd
    end

    return isOdd(n - 1, not odd)
end
""";

Parser parser = new();
List<Token> tokens = parser.Tokenize(code, VmConstants.MainLibraryPath, out AssemblyManager assemblyManager);
VmCompiler.VmCompiler compiler = new(assemblyManager);
VmImage vmImage = compiler.Compile(tokens);

VirtualMachine.VirtualMachine.RunAndWait(vmImage);

#if !DEBUG
Console.Read();
#endif