namespace VirtualMachine;

public enum InstructionName : byte
{
    JumpIfZero = 1,

    Not,
    Equals,

    Add,
    Sub,
    Multiply,
    Divide,

    Halt,
    SetVariable,
    LoadVariable,
    CallMethod,
    Duplicate,
    LessThan,
    JumpIfNotZero,
    Ret,
    Jump,
    EndOfProgram,
    PushAddress,
    CreateVariable,
    PushConstant,
    NoOperation,
    GreatThan,
    Drop,

    Or,
    And,
    PushField,
    SetField,
    Modulo,
    Increase,
    Decrease,
    NotEquals,
    PushFailed
}