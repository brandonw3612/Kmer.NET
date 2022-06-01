using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kmer.NET.Windows
{
    internal class ProgressController : Interfaces.IProgressController
    {
        private MainWindowViewModel _viewModel;
        private Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;

        public ProgressController(MainWindowViewModel viewModel, Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue)
        {
            _viewModel = viewModel;
            _dispatcherQueue = dispatcherQueue;
        }

        public void SetMaximumTick(int maxTicks)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                _viewModel.ProgressMaximum = maxTicks;
            });
        }

        public void UpdateProgress(int incrementTick)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                _viewModel.ProgressValue += incrementTick;
            });
        }

        public void UpdateProgressWithRatio(double ratio)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                _viewModel.ProgressPercentage = $"{ratio:F2}%";
            });
        }
    }
}
