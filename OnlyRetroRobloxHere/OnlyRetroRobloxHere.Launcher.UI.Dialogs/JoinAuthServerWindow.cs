using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using OnlyRetroRobloxHere.Common;
using OnlyRetroRobloxHere.Launcher.Enums;
using OnlyRetroRobloxHere.Launcher.UI.Helpers;
using OnlyRetroRobloxHere.Launcher.UI.ViewModels.Dialogs;

namespace OnlyRetroRobloxHere.Launcher.UI.Dialogs;

public partial class JoinAuthServerWindow : Window, IComponentConnector
{
	private const string UnsupportedApiError = "This server requires a newer version of Only Retro Roblox Now or You Need Regular ORRH";

	private JoinAuthServerViewModel _viewModel;

	private CancellationTokenSource _cancellationTokenSource;

	private CancellationToken _cancellationToken;

	private string _url;

	private bool _initialised;

	public JoinAuthServerWindow()
	{
		_cancellationTokenSource = new CancellationTokenSource();
		_cancellationToken = _cancellationTokenSource.Token;
		_url = $"http://{SharedViewModels.Play.SelectedIp}:{SharedViewModels.Play.SelectedPort}";
		InitializeComponent();
		_viewModel = new JoinAuthServerViewModel();
		base.DataContext = _viewModel;
		ImmersiveDarkMode.ApplyWindow(this);
	}

	private void LaunchAndClose()
	{
		_cancellationTokenSource.Cancel();
		_viewModel.Status = "Launching";
		RobloxLauncher.Launch(LaunchType.Play);
		Close();
	}

	private async Task<bool> RequestAuthenticationStatus(CancellationToken token)
	{
		HttpResponseMessage obj = await Http.Client.GetAsync(_url + "/v1/is-auth", token);
		if (obj.StatusCode == HttpStatusCode.NotImplemented)
		{
			throw new DeprecatedApiException();
		}
		return bool.Parse(await obj.Content.ReadAsStringAsync(token));
	}

	private async Task<bool> GetAuthenticationStatus(CancellationToken token)
	{
		Logger.Instance.Info("Fetching authentication status");
		int i = 1;
		while (!token.IsCancellationRequested)
		{
			_viewModel.Status = "Requesting authentication status";
			if (i != 1)
			{
				_viewModel.Status += $" (attempt {i})";
			}
			try
			{
				return await RequestAuthenticationStatus(token);
			}
			catch (DeprecatedApiException)
			{
				throw;
			}
			catch (TaskCanceledException)
			{
			}
			catch (Exception value)
			{
				Logger.Instance.Error($"Failed to get auth status ({i}): {value}");
			}
			i++;
		}
		return false;
	}

	private async Task<bool> RequestCheckKey(string key, CancellationToken token)
	{
		HttpResponseMessage obj = await Http.Client.GetAsync(_url + "/v1/auth/" + key, token);
		if (obj.StatusCode == HttpStatusCode.NotImplemented)
		{
			throw new DeprecatedApiException();
		}
		return bool.Parse(await obj.Content.ReadAsStringAsync(token));
	}

	private async Task<bool> CheckKey(string key, CancellationToken token)
	{
		Logger.Instance.Info("Checking key " + key);
		int i = 1;
		while (!token.IsCancellationRequested)
		{
			_viewModel.Status = "Checking key";
			if (i != 1)
			{
				_viewModel.Status += $" (attempt {i})";
			}
			try
			{
				return await RequestCheckKey(key, token);
			}
			catch (DeprecatedApiException)
			{
				throw;
			}
			catch (TaskCanceledException)
			{
			}
			catch (Exception value)
			{
				Logger.Instance.Error($"Failed to check key ({i}): {value}");
			}
			i++;
		}
		return false;
	}

	private async Task PerformCheckKey(string key)
	{
		_viewModel.KeyTextBoxEnabled = false;
		_viewModel.SubmitButtonEnabled = false;
		bool flag;
		try
		{
			flag = await CheckKey(key, _cancellationToken);
		}
		catch (DeprecatedApiException)
		{
			Logger.Instance.Error("Requested check key API is deprecated!");
			_viewModel.Status = "This server requires a newer version of Only Retro Roblox Now or you need ORRH";
			return;
		}
		if (flag)
		{
			LaunchAndClose();
			return;
		}
		_viewModel.Status = "Invalid key";
		_viewModel.KeyTextBoxEnabled = true;
		_viewModel.SubmitButtonEnabled = true;
	}

	private async Task Start()
	{
		Logger.Instance.Info("Locked server window - using URL " + _url);
		bool flag;
		try
		{
			flag = await GetAuthenticationStatus(_cancellationToken);
		}
		catch (DeprecatedApiException)
		{
			Logger.Instance.Error("Requested auth status API is deprecated!");
			_viewModel.Status = "This server requires a newer version of Only Retro Roblox Now or you need ORRH";
			return;
		}
		if (flag)
		{
			LaunchAndClose();
			return;
		}
		_viewModel.Status = "Key required";
		_viewModel.KeyTextBoxEnabled = true;
		_viewModel.SubmitButtonEnabled = true;
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		if (!_initialised)
		{
			Start();
			_initialised = true;
		}
	}

	private void SubmitButton_Click(object sender, RoutedEventArgs e)
	{
		if (!string.IsNullOrWhiteSpace(KeyTextBox.Text))
		{
			PerformCheckKey(KeyTextBox.Text);
		}
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		_cancellationTokenSource.Cancel();
	}
}
