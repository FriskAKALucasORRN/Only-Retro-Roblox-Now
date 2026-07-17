using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using OnlyRetroRobloxHere.Common;
using OnlyRetroRobloxHere.Launcher.UI.ViewModels.Pages.SettingsPages;

namespace OnlyRetroRobloxHere.Launcher.UI.Pages.SettingsPages;

public partial class LauncherSettingsPage : BasePage, IComponentConnector
{
	private LauncherSettingsViewModel _viewModel;

	private FolderBrowserDialog _dialog = new FolderBrowserDialog();

	public LauncherSettingsPage()
	{
		InitializeComponent();
		_viewModel = SharedViewModels.LauncherSettings;
		base.DataContext = _viewModel;
	}

	private void BrowseDirectory()
	{
		if (_dialog.ShowDialog() == DialogResult.OK)
		{
			_viewModel.CustomMapsDirectory = _dialog.SelectedPath;
		}
	}

	private void OnBrowseMapDirectory(object sender, RoutedEventArgs e)
	{
		BrowseDirectory();
	}

    private void OnRobloxCookieChanged(object sender, RoutedEventArgs e)
	{
		SecureSettings.Default.RobloxCookie = RobloxCookieBox.Password;
        SecureSettings.Save();
    }

	private void OnAssetUrlChanged(object sender, RoutedEventArgs e)
	{
		SecureSettings.Default.AssetUrl = AssetURLBox.Text;
        SecureSettings.Save();
    }

    public override void OnPageOpening()
	{
		RobloxCookieBox.Password = SecureSettings.Default.RobloxCookie;
		RobloxCookieBox.PasswordChanged += OnRobloxCookieChanged;
        AssetURLBox.Text = SecureSettings.Default.AssetUrl;
        AssetURLBox.TextChanged += OnAssetUrlChanged;
        base.OnPageOpening();
	}

	public override void OnPageClosing()
	{
		RobloxCookieBox.PasswordChanged -= OnRobloxCookieChanged;
        AssetURLBox.TextChanged -= OnAssetUrlChanged;
        base.OnPageClosing();
	}

}
