using Kmer.NET.Interfaces;
using System.Text;

namespace Kmer.NET;

public class SsrFinder
{
    private readonly Arguments _args;
    private List<Thread> _threads;
    private readonly OutputFile _outputFile;
    private readonly AtomicityChecker _atomicityChecker;
    private readonly TaskQueue _tasks;
    private readonly IProgressController _progressController;

    private void CreateThreads()
    {
        _threads = new();
        for (int i = 1; i < _args.Threads; i++)
        {
            Thread thread = new(Consume);
            _threads.Add(thread);
            thread.Start(new ConsumerArguments()
            {
                Tasks = _tasks,
                ProgressController = _progressController,
                OutputFile = _outputFile,
                Arguments = _args,
                AtomicityChecker = _atomicityChecker
            });
        }
    }

    private void JoinAndForgetAllThreads()
    {
        foreach (var t in _threads)
        {
            t.Join();
        }

        _threads.Clear();
    }

    private void SplitStringOnIgnoredCharacters(ref List<int> starts, ref List<int> sizes, string sequence,
        ref int actuallyIgnoredCharacters)
    {
        for (var i = 0; i < sequence.Length; i++)
        {
            if (!_args.Alphabet.Contains(sequence[i]))
            {
                sizes.Add(i - starts[^1]);
                actuallyIgnoredCharacters++;

                if (sizes[^1] == 0)
                {
                    starts.RemoveAt(starts.Count - 1);
                    sizes.RemoveAt(sizes.Count - 1);
                }

                starts.Add(i + 1);
            }
        }

        sizes.Add(sequence.Length - starts[^1]);

        if (sizes[^1] == 0)
        {
            starts.RemoveAt(starts.Count - 1);
            sizes.RemoveAt(sizes.Count - 1);
        }

        if (starts.Count != sizes.Count)
        {
            throw new Exception("Starts and sizes are not the same size.");
        }
    }

    private void ProcessInput()
    {
        StreamReader inputFile = new(_args.InputFileName);

        int initSize = _args.InputFileName == "/dev/stdin" ? 0 : CalculateDataSizeFromFasta(_args.InputFileName);

        _progressController.SetMaximumTick(initSize);

        string header = string.Empty;
        StringBuilder sequenceBuilder = new();
        while (inputFile.ReadLine() is string line)
        {
            if (line.Length > 0)
            {
                if (line[0] != '>')
                {
                    _progressController.UpdateProgress(1);
                    sequenceBuilder.Append(line.ToUpper());
                }
                else
                {
                    ProcessSequence(ref header, sequenceBuilder.ToString());

                    header = line[1..];
                    sequenceBuilder = new();

                    _progressController.UpdateProgress(header.Length + 2);
                }
            }
            else
            {
                _progressController.UpdateProgress(1);
            }
        }

        ProcessSequence(ref header, sequenceBuilder.ToString());

        inputFile.Close();

        switch (_args.Threads)
        {
            case 1: break;
            default:
                for (int i = 1; i < _args.Threads; i++)
                {
                    _tasks.Add(null);
                }
                break;
        }

        if (_tasks.Size > _args.Threads * 3)
        {
            SsrContainer ssrs = new();

            while (_tasks.Size > _args.Threads * 3)
            {
                var task = _tasks.Get();

                ssrs.Add(task.Identifier, FindSsrs(task, _args, _atomicityChecker));
                ssrs.WriteToFile(_outputFile, true, true);
                
                _progressController.UpdateProgress(task.Size);
            }

            if (!ssrs.IsEmpty)
            {
                ssrs.WriteToFile(_outputFile, true, false);
                
            }
        }
    }

    private void ProcessSequence(ref string header, string sequence)
    {
        if (sequence.Length == 0 || sequence.Length < _args.MinSequenceLength ||
            sequence.Length > _args.MaxSequenceLength)
        {
            _progressController.UpdateProgress(sequence.Length);
            return;
        }

        List<int> starts = new() { 0 }, sizes = new();
        int ignoredCharactersCount = 0;

        SplitStringOnIgnoredCharacters(ref starts, ref sizes, sequence, ref ignoredCharactersCount);

        _progressController.UpdateProgress(ignoredCharactersCount);

        switch (_args.Threads)
        {
            case 1:
                {
                    SsrContainer ssrs = new();

                    for (int i = 0; i < starts.Count; i++)
                    {
                        var t = new Task(header, sequence[starts[i]..(starts[i] + sizes[i])], starts[i], sequence.Length);

                        ssrs.Add(t.Identifier, FindSsrs(t, _args, _atomicityChecker));
                        ssrs.WriteToFile(_outputFile, true, true);
                        
                        _progressController.UpdateProgress(t.Size);
                    }
                    break;
                }
            default:
                {
                    for (int i = 0; i < starts.Count; i++)
                    {
                        _tasks.Add(new(header, sequence[starts[i]..(starts[i] + sizes[i])], starts[i], sequence.Length));
                    }
                    break;
                }
        }
    }

    public SsrFinder(Arguments args, IProgressController progressController = null)
    {
        _args = args;
        _progressController = progressController;
        _outputFile = new(args.OutputFileName);
        _outputFile.Write("#Sequence Name\tSSR\tRepeats\tPosition\n");
        _atomicityChecker = new();
        _tasks = new(args.MaxTaskQueueSize);
    }

    public void Run()
    {
        CreateThreads();

        ProcessInput();

        JoinAndForgetAllThreads();

        _outputFile.FlushToFileClose();
    }

    #region Static methods

    private static int CalculateDataSizeFromFasta(string fileName)
    {
        StreamReader sr = new(fileName);
        int length = sr.ReadToEnd().Length;
        sr.Close();
        return length;
    }

    private void Consume(object consumer)
    {
        if (consumer is not ConsumerArguments consumerArgs) return;
        var tasks = consumerArgs.Tasks;
        var progressController = consumerArgs.ProgressController;
        var outputFile = consumerArgs.OutputFile;
        var atomicityChecker = consumerArgs.AtomicityChecker;
        var args = consumerArgs.Arguments;

        SsrContainer ssrs = new();

        while (true)
        {
            var task = tasks.Get();
            if (task is null) break;

            ssrs.Add(task.Identifier, FindSsrs(task, args, atomicityChecker));
            ssrs.WriteToFile(outputFile, false, true);
            
            progressController.UpdateProgress(task.Size);
        }

        if (!ssrs.IsEmpty)
        {
            ssrs.WriteToFile(outputFile, true, false);
            
        }
    }

    private static Ssr SeekSinglePeriodSizeSsrAtIndex(string sequence, int index, int globalPosition, int period)
    {
        string baseStr = sequence[index..(Math.Min(index + period, sequence.Length))];
        string next = baseStr;
        int repeats = 0;
        int pos = index;
        while (baseStr == next && pos < sequence.Length - period + 1)
        {
            repeats++;
            pos += period;
            next = sequence[pos..(Math.Min(pos + period, sequence.Length))];
        }
        return new Ssr(baseStr, repeats, index + globalPosition);
    }

    private static bool IsGoodSsr(Ssr ssr, int globalPosition, List<bool> filter, Arguments args,
        AtomicityChecker atomicityChecker)
    {
        if (args.EnumeratedSsrs.Count > 0 && args.EnumeratedSsrs.Contains(ssr.BaseSsr)) return false;
        if (ssr.Repeats < 2) return false;
        if (!args.AllowNonAtomic && !atomicityChecker.IsAtomic(ssr.BaseSsr)) return false;

        for (int i = ssr.Position - globalPosition; i < ssr.ExclusiveEndPosition - globalPosition; i++)
        {
            if (!filter[i]) return true;
        }

        return false;
    }

    private static bool KeepSsr(Ssr ssr, Arguments args)
    {
        if (ssr.Repeats < args.MinRepeats || ssr.Repeats > args.MaxRepeats) return false;
        if (ssr.Length < args.MinNucleotides || ssr.Length > args.MaxNucleotides) return false;
        return true;
    }

    private static List<Ssr> FindSsrsExhaustively(Task task, SortedSet<int> periods)
    {
        List<Ssr> ssrs = new();

        string seq = task.Sequence;

        foreach (var period in periods)
        {
            for (int index = 0; index < task.Size; index++)
            {
                ssrs.Add(SeekSinglePeriodSizeSsrAtIndex(seq, index, task.GlobalPosition, period));
            }
        }

        return ssrs;
    }

    private static List<Ssr> FindSsrsNormally(Task task, Arguments args, AtomicityChecker atomicityChecker)
    {
        List<bool> filter = new bool[task.Size].ToList();
        List<Ssr> ssrs = new();

        string seq = task.Sequence;
        SortedSet<int> periods = args.Periods;

        foreach (var period in periods)
        {
            if (period * 2 <= seq.Length)
            {
                for (int index = 0; index < task.Size; index++)
                {
                    var ssr = SeekSinglePeriodSizeSsrAtIndex(seq, index, task.GlobalPosition, period);

                    if (IsGoodSsr(ssr, task.GlobalPosition, filter, args, atomicityChecker))
                    {
                        index += ssr.Length - period;

                        if (KeepSsr(ssr, args))
                        {
                            ssrs.Add(ssr);
                            for (int j = ssr.Position - task.GlobalPosition;
                                 j < ssr.ExclusiveEndPosition - task.GlobalPosition;
                                 j++)
                            {
                                filter[j] = true;
                            }
                        }
                    }
                }
            }
        }

        filter.Clear();
        return ssrs;
    }

    private static List<Ssr> FindSsrs(Task task, Arguments args, AtomicityChecker atomicityChecker)
    {
        if (args.Exhaustive) return FindSsrsExhaustively(task, args.Periods);
        return FindSsrsNormally(task, args, atomicityChecker);
    }

    #endregion

    private class ConsumerArguments
    {
        public TaskQueue Tasks { get; init; }
        public IProgressController ProgressController { get; init; }
        public AtomicityChecker AtomicityChecker { get; init; }
        public Arguments Arguments { get; init; }
        public OutputFile OutputFile { get; init; }
    }
}