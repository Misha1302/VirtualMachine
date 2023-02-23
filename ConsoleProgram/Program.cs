using ConsoleProgram;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;

const string code = """
var arrayOfPrimeNumbers = []

Print('start: ')
var start = StringToNumber(Input())
Print('upper: ')
var upper = StringToNumber(Input())

var n = 0
loop var i = start, i < upper, i = i + 1
    var isPrime = IsPrime(i)

    if isPrime == 1 
        n setElem of arrayOfPrimeNumbers to i
        n = n + 1
    end
end

var minDelta = MaxNumber()
var maxDelta = MinNumber()

Print('all prime numbers in the given range: ')
Print(0 elemOf arrayOfPrimeNumbers + ',')
loop var i = 1, i < LenOf(arrayOfPrimeNumbers), i = i + 1
    Print(i elemOf arrayOfPrimeNumbers + ',')

    var delta = (i elemOf arrayOfPrimeNumbers) - ((i - 1) elemOf arrayOfPrimeNumbers)
    if delta > maxDelta
        maxDelta = delta
    end

    if delta < minDelta
        minDelta = delta
    end
end
Print('\b \n')

PrintLn('min delta - ' + minDelta)
PrintLn('max delta - ' +maxDelta)


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