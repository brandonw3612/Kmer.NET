namespace Kmer.NET;

public class OutputFile
{
    private StreamWriter _sw;
    private Semaphore _lock;

    private void SetUp() => _lock = new(1, 1);

    private void SetUp(string fileName)
    {
        SetUp();
        _sw = new(fileName);
    }

    private void WriteHeader(string header)
    {
        _lock.WaitOne();
        _sw.Write(header);
        _lock.Release();
    }

    private void WriteHeaders(List<string> headers)
    {
        _lock.WaitOne();
        foreach (var header in headers)
        {
            _sw.Write(header);
            if (header.Length > 0 && header[^1] != '\n') _sw.Write('\n');
        }
        _lock.Release();
    }

    public OutputFile() => SetUp();

    public OutputFile(string fileName) => SetUp(fileName);

    public OutputFile(string fileName, string header)
    {
        SetUp(fileName);
        WriteHeader(header);
    }

    public OutputFile(string fileName, List<string> headers)
    {
        SetUp(fileName);
        WriteHeaders(headers);
    }

    ~OutputFile() => _sw.Close();

    public void ChangeFile(string fileName)
    {
        _lock.WaitOne();
        if (_sw is not null)
        {
            _sw.Close();
        }
        _sw = new(fileName);
        _lock.Release();
    }

    public void ChangeFile(string fileName, List<string> headers)
    {
        ChangeFile(fileName);
        WriteHeaders(headers);
    }

    public bool ObtainLock(bool block)
    {
        if (block) return _lock.WaitOne();
        return _lock.WaitOne(0);
    }

    public void ReleaseLock()
    {
        _lock.Release();
    }

    public void Write(object output)
    {
        _sw.Write(output);
    }

    public void FlushToFileClose()
    {
        _sw.Flush();
        _sw.Close();
    }
}