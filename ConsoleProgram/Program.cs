const string code = """
loop var i = 0; i < 10; i = i + 1
    PrintLn(i)
end
""";

const string code0 = """
PrintLn(fib(32))

func fib(var n)
    if n == 1 or n == 2
        return 1
    end

    return fib(n - 1) + fib(n - 2)
end
""";

VmFacade.VmFacade.Run(code);

#if !DEBUG
Console.Read();
#endif