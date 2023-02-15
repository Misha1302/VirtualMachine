﻿namespace VmCompiler;

using Tokenizer.Token;
using VirtualMachine;

public class VmCompiler
{
    private readonly VmImage _image;
    private string? _labelName = "label0";
    private List<Token> _tokens = new();

    public VmCompiler(string mainLibPath)
    {
        _image = new VmImage(mainLibPath);
    }

    public VmImage Compile(List<Token> tokens)
    {
        _tokens = tokens;
        int i = 0;
        CompileNextBlock(ref i, TokenType.Eof);

        return _image;
    }

    private void CompileEqualsSign(ref int i)
    {
        string? varName = _tokens[i - 1].Text;

        i++;
        CompileNextBlock(ref i, TokenType.NewLine | TokenType.Comma | TokenType.CloseParentheses);

        _image.SetVariable(varName);
    }

    private void CompileMethod(ref int i)
    {
        Token methodToken = _tokens[i];
        i += 2;
        CompileNextBlock(ref i, TokenType.CloseParentheses);

        _image.CallForeignMethod(methodToken.Text);
    }


    private void CompileFor(ref int i)
    {
        /*
        for(var i = 0, i < q, i = i + 1)
            Print('Number ' i +)
        end
        */


        string? loopLabel = GetNextLabelName();
        string? endOfLoopLabel = GetNextLabelName();

        i += 2;
        CompileNextBlock(ref i, TokenType.Comma);
        _image.SetLabel(loopLabel);
        i++;
        CompileNextBlock(ref i, TokenType.Comma);
        _image.Goto(endOfLoopLabel, InstructionName.JumpIfZero);

        int iCopy = i;
        while (_tokens[i - 1].TokenType != TokenType.CloseParentheses) i++;
        CompileNextBlock(ref i, TokenType.End);
        CompileNextBlock(ref iCopy, TokenType.CloseParentheses | TokenType.NewLine);

        _image.Goto(loopLabel, InstructionName.Jump);
        _image.SetLabel(endOfLoopLabel);
    }

    private void CompileNextBlock(ref int i, TokenType endTokenType)
    {
        while (true)
        {
            bool contains = _tokens[i].TokenType.IsContains(endTokenType);
            if (contains) break;

            switch (_tokens[i].TokenType)
            {
                case TokenType.Variable:
                    _image.LoadVariable(_tokens[i].Text);
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
                    CompileEqualsSign(ref i);
                    i--;
                    break;
                case TokenType.If:
                    CompileIf(ref i);
                    i--;
                    break;
                case TokenType.Else:
                    CompileElse(ref i);
                    i--;
                    break;
                case TokenType.NewVariable:
                    _image.CreateVariable(_tokens[i].Text);
                    break;
                case TokenType.Number:
                    _image.WriteNextOperation(InstructionName.PushConstant, _tokens[i].Value);
                    break;
                case TokenType.String:
                    _image.WriteNextOperation(InstructionName.PushConstant, _tokens[i].Value);
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
                case TokenType.Divide:
                    _image.WriteNextOperation(InstructionName.Divide);
                    break;
                case TokenType.ForeignMethod:
                    CompileMethod(ref i);
                    i--;
                    break;
                case TokenType.For:
                    CompileFor(ref i);
                    break;
            }

            i++;
        }
    }

    private void CompileElse(ref int i)
    {
        i++;
        CompileNextBlock(ref i, TokenType.End);
        i--;
    }

    private void CompileIf(ref int i)
    {
        i++;
        CompileNextBlock(ref i, TokenType.NewLine);

        string? labelNameElse = GetNextLabelName();
        string? labelNameEnd = GetNextLabelName();

        _image.Goto(labelNameElse, InstructionName.JumpIfZero);
        CompileNextBlock(ref i, TokenType.End | TokenType.Else);
        _image.Goto(labelNameEnd, InstructionName.Jump);
        _image.SetLabel(labelNameElse);

        if (_tokens[i].TokenType == TokenType.Else) CompileElse(ref i);
        _image.SetLabel(labelNameEnd);
    }


    private string? GetNextLabelName()
    {
        return GenerateName(ref _labelName);
    }

    private static string? GenerateName(ref string? name)
    {
        int number = Convert.ToInt32(name[^1].ToString());
        int next = number + 1;

        if (next == 10) name += '0';
        else name = name[..^1] + next;

        return name;
    }
}