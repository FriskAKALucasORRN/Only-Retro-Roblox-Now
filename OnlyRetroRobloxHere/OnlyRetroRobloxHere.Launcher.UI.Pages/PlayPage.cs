using OnlyRetroRobloxHere.Common;
using OnlyRetroRobloxHere.Launcher.Comparer;
using OnlyRetroRobloxHere.Launcher.Enums;
using OnlyRetroRobloxHere.Launcher.Extensions;
using OnlyRetroRobloxHere.Launcher.UI.Dialogs;
using OnlyRetroRobloxHere.Launcher.UI.ViewModels.Pages;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace OnlyRetroRobloxHere.Launcher.UI.Pages;

public partial class PlayPage : BasePage, IComponentConnector
{
    private object _showcaseLock = new object();

    private CancellationTokenSource? _showcaseCancellationTokenSource;

    private static readonly string[] _trollClients = new string[21]
    {
        "2014E", "2014M", "2014L", "2015E", "2015M", "2015L", "2016E", "2016M", "2016L", "2017E",
        "2017M", "2017L", "2018E", "2018M", "2018L", "2019E", "2019M", "2019L", "2020E", "2020M",
        "2020L"
    };

    private PlayViewModel _viewModel;

    private bool _initialised;

    private ClientNameComparer _clientNameComparer;

    private Random _random;

    private void RefreshShowcase()
    {
        lock (_showcaseLock)
        {
            if (_showcaseCancellationTokenSource != null)
            {
                _showcaseCancellationTokenSource.Cancel();
            }
        }
    }

    private async void StartShowcase()
    {
        while (true)
        {
            lock (_showcaseLock)
            {
                _showcaseCancellationTokenSource = new CancellationTokenSource();
            }
            string path = Path.Combine(PathHelper.Clients, _viewModel.SelectedClient ?? "", "images");
            string[] array = (Directory.Exists(path) ? Directory.GetFiles(path) : Array.Empty<string>());
            if (!array.Any())
            {
                _viewModel.ShowcaseImageBottomOpacity = 0.0;
                _viewModel.ShowcaseImageBottomSource = new Uri("pack://application:,,,/OnlyRetroRobloxHere;component/Resources/Blank.png");
                _viewModel.ShowcaseImageTopOpacity = 0.0;
                _viewModel.ShowcaseImageTopSource = new Uri("pack://application:,,,/OnlyRetroRobloxHere;component/Resources/Blank.png");
                _showcaseCancellationTokenSource.Token.WaitHandle.WaitOne();
            }
            else if (array.Length == 1)
            {
                _viewModel.ShowcaseImageBottomOpacity = 1.0;
                _viewModel.ShowcaseImageBottomSource = new Uri(array[0]);
                _viewModel.ShowcaseImageTopOpacity = 0.0;
                _viewModel.ShowcaseImageTopSource = new Uri("pack://application:,,,/OnlyRetroRobloxHere;component/Resources/Blank.png");
                _showcaseCancellationTokenSource.Token.WaitHandle.WaitOne();
            }
            else
            {
                _viewModel.ShowcaseImageBottomOpacity = 1.0;
                _viewModel.ShowcaseImageTopOpacity = 1.0;
                try
                {
                    await ShowcaseGoThroughImages(array, _showcaseCancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                }
            }
        }
    }

    private async Task ShowcaseGoThroughImages(string[] images, CancellationToken token)
    {
        bool top = false;
        bool flag = true;
        while (!token.IsCancellationRequested)
        {
            foreach (string uriString in images)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                Uri uri = new Uri(uriString);
                if (top)
                {
                    _viewModel.ShowcaseImageTopSource = uri;
                    if (!flag)
                    {
                        _viewModel.ShowcaseImageTopOpacity = 0.0;
                        for (double i2 = 0.0; i2 < 1.0; i2 += 0.009999999776482582)
                        {
                            _viewModel.ShowcaseImageTopOpacity = EaseOutCubic(i2);
                            await Task.Delay(1, token);
                        }
                    }
                    _viewModel.ShowcaseImageTopOpacity = 1.0;
                }
                else if (!top)
                {
                    _viewModel.ShowcaseImageBottomSource = uri;
                    if (!flag)
                    {
                        _viewModel.ShowcaseImageTopOpacity = 1.0;
                        for (double i2 = 1.0; i2 > 0.0; i2 -= 0.009999999776482582)
                        {
                            _viewModel.ShowcaseImageTopOpacity = EaseOutCubic(i2);
                            await Task.Delay(1, token);
                        }
                    }
                    _viewModel.ShowcaseImageTopOpacity = 0.0;
                }
                await Task.Delay(5000, token);
                top = !top;
                flag = false;
            }
        }
    }

    private static double EaseOutCubic(double x)
    {
        return 1.0 - Math.Pow(1.0 - x, 3.0);
    }

    public PlayPage()
    {
        InitializeComponent();
        _clientNameComparer = new ClientNameComparer();
        _random = new Random();
        _viewModel = SharedViewModels.Play;
        base.DataContext = _viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Settings.Default.PreSerialization += OnSettingsPreSerialization;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!(e.PropertyName == "SelectedClient"))
        {
            return;
        }
        RefreshShowcase();
        if (!string.IsNullOrEmpty(_viewModel.SelectedClient))
        {
            if (!_trollClients.Contains<string>(_viewModel.SelectedClient))
            {
                ClientConfigManager.Select(_viewModel.SelectedClient);
                _viewModel.ClientName = ClientConfigManager.SelectedClientConfig?.Name ?? "ur client broke";
                _viewModel.ClientDescription = ClientConfigManager.SelectedClientConfig?.Description ?? "ur client broke";
            }
            else
            {
                _viewModel.ClientName = _viewModel.SelectedClient;
                _viewModel.ClientDescription = "Play our experimental version of the " + _viewModel.SelectedClient + " client.";
            }
        }
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        if (!_initialised)
        {
            LoadClientList();
            Task.Run((Action)StartShowcase);
            _initialised = true;
        }
    }

    private void LoadClientList()
    {
        string text = _viewModel.SelectedClient ?? Settings.Default.Launch.SelectedClient;
        _viewModel.Clients.Clear();
        string[] directories = Directory.GetDirectories(PathHelper.Clients);
        for (int i = 0; i < directories.Length; i++)
        {
            string fileName = Path.GetFileName(directories[i]);
            if (!Müllschutz.IsWhitelisted(fileName))
            {
                Logger.Instance.Info("Müllschutz Detected " + fileName + " (Titel)");
            }
            else if (File.Exists(ClientConfigManager.GetClientConfigPath(fileName)))
            {
                _viewModel.Clients.BinaryAdd(fileName, _clientNameComparer);
            }
        }
        if (DateEvents.AprilFools)
        {
            directories = _trollClients;
            foreach (string item in directories)
            {
                _viewModel.Clients.BinaryAdd(item, _clientNameComparer);
            }
        }
        Clients.NotifyChange();
        if (!_viewModel.Clients.Any())
        {
            Utils.ShowMessageBox("Could not find any clients.", MessageBoxButton.OK, MessageBoxImage.Hand);
            Environment.Exit(1);
            return;
        }
        ClientConfigManager.ClearCache();
        if (text == null || !_viewModel.Clients.Contains(text))
        {
            _viewModel.SelectedClient = "2010M";
        }
        else
        {
            _viewModel.SelectedClient = text;
        }
    }

    private void SelectRandomClient()
    {
        _viewModel.SelectedClient = _viewModel.Clients[_random.Next(_viewModel.Clients.Count)];
    }

    private void OnClientReloadClicked(object sender, RoutedEventArgs e)
    {
        LoadClientList();
    }

    private void OnClientRandomClicked(object sender, RoutedEventArgs e)
    {
        SelectRandomClient();
    }

    private void OnPlayButtonClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel.AllowClientLaunch())
        {
            RobloxLauncher.Launch(LaunchType.Play);
        }
    }

    private void OnPlayAuthServerButtonClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel.AllowClientLaunch())
        {
            new JoinAuthServerWindow().ShowDialog();
        }
    }

    private void OnStudioButtonClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel.AllowClientLaunch())
        {
            RobloxLauncher.Launch(LaunchType.Studio);
        }
    }

    private void OnSettingsPreSerialization(Settings settings)
    {
        settings.Launch.SelectedClient = _viewModel.SelectedClient;
    }

}
