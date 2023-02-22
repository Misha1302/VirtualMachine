using ConsoleProgram;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

const string code = """
loop var i = 0, i < 300, i = i + 1
    var isPrime = IsPrime(i)

    if isPrime == 1 
        PrintLn(i + ' is prime')
    else 
        PrintLn(i + ' is not prime') 
    end
end


func IsPrime(var q)
    if q < 2 
        return 0 
    end
    
    if q == 2 
        return 1 
    end
    
    if q % 2 == 0 
        return 0 
    end
    


    var upper = q + 0.01
    loop var i = 3, (i * i) < upper, i = i + 2
        if q % i == 0 
            return 0 
        end
    end

    return 1
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