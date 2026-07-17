using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using OnlyRetroRobloxHere.Common;

namespace OnlyRetroRobloxHere.Launcher.UI.Dialogs;

public class BootstrapperWindowShared : Window
{
	private bool _initialised;

	private Process? _process;

	private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

	public BootstrapperWindowShared(Process process)
	{
		_process = process;
		base.Loaded += Window_Loaded;
		base.Closing += Window_Closing;
	}

	private async Task StartProcessWait(CancellationToken token)
	{
		int exitCode = 0;
		bool showMessage = false;
		int i = 0;
		while (true)
		{
			if (i < 20)
			{
				if (_process == null)
				{
					Logger.Instance.Info("Bootstrapper: Process is null");
					break;
				}
				if (_process.HasExited)
				{
					Logger.Instance.Info("Bootstrapper: Process has exited");
					exitCode = _process.ExitCode;
					showMessage = exitCode != 0;
					break;
				}
				try
				{
					foreach (IntPtr item in Native.EnumerateProcessWindowHandles(_process.Id))
					{
						string windowTitle = Native.GetWindowTitle(item);
						if (windowTitle.StartsWith("Roblox") || windowTitle.StartsWith("ROBLOX"))
						{
							goto end_IL_01bb;
						}
					}
				}
				catch (Exception value)
				{
					Logger.Instance.Info($"Bootstrapper: Exception with checking window handles: {value}");
				}
				try
				{
					await Task.Delay(1000, token);
				}
				catch (TaskCanceledException)
				{
					_process?.Dispose();
					_process = null;
					return;
				}
				i++;
				continue;
			}
			Logger.Instance.Info("Bootstrapper: Roblox did not open in time. Closing.");
			break;
			continue;
			end_IL_01bb:
			break;
		}
		_process?.Dispose();
		_process = null;
		base.Dispatcher.Invoke(delegate
		{
			Close();
		});
		if (showMessage)
		{
			MessageBox.Show("The client closed unexpectedly! (0x" + exitCode.ToString("X") + ")", "Only Retro Roblox Now", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		if (!_initialised)
		{
			Task.Run(() => StartProcessWait(_cancellationTokenSource.Token));
			_initialised = true;
		}
	}

	private void Window_Closing(object? sender, CancelEventArgs e)
	{
		_cancellationTokenSource.Cancel();
	}
}
