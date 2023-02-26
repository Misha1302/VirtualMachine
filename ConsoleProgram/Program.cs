const string code = """
import 'MathLibrary\MathLibrary.dll' *

loop var i = 0, i < 3_000, i = i + 1
    var q = isOdd(i, 0)
end

func isOdd(var n, var odd)
    n = Abs(n)

    if n == 0
        return odd
    end

    return isOdd(n - 1, not odd)
end
""";

VmFacade.VmFacade.Run(code);

#if !DEBUG
Console.Read();
#endif