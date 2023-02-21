namespace VmCompiler;

using Tokenizer.Token;
using VirtualMachine;
using VirtualMachine.Variables;

public class VmCompiler
{
    private readonly VmImage _image;
    private string _labelName = "label0";
    private List<Token> _tokens = new();
    private int i;

    public VmCompiler(AssemblyManager assemblyManager)
    {
        _image = new VmImage(assemblyManager);
    }

    public VmImage Compile(List<Token> tokens)
    {
        _tokens = tokens;
        CompileNextBlock(TokenType.Eof);

        return _image;
    }

    private void CompileEqualsSign()
    {
        string varName = _tokens[i - 1].Text;

        i++;
        CompileNextBlock(TokenType.NewLine | TokenType.Comma | TokenType.CloseParentheses);

        _image.SetVariable(varName);
    }

    private void CompileMethod()
    {
        Token methodToken = _tokens[i];
        i += 2;
        CompileNextBlock(TokenType.CloseParentheses);
        i++;

        _image.CallForeignMethod(methodToken.Text);
    }


    private void CompileLoop()
    {
        string loopLabel = GetNextLabelName();
        string endOfLoopLabel = GetNextLabelName();

        i += 2;
        CompileNextBlock(TokenType.Comma);
        _image.SetLabel(loopLabel);
        i++;
        CompileNextBlock(TokenType.Comma);
        _image.Goto(endOfLoopLabel, InstructionName.JumpIfZero);

        int iCopy = i;
        PassTokensBeforeNext(TokenType.CloseParentheses | TokenType.NewLine);
        CompileNextBlock(TokenType.End);
        i = iCopy;
        CompileNextBlock(TokenType.CloseParentheses | TokenType.NewLine);

        _image.Goto(loopLabel, InstructionName.Jump);
        _image.SetLabel(endOfLoopLabel);
    }

    private void PassTokensBeforeNext(TokenType endTokens)
    {
        while (!endTokens.HasFlag(_tokens[i - 1].TokenType)) i++;
    }

    private void CompileNextBlock(TokenType endTokenType)
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
                    CompileList();
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
                    CompileSetElem();
                    break;
                case TokenType.IsEquals:
                    _image.WriteNextOperation(InstructionName.Equals);
                    break;
                case TokenType.EqualsSign:
                    CompileEqualsSign();
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
                    CompileIf();
                    i--;
                    break;
                case TokenType.Else:
                    CompileElse();
                    i--;
                    break;
                case TokenType.NewVariable:
                    _image.CreateVariable(_tokens[i].Text);
                    break;
                case TokenType.Number when CanLoadNumber(nextToken):
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
                    CompilePtr();
                    break;
                case TokenType.PushByPtr:
                    PushByPtr();
                    break;
                case TokenType.ElemOf:
                    CompileElemOf();
                    break;
                case TokenType.PtrEqualsSign:
                    CompilePtrSet();
                    i--;
                    break;
                case TokenType.ForeignMethod:
                    CompileMethod();
                    i--;
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
                case TokenType.Func:
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
                            if (CanLoadVariable(nextToken, previousToken))
                                throw new Exception($"Unexpected token {token}");
                            break;
                        case TokenType.Number:
                            if (CanLoadNumber(nextToken)) throw new Exception($"Unexpected token {token}");
                            break;
                        default:
                            throw new Exception($"Unexpected token {token}");
                    }

                    break;
            }

            i++;
        }
    }

    private void CompileReturn()
    {
        i++;
        CompileNextBlock(TokenType.NewLine);
        _image.WriteNextOperation(InstructionName.Ret);
    }

    private void CompileCallFunction()
    {
        string methodName = _tokens[i].Text;

        i += 2;
        CompileNextBlock(TokenType.CloseParentheses);

        _image.Call(methodName);
    }

    private void CompileNewFunction()
    {
        string methodName = _tokens[i].Text;

        i += 2;
        List<string> parameters = new();
        i--;
        Token token;
        do
        {
            i++;
            token = _tokens[i];
            if (token.TokenType is TokenType.Comma or TokenType.Var or TokenType.CloseParentheses) continue;
            parameters.Add(token.Text);
        } while (token.TokenType != TokenType.CloseParentheses);

        i++;
        _image.CreateFunction(methodName, parameters.ToArray(), () => { CompileNextBlock(TokenType.End); });
    }

    private static bool CanLoadVariable(TokenType nextToken, TokenType previousToken)
    {
        return nextToken is not TokenType.EqualsSign and not TokenType.PtrEqualsSign and not TokenType.ElemOf
               && previousToken is not TokenType.ElemOf;
    }

    private static bool CanLoadNumber(TokenType nextToken)
    {
        return nextToken is not TokenType.ElemOf;
    }

    private void CompileSetElem()
    {
        i++;
        CompileNextBlock(TokenType.NewLine);
        _image.WriteNextOperation(InstructionName.SetElem);
    }

    private void CompileList()
    {
        _image.WriteNextOperation(InstructionName.PushConstant, new VmList());
        PassTokensBeforeNext(TokenType.NewLine);
        i--;
    }

    private void CompileElemOf()
    {
        Token a = _tokens[i - 1];
        Token b = _tokens[i + 1];

        if (a.TokenType == TokenType.Number) _image.WriteNextOperation(InstructionName.PushConstant, a.Value);
        else _image.LoadVariable(a.Text);

        _image.LoadVariable(b.Text);
        _image.WriteNextOperation(InstructionName.ElemOf);
        i++;
    }

    private void CompilePtrSet()
    {
        int index = i - 1;
        i++; // '->'
        CompileNextBlock(TokenType.NewLine);
        _image.LoadVariable(_tokens[index].Text);
        _image.WriteNextOperation(InstructionName.SetToPtr);
    }

    private void CompilePtr()
    {
        i++;
        int index = i;
        int id =
            (_image.Variables.FindLast(x => x.Name == _tokens[index].Text) ?? throw new InvalidOperationException()).Id;
        if (id == 0) throw new InvalidOperationException($"Variable {_tokens[index].Text} was not found");
        _image.WriteNextOperation(InstructionName.PushConstant, id);
        _image.WriteNextOperation(InstructionName.GetPtr);
    }

    private void PushByPtr()
    {
        i++;
        int index = i;
        _image.LoadVariable(_tokens[index].Text);
        _image.WriteNextOperation(InstructionName.PushByPtr);
    }

    private void CompileElse()
    {
        i++;
        CompileNextBlock(TokenType.End);
        i--;
    }

    private void CompileIf()
    {
        i++;
        CompileNextBlock(TokenType.NewLine);

        string labelNameElse = GetNextLabelName();
        string labelNameEnd = GetNextLabelName();

        _image.Goto(labelNameElse, InstructionName.JumpIfZero);
        CompileNextBlock(TokenType.End | TokenType.Else);
        _image.Goto(labelNameEnd, InstructionName.Jump);
        _image.SetLabel(labelNameElse);

        if (_tokens[i].TokenType == TokenType.Else) CompileElse();
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