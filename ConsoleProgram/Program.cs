using static ConsoleProgram.ProgramConstants;

string codePath = args.Length == 0 ? TempCodePath : args[0];

if (!File.Exists(codePath)) File.WriteAllText(codePath, DefaultCode);

string code = File.ReadAllText(codePath);
VmFacade.VmFacade.Run(code);

/*
var q = 2

func IncreaseQ()
	q++
end

IncreaseQ()
PrintLn(q)
IncreaseQ()
PrintLn(q)

Input()
*/