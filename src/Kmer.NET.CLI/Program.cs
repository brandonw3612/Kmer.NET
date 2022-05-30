using CommandLine;
using Kmer.NET;
using Kmer.NET.CLI;

var parsingResult = Parser.Default.ParseArguments<CommandLineOptionHelper>(args);
if (parsingResult.Errors.Any() || parsingResult.Value is null) throw new Exception("Argument error.");
var arguments = parsingResult.Value;
var ssrFinderArgs = arguments.BuildArguments();

if (arguments.DisplayUsage)
{
    Console.WriteLine(Messages.UsageMessage);
    return;
}

if (arguments.DisplayVersion)
{
    Console.WriteLine(Messages.Version);
    return;
}

SsrFinder finder = new(ssrFinderArgs, new CliProgressController());
finder.Run();