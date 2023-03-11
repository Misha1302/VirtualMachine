namespace Tokenizer.Token;

[Flags]
public enum TokenType : ulong
{
    OpenParentheses = 1UL << 0, // (
    CloseParentheses = 1UL << 1, // )
    Struct = 1UL << 2, // STRUCT name
    NewStruct = 1UL << 3, // struct NAME
    End = 1UL << 4, // end
    Loop = 1UL << 5, // loop (for)
    Number = 1UL << 6, // 0
    Variable = 1UL << 7, // i
    Modulo = 1UL << 8, // XXX
    String = 1UL << 9, // 'S t r'
    Plus = 1UL << 10, // +
    Minus = 1UL << 11, // -
    Multiply = 1UL << 12, // *
    Divide = 1UL << 13, // /
    ForeignMethod = 1UL << 14, // Print
    Eof = 1UL << 15, // \0
    Unknown = 1UL << 16, // X
    NewLine = 1UL << 17, // \r\n
    Comma = 1UL << 18,
    Dot = 1UL << 19, // .
    WhiteSpace = 1UL << 20, // ' '
    EqualsSign = 1UL << 21, // =
    Comment = 1UL << 22, // # XXX
    Import = 1UL << 23, // import 'X' 'X'
    Var = 1UL << 24, // VAR x
    NewVariable = 1UL << 25, // var X
    LessThan = 1UL << 26, // <
    GreatThan = 1UL << 27, // >
    IsEquals = 1UL << 28, // ==
    If = 1UL << 29, // if
    Else = 1UL << 30, // else
    Structure = 1UL << 31, // NameOfStruct
    Increase = 1UL << 32, // ++
    IsNotEquals = 1UL << 33, // !=
    IsNot = 1UL << 34, // !
    In = 1UL << 35, // in
    To = 1UL << 36, // to
    And = 1UL << 37, // and
    Or = 1UL << 38, // or
    Of = 1UL << 39, // of
    Func = 1UL << 40, // FUNC name
    NewFunction = 1UL << 41, // func NAME
    Function = 1UL << 42, // functionName()
    Return = 1UL << 43, // RETURN xxx
    AtSign = 1UL << 44, // @
    Semicolon = 1UL << 45, // ;
    Decrease = 1UL << 46, // --
    Try = 1UL << 47, // try
    Failed = 1UL << 48, // failed
    FunctionByStruct = 1UL << 49
}