namespace Kmer.NET;

public class Ssr
{
    public string BaseSsr { get; }
    public int Repeats { get; }
    public int Position { get; }

    public int Period => BaseSsr.Length;

    public int Length => BaseSsr.Length * Repeats;

    public int ExclusiveEndPosition => Position + BaseSsr.Length * Repeats;

    public Ssr(string baseSsr, int repeats, int position)
    {
        BaseSsr = baseSsr;
        Repeats = repeats;
        Position = position;
    }

    public void WriteToFile(StreamWriter sw) => sw.Write($"{BaseSsr}\t{Repeats}\t{Position}\n");

    public void WriteToFile(OutputFile outputFile) => outputFile.Write($"{BaseSsr}\t{Repeats}\t{Position}\n");
    
    public override string ToString() => $"SSR: {{base: {BaseSsr}, repeats: {Repeats}, position: {Position}}}";
}