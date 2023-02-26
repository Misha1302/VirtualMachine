using System.Reflection;
using System.Text.RegularExpressions;
using Tokenizer.Token;
using VirtualMachine;
using VirtualMachine.VmRuntime;

namespace Tokenizer.Parser;

public static class Parser
{
    private static AssemblyManager _assemblyManager = new();

    public static List<Token.Token> Tokenize(string code, string mainLibPath, out AssemblyManager assemblyManager)
    {
        _assemblyManager = new AssemblyManager();

        List<Token.Token> tokens = Lexer.Lexer.Tokenize(code);
        tokens = tokens.Where(x =>
            x.TokenType is not TokenType.WhiteSpace and not TokenType.Comment and not TokenType.AtSign).ToList();

        ImportMethods(tokens, mainLibPath);
        DetectMethods(tokens);
        DetectFunctions(tokens);
        DetectVariables(tokens);

        SetPartOfExpression(tokens);
        PrepareElemOfAndSetElem(tokens);
        PrecompileExpressions(tokens);

        assemblyManager = _assemblyManager;
        return tokens;
    }

    private static void PrepareElemOfAndSetElem(IList<Token.Token> tokens)
    {
        for (int i = 0; i < tokens.Count - 1; i++)
        {
            TokenType tokenType = tokens[i].TokenType;
            switch (tokenType)
            {
                case TokenType.ElemOf:
                    (tokens[i], tokens[i + 1]) = (tokens[i + 1], tokens[i]);
                    i++;
                    break;
                case TokenType.SetElem:
                    Token.Token token = tokens[i];
                    tokens.RemoveAt(i);
                    while (tokens[i].TokenType != TokenType.NewLine) i++;
                    tokens.Insert(i, token);
                    break;
            }
        }
    }

    private static void PrecompileExpressions(List<Token.Token> tokens)
    {
        ReversePolishNotation rpn = new();

        int left = 0;
        List<Token.Token> nextList = tokens.GetRange(left, tokens.Count);
        tokens.RemoveRange(left, tokens.Count);
        int previousLeft = left;
        while (ProcessNextExpression(out int offset))
        {
            left += offset;
            tokens.InsertRange(previousLeft, nextList);
            nextList = tokens.GetRange(left, tokens.Count - left);
            tokens.RemoveRange(left, tokens.Count - left);
            previousLeft = left;
        }

        tokens.AddRange(nextList);

        return;

        bool ProcessNextExpression(out int lastIndex)
        {
            lastIndex = -1;

            int startIndex = nextList.FindIndex(x => x.IsPartOfExpression);
            if (startIndex == -1) return false;

            List<Token.Token> list = nextList.GetRange(startIndex + 1, nextList.Count - startIndex - 1);
            int i = 0;
            while (list[i].IsPartOfExpression) i++;
            int len = i + 1;
            if (len + startIndex > nextList.Count) return false;

            List<Token.Token> range = nextList.GetRange(startIndex, len);
            nextList.RemoveRange(startIndex, len);

            Token.Token token = nextList[startIndex];

            IEnumerable<Token.Token> collection = rpn.Convert(range);
            nextList.InsertRange(startIndex, collection);

            lastIndex = nextList.FindIndex(x => x.Id == token.Id);
            return true;
        }
    }

    private static void SetPartOfExpression(IReadOnlyList<Token.Token> tokens)
    {
        for (int i = 0; i < tokens.Count; i++)
            if (IsDoubleOperator(tokens[i].TokenType))
            {
                MarkPreviousBlock(tokens, i - 1);
                MarkNextBlock(tokens, i + 1);
                tokens[i].IsPartOfExpression = true;
            }
            else if (IsSingleOperator(tokens[i].TokenType))
            {
                MarkNextBlock(tokens, i + 1);
                tokens[i].IsPartOfExpression = true;
            }
    }

    private static void MarkPreviousBlock(IReadOnlyList<Token.Token> tokens, int i)
    {
        MarkBlock(tokens, i, 1, -1, -1);
    }

    private static void MarkNextBlock(IReadOnlyList<Token.Token> tokens, int i)
    {
        MarkBlock(tokens, i, -1, 1, 1);
    }

    private static void MarkBlock(IReadOnlyList<Token.Token> tokens, int i, int closeParenthesesValue,
        int openParenthesesValue, int direction)
    {
        int nesting = 0;
        do
        {
            start:
            switch (tokens[i].TokenType)
            {
                case TokenType.CloseParentheses:
                    nesting += closeParenthesesValue;
                    break;
                case TokenType.OpenParentheses:
                    nesting += openParenthesesValue;
                    break;
            }

            tokens[i].IsPartOfExpression = true;
            i += direction;
            if (i >= 0 && nesting == 0 && direction > 0 && tokens[i - 1].IsCallMethodOrFunc) goto start;
            if (i >= 0 && nesting == 0 && direction < 0 && tokens[i].IsCallMethodOrFunc) goto start;
        } while (nesting != 0);
    }

    public static bool IsDoubleOperator(TokenType x)
    {
        return x is TokenType.Plus or TokenType.Minus or TokenType.Divide or TokenType.Multiply or TokenType.Modulo
            or TokenType.LessThan or TokenType.GreatThan or TokenType.IsEquals or TokenType.IsNotEquals;
    }

    public static bool IsSingleOperator(TokenType x)
    {
        return x is TokenType.IsNot;
    }

    private static void DetectVariables(IReadOnlyList<Token.Token> tokens)
    {
        List<string?> variables = new();
        for (int i = 1; i < tokens.Count; i++)
            if (tokens[i].TokenType == TokenType.Unknown && IsCorrectName(tokens, i))
            {
                if (tokens[i - 1].TokenType == TokenType.Var)
                {
                    variables.Add(tokens[i].Text);
                    tokens[i].TokenType = TokenType.NewVariable;
                }
                else if (variables.Contains(tokens[i].Text))
                {
                    tokens[i].TokenType = TokenType.Variable;
                }
            }
    }

    private static bool IsCorrectName(IReadOnlyList<Token.Token> tokens, int i)
    {
        return Regex.IsMatch(tokens[i].Text, "[_a-zA-Z][_a-zA-Z0-9]*");
    }

    private static void DetectFunctions(IReadOnlyList<Token.Token> tokens)
    {
        List<string> functions = new();
        for (int i = 1; i < tokens.Count; i++)
            if (tokens[i].TokenType == TokenType.Unknown && IsCorrectName(tokens, i))
                if (tokens[i - 1].TokenType == TokenType.Func)
                {
                    functions.Add(tokens[i].Text);
                    tokens[i].TokenType = TokenType.NewFunction;
                }

        foreach (Token.Token t in tokens)
            if (functions.Contains(t.Text) && t.TokenType != TokenType.NewFunction)
                t.TokenType = TokenType.Function;
    }

    private static void ImportMethods(List<Token.Token> tokens, string mainLibPath)
    {
        ImportAllMethodsFromLibrary(mainLibPath);

        for (int i = 0; i < tokens.Count; i++)
            if (tokens[i].TokenType == TokenType.Import)
            {
                string methodName = tokens[i + 2].Text;
                string libPath = tokens[i + 1].Text;

                if (methodName == "*") ImportAllMethodsFromLibrary(libPath);
                else _assemblyManager.ImportMethodFromAssembly(Path.GetFullPath(libPath), methodName);
                i += 2;
            }


        for (int i = 0; i < tokens.Count; i++)
            if (tokens[i].TokenType == TokenType.Import)
            {
                int iStart = i;
                while (tokens[i].TokenType != TokenType.NewLine) i++;
                int iEnd = i;

                tokens.RemoveRange(iStart, iEnd - iStart);
            }
    }

    private static void ImportAllMethodsFromLibrary(string libPath)
    {
        string assemblyFile = Path.GetFullPath(libPath);
        if (!File.Exists(assemblyFile)) assemblyFile = @"C:\VirtualMachine\Libs\" + libPath;
        if (!File.Exists(assemblyFile)) throw new InvalidOperationException($"library {libPath} was not found");

        Assembly assembly = Assembly.LoadFrom(assemblyFile);
        Type @class = assembly.GetType("Library.Library") ?? throw new InvalidOperationException();
        IEnumerable<MethodInfo> methods = @class.GetMethods()
            .Where(x =>
            {
                ParameterInfo[] parameters = x.GetParameters();
                if (parameters.Length == 0) return false;
                return parameters[0].ParameterType == typeof(VmRuntime);
            });

        foreach (MethodInfo method in methods)
            _assemblyManager.ImportMethodFromAssembly(libPath, method.Name);
    }

    private static void DetectMethods(IEnumerable<Token.Token> tokens)
    {
        IEnumerable<Token.Token> collection = tokens.Where(t =>
            t.TokenType == TokenType.Unknown && _assemblyManager.ImportedMethods.ContainsKey(t.Text));

        foreach (Token.Token t in collection)
            t.TokenType = TokenType.ForeignMethod;
    }

    private class ReversePolishNotation
    {
        private readonly IReadOnlyDictionary<TokenType, int> _priorities = new Dictionary<TokenType, int>
        {
            { TokenType.OpenParentheses, 0 },
            { TokenType.CloseParentheses, 0 },
            { TokenType.Plus, 1 },
            { TokenType.Minus, 1 },
            { TokenType.Multiply, 2 },
            { TokenType.Divide, 2 },
            { TokenType.Modulo, 3 },

            { TokenType.IsNot, 2 },
            { TokenType.And, 1 },
            { TokenType.Or, 1 },

            { TokenType.IsNotEquals, 0 },
            { TokenType.IsEquals, 0 },
            { TokenType.LessThan, 0 },
            { TokenType.GreatThan, 0 },

            { TokenType.ForeignMethod, 10 },
            { TokenType.Function, 10 },
            { TokenType.ElemOf, 11 }
        };

        public IEnumerable<Token.Token> Convert(IReadOnlyList<Token.Token> list)
        {
            List<List<Token.Token>> result = new();
            Stack<List<Token.Token>> tokensStack = new();

            List<List<Token.Token>> range = new();

            for (int i = 0; i < list.Count; i++)
            {
                Token.Token x = list[i];
                if (!x.IsCallMethodOrFunc)
                {
                    range.Add(new List<Token.Token> { x });
                }
                else
                {
                    List<Token.Token> retList = new();
                    int lvl = 0;
                    do
                    {
                        retList.Add(list[i]);
                        i++;
                        if (list[i].TokenType == TokenType.OpenParentheses) lvl++;
                        if (list[i].TokenType == TokenType.CloseParentheses) lvl--;
                    } while (lvl != 0);

                    retList.Add(list[i]);
                    range.Add(retList);
                }
            }


            foreach (List<Token.Token> tokens in range)
            {
                TokenType tokenType = tokens[0].TokenType;
                if (!IsDoubleOperator(tokenType) && !IsSingleOperator(tokenType) &&
                    tokenType is not TokenType.OpenParentheses and not TokenType.CloseParentheses)
                    result.Add(tokens);
                else
                    switch (tokenType)
                    {
                        case TokenType.OpenParentheses:
                            tokensStack.Push(tokens);
                            break;
                        case TokenType.CloseParentheses:
                            Token.Token t = tokensStack.Pop()[0];
                            while (t.TokenType != TokenType.OpenParentheses)
                            {
                                result.Add(new List<Token.Token> { t });
                                t = tokensStack.Pop()[0];
                            }

                            break;
                        default:
                            if (tokensStack.Count > 0)
                                if (_priorities[tokens[0].TokenType] <= _priorities[tokensStack.Peek()[0].TokenType])
                                    result.Add(tokensStack.Pop());

                            tokensStack.Push(tokens);
                            break;
                    }
            }

            while (tokensStack.Count > 0) result.Add(tokensStack.Pop());

            return result.SelectMany(x => x);
        }
    }
}