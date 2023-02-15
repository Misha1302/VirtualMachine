namespace VirtualMachine;

public static class Extension
{
    public static bool IsEquals(this decimal a, decimal b, int decimals = 25)
    {
        decimal a0 = decimal.Round(a, decimals);
        decimal b0 = decimal.Round(b, decimals);
        return a0 == b0;
    }
}