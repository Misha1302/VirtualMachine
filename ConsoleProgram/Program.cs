const string code = """
import 'MathLibrary\MathLibrary.dll' *

loop var i = 0; i != 0-1; i = i + 1
    PrintLn(2 + '**' + i + ' = ' + MyPow(2, i))
end

func MyPow(var n, var power)
    if power == 0
        return 1
    end

    if power % 2 == 0
        return MyPow(n * n, Round(power / 2))
    end

    var q = MyPow(n, power - 1)
    return n * q
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