namespace Kmer.NET;

public class AtomicityChecker
{
    private Dictionary<int, HashSet<int>> _factors;
    private Semaphore _semaphore;

    private void SetUp()
    {
        _semaphore = new(1, 1);
        _factors = new();
    }

    private void InitializeFactorsWithPreDefinedFactors(Dictionary<int, HashSet<int>> factors)
    {
        foreach (var key in factors.Keys)
        {
            _factors[key] = factors[key];
        }
    }

    private void SafeInsert(int key, HashSet<int> factors)
    {
        _semaphore.WaitOne();
        if (!_factors.ContainsKey(key))
        {
            _factors[key] = factors;
        }
        else if (!_factors[key].SequenceEqual(factors))
        {
            factors.Clear();
        }
        _semaphore.Release();
    }

    private HashSet<int> CalculateFactors(int key)
    {
        if (_factors.ContainsKey(key))
        {
            return _factors[key];
        }

        if (key == 1)
        {
            SafeInsert(1, new() {1});
            return _factors[key];
        }

        HashSet<int> keyFactors = new() {1, key};
        for (int i = 2; i < key / 2; i++)
        {
            if (key % i == 0)
            {
                keyFactors.Add(i);
                if (!_factors.ContainsKey(i))
                {
                    SafeInsert(i, CalculateFactors(i));
                }
            }
        }
        
        SafeInsert(key, keyFactors);
        return _factors[key];
    }

    private bool CheckAtomicityAtFactor(int factor, string baseSsr, int period)
    {
        int count = 1;

        for (int i = factor; i < period; i += factor)
        {
            if (baseSsr[(i - factor)..i] != baseSsr[i..(i + factor)])
            {
                break;
            }
            count++;
        }

        return count != (period / factor);
    }

    public AtomicityChecker()
    {
        SetUp();
        Dictionary<int, HashSet<int>> defaultFactors = new()
        {
            {1, new() {1}},
            {2, new() {1, 2}},
            {3, new() {1, 3}},
            {4, new() {1, 2, 4}},
            {5, new() {1, 5}},
            {6, new() {1, 2, 3, 6}},
            {7, new() {1, 7}},
            {8, new() {1, 2, 4, 8}},
            {9, new() {1, 3, 9}},
            {10, new() {1, 2, 5, 10}}
        };
        InitializeFactorsWithPreDefinedFactors(defaultFactors);
    }

    public AtomicityChecker(Dictionary<int, HashSet<int>> factors)
    {
        SetUp();
        InitializeFactorsWithPreDefinedFactors(factors);
    }

    ~AtomicityChecker()
    {
        foreach (var key in _factors.Keys)
        {
            _factors[key].Clear();
        }
        _factors.Clear();
    }

    public bool IsAtomic(string baseSsr)
    {
        int period = baseSsr.Length;

        HashSet<int> periodFactors = CalculateFactors(period);
        foreach (var factor in periodFactors)
        {
            if ((factor != period) && !CheckAtomicityAtFactor(factor, baseSsr, period))
            {
                return false;
            }
        }
        return true;
    }

    public override string ToString() => System.Text.Json.JsonSerializer.Serialize(_factors);
}