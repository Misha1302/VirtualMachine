namespace Tokenizer.Token;

public enum TokenType
{
    OpenParentheses = 1, // (
    CloseParentheses, // )

    OpenBracket, // [
    CloseBracket, // ]

    End, // end
    Repeat, // repeat
    For, // for

    Number, // 0

    Variable, // i

    Text, // XXX

    String, // 'S t r'

    Plus, // +
    Minus, // -
    Multiply, // *
    Divide, // /

    Method, // Print
    
    Eof,
    Unknown,
    NewLine,
    Comma,
    Dot,
    WhiteSpace,
    EqualsSign
}