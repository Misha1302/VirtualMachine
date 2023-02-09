namespace VirtualMachine;

public enum InstructionName : byte
{
    JumpIfZero = 1,

    NotNumber,
    EqualsNumber,

    AddNumber,
    SubNumber,
    MultiplyNumber,
    DivideNumber,

    Halt,
    SetVariable,
    LoadVariable,
    CallMethod,
    Duplicate,
    LessThan,
    JumpIfNotZero,
    Ret,
    Jump,
    End,
    PushAddress,
    CreateVariable,
    PushConstant,
    CopyVariable,
    DeleteVariable,
    NoOperation
}