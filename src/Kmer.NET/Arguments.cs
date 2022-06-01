namespace Kmer.NET;

public class Arguments
{
    public HashSet<char> Alphabet { get; set; }
    public bool AllowNonAtomic { get; set; }
    public bool BZipped2Input { get; set; }
    public bool BZipped2Output { get; set; }
    public bool Exhaustive { get; set; }
    public bool GZippedInput { get; set; }
    public bool GZippedOutput { get; set; }
    public string InputFileName { get; set; }
    public int MinSequenceLength { get; set; }
    public int MaxSequenceLength { get; set; }
    public int MinNucleotides { get; set; }
    public int MaxNucleotides { get; set; }
    public string OutputFileName { get; set; }
    public SortedSet<int> Periods { get; }
    public int MaxTaskQueueSize { get; set; }
    public int MinRepeats { get; set; }
    public int MaxRepeats { get; set; }
    public HashSet<string> EnumeratedSsrs { get; set; }
    public int Threads { get; set; }
    
    public Arguments()
    {
        Periods = new SortedSet<int>(new ReverseOrderComparer());
    }

    ~Arguments()
    {
        Alphabet.Clear();
        Periods.Clear();
        EnumeratedSsrs.Clear();
    }

    private class ReverseOrderComparer : IComparer<int>
    {
        public int Compare(int x, int y) => y.CompareTo(x);
    }
}