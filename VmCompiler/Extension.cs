namespace VmCompiler;

using Tokenizer.Token;

public static class Extension
{
    public static bool IsContains(this TokenType @enum, TokenType enum0)
    {
        return (@enum & enum0) != 0;
    }
}