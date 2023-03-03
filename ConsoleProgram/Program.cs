using System.Globalization;
using System.Text;

int l = 5_000 * "PrintLn(1000 * 100)".Length;
StringBuilder code = new(l);
for (int i = 0; i < 5_000; i++)
    code.AppendLine(
        $"PrintLn({i.ToString(CultureInfo.InvariantCulture)} * {i.ToString(CultureInfo.InvariantCulture)})");

VmFacade.VmFacade.Run(code.ToString());

#if !DEBUG
async void WaitAndExit(int timeMs)
{
    await Task.Delay(timeMs);
    Environment.Exit(0);
}

new Task(() => WaitAndExit(10_000)).Start();

Console.Read();
#endif