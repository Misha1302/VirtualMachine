namespace UnitTests;

using System.Globalization;
using Tokenizer.Lexer;
using Tokenizer.Token;

public class LexerTests
{
    public LexerTests()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Test]
    public void Test0()
    {
        List<TokenType> tokens = Lexer.Tokenize("""'Hello!' 3 (23 - 4 / '44')""").Select(x => x.TokenType).ToList();

        Assert.That(tokens, Is.EqualTo(new List<TokenType>
        {
            TokenType.String, TokenType.WhiteSpace, TokenType.Number, TokenType.WhiteSpace,
            TokenType.OpenParentheses, TokenType.Number, TokenType.WhiteSpace, TokenType.Minus, TokenType.WhiteSpace,
            TokenType.Number, TokenType.WhiteSpace,
            TokenType.Divide, TokenType.WhiteSpace, TokenType.String, TokenType.CloseParentheses, TokenType.Eof
        }));
    }

    [Test]
    public void Test1()
    {
        IEnumerable<string> enumerable = Lexer.Tokenize(""" '\n' '\0' 'Привет!\!' '\\ \\ \\n \\r EDYeq\n' """)
            .Select(x => (string)x.Value!);
        List<string> tokens = enumerable.Where(x => !string.IsNullOrEmpty(x)).ToList();

        Console.WriteLine(tokens);
        Assert.That(tokens, Is.EqualTo(new List<string>
        {
            "\n",
            "\0",
            "Привет!\\!",
            "\\\\ \\\\ \\\\n \\\\r EDYeq\n"
        }));
    }
}