using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000420 RID: 1056
[RequireComponent(typeof(Button))]
public class UIPanelElementPopupSpawner : UIPanelElement
{
	// Token: 0x06002D9B RID: 11675 RVA: 0x00144FA4 File Offset: 0x001433A4
	private void OnEnable()
	{
		this.button = base.GetComponent<Button>();
		this.button.onClick.AddListener(new UnityAction(this.Spawn));
	}

	// Token: 0x06002D9C RID: 11676 RVA: 0x00144FCE File Offset: 0x001433CE
	private void OnDisable()
	{
		this.button.onClick.RemoveListener(new UnityAction(this.Spawn));
	}

	// Token: 0x06002D9D RID: 11677 RVA: 0x00144FEC File Offset: 0x001433EC
	private void Spawn()
	{
		this.popup = BWStandalone.Overlays.ShowPopup(this.prefab, null);
		if (this.parentPanel != null)
		{
			this.popup.LoadData(this.parentPanel.dataSource, this.imageManager, this.parentPanel.itemId);
		}
	}

	// Token: 0x04002626 RID: 9766
	public UIPopup prefab;

	// Token: 0x04002627 RID: 9767
	private Button button;

	// Token: 0x04002628 RID: 9768
	private UIPopup popup;

	// Token: 0x04002629 RID: 9769
	private Dictionary<string, string> dataForPopup;
}
