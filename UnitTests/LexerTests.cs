using Tokenizer.Lexer;
using Tokenizer.Token;

namespace UnitTests;

public class LexerTests
{
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

    [Test]
    public void Test2()
    {
        List<TokenType> tokens = Lexer.Tokenize(""" 
var q = []
0 elemOf q
0 setElem of q to 3
""").Select(x => x.TokenType).Where(x => x != TokenType.WhiteSpace).ToList();

        Assert.That(tokens, Is.EqualTo(new List<TokenType>
        {
            TokenType.Var, TokenType.Unknown, TokenType.EqualsSign, TokenType.OpenBracket, TokenType.CloseBracket,
            TokenType.NewLine,
            TokenType.Number, TokenType.ElemOf, TokenType.Unknown,
            TokenType.NewLine,
            TokenType.Number, TokenType.SetElem, TokenType.Of, TokenType.Unknown, TokenType.To, TokenType.Number,
            TokenType.Eof
        }));
    }
}