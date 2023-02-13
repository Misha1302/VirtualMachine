using Tokenizer.Lexer;
using Tokenizer.Token;

const string code = """
repeat(0, 5, i)
    list = (('Hello, human number ' + i) / ' ') + 'Added!'
    Print(list[3] + ' ' + list[4])
end
""";

List<Token> tokens = Lexer.Tokenize(code);

foreach (Token item in tokens) 
    Console.WriteLine($"{item.TokenType}::{item.Text}::{item.Value}");