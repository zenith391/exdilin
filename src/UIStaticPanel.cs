using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000460 RID: 1120
[RequireComponent(typeof(UIPanelContents))]
public class UIStaticPanel : UISceneElement, PanelContentsDelegate
{
	// Token: 0x06002F5C RID: 12124 RVA: 0x0014F3C2 File Offset: 0x0014D7C2
	private void Awake()
	{
		this.Init();
	}

	// Token: 0x06002F5D RID: 12125 RVA: 0x0014F3CA File Offset: 0x0014D7CA
	public override void Init()
	{
		this.panelContents = base.GetComponent<UIPanelContents>();
		this.panelContents.Init();
		this.panelContents.panelContentsDelegate = this;
		this.contentLoaded = false;
	}

	// Token: 0x06002F5E RID: 12126 RVA: 0x0014F3F6 File Offset: 0x0014D7F6
	public void OnDisable()
	{
		if (this.dataSource != null)
		{
			this.dataSource.RemoveListener(new UIDataLoadedEventHandler(this.OnDataLoaded));
		}
	}

	// Token: 0x06002F5F RID: 12127 RVA: 0x0014F41A File Offset: 0x0014D81A
	protected override void LoadContentFromDataSource()
	{
		this.dataSource.AddListener(new UIDataLoadedEventHandler(this.OnDataLoaded));
		if (this.forceReloadData)
		{
			this.dataSource.ClearData();
		}
		this.dataSource.LoadIfNeeded();
	}

	// Token: 0x06002F60 RID: 12128 RVA: 0x0014F454 File Offset: 0x0014D854
	public override void UnloadContent()
	{
		this.dataSource.RemoveListener(new UIDataLoadedEventHandler(this.OnDataLoaded));
		this.Clear();
	}

	// Token: 0x06002F61 RID: 12129 RVA: 0x0014F473 File Offset: 0x0014D873
	private void Clear()
	{
		this.panelContents.Clear();
	}

	// Token: 0x06002F62 RID: 12130 RVA: 0x0014F480 File Offset: 0x0014D880
	public override void UnloadEditorExampleContent()
	{
		this.panelContents.Clear();
	}

	// Token: 0x06002F63 RID: 12131 RVA: 0x0014F48D File Offset: 0x0014D88D
	private void OnDataLoaded(List<string> modifiedKeys)
	{
		this.panelContents.Clear();
		this.contentLoaded = false;
		this.Layout();
	}

	// Token: 0x06002F64 RID: 12132 RVA: 0x0014F4A8 File Offset: 0x0014D8A8
	private void Layout()
	{
		if (this.contentLoaded)
		{
			return;
		}
		List<string> keys = this.dataSource.Keys;
		if (keys.Count > this.index)
		{
			this.id = keys[this.index];
			this.panelContents.SetupPanel(this.dataSource, this.imageManager, this.id);
		}
		this.contentLoaded = true;
	}

	// Token: 0x06002F65 RID: 12133 RVA: 0x0014F514 File Offset: 0x0014D914
	public void OnLayoutComplete(UIPanelContents panelContents)
	{
	}

	// Token: 0x06002F66 RID: 12134 RVA: 0x0014F516 File Offset: 0x0014D916
	public void OnCloseButtonPressed(UIPanelContents panelContents)
	{
	}

	// Token: 0x040027BD RID: 10173
	public int index;

	// Token: 0x040027BE RID: 10174
	private bool contentLoaded;

	// Token: 0x040027BF RID: 10175
	private UIPanelContents panelContents;

	// Token: 0x040027C0 RID: 10176
	private string id;
}
