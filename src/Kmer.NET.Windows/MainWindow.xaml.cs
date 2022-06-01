using Microsoft.UI.Xaml;
using System;
using Windows.UI;
using Kmer.NET.Windows.InteropHelpers;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT;
using WinRT.Interop;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using static Kmer.NET.Windows.InteropHelpers.FilePickerDialogHelpers;

namespace Kmer.NET.Windows
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly MainWindowViewModel viewModel;

        private WindowsSystemDispatcherQueueHelper m_wsdqHelper; // See separate sample below for implementation
        private Microsoft.UI.Composition.SystemBackdrops.MicaController m_micaController;
        private Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration m_configurationSource;

        private int time = 0;
        private System.Timers.Timer _ticker = new(1000)
        {
            AutoReset = true
        };

        public MainWindow()
        {
            this.InitializeComponent();

            viewModel = new();

            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var m_appWindow = AppWindow.GetFromWindowId(wndId);
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                m_appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            }

            m_appWindow.TitleBar.BackgroundColor = Colors.Transparent;
            m_appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            m_appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            TrySetMicaBackdrop();

            _ticker.Elapsed += _ticker_Elapsed;
        }

        private void _ticker_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            time++;
            DispatcherQueue.TryEnqueue(() =>
            {
                viewModel.TimeTicked = $"{time / 60}:{time % 60:D2}";
            });
        }

        bool TrySetMicaBackdrop()
        {
            if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

                // Hooking up the policy object
                m_configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();

                m_micaController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();

                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                m_micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
                return true; // succeeded
            }

            return false; // Mica is not supported on this system
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
            // use this closed window.
            if (m_micaController != null)
            {
                m_micaController.Dispose();
                m_micaController = null;
            }
            this.Activated -= Window_Activated;
            m_configurationSource = null;
        }

        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (m_configurationSource != null)
            {
                SetConfigurationSourceTheme();
            }
        }

        private void SetConfigurationSourceTheme()
        {
            switch (((FrameworkElement)this.Content).ActualTheme)
            {
                case ElementTheme.Dark: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark; break;
                case ElementTheme.Light: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light; break;
                case ElementTheme.Default: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default; break;
            }
        }

        private async void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag.ToString() == "Input")
            {
                FileOpenPicker picker = new();

                IInitializeWithWindow initializeWithWindowWrapper = picker.As<IInitializeWithWindow>();
                IntPtr hwnd = GetActiveWindow();
                initializeWithWindowWrapper.Initialize(hwnd);

                var file = await picker.PickSingleFileAsync();
                viewModel.InputFileName = file.Path;
            }
            else
            {
                FileSavePicker picker = new();

                IInitializeWithWindow initializeWithWindowWrapper = picker.As<IInitializeWithWindow>();
                IntPtr hwnd = GetActiveWindow();
                initializeWithWindowWrapper.Initialize(hwnd);

                var file = await picker.PickSaveFileAsync();
                viewModel.InputFileName = file.Path;
            }
        }

        private void RestoreDefaultOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ApplyDefaultOptions();
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(RootPage, "DoubleColumn", true);
            InfoBar.IsOpen = false;
            ((Button)sender).IsEnabled = false;

            try
            {
                var args = viewModel.ParseArguments();
                viewModel.ProgressValue = 0;
                SsrFinder finder = new(args, new ProgressController(viewModel, DispatcherQueue));
                time = 0;
                _ticker.Start();
                await System.Threading.Tasks.Task.Run(finder.Run);
                _ticker.Stop();
                ((Button)sender).IsEnabled = true;
            }
            catch (Exception ex)
            {
                InfoBar.Message = ex.Message;
                InfoBar.IsOpen = true;
            }
        }
    }
}
