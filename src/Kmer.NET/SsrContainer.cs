namespace Kmer.NET;

public class SsrContainer
{
    private readonly Dictionary<string, List<Ssr>> _ssrs;

    public int Size => _ssrs.Count;
    public bool IsEmpty => _ssrs.Count == 0;

    public SsrContainer()
    {
        _ssrs = new();
    }

    ~SsrContainer() => Clear();

    private void Clear()
    {
        foreach (var key in _ssrs.Keys)
        {
            _ssrs[key].Clear();
        }
        _ssrs.Clear();
    }
    
    public void Add(string identifier, Ssr ssr)
    {
        if (_ssrs.ContainsKey(identifier)) _ssrs[identifier].Add(ssr);
        else _ssrs[identifier] = new() {ssr};
    }

    public void Add(string identifier, IEnumerable<Ssr> ssrs)
    {
        if (_ssrs.ContainsKey(identifier)) _ssrs[identifier].AddRange(ssrs);
        else _ssrs[identifier] = ssrs.ToList();
    }

    public void WriteToFile(StreamWriter sw)
    {
        foreach (var key in _ssrs.Keys)
        {
            foreach (var ssr in _ssrs[key])
            {
                sw.Write($"{key}\t");
                ssr.WriteToFile(sw);
            }
        }
    }
    
    public void WriteToFile(OutputFile outputFile, bool block, bool clear)
    {
        if (!outputFile.ObtainLock(block)) return;
        foreach (var key in _ssrs.Keys)
        {
            foreach (var ssr in _ssrs[key])
            {
                outputFile.Write($"{key}\t");
                ssr.WriteToFile(outputFile);
            }
        }
        outputFile.ReleaseLock();
        if (clear) Clear();
    }
    
    
    
    public override string ToString() => System.Text.Json.JsonSerializer.Serialize(_ssrs);
}