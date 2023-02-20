namespace VmCompiler;

using Tokenizer.Token;
using VirtualMachine;
using VirtualMachine.Variables;

public class VmCompiler
{
    private readonly VmImage _image;
    private string _labelName = "label0";
    private List<Token> _tokens = new();

    public VmCompiler(AssemblyManager assemblyManager)
    {
        _image = new VmImage(assemblyManager);
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
        string varName = _tokens[i - 1].Text;

        i++;
        CompileNextBlock(ref i, TokenType.NewLine | TokenType.Comma | TokenType.CloseParentheses);

        _image.SetVariable(varName);
    }

    private void CompileMethod(ref int i)
    {
        Token methodToken = _tokens[i];
        i += 2;
        CompileNextBlock(ref i, TokenType.CloseParentheses);
        i++;

        _image.CallForeignMethod(methodToken.Text);
    }


    private void CompileLoop(ref int i)
    {
        string loopLabel = GetNextLabelName();
        string endOfLoopLabel = GetNextLabelName();

        i += 2;
        CompileNextBlock(ref i, TokenType.Comma);
        _image.SetLabel(loopLabel);
        i++;
        CompileNextBlock(ref i, TokenType.Comma);
        _image.Goto(endOfLoopLabel, InstructionName.JumpIfZero);

        int iCopy = i;
        PassTokensBeforeNext(ref i, TokenType.CloseParentheses | TokenType.NewLine);
        CompileNextBlock(ref i, TokenType.End);
        CompileNextBlock(ref iCopy, TokenType.CloseParentheses | TokenType.NewLine);

        _image.Goto(loopLabel, InstructionName.Jump);
        _image.SetLabel(endOfLoopLabel);
    }

    private void PassTokensBeforeNext(ref int i, TokenType endTokens)
    {
        while (!endTokens.HasFlag(_tokens[i - 1].TokenType)) i++;
    }

    private void CompileNextBlock(ref int i, TokenType endTokenType)
    {
        while (true)
        {
            bool contains = endTokenType.HasFlag(_tokens[i].TokenType);
            if (contains) break;

            TokenType nextToken = i + 1 != _tokens.Count ? _tokens[i + 1].TokenType : default;
            TokenType previousToken = i != 0 ? _tokens[i - 1].TokenType : default;

            switch (_tokens[i].TokenType)
            {
                case TokenType.Variable when CanLoadVariable(nextToken, previousToken):
                    _image.LoadVariable(_tokens[i].Text);
                    break;
                case TokenType.OpenBracket:
                    CompileList(ref i);
                    i--;
                    break;
                case TokenType.LessThan:
                    _image.WriteNextOperation(InstructionName.LessThan);
                    break;
                case TokenType.GreatThan:
                    _image.WriteNextOperation(InstructionName.GreatThan);
                    break;
                case TokenType.Modulo:
                    _image.WriteNextOperation(InstructionName.Modulo);
                    break;
                case TokenType.SetElem:
                    CompileSetElem(ref i);
                    break;
                case TokenType.IsEquals:
                    _image.WriteNextOperation(InstructionName.Equals);
                    break;
                case TokenType.EqualsSign:
                    CompileEqualsSign(ref i);
                    i--;
                    break;
                case TokenType.IsNot:
                    _image.WriteNextOperation(InstructionName.Not);
                    break;
                case TokenType.IsNotEquals:
                    _image.WriteNextOperation(InstructionName.Equals);
                    _image.WriteNextOperation(InstructionName.Not);
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
                case TokenType.Number when CanLoadNumber(nextToken, previousToken):
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
                case TokenType.Or:
                    _image.WriteNextOperation(InstructionName.Or);
                    break;
                case TokenType.And:
                    _image.WriteNextOperation(InstructionName.And);
                    break;
                case TokenType.Divide:
                    _image.WriteNextOperation(InstructionName.Divide);
                    break;
                case TokenType.Ptr:
                    CompilePtr(ref i);
                    break;
                case TokenType.PushByPtr:
                    PushByPtr(ref i);
                    break;
                case TokenType.ElemOf:
                    CompileElemOf(ref i);
                    break;
                case TokenType.PtrEqualsSign:
                    CompilePtrSet(ref i);
                    i--;
                    break;
                case TokenType.ForeignMethod:
                    CompileMethod(ref i);
                    i--;
                    break;
                case TokenType.Loop:
                    CompileLoop(ref i);
                    break;
                case TokenType.NewLine:
                case TokenType.Var:
                case TokenType.In:
                case TokenType.To:
                case TokenType.Of:
                case TokenType.Comma:
                    break;
                default:
                    Token token = _tokens[i];
                    switch (token.TokenType)
                    {
                        case TokenType.Variable:
                        {
                            if (CanLoadVariable(nextToken, previousToken))
                                throw new Exception($"Unexpected token {token}");
                            break;
                        }
                        case TokenType.Number:
                        {
                            if (CanLoadNumber(nextToken, previousToken))
                                throw new Exception($"Unexpected token {token}");
                            break;
                        }
                        default:
                            throw new Exception($"Unexpected token {token}");
                    }

                    break;
            }

            i++;
        }
    }

    private static bool CanLoadVariable(TokenType nextToken, TokenType previousToken)
    {
        return nextToken is not TokenType.EqualsSign and not TokenType.PtrEqualsSign and not TokenType.ElemOf
               && previousToken is not TokenType.ElemOf;
    }

    private static bool CanLoadNumber(TokenType nextToken, TokenType previousToken)
    {
        return nextToken is not TokenType.ElemOf;
    }

    private void CompileSetElem(ref int i)
    {
        i++;
        CompileNextBlock(ref i, TokenType.NewLine);
        _image.WriteNextOperation(InstructionName.SetElem);
    }

    private void CompileList(ref int i)
    {
        _image.WriteNextOperation(InstructionName.PushConstant, new VmList());
        PassTokensBeforeNext(ref i, TokenType.NewLine);
        i--;
    }

    private void CompileElemOf(ref int i)
    {
        Token a = _tokens[i - 1];
        Token b = _tokens[i + 1];

        if (a.TokenType == TokenType.Number) _image.WriteNextOperation(InstructionName.PushConstant, a.Value);
        else _image.LoadVariable(a.Text);

        _image.LoadVariable(b.Text);
        _image.WriteNextOperation(InstructionName.ElemOf);
        i++;
    }

    private void CompilePtrSet(ref int i)
    {
        int index = i - 1;
        i++; // '->'
        CompileNextBlock(ref i, TokenType.NewLine);
        _image.LoadVariable(_tokens[index].Text);
        _image.WriteNextOperation(InstructionName.SetToPtr);
    }

    private void CompilePtr(ref int i)
    {
        i++;
        int index = i;
        int id = (_image.Variables.FindLast(x => x.Name == _tokens[index].Text) ?? throw new InvalidOperationException()).Id;
        if (id == 0) throw new InvalidOperationException($"Variable {_tokens[index].Text} was not found");
        _image.WriteNextOperation(InstructionName.PushConstant, id);
        _image.WriteNextOperation(InstructionName.GetPtr);
    }

    private void PushByPtr(ref int i)
    {
        i++;
        int index = i;
        _image.LoadVariable(_tokens[index].Text);
        _image.WriteNextOperation(InstructionName.PushByPtr);
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

        string labelNameElse = GetNextLabelName();
        string labelNameEnd = GetNextLabelName();

        _image.Goto(labelNameElse, InstructionName.JumpIfZero);
        CompileNextBlock(ref i, TokenType.End | TokenType.Else);
        _image.Goto(labelNameEnd, InstructionName.Jump);
        _image.SetLabel(labelNameElse);

        if (_tokens[i].TokenType == TokenType.Else) CompileElse(ref i);
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