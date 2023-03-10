namespace ConsoleProgram;

using System.Text.RegularExpressions;

public static class ProgramConstants
{
    public const string TempCodePath = @"C:\VirtualMachine\code.txt";

    public static readonly string DefaultCode = $@"
import @'MathLibrary\MathLibrary.dll'

PrintLn('This is default code') 
PrintLn('It is located at {Regex.Escape(TempCodePath)}')
PrintLn('The repository with the project is located at the url:
https://github.com/Misha1302/VirtualMachine \n')

Print('Write the number: ')
var n = ToNumber(Input())
var sqrt = Sqrt(n)
PrintLn('Sqrt of ' + n + ' = ' + sqrt)
PrintLn(sqrt + ' * ' + sqrt + ' = ' + sqrt * sqrt)
";
}