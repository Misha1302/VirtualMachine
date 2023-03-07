const string code = """
loop var i = 0; i < 1_000_000; i = i + 1

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