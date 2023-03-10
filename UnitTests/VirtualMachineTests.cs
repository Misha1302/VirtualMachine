namespace UnitTests;

using System.Globalization;
using Library;
using Tokenizer.Parser;
using Tokenizer.Token;
using VirtualMachine;
using VirtualMachine.Variables;
using VmCompiler;
using VmFacade;

public class VirtualMachineTests
{
    public VirtualMachineTests()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Test]
    public void Test0()
    {
        const string code = """
var a = 'Hello, World?'
a
""";


        List<Token> tokens = Parser.Tokenize(code, VmConstants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);

        VmMemory memory = VirtualMachine.RunDebug(vmImage).Memory;
        Assert.Multiple(() => { Assert.That(memory.GetStack().Pop(), Is.EqualTo("Hello, World?")); });
    }

    [Test]
    public void Test1()
    {
        const string code = """
var a = 2
var b = 3
(a + b / 5 * (2 * 2.111 * (9.3 - 6.34))) / (2 + 9.32) + 3.45 / 4 - 45.3222
""";


        List<Token> tokens = Parser.Tokenize(code, VmConstants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);

        VmMemory memory = VirtualMachine.RunDebug(vmImage).Memory;
        Assert.Multiple(() =>
        {
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
import @'{VmConstants.MathLibraryPath}' *
Sqrt(2)
";


        List<Token> tokens = Parser.Tokenize(code, VmConstants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);

        VmMemory memory = VirtualMachine.RunDebug(vmImage).Memory;
        Assert.Multiple(() =>
        {
            object? actual = memory.GetStack().Pop();
            Assert.That(actual, Is.EqualTo(Library.NeilMath.Sqrt(2)));
        });
    }

    [Test]
    public void Test3()
    {
        const string code = @"
var q = hello()
q

func hello()
    return 2 + 2 + 'hello!'
end
";


        List<Token> tokens = Parser.Tokenize(code, VmConstants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);

        VmMemory memory = VirtualMachine.RunDebug(vmImage).Memory;
        Assert.Multiple(() =>
        {
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


        List<Token> tokens = Parser.Tokenize(code, VmConstants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);

        VmMemory memory = VirtualMachine.RunDebug(vmImage).Memory;
        Assert.Multiple(() =>
        {
            object? actual = memory.GetStack().Pop();
            Assert.That(actual, Is.EqualTo(11.704486637m));
        });
    }

    [Test]
    public void Test5()
    {
        const string code = @"
var arr = CreateArray(IsPrime(2), IsPrime(5), IsPrime(6), IsPrime(9))
SetElement(5, arr, IsPrime(257))

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
    loop var i = 3; (i * i) < upper; i = i + 2
        if q % i == 0 
            return 0 
        end
    end

    return 1
end
";


        List<Token> tokens = Parser.Tokenize(code, VmConstants.MainLibraryPath, out AssemblyManager assemblyManager);
        VmCompiler compiler = new(assemblyManager);
        VmImage vmImage = compiler.Compile(tokens);

        VmMemory memory = VirtualMachine.RunDebug(vmImage).Memory;
        Assert.Multiple(() =>
        {
            VmList objects = memory.Pop() as VmList ?? throw new InvalidOperationException();
            Assert.That(objects[1], Is.EqualTo(1));
            Assert.That(objects[2], Is.EqualTo(1));
            Assert.That(objects[3], Is.EqualTo(0));
            Assert.That(objects[4], Is.EqualTo(0));
            Assert.That(objects[5], Is.EqualTo(1));
        });
    }
}