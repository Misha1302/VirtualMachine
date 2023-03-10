using static ConsoleProgram.ProgramConstants;

string codePath = args.Length == 0 ? TempCodePath : args[0];
Console.WriteLine(string.Join(", ", args));

if (!File.Exists(codePath)) File.WriteAllText(codePath, DefaultCode);

string code = File.ReadAllText(codePath);
VmFacade.VmFacade.Run(code);