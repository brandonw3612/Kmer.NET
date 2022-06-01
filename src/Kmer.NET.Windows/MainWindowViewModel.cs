using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kmer.NET.Windows
{
    internal class MainWindowViewModel: Microsoft.Toolkit.Mvvm.ComponentModel.ObservableObject
    {
        private string inputFileName;
        public string InputFileName
        {
            get => inputFileName;
            set => SetProperty(ref inputFileName, value);
        }

        private string outputFileName;
        public string OutputFileName
        {
            get => outputFileName;
            set => SetProperty(ref outputFileName, value);
        }

        private bool allowNonAtomic;
        public bool AllowNonAtomic
        {
            get => allowNonAtomic;
            set => SetProperty(ref allowNonAtomic, value);
        }

        private bool exhaustive;
        public bool Exhaustive
        {
            get => exhaustive;
            set => SetProperty(ref exhaustive, value);
        }

        private string alphabet;
        public string Alphabet
        {
            get => alphabet;
            set => SetProperty(ref alphabet, value);
        }

        private string enumeratedSsrs;
        public string EnumeratedSsrs
        {
            get => enumeratedSsrs;
            set => SetProperty(ref enumeratedSsrs, value);
        }

        private string periods;
        public string Periods
        {
            get => periods;
            set => SetProperty(ref periods, value);
        }

        private string minSequenceLength;
        public string MinSequenceLength
        {
            get => minSequenceLength;
            set => SetProperty(ref minSequenceLength, value);
        }

        private string maxSequenceLength;
        public string MaxSequenceLength
        {
            get => maxSequenceLength;
            set => SetProperty(ref maxSequenceLength, value);
        }

        private string minSsrLength;
        public string MinSsrLength
        {
            get => minSsrLength;
            set => SetProperty(ref minSsrLength, value);
        }

        private string maxSsrLength;
        public string MaxSsrLength
        {
            get => maxSsrLength;
            set => SetProperty(ref maxSsrLength, value);
        }

        private string minRepeat;
        public string MinRepeat
        {
            get => minRepeat;
            set => SetProperty(ref minRepeat, value);
        }

        private string maxRepeat;
        public string MaxRepeat
        {
            get => maxRepeat;
            set => SetProperty(ref maxRepeat, value);
        }

        private string maxTaskQueueSize;
        public string MaxTaskQueueSize
        {
            get => maxTaskQueueSize;
            set => SetProperty(ref maxTaskQueueSize, value);
        }

        private string threads;
        public string Threads
        {
            get => threads;
            set => SetProperty(ref threads, value);
        }

        private int progressMaximum;
        public int ProgressMaximum
        {
            get => progressMaximum;
            set
            {
                SetProperty(ref progressMaximum, value);
                double per = (double)progressValue / (double)progressMaximum * 100.0;
                ProgressPercentage = $"{per:F2}%";
            }
        }

        private int progressValue;
        public int ProgressValue
        {
            get => progressValue;
            set
            {
                SetProperty(ref progressValue, value);
                double per = (double)progressValue / (double)progressMaximum * 100.0;
                ProgressPercentage = $"{per:F2}%";
            }
        }

        private string progressPercentage;
        public string ProgressPercentage
        {
            get => progressPercentage;
            set => SetProperty(ref progressPercentage, value);
        }

        private string timeTicked;
        public string TimeTicked
        {
            get => timeTicked;
            set => SetProperty(ref timeTicked, value);
        }

        private string log;
        public string Log
        {
            get => log;
            set => SetProperty(ref log, value);
        }

        public MainWindowViewModel()
        {
            ApplyDefaultOptions();

            InputFileName = string.Empty;
            OutputFileName = string.Empty;

            ProgressMaximum = 1;
            ProgressValue = 0;
        }

        public void ApplyDefaultOptions()
        {
            AllowNonAtomic = false;
            Exhaustive = false;
            Alphabet = "A,C,G,T";
            EnumeratedSsrs = "";
            Periods = "4-8";
            MinSequenceLength = "100";
            MaxSequenceLength = "500000000";
            MinSsrLength = "16";
            MaxSsrLength = "10000";
            MinRepeat = "2";
            MaxRepeat = "10000";
            MaxTaskQueueSize = "1000";
            Threads = "1";
        }

        public Arguments ParseArguments()
        {
            StringBuilder sb = new();

            if (InputFileName == string.Empty) sb.Append("Input file cannot be empty. ");
            if (OutputFileName == string.Empty) sb.Append("Output file cannot be empty. ");

            if (sb.Length > 0) throw new Exception(sb.ToString());

            bool GZippedInput = false, BZipped2Input = false, GZippedOutput = false, BZipped2Output = false;

            // Compressed input file check
            if (InputFileName.Length >= 3 && InputFileName[^3..] == ".gz") GZippedInput = true;
            if (InputFileName.Length >= 4 && InputFileName[^4..] == ".bz2") BZipped2Input = true;

            // Compressed output file check
            if (OutputFileName.Length >= 3 && OutputFileName[^3..] == ".gz") GZippedOutput = true;
            if (OutputFileName.Length >= 4 && OutputFileName[^4..] == ".bz2") BZipped2Output = true;

            int minSeq = int.MinValue, maxSeq = int.MinValue, minSsr = int.MinValue, maxSsr = int.MinValue, minRep = int.MinValue, maxRep = int.MinValue, taskQueue = int.MinValue, threads = int.MinValue;

            try { minSeq = int.Parse(MinSequenceLength); }
            catch { sb.Append("Cannot parse minimum sequence length. "); }
            try { maxSeq = int.Parse(MaxSequenceLength); }
            catch { sb.Append("Cannot parse maximum sequence length. "); }
            try { minSsr = int.Parse(MinSsrLength); }
            catch { sb.Append("Cannot parse minimun SSR length. "); }
            try { maxSsr = int.Parse(MaxSsrLength); }
            catch { sb.Append("Cannot parse maximum SSR length. "); }
            try { minRep = int.Parse(MinRepeat); }
            catch { sb.Append("Cannot parse minimun SSR repeat frequency. "); }
            try { maxRep = int.Parse(MaxRepeat); }
            catch { sb.Append("Cannot parse maximum SSR repeat frequency. "); }
            try { taskQueue = int.Parse(MaxTaskQueueSize); }
            catch { sb.Append("Cannot parse tasks queue max size. "); }
            try { threads = int.Parse(Threads); }
            catch { sb.Append("Cannot parse threads count. "); }

            if (sb.Length > 0) throw new Exception(sb.ToString());

            // Sanity check
            if (minSeq <= 0) sb.Append("Minimum sequence length must be greater than 0. ");
            if (maxSeq <= 0) sb.Append("Maximum sequence length must be greater than 0. ");
            if (minSsr <= 0) sb.Append("Minimum SSR length must be greater than 0. ");
            if (maxSsr <= 0) sb.Append("Maximum SSR length must be greater than 0. ");
            if (minRep <= 0) sb.Append("Minimum SSR repeat frequency must be greater than 0. ");
            if (maxRep <= 0) sb.Append("Maximum SSR repeat frequency must be greater than 0. ");
            if (taskQueue <= 0) sb.Append("Tasks queue max size must be greater than 0. ");

            if (sb.Length > 0) throw new Exception(sb.ToString());

            if (minSeq > 0 && maxSeq > 0 && minSeq > maxSeq) sb.Append("Minimum sequence length cannot be greater than maximum sequence length. ");
            if (minSsr > 0 && maxSsr > 0 && minSsr > maxSsr) sb.Append("Minimum SSR length cannot be greater than maximum SSR length. ");
            if (minRep > 0 && maxRep > 0 && minRep > maxRep) sb.Append("Minimum SSR repeat frequency cannot be greater than SSR repeat frequency. ");

            if (sb.Length > 0) throw new Exception(sb.ToString());

            Arguments args = new()
            {
                Alphabet = Alphabet.Where(char.IsLetter).ToHashSet(),
                AllowNonAtomic = AllowNonAtomic,
                BZipped2Input = BZipped2Input,
                BZipped2Output = BZipped2Output,
                Exhaustive = Exhaustive,
                GZippedInput = GZippedInput,
                GZippedOutput = GZippedOutput,
                InputFileName = InputFileName,
                MinSequenceLength = minSeq,
                MaxSequenceLength = maxSeq,
                MinNucleotides = minSsr,
                MaxNucleotides = maxSsr,
                OutputFileName = OutputFileName,
                MaxTaskQueueSize = taskQueue,
                MinRepeats = minRep,
                MaxRepeats = maxRep,
                EnumeratedSsrs = EnumeratedSsrs.ToUpper().Split(',').Where(s => s.Length > 0).ToHashSet(),
                Threads = threads
            };

            bool periodsError = false;
            foreach (var segment in Periods.ToUpper().Split(','))
            {
                if (segment.Length <= 0) continue;
                var rangeSplit = segment.Split('-');
                switch (rangeSplit.Length)
                {
                    case 2:
                        {
                            for (int i = int.Parse(rangeSplit[0]); i <= int.Parse(rangeSplit[1]); i++)
                            {
                                args.Periods.Add(i);
                            }
                            break;
                        }
                    case 1:
                        args.Periods.Add(int.Parse(rangeSplit[0]));
                        break;
                    default:
                        periodsError = true;
                        break;
                }
            }
            if (periodsError) sb.Append("Periods format is invalid (Example: [1-3,5,7,10-12]). ");

            if (sb.Length > 0) throw new Exception(sb.ToString());

            return args;
        }
    }
}
