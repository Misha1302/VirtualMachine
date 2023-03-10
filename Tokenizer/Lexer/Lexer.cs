namespace Tokenizer.Lexer;

using System.Text;
using System.Text.RegularExpressions;
using Tokenizer.Parser;
using Tokenizer.Token;

public static class Lexer
{
    private const char StringCharacter = '\'';
    private const char SingleLineComment = '#';

    private const char EofCharacter = '\0';
    private static int _position;

    private static string _code = string.Empty;

    private static readonly IReadOnlyDictionary<string, TokenType> _symbols = new Dictionary<string, TokenType>
    {
        { "(", TokenType.OpenParentheses },
        { ")", TokenType.CloseParentheses },

        { "+", TokenType.Plus },
        { "++", TokenType.Increase },
        { "--", TokenType.Decrease },
        { "-", TokenType.Minus },
        { "*", TokenType.Multiply },
        { "/", TokenType.Divide },
        { "%", TokenType.Modulo },
        { "<", TokenType.LessThan },
        { ">", TokenType.GreatThan },
        { "==", TokenType.IsEquals },
        { "!=", TokenType.IsNotEquals },
        { "!", TokenType.IsNot },
        { "@", TokenType.AtSign },

        { "=", TokenType.EqualsSign },
        { ",", TokenType.Comma },
        { ".", TokenType.Dot },
        { ";", TokenType.Semicolon }
    };

    private static readonly List<KeyValuePair<string, TokenType>> _sortedSymbols;

    private static readonly IReadOnlyDictionary<string, TokenType> _words = new Dictionary<string, TokenType>
    {
        { "end", TokenType.End },
        { "import", TokenType.Import },
        { "var", TokenType.Var },
        { "loop", TokenType.Loop },
        { "if", TokenType.If },
        { "in", TokenType.In },
        { "to", TokenType.To },
        { "of", TokenType.Of },
        { "else", TokenType.Else },
        { "func", TokenType.Func },
        { "return", TokenType.Return },
        { "try", TokenType.Try },
        { "failed", TokenType.Failed },

        { "is not", TokenType.IsNotEquals },
        { "is", TokenType.IsEquals },
        { "not", TokenType.IsNot },
        { "gt", TokenType.GreatThan },
        { "lt", TokenType.LessThan },
        { "struct", TokenType.Struct },

        { "and", TokenType.And },
        { "or", TokenType.Or }
    };

    private static readonly List<KeyValuePair<string, TokenType>> _sortedWords;

    private static List<Token> _list = new();

    static Lexer()
    {
        _sortedSymbols = _symbols.OrderBy(x => x.Key.Length).ToList();
        _sortedWords = _words.OrderBy(x => x.Key.Length).ToList();
    }

    public static List<Token> Tokenize(string code)
    {
        _list = new List<Token>();
        _code = code + '\0';
        _position = 0;

        while (true)
        {
            Token nextToken = GetNextToken();
            _list.Add(nextToken);
            if (nextToken.TokenType == TokenType.Eof) break;
        }

        ConnectUnknowns(_list);

        return _list;
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
        char c = _code[_position];
        if (_code[_position] == '\r' && _code[_position + 1] == '\n') return ReturnNewLine();
        if (char.IsWhiteSpace(c)) return ReturnWhitespace(c);
        if (char.IsDigit(c) && PreviousTokenIsDelimiter()) return GetNumber();

        Token? token = c switch
        {
            EofCharacter => new Token(TokenType.Eof, EofCharacter.ToString()),
            StringCharacter => GetString(_list.Count == 0 || _list[^1].TokenType != TokenType.AtSign),
            SingleLineComment => GetComment(),
            _ => null
        };
        if (token != null) return token;

        string trimmedCode = _code[_position..];
        if (trimmedCode.StartsWithAny(_sortedSymbols, out KeyValuePair<string, TokenType> symbol))
            return ReturnWordOrSymbol(symbol);

        bool canWord = PreviousTokenIsDelimiter();
        bool startsWithAny = trimmedCode.StartsWithAny(_sortedWords, out KeyValuePair<string, TokenType> word);
        if (canWord && startsWithAny && !IsCorrectChar(_code[_position + word.Key.Length]))
            return ReturnWordOrSymbol(word);


        return new Token(TokenType.Unknown, _code[_position++].ToString());
    }

    private static bool IsCorrectChar(char ch)
    {
        // ReSharper disable once MergeIntoPattern
        return (ch is >= 'A' and <= 'Z' && ch is >= 'a' and <= 'z') || ch == '_';
    }

    private static bool PreviousTokenIsDelimiter()
    {
        if (_list.Count == 0) return true;

        TokenType tokenType = _list[^1].TokenType;
        return tokenType is TokenType.WhiteSpace or TokenType.NewLine or TokenType.OpenParentheses
            or TokenType.CloseParentheses or TokenType.Comma || Parser.IsDoubleOperator(tokenType);
    }

    private static Token ReturnWordOrSymbol(KeyValuePair<string, TokenType> pair)
    {
        _position += pair.Key.Length;
        return new Token(pair.Value, pair.Key);
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
        int endOfStringIndex = Regex.Match(_code[_position..], "(\n|\r\n|\0)").Index + startOfStringIndex;

        string str = _code[startOfStringIndex..endOfStringIndex];
        _position += str.Length;

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
        while (char.IsDigit(ch = _code[++_position]) || ch is '.' or '_') numberString.Append(ch);

        string str = numberString.ToString().Replace("_", "");
        return new Token(TokenType.Number, str, decimal.Parse(str));
    }

    private static Token GetString(bool isNormalString)
    {
        _position++;
        int startOfStringIndex = _position;
        int endOfStringIndex = FindEndOfString(_code, _position);

        string str = _code[startOfStringIndex..endOfStringIndex];
        _position += str.Length + 1;
        if (isNormalString) str = Regex.Unescape(str);
        return new Token(TokenType.String, str, str);
    }

    private static int FindEndOfString(string code, int position)
    {
        while (true)
        {
            if (code[++position] != StringCharacter) continue;
            if (code[position - 1] != '\\' || (code[position - 1] == '\\' && code[position - 2] == '\\'))
                return position;
        }
    }
}