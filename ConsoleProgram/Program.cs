using ConsoleProgram;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

const string code = """
# Main(StringToNumber(Input()), StringToNumber(Input()))
Main(0, 300_000)

func Main(var start, var upper)
    var arrayOfPrimeNumbers = []

    var n = 0
    
    loop var i = start, i < upper, i = i + 1
        var isPrime = IsPrime(i)

        if isPrime == 1 
            n setElem of arrayOfPrimeNumbers to i
            n = n + 1
        end
    end

    if LenOf(arrayOfPrimeNumbers) == 0
        
        return 0
    end

    var minDelta = MaxNumber()
    var maxDelta = MinNumber()

    
    loop var i = 1, i < LenOf(arrayOfPrimeNumbers), i = i + 1
        

        var delta = (i elemOf arrayOfPrimeNumbers) - ((i - 1) elemOf arrayOfPrimeNumbers)
        if delta > maxDelta
            maxDelta = delta
        end

        if delta < minDelta
            minDelta = delta
        end
    end
    

    PrintLn(minDelta)
    PrintLn(maxDelta)
    
    
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
List<Token> tokens = parser.Tokenize(code, VmConstants.MainLibraryPath, out AssemblyManager assemblyManager);
VmCompiler.VmCompiler compiler = new(assemblyManager);
VmImage vmImage = compiler.Compile(tokens);

VirtualMachine.VirtualMachine.RunAndWait(vmImage);

#if !DEBUG
Console.Read();
#endif