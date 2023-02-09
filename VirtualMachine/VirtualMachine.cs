namespace VirtualMachine;

using System.Diagnostics;

public static class VirtualMachine
{
    private static Stopwatch? _stopwatch;
    private static volatile int _countOfTasks;

    public static void Run(VmImage vmImage)
    {
        if (_countOfTasks == 0) OnProgramStart();

        VmRuntime.VmRuntime runtime = new() { OnProgramExit = OnTaskExit };
        runtime.SetImage(vmImage);

        Interlocked.Increment(ref _countOfTasks);
        Task.Run(() =>
        {
            runtime.Run();
            Interlocked.Decrement(ref _countOfTasks);
        });
    }

    public static void WaitLast()
    {
        while (_countOfTasks != 0)
            Thread.Sleep(0);

        OnProgramExit(null, null);
    }

    private static void OnProgramStart()
    {
        _stopwatch = Stopwatch.StartNew();
    }

    private static void OnTaskExit(VmRuntime.VmRuntime vmRuntime, Exception? error)
    {
        if (error is not null) OnProgramExit(vmRuntime, error);
    }

    private static void OnProgramExit(VmRuntime.VmRuntime? vmRuntime, Exception? error)
    {
        _stopwatch?.Stop();
        Console.WriteLine($"Program completed in {_stopwatch?.ElapsedMilliseconds} ms");

        if (error is null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Program executed without errors");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Program executed with error [{error}]");
        }

        Console.ResetColor();

        vmRuntime?.PrintState();

        if (_stopwatch is null) throw new NullReferenceException(nameof(_stopwatch));
    }
}