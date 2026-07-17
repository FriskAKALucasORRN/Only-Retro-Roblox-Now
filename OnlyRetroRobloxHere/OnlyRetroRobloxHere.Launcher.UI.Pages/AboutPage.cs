using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using OnlyRetroRobloxHere.Launcher.UI.ViewModels.Pages;

namespace OnlyRetroRobloxHere.Launcher.UI.Pages;

public partial class AboutPage : BasePage, IComponentConnector
{
	private AboutViewModel _viewModel;

	public AboutPage()
	{
		InitializeComponent();
		_viewModel = new AboutViewModel();
		_viewModel.PrivateVersionWarningVisibility = ((!Utils.Version.Contains("_PB")) ? Visibility.Collapsed : Visibility.Visible);
        base.DataContext = _viewModel;
	}

	private void OnMattHeadClicked(object sender, RoutedEventArgs e)
	{
		_viewModel.MattHeadSize++;
	}

	private void OnGithubIconClicked(object sender, RoutedEventArgs e)
	{
		Utils.OpenUrl("https://github.com/FriskAKALucasORRN/Only-Retro-Roblox-Now");
	}

	private void OnDiscordIconClicked(object sender, RoutedEventArgs e)
	{
		Utils.OpenUrl("https://discord.gg/8FT3ApxhP8");
	}

	public override void OnPageOpening()
	{
		Snowfall.Start();
		base.OnPageOpening();
	}

	public override void OnPageClosing()
	{
		Snowfall.Stop();
		base.OnPageClosing();
	}

}
