namespace Tokenizer.Token;

public record Token(TokenType TokenType, string Text, object? Value = null)
{
    public readonly object? Value = Value;
    public string Text = Text;
    public TokenType TokenType = TokenType;
}