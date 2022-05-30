namespace Kmer.NET;

public class Task
{
    public string Identifier { get; }
    public string Sequence { get; }
    public int GlobalPosition { get; }
    public int GlobalSequenceLength { get; }

    public int Size => Sequence.Length;

    private string ShortenedId => Identifier.Length > 20 ? Identifier[..20] : Identifier;

    private string BothEndsOfSequence() =>
        Sequence.Length > 20 ? $"{Sequence[..20]}...{Sequence[^20..]}" : Sequence;

    public Task() => GlobalPosition = GlobalSequenceLength = 0;

    public Task(string identifier, string sequence, int globalPosition, int globalSequenceLength)
    {
        Identifier = identifier;
        Sequence = sequence;
        GlobalPosition = globalPosition;
        GlobalSequenceLength = globalSequenceLength;
    }

    public bool Equals(Task otherTask)
    {
        return Identifier == otherTask.Identifier && Sequence == otherTask.Sequence &&
               GlobalPosition == otherTask.GlobalPosition && GlobalSequenceLength == otherTask.GlobalSequenceLength;
    }

    public override string ToString() => System.Text.Json.JsonSerializer.Serialize(this);
}