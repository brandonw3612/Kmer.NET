namespace Kmer.NET;

public class TaskQueue
{
    private Queue<Task> _tasks;
    private Semaphore _lock;
    private Semaphore _unfinishedBusiness;
    private Semaphore _spaceAvailable;

    public int Size => _tasks.Count;
    public bool IsEmpty => _tasks.Count == 0;
    
    public TaskQueue() => SetUp(1000);

    public TaskQueue(int maxCapacity) => SetUp(maxCapacity);

    ~TaskQueue() => _tasks.Clear();

    private void SetUp(int maxCapacity)
    {
        _tasks = new();
        _lock = new(1, 1);
        _unfinishedBusiness = new(0, 1);
        _spaceAvailable = new(1, maxCapacity);
    }

    public void Add(Task task)
    {
        _spaceAvailable.WaitOne();
        _lock.WaitOne();
        _tasks.Enqueue(task);
        _lock.Release();
        _unfinishedBusiness.Release();
    }

    public Task Get()
    {
        _unfinishedBusiness.WaitOne();
        _lock.WaitOne();
        Task front = _tasks.Dequeue();
        _lock.Release();
        _spaceAvailable.Release();
        return front;
    }

    public void Clear()
    {
        _lock.WaitOne();
        while (_tasks.Count > 0)
        {
            _tasks.Dequeue();
            _unfinishedBusiness.WaitOne();
            _spaceAvailable.Release();
        }
        _lock.Release();
    }
}