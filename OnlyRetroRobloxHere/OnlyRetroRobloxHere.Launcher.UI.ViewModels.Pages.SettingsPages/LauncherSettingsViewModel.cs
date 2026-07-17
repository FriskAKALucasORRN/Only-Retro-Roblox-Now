using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using OnlyRetroRobloxHere.Common;
using OnlyRetroRobloxHere.Launcher.Enums;

namespace OnlyRetroRobloxHere.Launcher.UI.ViewModels.Pages.SettingsPages;

internal class LauncherSettingsViewModel : ViewModelBase
{
	public static IReadOnlyCollection<BootstrapperLaunchType> BootstrapperLaunchTypes => (IReadOnlyCollection<BootstrapperLaunchType>)(object)Enum.GetValues<BootstrapperLaunchType>();

	public string CustomMapsDirectory
	{
		get
		{
			return Settings.Default.Launch.CustomMapsDirectory;
		}
		set
		{
			if (Settings.Default.Launch.CustomMapsDirectory != value && !string.IsNullOrWhiteSpace(value))
			{
				if (!string.IsNullOrWhiteSpace(value) && !Directory.Exists(value))
				{
					Utils.ShowMessageBox("Custom maps directory does not exist.", MessageBoxButton.OK, MessageBoxImage.Hand);
					return;
				}
				Settings.Default.Launch.CustomMapsDirectory = value;
				OnPropertyChanged("CustomMapsDirectory");
				SharedViewModels.Host.OnPropertyChanged("MapsPath");
			}
		}
	}

	public BootstrapperLaunchType BootstrapperLaunchType
	{
		get
		{
			return Settings.Default.Launch.BootstrapperLaunchType;
		}
		set
		{
			if (Settings.Default.Launch.BootstrapperLaunchType != value)
			{
				Settings.Default.Launch.BootstrapperLaunchType = value;
				OnPropertyChanged("BootstrapperLaunchType");
			}
		}
	}

	public string AssetUrl
	{
		get
		{
			return SecureSettings.Default.AssetUrl;
		}
		set
		{
			if (SecureSettings.Default.AssetUrl != value)
			{
				SecureSettings.Default.AssetUrl = value;
				OnPropertyChanged("AssetUrl");
			}
		}
    }


    public string RobloxCookie
	{
		get
		{
			return SecureSettings.Default.RobloxCookie;
		}
		set
		{
			if (SecureSettings.Default.RobloxCookie != value)
			{
				SecureSettings.Default.RobloxCookie = value;
				OnPropertyChanged("RobloxCookie");
			}
		}
	}
    public bool DoUpdateAssets
    {
        get
        {
            return Settings.Default.Launch.DoUpdateAssets;
        }
        set
        {
            if (Settings.Default.Launch.DoUpdateAssets != value)
            {
                Settings.Default.Launch.DoUpdateAssets = value;
                OnPropertyChanged("DoUpdateAssets");
            }
        }
    }

    public static Visibility UpToDate =>
	!Settings.Default.Launch.OudatedAssets ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility OutOfDate =>
	Settings.Default.Launch.OudatedAssets ? Visibility.Visible : Visibility.Collapsed;

    public string UpdateNotice { get; } = ("Update available: " + Settings.Default.Launch.LatestHash + "");
}
