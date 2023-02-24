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
    End,
    PushAddress,
    CreateVariable,
    PushConstant,
    NoOperation,
    ElemOf,
    GreatThan,
    Drop,
    Modulo,
    SetElem,

    Or,
    And
}