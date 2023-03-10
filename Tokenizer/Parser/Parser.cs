namespace Tokenizer.Parser;

using System.Reflection;
using Tokenizer.Lexer;
using Tokenizer.Token;
using VirtualMachine;
using VirtualMachine.VmRuntime;

public static class Parser
{
    private static AssemblyManager _assemblyManager = new();

    public static List<Token> Tokenize(string code, string mainLibPath, out AssemblyManager assemblyManager)
    {
        _assemblyManager = new AssemblyManager();

        List<Token> tokens = Lexer.Tokenize(code);
        tokens = tokens.Where(x =>
            x.TokenType is not TokenType.WhiteSpace and not TokenType.Comment and not TokenType.AtSign).ToList();

        tokens.InsertRange(0, new[]
        {
            new Token(TokenType.Var, "var"), new Token(TokenType.Unknown, "_"),
            new Token(TokenType.Var, "var"), new Token(TokenType.Unknown, "error")
        });

        ImportMethods(tokens, mainLibPath);
        DetectEntities(tokens);
        SetPartOfExpression(tokens);
        PrepareFunctionsAndMethods(ref tokens);
        PrepareStructuresFieldsAndMethods(tokens);
        PrecompileExpressions(tokens);

        assemblyManager = _assemblyManager;
        return tokens;
    }

    private static void PrepareStructuresFieldsAndMethods(List<Token> tokens)
    {
        for (int i = 1; i < tokens.Count; i++)
            if (tokens[i].TokenType == TokenType.Dot)
                if (tokens[i + 1].TokenType != TokenType.OpenParentheses)
                {
                    tokens[i - 1].ExtraInfo.Params.Add(tokens[i + 1]);
                    tokens.RemoveRange(i, 2);
                }
                else
                {
                    Token token = tokens[i - 1];
                    int nestingLevel = 0;
                    int start = i;
                    do
                    {
                        i++;
                        switch (tokens[i].TokenType)
                        {
                            case TokenType.CloseParentheses:
                                nestingLevel--;
                                break;
                            case TokenType.OpenParentheses:
                                nestingLevel++;
                                break;
                        }

                        token.ExtraInfo.Params.Add(tokens[i]);
                    } while (nestingLevel != 0);

                    i++;
                    token.ExtraInfo.Params.Add(tokens[i]);
                    token.ExtraInfo.Params.Insert(1, new Token(token.TokenType, token.Text));
                    token.ExtraInfo.Params.Insert(2, new Token(TokenType.Comma, ","));

                    tokens.RemoveRange(start, i - start + 1);
                    i = start;
                }
    }

    private static void DetectEntities(IReadOnlyList<Token> tokens)
    {
        DetectMethods(tokens);
        DetectFunctions(tokens);
        DetectStructs(tokens);
        DetectVariables(tokens);
    }

    private static void DetectStructs(IReadOnlyList<Token> tokens)
    {
        List<string> structs = new();
        for (int i = 1; i < tokens.Count; i++)
            if (tokens[i].TokenType == TokenType.Unknown && IsCorrectName(tokens, i))
                if (tokens[i - 1].TokenType == TokenType.Struct)
                {
                    structs.Add(tokens[i].Text);
                    tokens[i].TokenType = TokenType.NewStruct;
                }

        foreach (Token t in tokens.Where(t => structs.Contains(t.Text) && t.TokenType != TokenType.NewStruct))
            t.TokenType = TokenType.Structure;
    }

    private static void PrepareFunctionsAndMethods(ref List<Token> tokens)
    {
        for (int i = 0; i < tokens.Count; i++)
            if (tokens[i].IsCallMethodOrFunc)
            {
                Token methodToken = tokens[i];
                if (methodToken.ExtraInfo.Marked) break;
                methodToken.ExtraInfo.Marked = true;
                int startPosition = i;
                int lvl = 0;
                int argsCount = 0;
                do
                {
                    i++;

                    switchLabel:
                    if (i + 1 >= tokens.Count) break;

                    switch (tokens[i].TokenType)
                    {
                        case TokenType.CloseParentheses:
                            lvl--;
                            break;
                        case TokenType.OpenParentheses:
                            lvl++;
                            break;
                        case TokenType.ForeignMethod or TokenType.Function:
                            if (tokens[i].ExtraInfo.Marked) break;
                            List<Token> range = tokens.GetRange(i, tokens.Count - i);
                            PrepareFunctionsAndMethods(ref range);
                            tokens.RemoveRange(i, tokens.Count - i);
                            tokens.AddRange(range);
                            goto switchLabel;
                        case TokenType.Comma:
                            argsCount++;
                            break;
                    }
                } while (lvl != 0);

                if (argsCount != 0) methodToken.ExtraInfo.ArgsCount = argsCount + 1;

                while (tokens[i].IsCallMethodOrFunc) i++;

                tokens.Insert(i + 1, tokens[startPosition]);
                tokens.RemoveAt(startPosition);
            }
    }

    private static void PrecompileExpressions(List<Token> tokens)
    {
        ReversePolishNotation rpn = new();

        int left = 0;
        List<Token> nextList = tokens.GetRange(left, tokens.Count);
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

            int startIndex = nextList.FindIndex(x => x.ExtraInfo.IsPartOfExpression);
            if (startIndex == -1) return false;

            List<Token> list = nextList.GetRange(startIndex + 1, nextList.Count - startIndex - 1);
            int i = 0;
            while (list[i].ExtraInfo.IsPartOfExpression) i++;
            int len = i + 1;
            if (len + startIndex > nextList.Count) return false;

            List<Token> range = nextList.GetRange(startIndex, len);
            nextList.RemoveRange(startIndex, len);

            Token token = nextList[startIndex];

            IEnumerable<Token> collection = rpn.Convert(range);
            nextList.InsertRange(startIndex, collection);

            lastIndex = nextList.FindIndex(x => x.Id == token.Id);
            return true;
        }
    }

    private static void SetPartOfExpression(IReadOnlyList<Token> tokens)
    {
        for (int i = 0; i < tokens.Count; i++)
            if (IsDoubleOperator(tokens[i].TokenType))
            {
                MarkPreviousBlock(tokens, i - 1);
                MarkNextBlock(tokens, i + 1);
                tokens[i].ExtraInfo.IsPartOfExpression = true;
            }
            else if (IsSingleOperator(tokens[i].TokenType))
            {
                MarkNextBlock(tokens, i + 1);
                tokens[i].ExtraInfo.IsPartOfExpression = true;
            }
    }

    private static void MarkPreviousBlock(IReadOnlyList<Token> tokens, int i)
    {
        MarkBlock(tokens, i, 1, -1, -1);
    }

    private static void MarkNextBlock(IReadOnlyList<Token> tokens, int i)
    {
        MarkBlock(tokens, i, -1, 1, 1);
    }

    private static void MarkBlock(IReadOnlyList<Token> tokens, int i, int closeParenthesesValue,
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

            tokens[i].ExtraInfo.IsPartOfExpression = true;
            i += direction;
            if (i >= 0 && nesting == 0 && direction > 0 && tokens[i - 1].IsCallMethodOrFunc) goto start;
            if (i >= 0 && nesting == 0 && direction < 0 && tokens[i].IsCallMethodOrFunc) goto start;
        } while (nesting != 0);
    }

    public static bool IsDoubleOperator(TokenType x)
    {
        return x is TokenType.Plus or TokenType.Minus or TokenType.Divide or TokenType.Multiply or TokenType.Modulo
            or TokenType.LessThan or TokenType.GreatThan or TokenType.IsEquals or TokenType.IsNotEquals or TokenType.Or
            or TokenType.And;
    }

    private static bool IsSingleOperator(TokenType x)
    {
        return x is TokenType.IsNot;
    }

    private static void DetectVariables(IReadOnlyList<Token> tokens)
    {
        List<string?> variables = new();
        for (int i = 1; i < tokens.Count; i++)
            if (tokens[i].TokenType == TokenType.Unknown && IsCorrectName(tokens, i))
                if (tokens[i - 1].TokenType == TokenType.Var)
                {
                    variables.Add(tokens[i].Text);
                    tokens[i].TokenType = TokenType.NewVariable;
                }


        foreach (Token t in tokens)
            if (variables.Contains(t.Text) && t.TokenType != TokenType.NewVariable)
                t.TokenType = TokenType.Variable;
    }

    private static bool IsCorrectName(IReadOnlyList<Token> tokens, int i)
    {
        string text = tokens[i].Text;
        char c = text[0];
        if (c is not (>= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_')) return false;

        for (int j = 1; j < text.Length; j++)
        {
            c = text[j];

            // ReSharper disable twice MergeIntoPattern
            if (c is not (>= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_')) return false;
        }

        return true;
    }

    private static void DetectFunctions(IReadOnlyList<Token> tokens)
    {
        List<string> functions = new();
        for (int i = 1; i < tokens.Count; i++)
            if (tokens[i].TokenType == TokenType.Unknown && IsCorrectName(tokens, i))
                if (tokens[i - 1].TokenType == TokenType.Func)
                {
                    functions.Add(tokens[i].Text);
                    tokens[i].TokenType = TokenType.NewFunction;
                }

        foreach (Token t in tokens)
            if (functions.Contains(t.Text) && t.TokenType != TokenType.NewFunction)
                t.TokenType = TokenType.Function;
    }

    private static void ImportMethods(List<Token> tokens, string mainLibPath)
    {
        ImportAllMethodsFromLibrary(mainLibPath);

        for (int i = 0; i < tokens.Count; i++)
            if (tokens[i].TokenType == TokenType.Import)
            {
                string libPath = tokens[i + 1].Text;

                ImportAllMethodsFromLibrary(libPath);
                i++;
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
        const string defaultLibsPath = @"C:\VirtualMachine\Libs";
        string assemblyFile = Path.GetFullPath(libPath);
        if (!File.Exists(assemblyFile)) assemblyFile = Path.Combine(defaultLibsPath, libPath);

        if (!File.Exists(assemblyFile)) throw new InvalidOperationException($"library {libPath} was not found");

        Assembly assembly = Assembly.LoadFrom(assemblyFile);
        Type @class = assembly.GetType("Library.Library") ?? throw new InvalidOperationException();
        IEnumerable<MethodInfo> methods = @class.GetMethods()
            .Where(x =>
            {
                ParameterInfo[] parameters = x.GetParameters();
                if (parameters.Length != 2) return false;
                return parameters[0].ParameterType == typeof(VmRuntime) && parameters[1].ParameterType == typeof(int);
            });

        foreach (MethodInfo method in methods)
            _assemblyManager.ImportMethod(method);
    }

    private static void DetectMethods(IEnumerable<Token> tokens)
    {
        IEnumerable<Token> collection = tokens.Where(t =>
            t.TokenType == TokenType.Unknown && _assemblyManager.ImportedMethods.ContainsKey(t.Text));

        foreach (Token t in collection)
            t.TokenType = TokenType.ForeignMethod;
    }

    private class ReversePolishNotation
    {
        private readonly IReadOnlyDictionary<TokenType, int> _priorities = new Dictionary<TokenType, int>
        {
            { TokenType.OpenParentheses, 0 },
            { TokenType.CloseParentheses, 0 },

            { TokenType.Plus, 1 + 10 },
            { TokenType.Minus, 1 + 10 },
            { TokenType.Multiply, 2 + 10 },
            { TokenType.Divide, 2 + 10 },
            { TokenType.Modulo, 3 + 10 },


            { TokenType.IsNot, 3 },

            { TokenType.LessThan, 2 },
            { TokenType.GreatThan, 2 },

            { TokenType.IsNotEquals, 1 },
            { TokenType.IsEquals, 1 },

            { TokenType.And, 0 },
            { TokenType.Or, 0 }
        };

        public IEnumerable<Token> Convert(IEnumerable<Token> list)
        {
            List<Token> result = new();
            Stack<Token> tokensStack = new();

            foreach (Token token in list)
            {
                TokenType tokenType = token.TokenType;
                if (!IsDoubleOperator(tokenType) && !IsSingleOperator(tokenType) &&
                    tokenType is not TokenType.OpenParentheses and not TokenType.CloseParentheses) result.Add(token);
                else
                    switch (tokenType)
                    {
                        case TokenType.OpenParentheses:
                            tokensStack.Push(token);
                            break;
                        case TokenType.CloseParentheses:
                            Token t = tokensStack.Pop();
                            while (t.TokenType != TokenType.OpenParentheses)
                            {
                                result.Add(t);
                                t = tokensStack.Pop();
                            }

                            break;
                        default:
                            if (tokensStack.Count > 0)
                                if (_priorities[token.TokenType] <= _priorities[tokensStack.Peek().TokenType])
                                    result.Add(tokensStack.Pop());

                            tokensStack.Push(token);
                            break;
                    }
            }

            while (tokensStack.Count > 0) result.Add(tokensStack.Pop());

            return result;
        }
    }
}