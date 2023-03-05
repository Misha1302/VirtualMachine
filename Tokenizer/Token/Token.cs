namespace Tokenizer.Token;

public record Token(TokenType TokenType, string Text, object? Value = null)
{
    private static int _number;
    public readonly ExtraInfo ExtraInfo = new();
    public readonly int Id = _number++;

    public readonly object? Value = Value;

    public string Text = Text;
    public TokenType TokenType = TokenType;
    public bool IsCallMethodOrFunc => TokenType is TokenType.ForeignMethod or TokenType.Function;
}

public record ExtraInfo
{
    public int ArgsCount;
    public bool IsPartOfExpression;
    public bool Marked;
    public List<Token> Params = new();
}