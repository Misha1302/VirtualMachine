namespace Tokenizer.Token;

public record Token(TokenType TokenType, string? Text, object? Value = null)
{
    public TokenType TokenType = TokenType;
    public readonly object? Value = Value;
    public string? Text = Text;
}