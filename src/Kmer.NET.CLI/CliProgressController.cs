using ShellProgressBar;

namespace Kmer.NET.CLI;

public class CliProgressController: Interfaces.IProgressController
{
    private readonly ProgressBar _progressBar;

    public CliProgressController()
    {
        _progressBar = new(0, "Kmer-SSR Finder", new ProgressBarOptions
        {
            ProgressBarOnBottom = true,
            BackgroundColor = ConsoleColor.Yellow,
            ForegroundColor = ConsoleColor.Green,
            ProgressCharacter = '—'
        });
    }
    
    public void SetMaximumTick(int maxTicks) => _progressBar.MaxTicks = maxTicks;

    public void UpdateProgress(int incrementTick) => _progressBar.Tick(_progressBar.CurrentTick + incrementTick);

    public void UpdateProgressWithRatio(double ratio) => _progressBar.Tick((int)(_progressBar.MaxTicks * ratio));
}