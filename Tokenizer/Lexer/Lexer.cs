namespace Tokenizer.Lexer;

using System.Text;
using System.Text.RegularExpressions;
using Tokenizer.Token;

public static class Lexer
{
    private const char StringCharacter = '\'';
    private const char SingleLineComment = '#';

    private const char EofCharacter = '\0';
    private static int _position;

    private static string _code = string.Empty;

    private static readonly IReadOnlyDictionary<string, TokenType> _words = new Dictionary<string, TokenType>
    {
        { "(", TokenType.OpenParentheses },
        { ")", TokenType.CloseParentheses },

        { "[", TokenType.OpenBracket },
        { "]", TokenType.CloseBracket },

        { "end", TokenType.End },
        { "import", TokenType.Import },
        { "var", TokenType.Var },
        { "for", TokenType.For },
        { "if", TokenType.If },
        { "else", TokenType.Else },

        { "+", TokenType.Plus },
        { "-", TokenType.Minus },
        { "*", TokenType.Multiply },
        { "/", TokenType.Divide },
        { "<", TokenType.LessThan },
        { ">", TokenType.GreatThan },
        { "==", TokenType.IsEquals },

        { "=", TokenType.EqualsSign },

        { ",", TokenType.Comma },
        { ".", TokenType.Dot }
    };

    public static List<Token> Tokenize(string code)
    {
        List<Token> list = new();

        _code = code + '\0';
        while (true)
        {
            Token nextToken = GetNextToken();
            list.Add(nextToken);
            if (nextToken.TokenType == TokenType.Eof) break;
        }

        ConnectUnknowns(list);

        return list;
    }

    private static void ConnectUnknowns(IList<Token> tokens)
    {
        for (int i = 0; i < tokens.Count; i++)
            if (tokens[i].TokenType == TokenType.Unknown)
                while (tokens[i + 1].TokenType == TokenType.Unknown)
                {
                    tokens[i].Text += tokens[i + 1].Text;
                    tokens.RemoveAt(i + 1);
                }
    }

    private static Token GetNextToken()
    {
        string trimmedCode = _code[_position..];

        char c = _code[_position];
        if (trimmedCode.StartsWith("\r\n")) return ReturnNewLine();
        if (char.IsWhiteSpace(c)) return ReturnWhitespace(c);
        if (char.IsDigit(c)) return GetNumber();

        Token? token = c switch
        {
            EofCharacter => new Token(TokenType.Eof, EofCharacter.ToString()),
            StringCharacter => GetString(),
            SingleLineComment => GetComment(),
            _ => null
        };
        if (token != null) return token;

        if (trimmedCode.StartsWithAny(_words, out KeyValuePair<string?, TokenType> word))
            return ReturnWord(word);

        return new Token(TokenType.Unknown, _code[_position++].ToString());
    }

    private static Token ReturnWord(KeyValuePair<string?, TokenType> word)
    {
        _position += word.Key.Length;
        return new Token(word.Value, word.Key);
    }

    private static Token ReturnNewLine()
    {
        _position += 2;
        return new Token(TokenType.NewLine, "\r\n");
    }

    private static Token GetComment()
    {
        _position++;
        int startOfStringIndex = _position;
        int endOfStringIndex = Regex.Match(_code[_position..], "(\n|\r\n)").Index + startOfStringIndex;

        string? str = _code[startOfStringIndex..endOfStringIndex];
        _position += str.Length + 1;
        return new Token(TokenType.Comment, str, str);
    }

    private static Token ReturnWhitespace(char c)
    {
        _position++;
        return new Token(TokenType.WhiteSpace, c.ToString());
    }

    private static Token GetNumber()
    {
        StringBuilder numberString = new();
        char ch;
        _position--;
        while (char.IsDigit(ch = _code[++_position]) || ch == '.') numberString.Append(ch);

        string? str = numberString.ToString().Replace('.', ',');
        _position += str.Length - 1;
        return new Token(TokenType.Number, str, decimal.Parse(str));
    }

    private static Token GetString()
    {
        _position++;
        int startOfStringIndex = _position;
        int endOfStringIndex = Regex.Match(_code[_position..], "(?<!(\\\\))'").Index + startOfStringIndex;

        string? str = _code[startOfStringIndex..endOfStringIndex];
        _position += str.Length + 1;
        return new Token(TokenType.String, str, str);
    }
}