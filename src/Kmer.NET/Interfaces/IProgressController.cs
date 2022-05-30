namespace Kmer.NET.Interfaces;

public interface IProgressController
{
    void SetMaximumTick(int maxTicks);

    void UpdateProgress(int incrementTick);

    void UpdateProgressWithRatio(double ratio);
}