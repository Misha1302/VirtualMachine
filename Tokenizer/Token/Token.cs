namespace Tokenizer.Token;

public record Token(TokenType TokenType, string Text, object? Value = null)
{
    private static uint _number;
    public readonly uint IndividualNumber = _number++;

    public readonly object? Value = Value;
    public string Text = Text;
    public TokenType TokenType = TokenType;
    
    public bool IsPartOfExpression;
    public bool IsCallMethodOrFunc => TokenType is TokenType.ForeignMethod or TokenType.Function;
}