namespace VirtualMachine;

using System.Diagnostics;

public static class VirtualMachine
{
    private static Stopwatch? _stopwatch;
    private static volatile int _countOfTasks;

    public static void Run(VmImage vmImage)
    {
        VmRuntime.VmRuntime runtime = CreateNewRuntime(vmImage);
        Interlocked.Increment(ref _countOfTasks);
        new Thread(() =>
        {
            if (_countOfTasks == 1) _stopwatch = Stopwatch.StartNew();
            runtime.Run();
        }).Start();
    }

    public static void RunAndWait(VmImage vmImage)
    {
        Run(vmImage);
        WaitLast();
    }

    public static VmMemory RunDebug(VmImage image)
    {
        Interlocked.Increment(ref _countOfTasks);
        VmRuntime.VmRuntime runtime = CreateNewRuntime(image);
        runtime.Run();

        return runtime.Memory;
    }

    public static void WaitLast()
    {
        while (_countOfTasks != 0)
            Thread.Sleep(0);

        OnProgramExit();
    }

    private static void OnProgramExit()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Program executed without exception");
        Console.ResetColor();

        _stopwatch!.Stop();
        Console.WriteLine($"{_stopwatch.ElapsedMilliseconds} ms");
    }

    private static void OnTaskExit(VmRuntime.VmRuntime vmRuntime, Exception? error)
    {
        Interlocked.Decrement(ref _countOfTasks);
        if (error is not null) GenerateException(vmRuntime, error);
    }

    private static void GenerateException(VmRuntime.VmRuntime vmRuntime, Exception error)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(error.Message);
        Console.ResetColor();

        Console.WriteLine(vmRuntime.GetStateAsString());
        Console.WriteLine(error.StackTrace);
    }

    private static VmRuntime.VmRuntime CreateNewRuntime(VmImage vmImage)
    {
        VmRuntime.VmRuntime runtime = new() { OnProgramExit = OnTaskExit };
        runtime.SetImage(vmImage);
        return runtime;
    }
}