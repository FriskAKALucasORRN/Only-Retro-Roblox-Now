using OnlyRetroRobloxHere.Common;
using OnlyRetroRobloxHere.Launcher.Enums;
using OnlyRetroRobloxHere.Launcher.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OnlyRetroRobloxHere.Launcher;

public partial class App : Application
{
	private InterProcessLock? _lock;

	protected override void OnStartup(StartupEventArgs e)
	{
		AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler.Handle;
		Logger.Instance.Info("Only Retro Roblox Now Version: " + Utils.Version);
		_lock = new InterProcessLock("Launcher", TimeSpan.FromSeconds(1.0));
		Utils.ThemeManager.ApplyTheme();
        if (!_lock.IsAcquired)
		{
			Logger.Instance.Error("Only Retro Roblox Now is Already running! The application will quit.");
			Utils.ShowMessageBox("Another instance of Only Retro Roblox Now is already running.", MessageBoxButton.OK, MessageBoxImage.Hand);
			Shutdown();
			return;
		}
        Task.Run(() => AssetUpdater.UpdateAssetsAsync(PathHelper.Base)).GetAwaiter().GetResult();
        if (!PathHelper.Base.All(char.IsAscii))
		{
			Logger.Instance.Warn("ORRN launch path has non-ASCII characters.");
			Utils.ShowMessageBox("Your path has non-ASCII characters. This will cause problems with Roblox! Please change it to somewhere else, Or.. Make sure that your path has no spaces, numbers, or non-english letters!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
		}
		List<VCPPRedist> uninstalledRedists = VCPPRedistInstallationDetector.GetUninstalledRedists();
		if (uninstalledRedists.Any())
		{
			string text = string.Join(" and ", uninstalledRedists.Select((VCPPRedist x) => x.GetDescription()));
			Logger.Instance.Warn(text + " redists are missing!");
			Utils.ShowMessageBox("ORRN detected that " + text + " redist are missing. Redistributables are required to play the clients. Check the documentation page for instructions.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
		}
		List<DotNetRuntime> uninstalledRuntimes = DotNetRuntimeInstallationDetector.GetUninstalledRuntimes();
		if (uninstalledRuntimes.Any())
		{
			string text2 = string.Join(" and ", uninstalledRuntimes.Select((DotNetRuntime x) => x.GetDescription()));
			Logger.Instance.Warn(text2 + " are missing!");
			if (Utils.ShowMessageBox("ORRN detected that " + text2 + " are missing. .NET runtimes are required for ORRN to function properly.\nThe runtimes can be found at https://dotnet.microsoft.com/en-us/download/dotnet/6.0. Would you like to open this URL in your browser?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				Utils.OpenUrl("https://dotnet.microsoft.com/en-us/download/dotnet/6.0");
			}
			else if
				(Utils.ShowMessageBox("ORRN will now close as without these two runtimes it cannot run. Please get the .NET 6.0 Desktop Runtime and ASP.NET Core Runtime. XP users, check the documentation for XP support. Vista users, do the same thing.", MessageBoxButton.OK, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
				{
                Shutdown();
					return;
            }
        }
		Directory.CreateDirectory(PathHelper.UserAppData);
		Utils.ClearClientAddonsCache();
		base.OnStartup(e);
	}

	protected override void OnExit(ExitEventArgs e)
	{
		_lock?.Dispose();
		base.OnExit(e);
	}
}
