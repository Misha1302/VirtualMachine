namespace VmCompiler;

using Tokenizer.Token;
using VirtualMachine;
using VirtualMachine.Variables;
using VirtualMachine.VmRuntime;

public class VmCompiler
{
    private readonly Dictionary<string, List<string>> _functions = new();
    private readonly VmImage _image;

    private readonly Dictionary<string, List<string>> _structures = new();
    private string _currentStructName = string.Empty;

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
            {
                PrepareNewFunction();
            }
            else if (_tokens[_i].TokenType == TokenType.NewStruct)
            {
                string structName = _tokens[_i].Text;
                int nestingLevel = 0;
                do
                {
                    switch (_tokens[_i].TokenType)
                    {
                        case TokenType.NewStruct:
                        case TokenType.Func:
                            nestingLevel++;
                            break;
                        case TokenType.End:
                            nestingLevel--;
                            break;
                        case TokenType.NewFunction:
                            _currentStructName = structName;
                            PrepareNewFunction();
                            _currentStructName = string.Empty;
                            break;
                    }

                    _i++;
                } while (nestingLevel != 0);
            }

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

    private void CompileCallMethod()
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

            List<Token> extraInfoParams = _tokens[_i].ExtraInfo.Params;
            switch (_tokens[_i].TokenType)
            {
                case TokenType.Variable when extraInfoParams.Any(x => x.TokenType == TokenType.Function):
                    CompileCallFunctionByStruct();
                    break;
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
                    _image.WriteNextOperation(InstructionName.NotEquals);
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
                case TokenType.FunctionByStruct:
                    _image.CallStructFunction(_tokens[_i].Text, _tokens[_i].ExtraInfo.ArgsCount);
                    break;
                case TokenType.ForeignMethod:
                    CompileCallMethod();
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
                case TokenType.Modulo:
                    _image.WriteNextOperation(InstructionName.Modulo);
                    break;
                case TokenType.Failed:
                    CompileFailed();
                    break;
                case TokenType.Increase:
                    _image.WriteNextOperation(InstructionName.Increase, IdManager.MakeHashCode(_tokens[_i - 1].Text));
                    break;
                case TokenType.Decrease:
                    _image.WriteNextOperation(InstructionName.Decrease, IdManager.MakeHashCode(_tokens[_i - 1].Text));
                    break;
                case TokenType.Try:
                    CompileTry();
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
                    if (token.TokenType != TokenType.Variable) throw new Exception($"Unexpected token {token}");
                    if (CanLoadVariable(nextToken)) throw new Exception($"Unexpected token {token}");

                    break;
            }

            _i++;
        }
    }

    private void CompileCallFunctionByStruct()
    {
        List<Token> tokensCopy = _tokens;
        int iCopy = _i;
        _i = 0;

        List<Token> extraInfoParams = _tokens[iCopy].ExtraInfo.Params;
        extraInfoParams.Add(new Token(TokenType.Eof, "\0"));
        _tokens = extraInfoParams;
        if (_tokens[^2].TokenType == TokenType.Function) _tokens[^2].TokenType = TokenType.FunctionByStruct;
        CompileNextBlock(TokenType.Eof);

        _tokens = tokensCopy;
        _i = iCopy;
    }

    private void CompileTry()
    {
        int constantPtr = _image.Ip;
        _image.WriteNextConstant((decimal)-1, constantPtr);
        _image.WriteNextOperation(InstructionName.PushFailed);

        _i++;
        CompileNextBlock(TokenType.Failed);
        _image.ReplaceConstant(constantPtr, (decimal)_image.Ip);
        CompileFailed();
    }

    private void CompileFailed()
    {
        int constantPtr = _image.Ip;
        _image.WriteNextConstant((decimal)-1, constantPtr);
        _image.WriteNextOperation(InstructionName.Jump);

        _image.CreateVariable("error");
        _image.SetVariable("error");

        _i++;
        CompileNextBlock(TokenType.End);

        _image.ReplaceConstant(constantPtr, (decimal)_image.Ip - 1);
    }

    private void CompileNewStructure()
    {
        string structName = _tokens[_i].Text;
        VmStruct newStructure = new(_structures[structName], structName);
        _image.WriteNextOperation(InstructionName.PushConstant, newStructure);
    }

    private void CompileLoadVariable()
    {
        _image.LoadVariable(_tokens[_i].Text);

        List<Token> extraInfoParams = _tokens[_i].ExtraInfo.Params;
        if (extraInfoParams.Count == 0) return;

        int paramId = IdManager.MakeHashCode(extraInfoParams[0].Text);
        _image.WriteNextOperation(InstructionName.PushField, paramId);
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
        _image.CallFunction(methodName, _functions[methodName].Count);
    }

    private void CompileNewFunction()
    {
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
        _functions.Add(_currentStructName + methodName, parameters);
    }

    private void PrepareNewStructure()
    {
        string structName = _tokens[_i].Text;

        List<string> entities = new();

        int startIndex = _i;
        while (_tokens[++_i].TokenType != TokenType.End)
            switch (_tokens[_i].TokenType)
            {
                case TokenType.NewVariable:
                    entities.Add(_tokens[_i].Text);
                    break;
                case TokenType.NewFunction:
                    string funcName = structName + _tokens[_i].Text;
                    PassTokensBeforeNext(TokenType.CloseParentheses);
                    _image.CreateFunction(funcName, _functions[funcName].ToArray(),
                        () => { CompileNextBlock(TokenType.End); });
                    break;
            }

        _tokens.RemoveRange(startIndex - 1, _i - startIndex + 2);
        _i = startIndex;

        _structures.Add(structName, entities);
    }

    private static bool CanLoadVariable(TokenType nextToken)
    {
        return nextToken is not TokenType.EqualsSign and not TokenType.Increase and not TokenType.Decrease;
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