namespace UnitTests;

using ConsoleProgram;
using Library;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;
using VirtualMachine.Variables;
using VmCompiler;

public class VirtualMachineTests
{
    [Test]
    public void Test0()
    {
        const string code = """
var a = 'Hello, World?'
a
""";


        Parser parser = new();
        List<Token> tokens = parser.Tokenize(code, Constants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);

        VmMemory memory = VirtualMachine.RunDebug(vmImage);
        Assert.Multiple(() =>
        {
            Assert.That(memory.FunctionFrames.Len, Is.EqualTo(1));
            Assert.That(memory.GetStack().Pop(), Is.EqualTo("Hello, World?"));
        });
    }

    [Test]
    public void Test1()
    {
        const string code = """
var a = 2
var b = 3
(a + b / 5 * (2 * 2.111 * (9.3 - 6.34))) / (2 + 9.32) + 3.45 / 4 - 45.3222
""";


        Parser parser = new();
        List<Token> tokens = parser.Tokenize(code, Constants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);

        VmMemory memory = VirtualMachine.RunDebug(vmImage);
        Assert.Multiple(() =>
        {
            Assert.That(memory.FunctionFrames.Len, Is.EqualTo(1));

            object? actual = memory.GetStack().Pop();
            Assert.That(actual, Is.EqualTo(-43.620630035335689045936395760m));
            Assert.That(actual, Is.EqualTo(Expected()));


            decimal Expected()
            {
                const decimal a = 2m;
                const decimal b = 3m;
                return (a + b / 5m * 2m * 2.111m * (9.3m - 6.34m)) / (2m + 9.32m) + 3.45m / 4m - 45.3222m;
            }
        });
    }

    [Test]
    public void Test2()
    {
        const string code = $@"
import @'{Constants.MathLibraryPath}' *
Sqrt(2)
";


        Parser parser = new();
        List<Token> tokens = parser.Tokenize(code, Constants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);

        VmMemory memory = VirtualMachine.RunDebug(vmImage);
        Assert.Multiple(() =>
        {
            Assert.That(memory.FunctionFrames.Len, Is.EqualTo(1));

            object? actual = memory.GetStack().Pop();
            Assert.That(actual, Is.EqualTo(Library.NeilMath.Sqrt(2)));
        });
    }

    [Test]
    public void Test3()
    {
        const string code = @"
hello()

func hello()
    return 2 + 2 + 'hello!'
end
";


        Parser parser = new();
        List<Token> tokens = parser.Tokenize(code, Constants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);

        VmMemory memory = VirtualMachine.RunDebug(vmImage);
        Assert.Multiple(() =>
        {
            Assert.That(memory.FunctionFrames.Len, Is.EqualTo(1));

            object? actual = memory.GetStack().Pop();
            Assert.That(actual, Is.EqualTo("4hello!"));
        });
    }

    [Test]
    public void Test4()
    {
        const string code = @"
cubeOfNumberPlusTwo(2.133)

func cubeOfNumberPlusTwo(var x)
    return x * x * x + 2
end
";


        Parser parser = new();
        List<Token> tokens = parser.Tokenize(code, Constants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);

        VmMemory memory = VirtualMachine.RunDebug(vmImage);
        Assert.Multiple(() =>
        {
            Assert.That(memory.FunctionFrames.Len, Is.EqualTo(1));

            object? actual = memory.GetStack().Pop();
            Assert.That(actual, Is.EqualTo(11.704486637m));
        });
    }

    [Test]
    public void Test5()
    {
        const string code = @"
var arr = []

0 setElem arr IsPrime(2) # 1
1 setElem arr IsPrime(5) # 1
2 setElem arr IsPrime(6) # 0
3 setElem arr IsPrime(9) # 0
4 setElem arr IsPrime(257) # 1

arr



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
";


        Parser parser = new();
        List<Token> tokens = parser.Tokenize(code, Constants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);

        VmMemory memory = VirtualMachine.RunDebug(vmImage);
        Assert.Multiple(() =>
        {
            Assert.That(memory.FunctionFrames.Len, Is.EqualTo(1));

            Stack<object?> stack = memory.GetStack();
            VmList objects = stack.Pop() as VmList ?? throw new InvalidOperationException();
            Assert.That(objects[0], Is.EqualTo(1));
            Assert.That(objects[1], Is.EqualTo(1));
            Assert.That(objects[2], Is.EqualTo(0));
            Assert.That(objects[3], Is.EqualTo(0));
            Assert.That(objects[4], Is.EqualTo(1));
        });
    }
}