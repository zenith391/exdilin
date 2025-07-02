using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class UIDialog : MonoBehaviour, IPopupDelegate
{
	public GameObject screenDimmer;

	public UIDialogPanel[] dialogPrefabs;

	private Dictionary<DialogTypeEnum, UIDialogPanel> dialogs;

	private Dictionary<DialogTypeEnum, UIDialogPanel> smallScreenDialogs;

	private UIDialogPanel activeDialog;

	private RectTransform uiParentTranform;

	private Canvas canvas;

	private bool unpauseOnDismiss;

	public void Init()
	{
		uiParentTranform = GetComponent<RectTransform>();
		dialogs = new Dictionary<DialogTypeEnum, UIDialogPanel>();
		smallScreenDialogs = new Dictionary<DialogTypeEnum, UIDialogPanel>();
		canvas = GetComponent<Canvas>();
		screenDimmer.SetActive(value: false);
		for (int i = 0; i < dialogPrefabs.Length; i++)
		{
			UIDialogPanel uIDialogPanel = UnityEngine.Object.Instantiate(dialogPrefabs[i]);
			uIDialogPanel.Init();
			uIDialogPanel.doCloseDialog = delegate
			{
				CloseActiveDialog();
			};
			if (uIDialogPanel.isSmallScreenVersion)
			{
				smallScreenDialogs.Add(uIDialogPanel.dialogType, uIDialogPanel);
			}
			else
			{
				dialogs.Add(uIDialogPanel.dialogType, uIDialogPanel);
			}
			RectTransform component = uIDialogPanel.GetComponent<RectTransform>();
			component.SetParent(uiParentTranform, worldPositionStays: false);
			component.SetAsLastSibling();
			uIDialogPanel.gameObject.SetActive(value: false);
		}
		canvas.enabled = false;
	}

	public void ShowModelPurchaseConfirmation(string modelTitle, int modelPrice, Texture2D modelPreviewImage)
	{
		Dialog_ModelPurchaseConfirmation dialog_ModelPurchaseConfirmation = dialogs[DialogTypeEnum.ModelPurchaseConfirmation] as Dialog_ModelPurchaseConfirmation;
		dialog_ModelPurchaseConfirmation.SetPrice(modelPrice);
		dialog_ModelPurchaseConfirmation.SetImage(modelPreviewImage);
		ShowDialog(dialog_ModelPurchaseConfirmation);
	}

	public void ShowModelSaveDialog(Texture2D modelImage, Action completion, Action<string> titleEditAction)
	{
		Dialog_SaveModel dialog = GetDialog<Dialog_SaveModel>(DialogTypeEnum.SaveModel);
		dialog.Setup(completion, titleEditAction);
		dialog.SetModelImage(modelImage);
		ShowDialog(dialog);
		dialog.SetText(string.Empty);
	}

	public void ShowGenericDialog(string mainText, string buttonAText, string buttonBText, Action buttonAAction, Action buttonBAction)
	{
		Dialog_GenericTwoButton dialog_GenericTwoButton = dialogs[DialogTypeEnum.GenericTwoButton] as Dialog_GenericTwoButton;
		dialog_GenericTwoButton.Setup(mainText, buttonAText, buttonBText, buttonAAction, buttonBAction);
		ShowDialog(dialog_GenericTwoButton);
	}

	public void ShowGenericDialog(string mainText, string[] buttonLabels, Action[] buttonActions)
	{
		Dialog_GenericMultiButton dialog_GenericMultiButton = dialogs[DialogTypeEnum.GenericMultiButton] as Dialog_GenericMultiButton;
		dialog_GenericMultiButton.Setup(mainText, buttonLabels, buttonActions);
		ShowDialog(dialog_GenericMultiButton);
	}

	public void ShowUnsellableBlocksInModelWarning(HashSet<GAF> unsellableBlocks, Action completion)
	{
		Dialog_TwoButtonWithTilesExtraText dialog_TwoButtonWithTilesExtraText = dialogs[DialogTypeEnum.TwoButtonWithTilesAndExtraText] as Dialog_TwoButtonWithTilesExtraText;
		dialog_TwoButtonWithTilesExtraText.Setup("You can't sell a model with these blocks:", "No", "Yes", null, completion);
		dialog_TwoButtonWithTilesExtraText.tileScrollView.SetTiles(unsellableBlocks);
		ShowDialog(dialog_TwoButtonWithTilesExtraText);
	}

	public void ShowSetPurchasePrompt(Dictionary<GAF, int> rewards, string dialogCaption, string setTitle, int setId, int setPrice)
	{
		Dialog_SetPurchasePrompt dialog_SetPurchasePrompt = dialogs[DialogTypeEnum.SetPurchasePrompt] as Dialog_SetPurchasePrompt;
		dialog_SetPurchasePrompt.Setup(dialogCaption, setTitle, setId, setPrice);
		dialog_SetPurchasePrompt.SetupRewardTiles(rewards);
		ShowDialog(dialog_SetPurchasePrompt);
	}

	public void ShowZeroInventoryDialog(Tile tile)
	{
		string mainText = "Looks like you're out of this type of block. Don't worry! You can get more from the store or by moving blocks back into the build panel!";
		BlockItem blockItem = BlockItem.FindByGafPredicateNameAndArguments(tile.gaf.Predicate.Name, tile.gaf.Args);
		int blockItemId = 0;
		if (blockItem != null)
		{
			blockItemId = blockItem.Id;
		}
		TabBarTabId tab = Blocksworld.buildPanel.GetTabBar().SelectedTab;
		Action action = delegate
		{
			WorldSession.OpenStoreWithBlockItemId(tab, blockItemId);
		};
		Action action2 = delegate
		{
			BlocksInventory blocksInventory = new BlocksInventory();
			blocksInventory.Add(blockItemId, 1);
			WorldSession.platformDelegate.AddItemsToCart(blocksInventory);
		};
		string[] buttonLabels = new string[3] { "To the store!", "Add to cart", "Close" };
		Action[] buttonActions = new Action[3] { action, action2, null };
		ShowGenericDialog(mainText, buttonLabels, buttonActions);
	}

	public void ShowPasteFailInfo(Dictionary<GAF, int> missing, List<GAF> uniquesInModel, string caption)
	{
		Dialog_GenericMultiButton dialog_GenericMultiButton = dialogs[DialogTypeEnum.GenericMultiButtonWithTiles] as Dialog_GenericMultiButton;
		Action action = delegate
		{
			if (Blocksworld.selectedBunch != null)
			{
				Blocksworld.selectedBunch.IgnoreRaycasts(value: false);
			}
			else if (Blocksworld.selectedBlock != null)
			{
				Blocksworld.selectedBlock.IgnoreRaycasts(value: false);
			}
			Blocksworld.Deselect(silent: true);
			Blocksworld.tBoxGesture.Reset();
		};
		if (uniquesInModel.Count > 0)
		{
			string[] buttonLabels = new string[1] { "Ok" };
			Action[] buttonActions = new Action[1] { action };
			dialog_GenericMultiButton.Setup(caption, buttonLabels, buttonActions);
			dialog_GenericMultiButton.tileScrollView.SetTiles(uniquesInModel);
		}
		else
		{
			Action action2 = delegate
			{
				BlocksInventory blocksInventory = new BlocksInventory();
				foreach (KeyValuePair<GAF, int> item in missing)
				{
					int blockItemId = item.Key.BlockItemId;
					if (blockItemId > 0)
					{
						Debug.Log("item id " + blockItemId + " missing " + item.Value);
						blocksInventory.Add(blockItemId, item.Value);
					}
				}
				WorldSession.platformDelegate.AddItemsToCart(blocksInventory);
				ShowGoToShoppingCartConfirmationPrompt();
			};
			string[] buttonLabels2 = new string[2] { "Cancel", "Add to cart" };
			Action[] buttonActions2 = new Action[2] { action, action2 };
			dialog_GenericMultiButton.Setup(caption, buttonLabels2, buttonActions2);
			dialog_GenericMultiButton.tileScrollView.SetTilesAndQuantities(missing);
		}
		ShowDialog(dialog_GenericMultiButton);
	}

	public void ShowModelPasteConflictsInfo(HashSet<GAF> conflictingGafs, string caption)
	{
		Dialog_GenericTwoButtonWithTiles dialog_GenericTwoButtonWithTiles = dialogs[DialogTypeEnum.GenericTwoButtonWithTiles] as Dialog_GenericTwoButtonWithTiles;
		Action buttonAction = delegate
		{
			if (Blocksworld.selectedBunch != null)
			{
				Blocksworld.selectedBunch.IgnoreRaycasts(value: false);
			}
			else if (Blocksworld.selectedBlock != null)
			{
				Blocksworld.selectedBlock.IgnoreRaycasts(value: false);
			}
			Blocksworld.Deselect(silent: true);
			Blocksworld.tBoxGesture.Reset();
		};
		if (conflictingGafs.Count > 0)
		{
			dialog_GenericTwoButtonWithTiles.Setup(caption, "Ok", buttonAction);
			dialog_GenericTwoButtonWithTiles.tileScrollView.SetTiles(conflictingGafs);
		}
		ShowDialog(dialog_GenericTwoButtonWithTiles);
	}

	public void ShowScriptExistsDialog(Block block)
	{
		string[] buttonLabels = new string[3] { "Replace", "Add", "Cancel" };
		string mainText = "This block contains a script.";
		Action[] buttonActions = new Action[3]
		{
			delegate
			{
				Blocksworld.DoPasteScriptFromClipboard(block, replace: true);
				History.AddStateIfNecessary();
			},
			delegate
			{
				Blocksworld.DoPasteScriptFromClipboard(block);
				History.AddStateIfNecessary();
			},
			delegate
			{
			}
		};
		ShowGenericDialog(mainText, buttonLabels, buttonActions);
	}

	public void ShowPasteScriptIncompatibleDialog(Block block, List<List<Tile>> script, List<GAF> incompatible, bool replace = false)
	{
		string empty = string.Empty;
		string text = ". Do you want to use the rest of the tiles?";
		if (script.Count != 1 || script[0].Count != 1)
		{
			empty = ((incompatible.Count != 1) ? "\nSome tiles are not compatible with the target block" : "\nOne tile is not compatible with the target block");
		}
		else
		{
			empty = "\nAll tiles are incompatible with the target block.";
			text = string.Empty;
		}
		string mainTextStr = empty + text;
		Dialog_GenericTwoButtonWithTiles dialog_GenericTwoButtonWithTiles = dialogs[DialogTypeEnum.GenericTwoButtonWithTiles] as Dialog_GenericTwoButtonWithTiles;
		dialog_GenericTwoButtonWithTiles.Setup(mainTextStr, "No", "Yes", delegate
		{
		}, delegate
		{
			Blocksworld.PasteScript(block, script, replace, force: true);
			History.AddStateIfNecessary();
		});
		dialog_GenericTwoButtonWithTiles.tileScrollView.SetTiles(incompatible);
		ShowDialog(dialog_GenericTwoButtonWithTiles);
	}

	public void ShowRenameAllSignalsDialog(string signalType, string oldSignalName, string newSignalName, Action completion)
	{
		string mainText = "\nRename all " + signalType + " signals named '" + oldSignalName + "' to '" + newSignalName + "'?";
		ShowGenericDialog(mainText, "No", "Yes", null, completion);
	}

	public void ShowCustomNameEditor(Action completion, Action<string> textInputAction, string visibleType)
	{
		Dialog_TextInput dialog_TextInput = dialogs[DialogTypeEnum.CustomSignalName] as Dialog_TextInput;
		dialog_TextInput.Setup(completion, textInputAction);
		dialog_TextInput.SetPromptText("Enter a name for this " + visibleType + ":");
		ShowDialog(dialog_TextInput);
	}

	public void ShowStringParameterEditorDialog(Action completion, Action<string> textInputAction, string startText)
	{
		Dialog_TextInput dialog_TextInput = dialogs[DialogTypeEnum.StringParameterInput] as Dialog_TextInput;
		dialog_TextInput.Setup(completion, textInputAction);
		ShowDialog(dialog_TextInput);
		dialog_TextInput.SetText(startText);
		dialog_TextInput.FocusOnInputField();
	}

	public void ShowWorldIdParameterEditorDialog(Action completion, Action<string> textInputAction, string startText)
	{
		Dialog_WorldIdEntry dialog_WorldIdEntry = dialogs[DialogTypeEnum.WorldIdEntry] as Dialog_WorldIdEntry;
		dialog_WorldIdEntry.Setup(completion, textInputAction);
		dialog_WorldIdEntry.ClearWorldInfo();
		ShowDialog(dialog_WorldIdEntry);
		dialog_WorldIdEntry.SetText(startText);
		dialog_WorldIdEntry.FocusOnInputField();
	}

	public void ShowCurrentUserWorldList(Action<string> completion, string selectedWorldId)
	{
		Dialog_CurrentUserWorldList dialog = GetDialog<Dialog_CurrentUserWorldList>(DialogTypeEnum.CurrentUserWorldList);
		ShowDialog(dialog);
		dialog.Setup(completion, selectedWorldId);
	}

	public void ShowMaximumModelsDialog()
	{
		Dialog_Generic dialog = dialogs[DialogTypeEnum.MaximumModels] as Dialog_Generic;
		ShowDialog(dialog);
	}

	public void ShowGoToStoreConfirmationPrompt()
	{
		Dialog_Generic dialog_Generic = dialogs[DialogTypeEnum.GoToStoreConfirmationPrompt] as Dialog_Generic;
		TabBarTabId tab = Blocksworld.buildPanel.GetTabBar().SelectedTab;
		dialog_Generic.yesAction = delegate
		{
			WorldSession.OpenStoreWithBlockItemId(tab, 0);
		};
		ShowDialog(dialog_Generic);
	}

	public void ShowGoToShoppingCartConfirmationPrompt()
	{
		ShowGenericDialog("Exit world and go to Shopping Cart?", "No", "Yes", CloseActiveDialog, delegate
		{
			WorldSession.Save();
			WorldSession.QuitWithDeepLink("shopping_cart");
		});
	}

	public void ShowEscapeMenuForCommunityWorld()
	{
		UIPopup escapeMenuInWorldPopupPrefab = BWStandalone.Overlays.escapeMenuInWorldPopupPrefab;
		ShowMenuPopup(escapeMenuInWorldPopupPrefab);
	}

	public void ShowEscapeMenuForLocalWorldPlayMode()
	{
		UIPopup escapeMenuInWorldPopupPrefab = BWStandalone.Overlays.escapeMenuInWorldPopupPrefab;
		ShowMenuPopup(escapeMenuInWorldPopupPrefab);
	}

	public void ShowEscapeMenuForLocalWorldBuildMode()
	{
		UIPopup escapeMenuInWorldPopupPrefab = BWStandalone.Overlays.escapeMenuInWorldPopupPrefab;
		ShowMenuPopup(escapeMenuInWorldPopupPrefab);
	}

	public void ShowProfilePictureConfirmation(byte[] imageData)
	{
		Dialog_ImageData dialog = GetDialog<Dialog_ImageData>(DialogTypeEnum.ProfilePictureConfirmation);
		dialog.LoadImageFromData(imageData);
		dialog.yesAction = delegate
		{
			BWStandalone.Instance.SetCurrentUserProfilePicture(imageData);
		};
		ShowDialog(dialog);
	}

	private T GetDialog<T>(DialogTypeEnum dialogType) where T : UIDialogPanel
	{
		if (smallScreenDialogs.TryGetValue(dialogType, out var value) && Screen.height < value.smallScreenMaxHeight)
		{
			return (T)value;
		}
		return (T)dialogs[dialogType];
	}

	private void ShowDialog(UIDialogPanel dialog)
	{
		unpauseOnDismiss = Blocksworld.CurrentState != State.Paused && Blocksworld.CurrentState != State.WaitForOption;
		unpauseOnDismiss &= !Blocksworld.UI.Leaderboard.IsVisible();
		CloseActiveDialog();
		canvas.enabled = true;
		if (unpauseOnDismiss)
		{
			Blocksworld.lockInput = true;
			if (WorldSession.current != null)
			{
				WorldSession.current.Pause();
			}
		}
		State blocksworldState = ((!dialog.isParameterEditor) ? State.WaitForOption : State.EditTile);
		Blocksworld.SetBlocksworldState(blocksworldState);
		screenDimmer.SetActive(value: true);
		dialog.Show();
		activeDialog = dialog;
	}

	private UIPopup ShowMenuPopup(UIPopup popupPrefab)
	{
		if (BWStandalone.Instance == null)
		{
			return null;
		}
		unpauseOnDismiss = Blocksworld.CurrentState != State.Paused && Blocksworld.CurrentState != State.WaitForOption;
		if (unpauseOnDismiss)
		{
			Blocksworld.lockInput = true;
			if (WorldSession.current != null)
			{
				WorldSession.current.Pause();
			}
		}
		State blocksworldState = State.WaitForOption;
		Blocksworld.SetBlocksworldState(blocksworldState);
		return BWStandalone.Overlays.ShowPopup(popupPrefab, this);
	}

	public void ClosePopup()
	{
		BWStandalone.Overlays.ClosePopup();
		if (WorldSession.current != null)
		{
			Blocksworld.SetBlocksworldState(State.Paused);
			if (unpauseOnDismiss)
			{
				Blocksworld.lockInput = false;
				WorldSession.current.Unpause();
			}
		}
	}

	public void CloseActiveDialog()
	{
		if (activeDialog == null)
		{
			return;
		}
		activeDialog.Hide();
		screenDimmer.SetActive(value: false);
		activeDialog = null;
		Blocksworld.SetBlocksworldState(State.Paused);
		if (unpauseOnDismiss)
		{
			Blocksworld.lockInput = false;
			if (WorldSession.current != null)
			{
				WorldSession.current.Unpause();
			}
		}
		canvas.enabled = false;
	}
}
