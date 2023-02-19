namespace Tokenizer.Parser;

using System.Reflection;
using System.Text.RegularExpressions;
using Tokenizer.Lexer;
using Tokenizer.Token;
using VirtualMachine;
using VirtualMachine.VmRuntime;

public class Parser
{
    private readonly AssemblyManager _assemblyManager = new();

    public List<Token> Tokenize(string code, string mainLibPath, out AssemblyManager assemblyManager)
    {
        List<Token> tokens = Lexer.Tokenize(code);
        tokens = tokens.Where(x => x.TokenType != TokenType.WhiteSpace && x.TokenType != TokenType.Comment).ToList();
        ImportMethods(tokens, mainLibPath);
        DetectMethods(tokens);
        DetectVariables(tokens);
        SetPartOfExpression(tokens);
        PrecompileExpressions(tokens);

        assemblyManager = _assemblyManager;
        return tokens;
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

            int startIndex = nextList.FindIndex(x => x.IsPartOfExpression);
            if (startIndex == -1) return false;

            List<Token> list = nextList.GetRange(startIndex + 1, nextList.Count - startIndex - 1);
            int i = 0;
            while (list[i].IsPartOfExpression) i++;
            int len = i + 1;
            if (len + startIndex > nextList.Count) return false;

            List<Token> range = nextList.GetRange(startIndex, len);
            nextList.RemoveRange(startIndex, len);

            Token token = nextList[startIndex];

            IEnumerable<Token> collection = rpn.Convert(range);
            nextList.InsertRange(startIndex, collection);

            lastIndex = nextList.FindIndex(x => x.IndividualNumber == token.IndividualNumber);
            return true;
        }
    }

    private static void SetPartOfExpression(IReadOnlyList<Token> tokens)
    {
        for (int i = 0; i < tokens.Count; i++)
            if (IsOperator(tokens[i].TokenType))
            {
                MarkPreviousBlock(tokens, i - 1);
                MarkNextBlock(tokens, i + 1);
                tokens[i].IsPartOfExpression = true;
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
        } while (nesting != 0);
    }

    private static bool IsOperator(TokenType x)
    {
        return x is TokenType.Plus or TokenType.Minus or TokenType.Divide or TokenType.Multiply or TokenType.Modulo
            or TokenType.LessThan or TokenType.GreatThan or TokenType.IsEquals;
    }

    private static void DetectVariables(IReadOnlyList<Token> tokens)
    {
        List<string?> variables = new();
        for (int i = 1; i < tokens.Count; i++)
            if (tokens[i].TokenType == TokenType.Unknown && Regex.IsMatch(tokens[i].Text, "[_a-zA-Z]+"))
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

    private void ImportMethods(IReadOnlyList<Token> tokens, string mainLibPath)
    {
        ImportMethodsFromMainLibrary(mainLibPath);

        for (int i = 0; i < tokens.Count; i++)
            if (tokens[i].TokenType == TokenType.Import)
                _assemblyManager.ImportMethodFromAssembly(Path.GetFullPath(tokens[i + 1].Text), tokens[i + 2].Text);
    }

    private void ImportMethodsFromMainLibrary(string mainLibPath)
    {
        Assembly assembly = Assembly.LoadFrom(mainLibPath);
        Type @class = assembly.GetType("Library.Library") ?? throw new InvalidOperationException();
        IEnumerable<MethodInfo> methods = @class.GetMethods()
            .Where(x =>
            {
                ParameterInfo[] parameters = x.GetParameters();
                if (parameters.Length == 0) return false;
                return parameters[0].ParameterType == typeof(VmRuntime);
            });

        foreach (MethodInfo method in methods)
            _assemblyManager.ImportMethodFromAssembly(mainLibPath, method.Name);
    }

    private void DetectMethods(IEnumerable<Token> tokens)
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
            { TokenType.CloseParentheses, 1 },
            { TokenType.Plus, 2 },
            { TokenType.Minus, 2 },
            { TokenType.Multiply, 3 },
            { TokenType.Divide, 3 }
        };

        public IEnumerable<Token> Convert(IEnumerable<Token> range)
        {
            List<Token> result = new();
            Stack<Token> tokensStack = new();

            foreach (Token token in range)
            {
                TokenType tokenType = token.TokenType;
                if (!IsOperator(tokenType) &&
                    tokenType is not TokenType.OpenParentheses and not TokenType.CloseParentheses)
                    result.Add(token);
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
                            if (tokensStack.Count > 0 &&
                                _priorities[token.TokenType] <= _priorities[tokensStack.Peek().TokenType])
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