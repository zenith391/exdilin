using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000410 RID: 1040
public class UIPanelContents : MonoBehaviour
{
	// Token: 0x06002D46 RID: 11590 RVA: 0x00143324 File Offset: 0x00141724
	public void Init()
	{
		if (this._initComplete)
		{
			return;
		}
		UIPanelElement[] componentsInChildren = base.GetComponentsInChildren<UIPanelElement>();
		this._panelElements = new List<UIPanelElement>();
		foreach (UIPanelElement uipanelElement in componentsInChildren)
		{
			UIPanelContents componentInParent = uipanelElement.GetComponentInParent<UIPanelContents>();
			if (componentInParent == this)
			{
				this._panelElements.Add(uipanelElement);
				uipanelElement.Init(this);
				uipanelElement.Clear();
			}
		}
		this._animator = base.GetComponent<Animator>();
		this._initComplete = true;
	}

	// Token: 0x06002D47 RID: 11591 RVA: 0x001433AC File Offset: 0x001417AC
	public void SetupPanel(UIDataSource dataSource, ImageManager imageManager)
	{
		this.dataSource = dataSource;
		if (this._panelElements == null)
		{
			this.Init();
		}
		foreach (UIPanelElement uipanelElement in this._panelElements)
		{
			uipanelElement.SetImageManager(imageManager);
			uipanelElement.Clear();
		}
		this._layoutComplete = false;
		dataSource.AddListener(new UIDataLoadedEventHandler(this.OnDataSourceReloaded));
		this.Layout();
	}

	// Token: 0x06002D48 RID: 11592 RVA: 0x00143448 File Offset: 0x00141848
	public void SetupPanel(UIDataSource dataSource, ImageManager imageManager, string itemId)
	{
		if (this.dataSource == dataSource && this.itemId == itemId)
		{
			return;
		}
		this.dataSource = dataSource;
		if (this._panelElements == null)
		{
			this.Init();
		}
		foreach (UIPanelElement uipanelElement in this._panelElements)
		{
			uipanelElement.SetImageManager(imageManager);
			uipanelElement.Clear();
		}
		this.itemId = itemId;
		this._layoutComplete = false;
		if (this.requestFullData)
		{
			dataSource.RequestFullData(itemId);
		}
		dataSource.AddListener(new UIDataLoadedEventHandler(this.OnDataSourceReloaded));
		this.Layout();
	}

	// Token: 0x06002D49 RID: 11593 RVA: 0x00143518 File Offset: 0x00141918
	public void OnDataSourceReloaded(List<string> modifedKeys)
	{
		if (this.loadFromDataSourceInfo || modifedKeys.Contains(this.itemId))
		{
			this._layoutComplete = false;
			this.Layout();
		}
	}

	// Token: 0x06002D4A RID: 11594 RVA: 0x00143544 File Offset: 0x00141944
	public void Clear()
	{
		if (this._panelElements == null)
		{
			return;
		}
		foreach (UIPanelElement uipanelElement in this._panelElements)
		{
			if (!(uipanelElement == null))
			{
				uipanelElement.Clear();
			}
		}
		this.itemId = string.Empty;
		this._layoutComplete = false;
	}

	// Token: 0x06002D4B RID: 11595 RVA: 0x001435D0 File Offset: 0x001419D0
	public void ButtonPressed_Close()
	{
		if (this.panelContentsDelegate != null)
		{
			this.panelContentsDelegate.OnCloseButtonPressed(this);
		}
	}

	// Token: 0x06002D4C RID: 11596 RVA: 0x001435EC File Offset: 0x001419EC
	public void ButtonPressed_Play()
	{
		string playButtonMessage = this.dataSource.GetPlayButtonMessage();
		if (!string.IsNullOrEmpty(playButtonMessage) && MainUIController.active)
		{
			MainUIController.Instance.HandleMessage(playButtonMessage, this.itemId, this.dataSource.dataType, this.dataSource.dataSubtype);
		}
	}

	// Token: 0x06002D4D RID: 11597 RVA: 0x00143642 File Offset: 0x00141A42
	public void ButtonPressed(string message)
	{
		if (MainUIController.active)
		{
			MainUIController.Instance.HandleMessage(message, this.itemId, this.dataSource.dataType, this.dataSource.dataSubtype);
		}
	}

	// Token: 0x06002D4E RID: 11598 RVA: 0x00143676 File Offset: 0x00141A76
	public void ElementEditedText(string dataKey, string newValueStr)
	{
		this.dataSource.OverwriteData(this.itemId, dataKey, newValueStr);
	}

	// Token: 0x06002D4F RID: 11599 RVA: 0x0014368B File Offset: 0x00141A8B
	public void ElementEditedBool(string dataKey, bool newValue)
	{
		this.dataSource.OverwriteData(this.itemId, dataKey, (!newValue) ? "false" : "true");
	}

	// Token: 0x06002D50 RID: 11600 RVA: 0x001436B4 File Offset: 0x00141AB4
	public void ElementEditedFloat(string dataKey, float newValue)
	{
		this.dataSource.OverwriteData(this.itemId, dataKey, newValue.ToString());
	}

	// Token: 0x06002D51 RID: 11601 RVA: 0x001436D8 File Offset: 0x00141AD8
	private void Layout()
	{
		if (this._layoutComplete)
		{
			return;
		}
		if (!this.dataSource.IsDataLoaded())
		{
			return;
		}
		if (this.loadFromDataSourceInfo)
		{
			Dictionary<string, string> info = this.dataSource.Info;
			foreach (UIPanelElement uipanelElement in this._panelElements)
			{
				uipanelElement.Fill(info);
			}
			this._layoutComplete = true;
			this.debugLoadedData = new List<string>();
			foreach (KeyValuePair<string, string> keyValuePair in this.dataSource.Info)
			{
				this.debugLoadedData.Add(keyValuePair.Key + ": " + keyValuePair.Value);
			}
			if (this.panelContentsDelegate != null)
			{
				this.panelContentsDelegate.OnLayoutComplete(this);
			}
			return;
		}
		Dictionary<string, Dictionary<string, string>> data = this.dataSource.Data;
		if (!data.ContainsKey(this.itemId))
		{
			BWLog.Info("no data with id " + this.itemId);
			return;
		}
		Dictionary<string, string> data2 = data[this.itemId];
		foreach (UIPanelElement uipanelElement2 in this._panelElements)
		{
			uipanelElement2.Fill(data2);
		}
		UISceneElement[] componentsInChildren = base.GetComponentsInChildren<UISceneElement>();
		foreach (UISceneElement uisceneElement in componentsInChildren)
		{
			UIPanelContents componentInParent = uisceneElement.GetComponentInParent<UIPanelContents>();
			if (componentInParent == this && uisceneElement.getIDFromParentPanel)
			{
				uisceneElement.dataSubtype = this.itemId;
				if (MainUIController.active)
				{
					uisceneElement.LoadContent(MainUIController.Instance.dataManager, MainUIController.Instance.imageManager);
				}
			}
		}
		this._layoutComplete = true;
		if (this.panelContentsDelegate != null)
		{
			this.panelContentsDelegate.OnLayoutComplete(this);
		}
	}

	// Token: 0x06002D52 RID: 11602 RVA: 0x00143930 File Offset: 0x00141D30
	public void Show()
	{
		if (this._animator != null && this._animator.runtimeAnimatorController != null)
		{
			this._animator.SetBool("Visible", true);
		}
	}

	// Token: 0x06002D53 RID: 11603 RVA: 0x0014396A File Offset: 0x00141D6A
	public void Hide()
	{
		if (this._animator != null && this._animator.runtimeAnimatorController != null)
		{
			this._animator.SetBool("Visible", false);
		}
	}

	// Token: 0x06002D54 RID: 11604 RVA: 0x001439A4 File Offset: 0x00141DA4
	public void OnDestroy()
	{
		if (this.dataSource != null)
		{
			this.dataSource.RemoveListener(new UIDataLoadedEventHandler(this.OnDataSourceReloaded));
		}
	}

	// Token: 0x040025D7 RID: 9687
	public PanelContentsDelegate panelContentsDelegate;

	// Token: 0x040025D8 RID: 9688
	[HideInInspector]
	public string itemId;

	// Token: 0x040025D9 RID: 9689
	public UIDataSource dataSource;

	// Token: 0x040025DA RID: 9690
	public bool autoLoadDataSourceFromScene;

	// Token: 0x040025DB RID: 9691
	public bool requestFullData;

	// Token: 0x040025DC RID: 9692
	public bool loadFromDataSourceInfo;

	// Token: 0x040025DD RID: 9693
	public List<string> debugLoadedData;

	// Token: 0x040025DE RID: 9694
	private List<UIPanelElement> _panelElements;

	// Token: 0x040025DF RID: 9695
	private Animator _animator;

	// Token: 0x040025E0 RID: 9696
	private bool _showing;

	// Token: 0x040025E1 RID: 9697
	private bool _initComplete;

	// Token: 0x040025E2 RID: 9698
	private bool _layoutComplete;
}
