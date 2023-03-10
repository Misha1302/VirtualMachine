namespace VirtualMachine;

using System.Diagnostics;
using System.Globalization;

public static class VirtualMachine
{
    private static Stopwatch? _stopwatch;
    private static long _countOfTasks;
    private static bool _programWasExit;

    public static void Run(VmImage vmImage)
    {
        VmRuntime.VmRuntime runtime = CreateNewRuntime(vmImage);
        Thread thread = new(runtime.Run) { CurrentCulture = CultureInfo.InvariantCulture };

        Interlocked.Increment(ref _countOfTasks);
        if (_countOfTasks == 1) _stopwatch = Stopwatch.StartNew();
        thread.Start();
    }

    public static void RunAndWait(VmImage vmImage)
    {
        Run(vmImage);
        WaitLast();
    }

    public static (long ElapsedMilliseconds, VmMemory Memory) RunDebug(VmImage image)
    {
        VmRuntime.VmRuntime runtime = CreateNewRuntime(image);

        Interlocked.Increment(ref _countOfTasks);

        Stopwatch stopwatch = Stopwatch.StartNew();
        runtime.Run();
        stopwatch.Stop();

        return (stopwatch.ElapsedMilliseconds, runtime.Memory);
    }

    public static void WaitLast()
    {
        while (_countOfTasks != 0)
            Thread.Sleep(0);

        OnProgramExit();
        _programWasExit = false;
    }

    private static void OnProgramExit()
    {
        if (_programWasExit) return;
        _programWasExit = true;

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

    private static void GenerateException(VmRuntime.VmRuntime vmRuntime, Exception exception)
    {
        if (_programWasExit) return;
        _programWasExit = true;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(exception.Message);
        Console.ResetColor();

        Console.WriteLine(vmRuntime.GetStateAsString());
        Console.WriteLine(exception.StackTrace);
    }

    private static VmRuntime.VmRuntime CreateNewRuntime(VmImage vmImage)
    {
        VmRuntime.VmRuntime runtime = new() { OnProgramExit = OnTaskExit };
        runtime.SetImage(vmImage);
        return runtime;
    }
}