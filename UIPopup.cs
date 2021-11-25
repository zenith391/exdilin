using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x0200044D RID: 1101
public class UIPopup : MonoBehaviour, IMenuInputHandler
{
	// Token: 0x06002EEE RID: 12014 RVA: 0x0014D2D3 File Offset: 0x0014B6D3
	public virtual void LoadData(UIDataSource dataSource, ImageManager imageManager, string itemID)
	{
		this.dataType = dataSource.dataType;
		this.dataSubtype = dataSource.dataSubtype;
		this.itemID = itemID;
		if (this.popupPanel != null)
		{
			this.popupPanel.SetupPanel(dataSource, imageManager, itemID);
		}
	}

	// Token: 0x06002EEF RID: 12015 RVA: 0x0014D313 File Offset: 0x0014B713
	public void Show(IPopupDelegate popupDelegate)
	{
		this.popupDelegate = popupDelegate;
		if (this.hideOnSceneLoad)
		{
			SceneManager.activeSceneChanged += this.OnSceneChange;
		}
		MenuInputHandler.RequestControl(this);
	}

	// Token: 0x06002EF0 RID: 12016 RVA: 0x0014D33E File Offset: 0x0014B73E
	public void Hide()
	{
		this.popupDelegate.ClosePopup();
		SceneManager.activeSceneChanged -= this.OnSceneChange;
		MenuInputHandler.Release(this);
	}

	// Token: 0x06002EF1 RID: 12017 RVA: 0x0014D362 File Offset: 0x0014B762
	public void OnButtonPressed(string message)
	{
		if (MainUIController.active)
		{
			MainUIController.Instance.HandleMessage(message, this.itemID, this.dataType, this.dataSubtype);
		}
	}

	// Token: 0x06002EF2 RID: 12018 RVA: 0x0014D38C File Offset: 0x0014B78C
	private void OnDestroy()
	{
		MenuInputHandler.Release(this);
	}

	// Token: 0x06002EF3 RID: 12019 RVA: 0x0014D394 File Offset: 0x0014B794
	private void OnSceneChange(Scene fromScene, Scene toScene)
	{
		this.Hide();
	}

	// Token: 0x06002EF4 RID: 12020 RVA: 0x0014D39C File Offset: 0x0014B79C
	public virtual void HandleMenuInputEvents()
	{
		if (MappedInput.InputDown(MappableInput.MENU_CANCEL))
		{
			this.Hide();
		}
	}

	// Token: 0x04002749 RID: 10057
	public UIPanelContents popupPanel;

	// Token: 0x0400274A RID: 10058
	public Text mainText;

	// Token: 0x0400274B RID: 10059
	public bool hideOnSceneLoad = true;

	// Token: 0x0400274C RID: 10060
	public bool closeOnBackgroundClick;

	// Token: 0x0400274D RID: 10061
	public Selectable defaultSelectable;

	// Token: 0x0400274E RID: 10062
	private string itemID;

	// Token: 0x0400274F RID: 10063
	private string dataType;

	// Token: 0x04002750 RID: 10064
	private string dataSubtype;

	// Token: 0x04002751 RID: 10065
	private IPopupDelegate popupDelegate;
}
