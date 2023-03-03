const string code = """
loop var i = 0; i < 100_000_000; i = i + 1
    # PrintLn(i)
end
""";

const string code0 = """
PrintLn(fib(32))

func fib(var n)
    if n < 2.001
        return 1
    end
    return fib(n - 1) + fib(n - 2)
end
""";

VmFacade.VmFacade.Run(code);

#if !DEBUG
async void WaitAndExit(int timeMs)
{
    await Task.Delay(timeMs);
    Environment.Exit(0);
}

new Task(() => WaitAndExit(10_000)).Start();

Console.Read();
#endif