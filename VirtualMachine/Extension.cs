namespace VirtualMachine;

public static class Extension
{
    public static bool IsEquals(this decimal a, decimal b, int decimals = 25)
    {
        decimal a0 = decimal.Round(a, decimals, MidpointRounding.ToZero);
        decimal b0 = decimal.Round(b, decimals, MidpointRounding.ToZero);
        return a0 == b0;
    }
}