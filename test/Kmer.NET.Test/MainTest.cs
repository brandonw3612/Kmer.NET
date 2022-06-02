namespace Kmer.NET.Test;

public class MainTest
{
    private const string InputDirectory = "../../../../../data/input/";
    private const string ExpectedOutputDirectory = "../../../../../data/expected_output/";
    private const string ActualOutputDirectory = "../../../../../data/actual_output/";
    
    [Theory]
    [InlineData("1.fasta", "1.tsv")]
    [InlineData("2.fasta", "2.tsv")]
    [InlineData("3.fasta", "3.tsv")]
    [InlineData("4.fasta", "4.tsv")]
    [InlineData("5.fasta", "5.tsv")]
    [InlineData("6.fasta", "6.tsv")]
    [InlineData("7.fasta", "7.tsv")]
    [InlineData("8.fasta", "8.tsv")]
    [InlineData("9.fasta", "9.tsv")]
    [InlineData("10.fasta", "10.tsv")]
    [InlineData("11.fasta", "11.tsv")]
    [InlineData("12.fasta", "12.tsv")]
    public void SimpleTest(string input, string output)
    {
        string inputFilePath = Path.Join(Environment.CurrentDirectory, InputDirectory, input);
        string expectedOutputFilePath = Path.Join(Environment.CurrentDirectory, ExpectedOutputDirectory, output);
        string actualOutputFilePath = Path.Join(Environment.CurrentDirectory, ActualOutputDirectory, output);

        var args = new Arguments
        {
            AllowNonAtomic = false,
            Alphabet = new() { 'A', 'C', 'T', 'G' },
            BZipped2Input = false,
            BZipped2Output = false,
            EnumeratedSsrs = new(),
            Threads = 1,
            Exhaustive = true,
            GZippedInput = false,
            GZippedOutput = false,
            InputFileName = inputFilePath,
            MinSequenceLength = 0,
            MaxSequenceLength = 0,
            MinNucleotides = 0,
            MaxNucleotides = 0,
            OutputFileName = actualOutputFilePath,
            MaxTaskQueueSize = 1000,
            MinRepeats = 0,
            MaxRepeats = 0,
        };

        SsrFinder finder = new(args);
        finder.Run();

        StreamReader expectedSw = new(expectedOutputFilePath), actualSw = new(actualOutputFilePath);
        while (!actualSw.EndOfStream && expectedSw.EndOfStream)
        {
            Assert.Equal(actualSw.ReadLine(), expectedSw.ReadLine());
        }
        
        File.Delete(actualOutputFilePath);
    }
    
    [Theory]
    [InlineData("test_input_13.fa", "test_output_13.tsv")]
    // The line below is commented because the input file is too big to upload to the remote repository.
    // If you want to run this specific test, you can download the input file from
    // https://www.ncbi.nlm.nih.gov/projects/genome/guide/human/index.shtml .
    // Download GRCh38 - RefSeq Transcripts [fasta], unzip the compressed file,
    // copy it to data/input, and everything will be all set.
    // [InlineData("GRCh38_latest_rna.fna", "GRCh38_latest_rna.tsv")]
    public void ComplicatedTest(string input, string output)
    {
        string inputFilePath = Path.Join(Environment.CurrentDirectory, InputDirectory, input);
        string expectedOutputFilePath = Path.Join(Environment.CurrentDirectory, ExpectedOutputDirectory, output);
        string actualOutputFilePath = Path.Join(Environment.CurrentDirectory, ActualOutputDirectory, output);

        var args = new Arguments
        {
            AllowNonAtomic = false,
            Alphabet = new() { 'A', 'C', 'T', 'G' },
            BZipped2Input = false,
            BZipped2Output = false,
            EnumeratedSsrs = new(),
            Threads = 1,
            Exhaustive = false,
            GZippedInput = false,
            GZippedOutput = false,
            InputFileName = inputFilePath,
            MinSequenceLength = 100,
            MaxSequenceLength = 500000000,
            MinNucleotides = 16,
            MaxNucleotides = 10000,
            OutputFileName = actualOutputFilePath,
            MaxTaskQueueSize = 1000,
            MinRepeats = 2,
            MaxRepeats = 10000,
        };

        for (int i = 8; i >= 4; i--)
        {
            args.Periods.Add(i);
        }
        
        SsrFinder finder = new(args);
        finder.Run();

        StreamReader expectedSw = new(expectedOutputFilePath), actualSw = new(actualOutputFilePath);
        while (!actualSw.EndOfStream && expectedSw.EndOfStream)
        {
            Assert.Equal(actualSw.ReadLine(), expectedSw.ReadLine());
        }
        
        File.Delete(actualOutputFilePath);
    }
}