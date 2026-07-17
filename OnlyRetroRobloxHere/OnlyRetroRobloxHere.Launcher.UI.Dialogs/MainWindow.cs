using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using OnlyRetroRobloxHere.Common;
using OnlyRetroRobloxHere.Launcher.UI.Controls;
using OnlyRetroRobloxHere.Launcher.UI.Helpers;
using OnlyRetroRobloxHere.Launcher.UI.Pages;
using OnlyRetroRobloxHere.Launcher.UI.ViewModels.Dialogs;

namespace OnlyRetroRobloxHere.Launcher.UI.Dialogs;

public partial class MainWindow : Window, IComponentConnector
{
	private MainWindowViewModel _viewModel;

	private SelectableButton? _selectedButton;

	private BasePage? _currentPage;

	private Dictionary<string, BasePage> _pages;

	public MainWindow()
	{
		_pages = new Dictionary<string, BasePage>();
		InitializeComponent();
		_viewModel = new MainWindowViewModel();
		base.DataContext = _viewModel;
		OnNavigationButtonClick(PlayNavigationButton, null);
		ImmersiveDarkMode.ApplyWindow(this);
	}

	private BasePage GetPageFromName(string name)
	{
		return name switch
		{
			"Play" => new PlayPage(), 
			"Host" => new HostPage(), 
			"Customization" => new CustomizationPage(), 
			"Catalog" => new CatalogPage(), 
			"Membership" => new MembershipPage(), 
			"Inventory" => new InventoryPage(), 
			"AssetPacks" => new AssetPacksPage(), 
			"Settings" => new SettingsPage(), 
			"About" => new AboutPage(),
			"Info" => new AboutPage(),
            _ => throw new Exception("Unrecognised page name"), 
		};
	}

	private BasePage GetPage(string name)
	{
		if (!_pages.ContainsKey(name))
		{
			BasePage pageFromName = GetPageFromName(name);
			_pages[name] = pageFromName;
			return pageFromName;
		}
		return _pages[name];
	}

	private void NavigateToPage(BasePage page)
	{
		if (_currentPage != null)
		{
			_currentPage.OnPageClosing();
		}
		_currentPage = page;
		page.OnPageOpening();
		NavigationFrame.Navigate(page);
		NavigationFrame.NavigationService.RemoveBackEntry();
	}

	private void OnNavigationButtonClick(object sender, RoutedEventArgs e)
	{
		SelectableButton selectableButton = (SelectableButton)sender;
		if (!selectableButton.IsSelected)
		{
			if (!(selectableButton.Tag is string name))
			{
				throw new Exception("Page tag is not a string");
			}
			BasePage page = GetPage(name);
			NavigateToPage(page);
			selectableButton.IsSelected = true;
			if (_selectedButton != null)
			{
				_selectedButton.IsSelected = false;
			}
			_selectedButton = selectableButton;
		}
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (Settings.Initalized)
		{
			Settings.Default.Serialize();
		}
		SecureSettings.Save();
	}

	private SelectableButton GetNavigationButtonFromShortName(string name)
	{
		if (!(name == "Character"))
		{
			if (name == "Catalog")
			{
				return CatalogNavigationButton;
			}
			throw new Exception("Unrecognised short name navigation button name");
		}
		return CharacterNavigationButton;
	}

	public void NavigateByButton(string name)
	{
		Button navigationButtonFromShortName = GetNavigationButtonFromShortName(name);
		OnNavigationButtonClick(navigationButtonFromShortName, null);
	}

	public void Navigate(string name)
	{
		BasePage page = GetPage(name);
		NavigateToPage(page);
	}

	public void Navigate(BasePage page)
	{
		NavigateToPage(page);
	}

	public void SetActiveButton(string name)
	{
		SelectableButton navigationButtonFromShortName = GetNavigationButtonFromShortName(name);
		navigationButtonFromShortName.IsSelected = true;
		if (_selectedButton != null)
		{
			_selectedButton.IsSelected = false;
		}
		_selectedButton = navigationButtonFromShortName;
	}

}
