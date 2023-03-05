const string code = """
var a = MyStruct

a.x = 5
a.y = 6
a.z = 7

var b = MyStruct
b.q0 = RandomInteger(0, 5)
b.q1 = RandomInteger(0, 5)
b.q2 = RandomInteger(0, 5)

PrintLn(a.x + b.q0)
PrintLn(a.y + b.q1)
PrintLn(a.z + b.q2)

struct MyStruct 
    var x
    var y
    var z
end

struct MyStruct2
    var q0
    var q1
    var q2
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