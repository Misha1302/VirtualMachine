namespace Tokenizer.Lexer;

using System.Text;
using System.Text.RegularExpressions;
using Tokenizer.Token;

public static class Lexer
{
    private static int _position;

    private static string _code = string.Empty;

    private static readonly IReadOnlyDictionary<string, TokenType> _words = new Dictionary<string, TokenType>
    {
        { "(", TokenType.OpenParentheses },
        { ")", TokenType.CloseParentheses },

        { "[", TokenType.OpenBracket },
        { "]", TokenType.CloseBracket },

        { "end", TokenType.End },
        { "repeat", TokenType.Repeat },
        { "for", TokenType.For },

        { "+", TokenType.Plus },
        { "-", TokenType.Minus },
        { "*", TokenType.Multiply },
        { "/", TokenType.Divide },

        { "=", TokenType.EqualsSign },

        { ",", TokenType.Comma },
        { ".", TokenType.Dot },

        { "\n", TokenType.NewLine },
        { "\r\n", TokenType.NewLine }
    };

    public static List<Token> Tokenize(string code)
    {
        List<Token> list = new();

        _code = code + '\0';
        while (true)
        {
            Token nextToken = GetNextToken();
            if (nextToken.TokenType == TokenType.WhiteSpace) continue;
            if (nextToken.TokenType == TokenType.Eof) break;
            list.Add(nextToken);
        }

        GlueUnknowns(list);

        return list;
    }

    private static void GlueUnknowns(List<Token> tokens)
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
        if (_code.Length <= _position || c == '\0')
            return new Token(TokenType.Eof, "\0");

        if (char.IsWhiteSpace(c)) return ReturnWhitespace(c);
        if (c == '\'') return GetString();
        if (char.IsDigit(c)) return GetNumber();

        if (trimmedCode.StartsWithAny(_words, out KeyValuePair<string, TokenType> word))
        {
            _position += word.Key.Length;
            return new Token(word.Value, word.Key);
        }

        return new Token(TokenType.Unknown, _code[_position++].ToString());
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

        string str = numberString.ToString().Replace('.', ',');
        _position += str.Length;
        return new Token(TokenType.Number, str, decimal.Parse(str));
    }

    private static Token GetString()
    {
        _position++;
        int startOfStringIndex = _position;
        int endOfStringIndex = Regex.Match(_code[_position..], "(?<!(\\\\))'").Index + startOfStringIndex;

        string str = _code[startOfStringIndex..endOfStringIndex];
        _position += str.Length + 1;
        return new Token(TokenType.String, str, str);
    }
}