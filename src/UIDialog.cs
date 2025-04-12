using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020002FD RID: 765
public class UIDialog : MonoBehaviour, IPopupDelegate
{
	// Token: 0x0600227D RID: 8829 RVA: 0x00101230 File Offset: 0x000FF630
	public void Init()
	{
		this.uiParentTranform = base.GetComponent<RectTransform>();
		this.dialogs = new Dictionary<DialogTypeEnum, UIDialogPanel>();
		this.smallScreenDialogs = new Dictionary<DialogTypeEnum, UIDialogPanel>();
		this.canvas = base.GetComponent<Canvas>();
		this.screenDimmer.SetActive(false);
		for (int i = 0; i < this.dialogPrefabs.Length; i++)
		{
			UIDialogPanel uidialogPanel = UnityEngine.Object.Instantiate<UIDialogPanel>(this.dialogPrefabs[i]);
			uidialogPanel.Init();
			uidialogPanel.doCloseDialog = delegate()
			{
				this.CloseActiveDialog();
			};
			if (uidialogPanel.isSmallScreenVersion)
			{
				this.smallScreenDialogs.Add(uidialogPanel.dialogType, uidialogPanel);
			}
			else
			{
				this.dialogs.Add(uidialogPanel.dialogType, uidialogPanel);
			}
			RectTransform component = uidialogPanel.GetComponent<RectTransform>();
			component.SetParent(this.uiParentTranform, false);
			component.SetAsLastSibling();
			uidialogPanel.gameObject.SetActive(false);
		}
		this.canvas.enabled = false;
	}

	// Token: 0x0600227E RID: 8830 RVA: 0x0010131C File Offset: 0x000FF71C
	public void ShowModelPurchaseConfirmation(string modelTitle, int modelPrice, Texture2D modelPreviewImage)
	{
		Dialog_ModelPurchaseConfirmation dialog_ModelPurchaseConfirmation = this.dialogs[DialogTypeEnum.ModelPurchaseConfirmation] as Dialog_ModelPurchaseConfirmation;
		dialog_ModelPurchaseConfirmation.SetPrice(modelPrice);
		dialog_ModelPurchaseConfirmation.SetImage(modelPreviewImage);
		this.ShowDialog(dialog_ModelPurchaseConfirmation);
	}

	// Token: 0x0600227F RID: 8831 RVA: 0x00101350 File Offset: 0x000FF750
	public void ShowModelSaveDialog(Texture2D modelImage, Action completion, Action<string> titleEditAction)
	{
		Dialog_SaveModel dialog = this.GetDialog<Dialog_SaveModel>(DialogTypeEnum.SaveModel);
		dialog.Setup(completion, titleEditAction);
		dialog.SetModelImage(modelImage);
		this.ShowDialog(dialog);
		dialog.SetText(string.Empty);
	}

	// Token: 0x06002280 RID: 8832 RVA: 0x00101388 File Offset: 0x000FF788
	public void ShowGenericDialog(string mainText, string buttonAText, string buttonBText, Action buttonAAction, Action buttonBAction)
	{
		Dialog_GenericTwoButton dialog_GenericTwoButton = this.dialogs[DialogTypeEnum.GenericTwoButton] as Dialog_GenericTwoButton;
		dialog_GenericTwoButton.Setup(mainText, buttonAText, buttonBText, buttonAAction, buttonBAction);
		this.ShowDialog(dialog_GenericTwoButton);
	}

	// Token: 0x06002281 RID: 8833 RVA: 0x001013BC File Offset: 0x000FF7BC
	public void ShowGenericDialog(string mainText, string[] buttonLabels, Action[] buttonActions)
	{
		Dialog_GenericMultiButton dialog_GenericMultiButton = this.dialogs[DialogTypeEnum.GenericMultiButton] as Dialog_GenericMultiButton;
		dialog_GenericMultiButton.Setup(mainText, buttonLabels, buttonActions);
		this.ShowDialog(dialog_GenericMultiButton);
	}

	// Token: 0x06002282 RID: 8834 RVA: 0x001013EC File Offset: 0x000FF7EC
	public void ShowUnsellableBlocksInModelWarning(HashSet<GAF> unsellableBlocks, Action completion)
	{
		Dialog_TwoButtonWithTilesExtraText dialog_TwoButtonWithTilesExtraText = this.dialogs[DialogTypeEnum.TwoButtonWithTilesAndExtraText] as Dialog_TwoButtonWithTilesExtraText;
		dialog_TwoButtonWithTilesExtraText.Setup("You can't sell a model with these blocks:", "No", "Yes", null, completion);
		dialog_TwoButtonWithTilesExtraText.tileScrollView.SetTiles(unsellableBlocks);
		this.ShowDialog(dialog_TwoButtonWithTilesExtraText);
	}

	// Token: 0x06002283 RID: 8835 RVA: 0x00101438 File Offset: 0x000FF838
	public void ShowSetPurchasePrompt(Dictionary<GAF, int> rewards, string dialogCaption, string setTitle, int setId, int setPrice)
	{
		Dialog_SetPurchasePrompt dialog_SetPurchasePrompt = this.dialogs[DialogTypeEnum.SetPurchasePrompt] as Dialog_SetPurchasePrompt;
		dialog_SetPurchasePrompt.Setup(dialogCaption, setTitle, setId, setPrice);
		dialog_SetPurchasePrompt.SetupRewardTiles(rewards);
		this.ShowDialog(dialog_SetPurchasePrompt);
	}

	// Token: 0x06002284 RID: 8836 RVA: 0x00101474 File Offset: 0x000FF874
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
		Action action = delegate()
		{
			WorldSession.OpenStoreWithBlockItemId(tab, blockItemId);
		};
		Action action2 = delegate()
		{
			BlocksInventory blocksInventory = new BlocksInventory();
			blocksInventory.Add(blockItemId, 1, 0);
			WorldSession.platformDelegate.AddItemsToCart(blocksInventory);
		};
		string[] buttonLabels = new string[]
		{
			"To the store!",
			"Add to cart",
			"Close"
		};
		Action[] array = new Action[3];
		array[0] = action;
		array[1] = action2;
		Action[] buttonActions = array;
		this.ShowGenericDialog(mainText, buttonLabels, buttonActions);
	}

	// Token: 0x06002285 RID: 8837 RVA: 0x00101534 File Offset: 0x000FF934
	public void ShowPasteFailInfo(Dictionary<GAF, int> missing, List<GAF> uniquesInModel, string caption)
	{
		Dialog_GenericMultiButton dialog_GenericMultiButton = this.dialogs[DialogTypeEnum.GenericMultiButtonWithTiles] as Dialog_GenericMultiButton;
		Action action = delegate()
		{
			if (Blocksworld.selectedBunch != null)
			{
				Blocksworld.selectedBunch.IgnoreRaycasts(false);
			}
			else if (Blocksworld.selectedBlock != null)
			{
				Blocksworld.selectedBlock.IgnoreRaycasts(false);
			}
			Blocksworld.Deselect(true, true);
			Blocksworld.tBoxGesture.Reset();
		};
		if (uniquesInModel.Count > 0)
		{
			string[] buttonLabels = new string[]
			{
				"Ok"
			};
			Action[] buttonActions = new Action[]
			{
				action
			};
			dialog_GenericMultiButton.Setup(caption, buttonLabels, buttonActions);
			dialog_GenericMultiButton.tileScrollView.SetTiles(uniquesInModel);
		}
		else
		{
			Action action2 = delegate()
			{
				BlocksInventory blocksInventory = new BlocksInventory();
				foreach (KeyValuePair<GAF, int> keyValuePair in missing)
				{
					int blockItemId = keyValuePair.Key.BlockItemId;
					if (blockItemId > 0)
					{
						Debug.Log(string.Concat(new object[]
						{
							"item id ",
							blockItemId,
							" missing ",
							keyValuePair.Value
						}));
						blocksInventory.Add(blockItemId, keyValuePair.Value, 0);
					}
				}
				WorldSession.platformDelegate.AddItemsToCart(blocksInventory);
				this.ShowGoToShoppingCartConfirmationPrompt();
			};
			string[] buttonLabels2 = new string[]
			{
				"Cancel",
				"Add to cart"
			};
			Action[] buttonActions2 = new Action[]
			{
				action,
				action2
			};
			dialog_GenericMultiButton.Setup(caption, buttonLabels2, buttonActions2);
			dialog_GenericMultiButton.tileScrollView.SetTilesAndQuantities(missing);
		}
		this.ShowDialog(dialog_GenericMultiButton);
	}

	// Token: 0x06002286 RID: 8838 RVA: 0x00101624 File Offset: 0x000FFA24
	public void ShowModelPasteConflictsInfo(HashSet<GAF> conflictingGafs, string caption)
	{
		Dialog_GenericTwoButtonWithTiles dialog_GenericTwoButtonWithTiles = this.dialogs[DialogTypeEnum.GenericTwoButtonWithTiles] as Dialog_GenericTwoButtonWithTiles;
		Action buttonAction = delegate()
		{
			if (Blocksworld.selectedBunch != null)
			{
				Blocksworld.selectedBunch.IgnoreRaycasts(false);
			}
			else if (Blocksworld.selectedBlock != null)
			{
				Blocksworld.selectedBlock.IgnoreRaycasts(false);
			}
			Blocksworld.Deselect(true, true);
			Blocksworld.tBoxGesture.Reset();
		};
		if (conflictingGafs.Count > 0)
		{
			dialog_GenericTwoButtonWithTiles.Setup(caption, "Ok", buttonAction);
			dialog_GenericTwoButtonWithTiles.tileScrollView.SetTiles(conflictingGafs);
		}
		this.ShowDialog(dialog_GenericTwoButtonWithTiles);
	}

	// Token: 0x06002287 RID: 8839 RVA: 0x00101690 File Offset: 0x000FFA90
	public void ShowScriptExistsDialog(Block block)
	{
		string[] buttonLabels = new string[]
		{
			"Replace",
			"Add",
			"Cancel"
		};
		string mainText = "This block contains a script.";
		Action[] array = new Action[3];
		array[0] = delegate()
		{
			Blocksworld.DoPasteScriptFromClipboard(block, true);
			History.AddStateIfNecessary();
		};
		array[1] = delegate()
		{
			Blocksworld.DoPasteScriptFromClipboard(block, false);
			History.AddStateIfNecessary();
		};
		array[2] = delegate()
		{
		};
		Action[] buttonActions = array;
		this.ShowGenericDialog(mainText, buttonLabels, buttonActions);
	}

	// Token: 0x06002288 RID: 8840 RVA: 0x00101720 File Offset: 0x000FFB20
	public void ShowPasteScriptIncompatibleDialog(Block block, List<List<Tile>> script, List<GAF> incompatible, bool replace = false)
	{
		string str = string.Empty;
		string str2 = ". Do you want to use the rest of the tiles?";
		bool flag = script.Count == 1 && script[0].Count == 1;
		if (flag)
		{
			str = "\nAll tiles are incompatible with the target block.";
			str2 = string.Empty;
		}
		else if (incompatible.Count == 1)
		{
			str = "\nOne tile is not compatible with the target block";
		}
		else
		{
			str = "\nSome tiles are not compatible with the target block";
		}
		string mainTextStr = str + str2;
		Dialog_GenericTwoButtonWithTiles dialog_GenericTwoButtonWithTiles = this.dialogs[DialogTypeEnum.GenericTwoButtonWithTiles] as Dialog_GenericTwoButtonWithTiles;
		dialog_GenericTwoButtonWithTiles.Setup(mainTextStr, "No", "Yes", delegate()
		{
		}, delegate()
		{
			Blocksworld.PasteScript(block, script, replace, true);
			History.AddStateIfNecessary();
		});
		dialog_GenericTwoButtonWithTiles.tileScrollView.SetTiles(incompatible);
		this.ShowDialog(dialog_GenericTwoButtonWithTiles);
	}

	// Token: 0x06002289 RID: 8841 RVA: 0x00101820 File Offset: 0x000FFC20
	public void ShowRenameAllSignalsDialog(string signalType, string oldSignalName, string newSignalName, Action completion)
	{
		string mainText = string.Concat(new string[]
		{
			"\nRename all ",
			signalType,
			" signals named '",
			oldSignalName,
			"' to '",
			newSignalName,
			"'?"
		});
		this.ShowGenericDialog(mainText, "No", "Yes", null, completion);
	}

	// Token: 0x0600228A RID: 8842 RVA: 0x0010187C File Offset: 0x000FFC7C
	public void ShowCustomNameEditor(Action completion, Action<string> textInputAction, string visibleType)
	{
		Dialog_TextInput dialog_TextInput = this.dialogs[DialogTypeEnum.CustomSignalName] as Dialog_TextInput;
		dialog_TextInput.Setup(completion, textInputAction);
		dialog_TextInput.SetPromptText("Enter a name for this " + visibleType + ":");
		this.ShowDialog(dialog_TextInput);
	}

	// Token: 0x0600228B RID: 8843 RVA: 0x001018C0 File Offset: 0x000FFCC0
	public void ShowStringParameterEditorDialog(Action completion, Action<string> textInputAction, string startText)
	{
		Dialog_TextInput dialog_TextInput = this.dialogs[DialogTypeEnum.StringParameterInput] as Dialog_TextInput;
		dialog_TextInput.Setup(completion, textInputAction);
		this.ShowDialog(dialog_TextInput);
		dialog_TextInput.SetText(startText);
		dialog_TextInput.FocusOnInputField();
	}

	// Token: 0x0600228C RID: 8844 RVA: 0x001018FC File Offset: 0x000FFCFC
	public void ShowWorldIdParameterEditorDialog(Action completion, Action<string> textInputAction, string startText)
	{
		Dialog_WorldIdEntry dialog_WorldIdEntry = this.dialogs[DialogTypeEnum.WorldIdEntry] as Dialog_WorldIdEntry;
		dialog_WorldIdEntry.Setup(completion, textInputAction);
		dialog_WorldIdEntry.ClearWorldInfo();
		this.ShowDialog(dialog_WorldIdEntry);
		dialog_WorldIdEntry.SetText(startText);
		dialog_WorldIdEntry.FocusOnInputField();
	}

	// Token: 0x0600228D RID: 8845 RVA: 0x00101940 File Offset: 0x000FFD40
	public void ShowCurrentUserWorldList(Action<string> completion, string selectedWorldId)
	{
		Dialog_CurrentUserWorldList dialog = this.GetDialog<Dialog_CurrentUserWorldList>(DialogTypeEnum.CurrentUserWorldList);
		this.ShowDialog(dialog);
		dialog.Setup(completion, selectedWorldId);
	}

	// Token: 0x0600228E RID: 8846 RVA: 0x00101968 File Offset: 0x000FFD68
	public void ShowMaximumModelsDialog()
	{
		Dialog_Generic dialog = this.dialogs[DialogTypeEnum.MaximumModels] as Dialog_Generic;
		this.ShowDialog(dialog);
	}

	// Token: 0x0600228F RID: 8847 RVA: 0x00101990 File Offset: 0x000FFD90
	public void ShowGoToStoreConfirmationPrompt()
	{
		Dialog_Generic dialog_Generic = this.dialogs[DialogTypeEnum.GoToStoreConfirmationPrompt] as Dialog_Generic;
		TabBarTabId tab = Blocksworld.buildPanel.GetTabBar().SelectedTab;
		dialog_Generic.yesAction = delegate()
		{
			WorldSession.OpenStoreWithBlockItemId(tab, 0);
		};
		this.ShowDialog(dialog_Generic);
	}

	// Token: 0x06002290 RID: 8848 RVA: 0x001019E4 File Offset: 0x000FFDE4
	public void ShowGoToShoppingCartConfirmationPrompt()
	{
		this.ShowGenericDialog("Exit world and go to Shopping Cart?", "No", "Yes", new Action(this.CloseActiveDialog), delegate()
		{
			WorldSession.Save();
			WorldSession.QuitWithDeepLink("shopping_cart");
		});
	}

	// Token: 0x06002291 RID: 8849 RVA: 0x00101A24 File Offset: 0x000FFE24
	public void ShowEscapeMenuForCommunityWorld()
	{
		UIPopup escapeMenuInWorldPopupPrefab = BWStandalone.Overlays.escapeMenuInWorldPopupPrefab;
		this.ShowMenuPopup(escapeMenuInWorldPopupPrefab);
	}

	// Token: 0x06002292 RID: 8850 RVA: 0x00101A44 File Offset: 0x000FFE44
	public void ShowEscapeMenuForLocalWorldPlayMode()
	{
		UIPopup escapeMenuInWorldPopupPrefab = BWStandalone.Overlays.escapeMenuInWorldPopupPrefab;
		this.ShowMenuPopup(escapeMenuInWorldPopupPrefab);
	}

	// Token: 0x06002293 RID: 8851 RVA: 0x00101A64 File Offset: 0x000FFE64
	public void ShowEscapeMenuForLocalWorldBuildMode()
	{
		UIPopup escapeMenuInWorldPopupPrefab = BWStandalone.Overlays.escapeMenuInWorldPopupPrefab;
		this.ShowMenuPopup(escapeMenuInWorldPopupPrefab);
	}

	// Token: 0x06002294 RID: 8852 RVA: 0x00101A84 File Offset: 0x000FFE84
	public void ShowProfilePictureConfirmation(byte[] imageData)
	{
		Dialog_ImageData dialog = this.GetDialog<Dialog_ImageData>(DialogTypeEnum.ProfilePictureConfirmation);
		dialog.LoadImageFromData(imageData);
		dialog.yesAction = delegate()
		{
			BWStandalone.Instance.SetCurrentUserProfilePicture(imageData);
		};
		this.ShowDialog(dialog);
	}

	// Token: 0x06002295 RID: 8853 RVA: 0x00101ACC File Offset: 0x000FFECC
	private T GetDialog<T>(DialogTypeEnum dialogType) where T : UIDialogPanel
	{
		UIDialogPanel uidialogPanel;
		if (this.smallScreenDialogs.TryGetValue(dialogType, out uidialogPanel) && Screen.height < uidialogPanel.smallScreenMaxHeight)
		{
			return (T)((object)uidialogPanel);
		}
		return (T)((object)this.dialogs[dialogType]);
	}

	// Token: 0x06002296 RID: 8854 RVA: 0x00101B14 File Offset: 0x000FFF14
	private void ShowDialog(UIDialogPanel dialog)
	{
		this.unpauseOnDismiss = (Blocksworld.CurrentState != State.Paused && Blocksworld.CurrentState != State.WaitForOption);
		this.unpauseOnDismiss &= !Blocksworld.UI.Leaderboard.IsVisible();
		this.CloseActiveDialog();
		this.canvas.enabled = true;
		if (this.unpauseOnDismiss)
		{
			Blocksworld.lockInput = true;
			if (WorldSession.current != null)
			{
				WorldSession.current.Pause();
			}
		}
		State blocksworldState = (!dialog.isParameterEditor) ? State.WaitForOption : State.EditTile;
		Blocksworld.SetBlocksworldState(blocksworldState);
		this.screenDimmer.SetActive(true);
		dialog.Show();
		this.activeDialog = dialog;
	}

	// Token: 0x06002297 RID: 8855 RVA: 0x00101BCC File Offset: 0x000FFFCC
	private UIPopup ShowMenuPopup(UIPopup popupPrefab)
	{
		if (BWStandalone.Instance == null)
		{
			return null;
		}
		this.unpauseOnDismiss = (Blocksworld.CurrentState != State.Paused && Blocksworld.CurrentState != State.WaitForOption);
		if (this.unpauseOnDismiss)
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

	// Token: 0x06002298 RID: 8856 RVA: 0x00101C44 File Offset: 0x00100044
	public void ClosePopup()
	{
		BWStandalone.Overlays.ClosePopup();
		if (WorldSession.current != null)
		{
			Blocksworld.SetBlocksworldState(State.Paused);
			if (this.unpauseOnDismiss)
			{
				Blocksworld.lockInput = false;
				WorldSession.current.Unpause();
			}
		}
	}

	// Token: 0x06002299 RID: 8857 RVA: 0x00101C7C File Offset: 0x0010007C
	public void CloseActiveDialog()
	{
		if (this.activeDialog == null)
		{
			return;
		}
		this.activeDialog.Hide();
		this.screenDimmer.SetActive(false);
		this.activeDialog = null;
		Blocksworld.SetBlocksworldState(State.Paused);
		if (this.unpauseOnDismiss)
		{
			Blocksworld.lockInput = false;
			if (WorldSession.current != null)
			{
				WorldSession.current.Unpause();
			}
		}
		this.canvas.enabled = false;
	}

	// Token: 0x04001D82 RID: 7554
	public GameObject screenDimmer;

	// Token: 0x04001D83 RID: 7555
	public UIDialogPanel[] dialogPrefabs;

	// Token: 0x04001D84 RID: 7556
	private Dictionary<DialogTypeEnum, UIDialogPanel> dialogs;

	// Token: 0x04001D85 RID: 7557
	private Dictionary<DialogTypeEnum, UIDialogPanel> smallScreenDialogs;

	// Token: 0x04001D86 RID: 7558
	private UIDialogPanel activeDialog;

	// Token: 0x04001D87 RID: 7559
	private RectTransform uiParentTranform;

	// Token: 0x04001D88 RID: 7560
	private Canvas canvas;

	// Token: 0x04001D89 RID: 7561
	private bool unpauseOnDismiss;
}
