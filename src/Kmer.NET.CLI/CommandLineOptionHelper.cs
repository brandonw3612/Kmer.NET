using CommandLine;

namespace Kmer.NET.CLI;

public class CommandLineOptionHelper
{
    [Option(shortName: 'i', Default = "/dev/stdin", Required = false, HelpText = "The input file in fasta format. All sequence characters will be converted to uppercase.")]
    public string InputFileName { get; set; }

    [Option(shortName: 'o', Default = "/dev/stdout", Required = false, HelpText = "The output file in tab-separated value (tsv) format. Please see `README' column details.")]
    public string OutputFileName { get; set; }

    [Option(shortName: 'a', Default = "A,C,G,T", Required = false, HelpText = "A comma-separated list of valid, uppercase characters (nucleotides). Characters not in this list will be ignored.")]
    public string AlphabetString { get; set; }

    [Option(shortName: 'A', Default = false, Required = false, HelpText = "Report non-atomic SSRs.")]
    public bool AllowNonAtomic { get; set; }
    
    [Option(shortName: 'b', Default = false, Required = false)]
    public bool BZipped2Input { get; set; }
    
    [Option(shortName: 'B', Default = false, Required = false)]
    public bool BZipped2Output { get; set; }
    
    [Option(shortName: 'd', Default = false, Required = false, HelpText = "Disable the progress bar that normally prints to stderr. Will automatically be disabled if (a) reading from stdin or (b) writing to stdout without redirecting it to a file.")]
    public bool HideProgressBar { get; set; }
    
    [Option(shortName: 'e', Default = false, Required = false, HelpText = "Disable all filters and SSR validation to report every SSR. Similar to: -A -r 2 -R <big_number> -n 2 -N <big_number>. This will override any options set for -n, -N, -r, -R, and -s.")]
    public bool Exhaustive { get; set; }
    
    [Option(shortName: 'g', Default = false, Required = false)]
    public bool GZippedInput { get; set; }
    
    [Option(shortName: 'G', Default = false, Required = false)]
    public bool GZippedOutput { get; set; }
    
    [Option(shortName: 'u', Default = false, Required = false)]
    public bool DisplayUsage { get; set; }
    
    [Option(shortName: 'l', Default = 10, Required = false, HelpText = "Only search for SSRs in sequences with total length >= l.")]
    public int MinSequenceLength { get; set; }
    
    [Option(shortName: 'L', Default = 500000000, Required = false, HelpText = "Only search for SSRs in sequences with total length <= L.")]
    public int MaxSequenceLength { get; set; }
    
    [Option(shortName: 'n', Default = 16, Required = false, HelpText = "Keep only SSRs with total length (number of nucleotides) >= n.")]
    public int MinNucleotides { get; set; }
    
    [Option(shortName: 'N', Default = 10000, Required = false, HelpText = "Keep only SSRs with total length (number of nucleotides) <= N.")]
    public int MaxNucleotides { get; set; }
    
    [Option(shortName: 'p', Default = "4-8", Required = false, HelpText = "A comma-separated list of period sizes (i.e., kmer lengths). Inclusive ranges are also supported using a hyphen.")]
    public string PeriodStr { get; set; }
    
    [Option(shortName: 'Q', Default = 1000, Required = false, HelpText = "Max size of the tasks queue.")]
    public int MaxTaskQueueSize { get; set; }
    
    [Option(shortName: 'r', Default = 2, Required = false, HelpText = "Keep only SSRs that repeat >= r times.")]
    public int MinRepeats { get; set; }
    
    [Option(shortName: 'R', Default = 1000, Required = false, HelpText = "Keep only SSRs that repeat <= R times.")]
    public int MaxRepeats { get; set; }
    
    [Option(shortName: 's', Default = "", Required = false, HelpText = "A comma-separated list of SSRs to search for; e.g. \"AC,GTTA,TTCTG,CCG\" or \"TGA\". Please note that other options may prevent SSRs specified with this option from appearing in the output. For example, if -p is \"4-6\", then an SSR with a repeating \"AC\" will never be displayed because \"AC\" has a period size of 2 (and, as it turns out, 2 is not in the range 4-6).")]
    public string EnumeratedSsrsStr { get; set; }

    [Option(shortName: 't', Default = 1, Required = false, HelpText = "Number of threads.")]
    public int Threads { get; set; }
    
    [Option(shortName: 'v', Default = false, Required = false)]
    public bool DisplayVersion { get; set; }

    public Arguments BuildArguments()
    {
        // Sanity check
        if (MinNucleotides > MaxNucleotides || MinRepeats > MaxRepeats || MinSequenceLength > MaxSequenceLength)
            throw new Exception("Minimum arguments cannot be greater than maximum arguments");
        
        // Compressed input file check
        if (InputFileName.Length >= 3 && InputFileName[^3..] == ".gz") GZippedInput = true;
        if (InputFileName.Length >= 4 && InputFileName[^4..] == ".bz2") BZipped2Input = true;
        
        // Compressed output file check
        if (OutputFileName.Length >= 3 && OutputFileName[^3..] == ".gz") GZippedOutput = true;
        if (OutputFileName.Length >= 4 && OutputFileName[^4..] == ".bz2") BZipped2Output = true;
        
        Arguments args = new()
        {
            Alphabet = AlphabetString.Where(char.IsLetter).ToHashSet(),
            AllowNonAtomic = AllowNonAtomic,
            BZipped2Input = BZipped2Input,
            BZipped2Output = BZipped2Output,
            Exhaustive = Exhaustive,
            GZippedInput = GZippedInput,
            GZippedOutput = GZippedOutput,
            InputFileName = InputFileName,
            MinSequenceLength = MinSequenceLength,
            MaxSequenceLength = MaxSequenceLength,
            MinNucleotides = MinNucleotides,
            MaxNucleotides = MaxNucleotides,
            OutputFileName = OutputFileName,
            MaxTaskQueueSize = MaxTaskQueueSize,
            MinRepeats = MinRepeats,
            MaxRepeats = MaxRepeats,
            EnumeratedSsrs = EnumeratedSsrsStr.ToUpper().Split(',').Where(s => s.Length > 0).ToHashSet(),
            Threads = Threads
        };

        foreach (var segment in PeriodStr.ToUpper().Split(','))
        {
            if (segment.Length <= 0) continue;
            var rangeSplit = segment.Split('-');
            switch (rangeSplit.Length)
            {
                case 2:
                {
                    for (int i = int.Parse(rangeSplit[0]); i <= int.Parse(rangeSplit[1]); i++)
                    {
                        args.Periods.Add(i);
                    }
                    break;
                }
                case 1:
                    args.Periods.Add(int.Parse(rangeSplit[0]));
                    break;
                default:
                    throw new Exception("Argument error: -p.");
            }
        }

        args.MinSequenceLength = Math.Max(MinSequenceLength, args.Periods.Min * 2);

        return args;
    }
}