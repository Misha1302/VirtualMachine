using static ConsoleProgram.ProgramConstants;

string codePath = args.Length == 0 ? TempCodePath : args[0];
Console.WriteLine(string.Join(", ", args));

if (!File.Exists(codePath)) File.WriteAllText(codePath, DefaultCode);

string code = File.ReadAllText(codePath);
VmFacade.VmFacade.Run(code);

/*
# absolutely useless program that checks the operation of Jump and Try / Failed

var time = GetTime()

loop var i = 0; i < 400; i++
	_ = SuperFunc(i)
	PrintLn(i)
end

PrintLn(GetTime() - time)


func SuperFunc(var i)
	try
		PrintLn('hello!')
		
		try
			PrintLn(i / 0)
		failed
			PrintLn(error + '!!!')
		end
		
		PrintLn(i / 0)
	failed
		PrintLn(error + 'QQQ ' + i)
	end
end

*/