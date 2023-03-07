﻿namespace VmCompiler;

using Tokenizer.Token;
using VirtualMachine;
using VirtualMachine.Variables;
using VirtualMachine.VmRuntime;

public class VmCompiler
{
    private readonly Dictionary<string, List<string>> _functions = new();
    private readonly VmImage _image;
    private readonly Dictionary<string, List<string>> _structures = new();
    private int _i;
    private string _labelName = "label0";
    private List<Token> _tokens = new();

    public VmCompiler(AssemblyManager assemblyManager)
    {
        _image = new VmImage(assemblyManager);
    }

    public VmImage Compile(List<Token> tokens)
    {
        _tokens = tokens;
        PrepareCode();
        CompileNextBlock(TokenType.Eof);

        return _image;
    }

    private void PrepareCode()
    {
        for (_i = 0; _i < _tokens.Count; _i++)
            if (_tokens[_i].TokenType == TokenType.NewFunction)
                PrepareNewFunction();

        for (_i = 0; _i < _tokens.Count; _i++)
            if (_tokens[_i].TokenType == TokenType.NewStruct)
                PrepareNewStructure();

        _i = 0;
    }

    private void CompileEqualsSign()
    {
        Token varToken = _tokens[_i - 1];

        _i++;
        CompileNextBlock(TokenType.NewLine | TokenType.Semicolon);
        _i--;


        List<Token> extraInfoParams = varToken.ExtraInfo.Params;
        if (extraInfoParams.Count == 0)
        {
            _image.SetVariable(varToken.Text);
        }
        else
        {
            _image.LoadVariable(varToken.Text);
            int paramId = IdManager.MakeHashCode(extraInfoParams[0].Text);
            _image.WriteNextOperation(InstructionName.SetField, paramId);
        }
    }

    private void CompileMethod()
    {
        Token methodToken = _tokens[_i];
        _image.CallForeignMethod(methodToken.Text, methodToken.ExtraInfo.ArgsCount);
    }


    private void CompileLoop()
    {
        string loopLabel = GetNextLabelName();
        string endOfLoopLabel = GetNextLabelName();

        _i += 2;
        CompileNextBlock(TokenType.Semicolon);
        _image.SetLabel(loopLabel);
        _i++;
        CompileNextBlock(TokenType.Semicolon);
        _image.Goto(endOfLoopLabel, InstructionName.JumpIfZero);

        _i++;
        int iCopy = _i;
        PassTokensBeforeNext(TokenType.NewLine | TokenType.Semicolon);
        CompileNextBlock(TokenType.End);
        int copy = _i;
        _i = iCopy;
        CompileNextBlock(TokenType.NewLine | TokenType.Semicolon);
        _i = copy;

        _image.Goto(loopLabel, InstructionName.Jump);
        _image.SetLabel(endOfLoopLabel);
    }

    private void PassTokensBeforeNext(TokenType endTokens)
    {
        while (!endTokens.HasFlag(_tokens[_i].TokenType)) _i++;
    }

    private void CompileNextBlock(TokenType endTokenType)
    {
        while (true)
        {
            bool contains = endTokenType.HasFlag(_tokens[_i].TokenType);
            if (contains) break;

            TokenType nextToken = _i + 1 != _tokens.Count ? _tokens[_i + 1].TokenType : default;

            switch (_tokens[_i].TokenType)
            {
                case TokenType.Variable when CanLoadVariable(nextToken):
                    CompileLoadVariable();
                    break;
                case TokenType.LessThan:
                    _image.WriteNextOperation(InstructionName.LessThan);
                    break;
                case TokenType.GreatThan:
                    _image.WriteNextOperation(InstructionName.GreatThan);
                    break;
                case TokenType.IsEquals:
                    _image.WriteNextOperation(InstructionName.Equals);
                    break;
                case TokenType.EqualsSign:
                    CompileEqualsSign();
                    break;
                case TokenType.IsNot:
                    _image.WriteNextOperation(InstructionName.Not);
                    break;
                case TokenType.IsNotEquals:
                    _image.WriteNextOperation(InstructionName.Equals);
                    _image.WriteNextOperation(InstructionName.Not);
                    break;
                case TokenType.Structure:
                    CompileNewStructure();
                    break;
                case TokenType.If:
                    CompileIf();
                    break;
                case TokenType.Else:
                    CompileElse();
                    break;
                case TokenType.NewVariable:
                    _image.CreateVariable(_tokens[_i].Text);
                    break;
                case TokenType.Number:
                    _image.WriteNextOperation(InstructionName.PushConstant, _tokens[_i].Value);
                    break;
                case TokenType.String:
                    _image.WriteNextOperation(InstructionName.PushConstant, _tokens[_i].Value);
                    break;
                case TokenType.Plus:
                    _image.WriteNextOperation(InstructionName.Add);
                    break;
                case TokenType.Minus:
                    _image.WriteNextOperation(InstructionName.Sub);
                    break;
                case TokenType.Multiply:
                    _image.WriteNextOperation(InstructionName.Multiply);
                    break;
                case TokenType.Or:
                    _image.WriteNextOperation(InstructionName.Or);
                    break;
                case TokenType.And:
                    _image.WriteNextOperation(InstructionName.And);
                    break;
                case TokenType.Divide:
                    _image.WriteNextOperation(InstructionName.Divide);
                    break;
                case TokenType.ForeignMethod:
                    CompileMethod();
                    break;
                case TokenType.NewFunction:
                    CompileNewFunction();
                    break;
                case TokenType.Function:
                    CompileCallFunction();
                    break;
                case TokenType.Return:
                    CompileReturn();
                    break;
                case TokenType.Loop:
                    CompileLoop();
                    break;
                case TokenType.Struct:
                    PassTokensBeforeNext(TokenType.End);
                    break;
                case TokenType.Func:
                case TokenType.NewLine:
                case TokenType.Var:
                case TokenType.In:
                case TokenType.To:
                case TokenType.Of:
                case TokenType.Comma:
                case TokenType.OpenParentheses:
                case TokenType.CloseParentheses:
                case TokenType.Semicolon:
                    break;
                default:
                    Token token = _tokens[_i];
                    switch (token.TokenType)
                    {
                        case TokenType.Variable:
                            if (CanLoadVariable(nextToken))
                                throw new Exception($"Unexpected token {token}");
                            break;
                        default:
                            throw new Exception($"Unexpected token {token}");
                    }

                    break;
            }

            _i++;
        }
    }

    private void CompileNewStructure()
    {
        string structName = _tokens[_i].Text;
        Structure newStructure = new(_structures[structName]);
        _image.WriteNextOperation(InstructionName.PushConstant, newStructure);
    }

    private void CompileLoadVariable()
    {
        _image.LoadVariable(_tokens[_i].Text);

        List<Token> extraInfoParams = _tokens[_i].ExtraInfo.Params;
        if (extraInfoParams.Count != 0)
        {
            int paramId = IdManager.MakeHashCode(extraInfoParams[0].Text);
            _image.WriteNextOperation(InstructionName.PushField, paramId);
        }
    }

    private void CompileReturn()
    {
        _i++;
        CompileNextBlock(TokenType.NewLine | TokenType.Semicolon);

        _image.WriteNextOperation(InstructionName.Ret);
    }

    private void CompileCallFunction()
    {
        string methodName = _tokens[_i].Text;
        _image.Call(methodName, _functions[methodName].Count);
    }

    private void CompileNewFunction()
    {
        // string methodName = PrepareNewFunction(out List<string> parameters);
        string funcName = _tokens[_i].Text;
        PassTokensBeforeNext(TokenType.CloseParentheses);
        _image.CreateFunction(funcName, _functions[funcName].ToArray(), () => { CompileNextBlock(TokenType.End); });
    }

    private void PrepareNewFunction()
    {
        string methodName = _tokens[_i].Text;

        _i += 2;
        List<string> parameters = new();
        _i--;
        Token token;
        do
        {
            _i++;
            token = _tokens[_i];
            if (token.TokenType is TokenType.Comma or TokenType.Var or TokenType.CloseParentheses) continue;
            parameters.Add(token.Text);
        } while (token.TokenType != TokenType.CloseParentheses);

        _i++;
        _functions.Add(methodName, parameters);
    }

    private void PrepareNewStructure()
    {
        string structName = _tokens[_i].Text;

        List<string> entities = new();

        while (_tokens[++_i].TokenType != TokenType.End)
            if (_tokens[_i].TokenType == TokenType.NewVariable)
                entities.Add(_tokens[_i].Text);

        _structures.Add(structName, entities);
    }

    private static bool CanLoadVariable(TokenType nextToken)
    {
        return nextToken is not TokenType.EqualsSign;
    }

    private void CompileElse()
    {
        _i++;
        CompileNextBlock(TokenType.End);
    }

    private void CompileIf()
    {
        _i++;
        CompileNextBlock(TokenType.NewLine | TokenType.Semicolon);

        string labelNameElse = GetNextLabelName();
        string labelNameEnd = GetNextLabelName();

        _image.Goto(labelNameElse, InstructionName.JumpIfZero);
        CompileNextBlock(TokenType.End | TokenType.Else);
        _image.Goto(labelNameEnd, InstructionName.Jump);
        _image.SetLabel(labelNameElse);

        if (_tokens[_i].TokenType == TokenType.Else) CompileElse();
        _image.SetLabel(labelNameEnd);
    }


    private string GetNextLabelName()
    {
        return GenerateName(ref _labelName);
    }

    private static string GenerateName(ref string name)
    {
        int number = Convert.ToInt32(name[^1].ToString());
        int next = number + 1;

        if (next == 10) name += '0';
        else name = name[..^1] + next;

        return name;
    }
}