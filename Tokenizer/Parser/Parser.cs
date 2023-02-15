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

        assemblyManager = _assemblyManager;
        return tokens;
    }

    private static void DetectVariables(List<Token> tokens)
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
            t.TokenType == TokenType.Unknown && _assemblyManager.ImportedMethods.Contains(t.Text));

        foreach (Token t in collection)
            t.TokenType = TokenType.ForeignMethod;
    }
}