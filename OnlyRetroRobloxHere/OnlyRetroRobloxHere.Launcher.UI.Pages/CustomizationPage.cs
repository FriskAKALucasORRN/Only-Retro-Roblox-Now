using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.Win32;
using OnlyRetroRobloxHere.Common;
using OnlyRetroRobloxHere.Common.Enums;
using OnlyRetroRobloxHere.Common.Models;
using OnlyRetroRobloxHere.Launcher.Enums;
using OnlyRetroRobloxHere.Launcher.Extensions;
using OnlyRetroRobloxHere.Launcher.Models;
using OnlyRetroRobloxHere.Launcher.UI.Controls;
using OnlyRetroRobloxHere.Launcher.UI.Dialogs;
using OnlyRetroRobloxHere.Launcher.UI.ViewModels.Pages;

namespace OnlyRetroRobloxHere.Launcher.UI.Pages;

public partial class CustomizationPage : BasePage, IComponentConnector, IStyleConnector
{
	private static readonly string _renderPath = Path.Combine(PathHelper.UserAppData, "render.png");

	private static readonly AvatarSlot[] _avatarSlots = Enum.GetValues<AvatarSlot>();

	private CustomizationViewModel _viewModel;

	private string? _selectedLimb;

	private AvatarAssetType _selectedCategory;

	private SelectableButton? _selectedCategoryButton;

	private OutfitManager _outfitManager;

	private Process? _renderProcess;

	private FileSystemWatcher _renderWatcher;

	public CustomizationPage()
	{
		InitializeComponent();
		_viewModel = new CustomizationViewModel();
		_outfitManager = _viewModel.OutfitManager;
		base.DataContext = _viewModel;
		OnWardrobeCategoryClicked(TShirtButton, null);
		Appearance.Unequipped.CollectionChanged += OnNewUnequippedItem;
		RefreshUnequippedCategorisedItems();
		OnClientsListChanged();
		Clients.ClientsChangedEvent += OnClientsListChanged;
		Application.Current.Exit += OnApplicationExit;
		if (File.Exists(_renderPath))
		{
			_viewModel.CharacterImage = Utils.ToBitmapImage(new Bitmap(_renderPath));
		}
		_renderWatcher = new FileSystemWatcher
		{
			NotifyFilter = NotifyFilters.LastWrite,
			Path = PathHelper.UserAppData,
			Filter = "render.png"
		};
		_renderWatcher.Changed += OnRenderChanged;
		_renderWatcher.EnableRaisingEvents = true;
	}

	private void RefreshUnequippedCategorisedItems()
	{
		_viewModel.UnequippedCategorisedItems = Appearance.GetUnequippedAvatarItemWithType(_selectedCategory);
	}

	private void OnNewUnequippedItem(object? sender, NotifyCollectionChangedEventArgs e)
	{
		RefreshUnequippedCategorisedItems();
	}

	private string GetLimbName(string name)
	{
		return name switch
		{
			"RightArm" => "Right Arm", 
			"LeftArm" => "Left Arm", 
			"RightLeg" => "Right Leg", 
			"LeftLeg" => "Left Leg", 
			_ => name, 
		};
	}

	private void SetSelectedLimbColour(int id)
	{
		switch (_selectedLimb)
		{
		case "Head":
			_viewModel.HeadColour = id;
			break;
		case "Torso":
			_viewModel.TorsoColour = id;
			break;
		case "RightArm":
			_viewModel.RightArmColour = id;
			break;
		case "LeftArm":
			_viewModel.LeftArmColour = id;
			break;
		case "RightLeg":
			_viewModel.RightLegColour = id;
			break;
		case "LeftLeg":
			_viewModel.LeftLegColour = id;
			break;
		default:
			throw new Exception("Unrecognised limb name: " + (_selectedLimb ?? "NULL"));
		}
	}

	private void CloseColourSelectionModal()
	{
		_viewModel.ColourSelectionModalVisibility = Visibility.Collapsed;
		_selectedLimb = null;
	}

	private void OnCharacterLimbClick(object sender, RoutedEventArgs e)
	{
		if (!(((Button)sender).Tag is string text))
		{
			throw new Exception("Limb name is null");
		}
		string limbName = GetLimbName(text);
		_viewModel.ColourSelectionModalTitle = "Select a " + limbName + " Color";
		_viewModel.ColourSelectionModalVisibility = Visibility.Visible;
		_selectedLimb = text;
	}

	private void CloseLimbColourModalButton_Click(object sender, RoutedEventArgs e)
	{
		CloseColourSelectionModal();
	}

	private void OnLimbColourClicked(object sender, RoutedEventArgs e)
	{
		if (!(((Button)sender).Tag is int selectedLimbColour))
		{
			throw new Exception("Colour tag is not an integer");
		}
		SetSelectedLimbColour(selectedLimbColour);
		CloseColourSelectionModal();
	}

	private void OnItemWearClicked(object sender, RoutedEventArgs e)
	{
		if (!(((Button)sender).Tag is ulong id))
		{
			throw new Exception("Item tag is not a ulong");
		}
		Appearance.Equip(id);
	}

	private void OnItemRemoveClicked(object sender, RoutedEventArgs e)
	{
		if (!(((Button)sender).Tag is AvatarSlot slot))
		{
			throw new Exception("Item tag is not an asset slot");
		}
		Appearance.Unequip(slot);
	}

	private void OnWardrobeCategoryClicked(object sender, RoutedEventArgs e)
	{
		SelectableButton selectableButton = (SelectableButton)sender;
		if (selectableButton != _selectedCategoryButton)
		{
			AvatarAssetType type = (_selectedCategory = Enum.Parse<AvatarAssetType>((selectableButton.Tag as string) ?? throw new Exception("Category tag is not a string")));
			_viewModel.ShowCustomIdField = type.CanHaveCustomAssets();
			_viewModel.CustomId = 0uL;
			UnequippedItemsControl.ResetPageIndex(updateItems: false);
			RefreshUnequippedCategorisedItems();
			if (_selectedCategoryButton != null)
			{
				_selectedCategoryButton.IsSelected = false;
			}
			selectableButton.IsSelected = true;
			_selectedCategoryButton = selectableButton;
		}
	}

	private void OnCustomItemSubmitClicked(object sender, RoutedEventArgs e)
	{
		if (_selectedCategory.CanHaveCustomAssets())
		{
			AvatarItem avatarItem = AvatarItems.GetById(_viewModel.CustomId);
			if (avatarItem == null)
			{
				avatarItem = AvatarItem.GetCustom(_viewModel.CustomId, _selectedCategory);
			}
			else
			{
				Appearance.AwardItem(avatarItem);
			}
			Appearance.Equip(avatarItem);
		}
	}

	private void OnShopButtonClicked(object sender, RoutedEventArgs e)
	{
		((MainWindow)Application.Current.MainWindow).NavigateByButton("Catalog");
	}

	private void OnShowOlderCharacterColoursChecked(object sender, RoutedEventArgs e)
	{
		bool valueOrDefault = ShowOlderCharacterColours.IsChecked == true;
		_viewModel.CharacterColours = (valueOrDefault ? BrickColors.CharacterOlder : BrickColors.Character);
	}

	private void CloseFigureCharacterTypeModalButtonClicked(object sender, RoutedEventArgs e)
	{
		_viewModel.FigureCharacterTypeModalVisibility = Visibility.Collapsed;
	}

	private void OnFigureCharacterTypeClicked(object sender, RoutedEventArgs e)
	{
		if (!(((Button)sender).Tag is FigureCharacterType figureCharacterType))
		{
			throw new Exception("Category tag is not a FigureCharacterType");
		}
		_viewModel.FigureCharacterType = figureCharacterType;
		_viewModel.FigureCharacterTypeModalVisibility = Visibility.Collapsed;
	}

	private void OnOpenFigureCharacterTypeModalClicked(object sender, RoutedEventArgs e)
	{
		_viewModel.FigureCharacterTypeModalVisibility = Visibility.Visible;
	}

	private void OnTabClicked(object sender, RoutedEventArgs e)
	{
		bool flag = ((((SelectableButton)sender).Tag as string) ?? throw new Exception("Tab tag is not a string")) == "Wardrobe";
		_viewModel.TabWardrobeSelected = flag;
		_viewModel.TabOutfitsSelected = !flag;
	}

	private void OnOutfitCreateButtonClicked(object sender, RoutedEventArgs e)
	{
		_viewModel.OutfitCreateName = "";
		_viewModel.OutfitCreateClient = "None";
		_viewModel.OutfitCreateError = "";
		_viewModel.OutfitCreateModalVisibility = Visibility.Visible;
	}

	private void UpdateClientsList(ObservableCollection<string> list, string? keep = null)
	{
		lock (Clients.List)
		{
			for (int num = list.Count - 1; num > 0; num--)
			{
				list.RemoveAt(num);
			}
			List<string> clientsWithPreferred = _outfitManager.GetClientsWithPreferred();
			foreach (string item in Clients.List)
			{
				if (item == keep || !clientsWithPreferred.Contains(item))
				{
					list.Add(item);
				}
			}
		}
	}

	private void OnClientsListChanged()
	{
		string outfitCreateClient = _viewModel.OutfitCreateClient;
		UpdateClientsList(_viewModel.OutfitClients);
		_viewModel.OutfitCreateClient = (_viewModel.OutfitClients.Contains(outfitCreateClient) ? outfitCreateClient : "None");
		if (_viewModel.OutfitSelected != null)
		{
			outfitCreateClient = _viewModel.OutfitDetailsClient;
			UpdateClientsList(_viewModel.OutfitDetailsClients, _viewModel.OutfitSelected.Client);
			_viewModel.OutfitDetailsClient = (_viewModel.OutfitDetailsClients.Contains(outfitCreateClient) ? outfitCreateClient : "None");
		}
	}

	private void OnOutfitCreateCloseClicked(object sender, RoutedEventArgs e)
	{
		_viewModel.OutfitCreateModalVisibility = Visibility.Collapsed;
	}

	private void OnOutfitCreateSaveClicked(object sender, RoutedEventArgs e)
	{
		Settings.Default.Serialize();
		OutfitManager.Result result = _outfitManager.CreateOutfit(_viewModel.OutfitCreateName, Settings.Default.Character);
		if (!result.Success)
		{
			_viewModel.OutfitCreateError = result.Message;
			return;
		}
		if (_viewModel.OutfitCreateClient != "None")
		{
			Outfit outfit = _outfitManager.GetOutfit(_viewModel.OutfitCreateName);
			if (outfit == null)
			{
				_viewModel.OutfitCreateError = "Failed to find outfit after creating it.";
				return;
			}
			_outfitManager.SetPreferredOutfitForClient(outfit, _viewModel.OutfitCreateClient);
			OnClientsListChanged();
			OutfitItemsControl.Items.Refresh();
		}
		_viewModel.OutfitCreateModalVisibility = Visibility.Collapsed;
	}

	private void UpdateOutfitDetailsHeader()
	{
		_viewModel.OutfitDetailsHeaderText = "Outfit \"" + _viewModel.OutfitSelected.Name + "\"";
	}

	private void UpdateOutfitDetailsWearingText()
	{
		List<string> list = new List<string>();
		AvatarSlot[] avatarSlots = _avatarSlots;
		foreach (AvatarSlot avatarSlot in avatarSlots)
		{
			if (_viewModel.OutfitSelected.Character.Equipped.ContainsKey(avatarSlot))
			{
				ulong num = _viewModel.OutfitSelected.Character.Equipped[avatarSlot];
				AvatarItem byId = AvatarItems.GetById(num);
				if (byId != null)
				{
					list.Add(byId.Name);
					continue;
				}
				list.Add($"Custom {avatarSlot.GetDescription()} ({num})");
			}
		}
		for (int j = list.Count() - 1; j < _avatarSlots.Length - 1; j++)
		{
			list.Add("");
		}
		string outfitDetailsWearingText = string.Join('\n', list);
		_viewModel.OutfitDetailsWearingText = outfitDetailsWearingText;
	}

	private void SetErrorInOutfitDetails(string? message)
	{
		_viewModel.OutfitDetailsError = message;
		_viewModel.OutfitDetailsSuccess = null;
	}

	private void SetSuccessInOutfitDetails(string? message)
	{
		_viewModel.OutfitDetailsSuccess = message;
		_viewModel.OutfitDetailsError = null;
	}

	private void OnOutfitDetailsButtonClicked(object sender, RoutedEventArgs e)
	{
		if (!(((Button)sender).Tag is Outfit outfit))
		{
			throw new Exception("Details button tag is not an Outfit");
		}
		_viewModel.OutfitSelected = outfit;
		UpdateOutfitDetailsHeader();
		UpdateOutfitDetailsWearingText();
		UpdateClientsList(_viewModel.OutfitDetailsClients, outfit.Client);
		_viewModel.OutfitDetailsName = outfit.Name;
		_viewModel.OutfitDetailsClient = ((outfit.Client != null) ? outfit.Client : "None");
		_viewModel.OutfitDetailsSuccess = null;
		_viewModel.OutfitDetailsError = null;
		_viewModel.OutfitDetailsDeleteText = "Delete";
		_viewModel.OutfitDetailsModalVisibility = Visibility.Visible;
	}

	private void OnOutfitDetailsSaveClicked(object sender, RoutedEventArgs e)
	{
		bool flag = false;
		bool flag2 = false;
		if (_viewModel.OutfitDetailsName != _viewModel.OutfitSelected.Name)
		{
			OutfitManager.Result result = _outfitManager.RenameOutfit(_viewModel.OutfitSelected, _viewModel.OutfitDetailsName);
			if (!result.Success)
			{
				SetErrorInOutfitDetails(result.Message);
				return;
			}
			UpdateOutfitDetailsHeader();
			flag = true;
		}
		if (!_viewModel.OutfitSelected.IsClient(_viewModel.OutfitDetailsClient))
		{
			_outfitManager.SetPreferredOutfitForClient(_viewModel.OutfitSelected, _viewModel.OutfitDetailsClient);
			OnClientsListChanged();
			OutfitItemsControl.Items.Refresh();
			flag2 = true;
		}
		string successInOutfitDetails = null;
		if (flag && flag2)
		{
			successInOutfitDetails = "Successfully updated name and client!";
		}
		else if (flag)
		{
			successInOutfitDetails = "Successfully updated name!";
		}
		else if (flag2)
		{
			successInOutfitDetails = "Successfully updated client!";
		}
		SetSuccessInOutfitDetails(successInOutfitDetails);
	}

	private void OnOutfitDetailsDeleteClicked(object sender, RoutedEventArgs e)
	{
		if (_viewModel.OutfitDetailsDeleteText == "Delete")
		{
			_viewModel.OutfitDetailsDeleteText = "Are you sure? Click again to confirm.";
			return;
		}
		bool flag = _viewModel.OutfitSelected.Client != null;
		OutfitManager.Result result = _outfitManager.DeleteOutfit(_viewModel.OutfitSelected);
		if (!result.Success)
		{
			SetErrorInOutfitDetails(result.Message);
			return;
		}
		if (flag)
		{
			UpdateClientsList(_viewModel.OutfitClients);
		}
		_viewModel.OutfitDetailsModalVisibility = Visibility.Collapsed;
		Sounds.PageTurn.Play();
	}

	private void OnOutfitDetailsCloseClicked(object sender, RoutedEventArgs e)
	{
		_viewModel.OutfitDetailsModalVisibility = Visibility.Collapsed;
		_viewModel.OutfitSelected = null;
	}

	private void OnOutfitEquipButtonClicked(object sender, RoutedEventArgs e)
	{
		if (!(((Button)sender).Tag is Outfit outfit))
		{
			throw new Exception("Equip button tag is not an Outfit");
		}
		Appearance.UnequipAll();
		foreach (KeyValuePair<AvatarSlot, ulong> item in outfit.Character.Equipped)
		{
			AvatarItem avatarItem = AvatarItems.GetById(item.Value);
			if (avatarItem == null)
			{
				if (!item.Key.ConvertToAvatarAssetType().CanHaveCustomAssets())
				{
					continue;
				}
				avatarItem = AvatarItem.GetCustom(item.Value, item.Key.ConvertToAvatarAssetType());
			}
			else
			{
				Appearance.AwardItem(avatarItem);
			}
			Appearance.Equip(avatarItem);
		}
		_viewModel.HeadColour = outfit.Character.Head;
		_viewModel.LeftArmColour = outfit.Character.LeftArm;
		_viewModel.RightArmColour = outfit.Character.RightArm;
		_viewModel.LeftLegColour = outfit.Character.LeftLeg;
		_viewModel.RightLegColour = outfit.Character.RightLeg;
		_viewModel.TorsoColour = outfit.Character.Torso;
		_viewModel.FigureCharacterType = outfit.Character.FigureCharacterType;
	}

	private void OnApplicationExit(object sender, EventArgs e)
	{
		_renderProcess?.Kill();
	}

	private void OnRenderChanged(object source, FileSystemEventArgs e)
	{
		for (int i = 0; i < 5; i++)
		{
			Thread.Sleep(100);
			try
			{
				Bitmap bitmap = new Bitmap(_renderPath);
				_viewModel.CharacterImage = Utils.ToBitmapImage(bitmap);
			}
			catch (Exception value)
			{
				Logger.Instance.Warn($"Failed to read render: {value}");
				continue;
			}
			_viewModel.CharacterProgressIndicatorVisible = Visibility.Collapsed;
			break;
		}
	}

	private void RequestRedraw()
	{
		_renderProcess?.Kill();
		_renderProcess = RobloxLauncher.LaunchProcess(LaunchType.Render, authLaunch: false, hide: true);
		_viewModel.CharacterProgressIndicatorVisible = Visibility.Visible;
	}

	private void OnRedrawClicked(object sender, RoutedEventArgs e)
	{
		RequestRedraw();
	}

	private void OnCharacterRenderModalCloseClicked(object sender, RoutedEventArgs e)
	{
		_viewModel.CharacterRenderModalVisibility = Visibility.Collapsed;
	}

	private void OnRenderClicked(object sender, RoutedEventArgs e)
	{
		_viewModel.CharacterRenderModalVisibility = Visibility.Visible;
	}

	private void OnCharacterSaveClicked(object sender, RoutedEventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Title = "Save your Only Retro Roblox Now render",
			FileName = "ORRH_Render.png",
			Filter = "PNG file|*.png"
		};
		if (saveFileDialog.ShowDialog() != true)
		{
			return;
		}
		try
		{
			if (File.Exists(saveFileDialog.FileName))
			{
				File.Delete(saveFileDialog.FileName);
			}
			File.Copy(_renderPath, saveFileDialog.FileName);
			Utils.ShowMessageBox("Render saved!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
		}
		catch (IOException ex)
		{
			Logger.Instance.Error($"Failed to save render: {ex}");
			Utils.ShowMessageBox("Failed to save render: " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

}
