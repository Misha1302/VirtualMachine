namespace UnitTests;

using Tokenizer.Parser;
using Tokenizer.Token;
using VmFacade;

public class ParserTests
{
    [Test]
    public void Test0()
    {
        List<TokenType> tokens = Parser.Tokenize("2 + 2 / 3", VmConstants.MainLibraryPath, out _)
            .Select(x => x.TokenType)
            .ToList();

        Assert.That(tokens, Is.EqualTo(new List<TokenType>
            {
                TokenType.Number,
                TokenType.Number,
                TokenType.Number,
                TokenType.Divide,
                TokenType.Plus,
                TokenType.Eof
            }
        ));
    }

    [Test]
    public void Test1()
    {
        List<TokenType> tokens = Parser.Tokenize(
            "var a = 2\nvar b = 3\n(a + b / 5 * (2 * 2.111 * (9.3 - 6.34))) / (2 + 9.32) + 3.45 / 4 - 45.3222",
            VmConstants.MainLibraryPath, out _).Select(x => x.TokenType).ToList();

        Assert.That(tokens, Is.EqualTo(new List<TokenType>
            {
                TokenType.Var,
                TokenType.NewVariable,
                TokenType.EqualsSign,
                TokenType.Number,

                TokenType.Var,
                TokenType.NewVariable,
                TokenType.EqualsSign,
                TokenType.Number,

                TokenType.Variable, TokenType.Variable, TokenType.Number, TokenType.Divide,
                TokenType.Number, TokenType.Number, TokenType.Multiply,
                TokenType.Number, TokenType.Number, TokenType.Minus,
                TokenType.Multiply, TokenType.Multiply, TokenType.Plus,
                TokenType.Number, TokenType.Number, TokenType.Plus, TokenType.Divide,
                TokenType.Number, TokenType.Number, TokenType.Divide,
                TokenType.Number, TokenType.Minus, TokenType.Plus,

                TokenType.Eof
            }
        ));
    }

    [Test]
    public void Test2()
    {
        List<TokenType> tokens = Parser.Tokenize(
            """
func IsPrime(var q)
    if q < 2 
        return 0 
    end
    
    if q == 2 
        return 1 
    end
    
    if q % 2 == 0 
        return 0 
    end
    


    var upper = q + 0.01
    loop var i = 3, (i * i) < upper, i = i + 2
        if q % i == 0 
            return 0 
        end
    end

    return 1
end
""", VmConstants.MainLibraryPath, out _).Select(x => x.TokenType).ToList();


        Assert.That(tokens, Is.EqualTo(new List<TokenType>
            {
                TokenType.Func, TokenType.NewFunction, TokenType.OpenParentheses, TokenType.Var, TokenType.NewVariable,
                TokenType.CloseParentheses, TokenType.NewLine, TokenType.If, TokenType.Variable, TokenType.Number,
                TokenType.LessThan, TokenType.NewLine, TokenType.Return, TokenType.Number, TokenType.NewLine,
                TokenType.End, TokenType.NewLine, TokenType.NewLine, TokenType.If, TokenType.Variable, TokenType.Number,
                TokenType.IsEquals, TokenType.NewLine, TokenType.Return, TokenType.Number, TokenType.NewLine,
                TokenType.End, TokenType.NewLine, TokenType.NewLine, TokenType.If, TokenType.Variable, TokenType.Number,
                TokenType.Modulo, TokenType.Number, TokenType.IsEquals, TokenType.NewLine, TokenType.Return,
                TokenType.Number, TokenType.NewLine, TokenType.End, TokenType.NewLine, TokenType.NewLine,
                TokenType.NewLine, TokenType.NewLine, TokenType.Var, TokenType.NewVariable, TokenType.EqualsSign,
                TokenType.Variable, TokenType.Number, TokenType.Plus, TokenType.NewLine, TokenType.Loop, TokenType.Var,
                TokenType.NewVariable, TokenType.EqualsSign, TokenType.Number, TokenType.Comma, TokenType.Variable,
                TokenType.Variable, TokenType.Multiply, TokenType.Variable, TokenType.LessThan, TokenType.Comma,
                TokenType.Variable, TokenType.EqualsSign, TokenType.Variable, TokenType.Number, TokenType.Plus,
                TokenType.NewLine, TokenType.If, TokenType.Variable, TokenType.Variable, TokenType.Modulo,
                TokenType.Number, TokenType.IsEquals, TokenType.NewLine, TokenType.Return, TokenType.Number,
                TokenType.NewLine, TokenType.End, TokenType.NewLine, TokenType.End, TokenType.NewLine,
                TokenType.NewLine, TokenType.Return, TokenType.Number, TokenType.NewLine, TokenType.End, TokenType.Eof
            }
        ));
    }
}