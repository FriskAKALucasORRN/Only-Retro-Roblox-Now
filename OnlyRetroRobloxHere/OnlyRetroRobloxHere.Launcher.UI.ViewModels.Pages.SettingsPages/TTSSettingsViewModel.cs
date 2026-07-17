using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using OnlyRetroRobloxHere.Launcher.Enums;

namespace OnlyRetroRobloxHere.Launcher.UI.ViewModels.Pages.SettingsPages;

internal class TTSSettingsViewModel : ViewModelBase
{
	public Visibility TTSCustomisationVisibility { get; set; }

	public Visibility TTSErrorVisibility { get; set; }

	public List<TTSEngine> TTSEngines { get; } = new List<TTSEngine>();

	public ObservableCollection<string> TTSVoices { get; } = new ObservableCollection<string>();

	public string TTSError { get; set; } = "TTS Has been Removed until further notice. We Are Sorry! :-(";

	public bool TTSEnabled
	{
		get
		{
			return Settings.Default.Launch.TTSEnabled;
		}
		set
		{
			if (Settings.Default.Launch.TTSEnabled != value)
			{
				Settings.Default.Launch.TTSEnabled = value;
				OnPropertyChanged("TTSEnabled");
			}
		}
	}

	public TTSEngine TTSEngine
	{
		get
		{
			return Settings.Default.Launch.TTSType;
		}
		set
		{
			if (Settings.Default.Launch.TTSType != value)
			{
				Settings.Default.Launch.TTSType = value;
				OnPropertyChanged("TTSEngine");
			}
		}
	}

	public string TTSVoice
	{
		get
		{
			return Settings.Default.Launch.TTSVoice;
		}
		set
		{
			if (Settings.Default.Launch.TTSVoice != value)
			{
				Settings.Default.Launch.TTSVoice = value;
				OnPropertyChanged("TTSVoice");
			}
		}
	}
}
