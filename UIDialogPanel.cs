using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020002FF RID: 767
public class UIDialogPanel : MonoBehaviour
{
	// Token: 0x060022A1 RID: 8865 RVA: 0x000FE725 File Offset: 0x000FCB25
	public virtual void Init()
	{
	}

	// Token: 0x060022A2 RID: 8866 RVA: 0x000FE727 File Offset: 0x000FCB27
	protected virtual void OnShow()
	{
	}

	// Token: 0x060022A3 RID: 8867 RVA: 0x000FE729 File Offset: 0x000FCB29
	protected virtual void OnHide()
	{
	}

	// Token: 0x060022A4 RID: 8868 RVA: 0x000FE72C File Offset: 0x000FCB2C
	public void Show()
	{
		base.gameObject.SetActive(true);
		if (MappedInput.Enabled)
		{
			this.initialInputMode = MappedInput.inputMode;
			MappedInput.SetMode(MappableInputMode.Menu);
		}
		if (this.defaultSelectable != null)
		{
			this.defaultSelectable.Select();
		}
		this.OnShow();
	}

	// Token: 0x060022A5 RID: 8869 RVA: 0x000FE782 File Offset: 0x000FCB82
	public void Hide()
	{
		base.gameObject.SetActive(false);
		if (MappedInput.Enabled)
		{
			MappedInput.SetMode(this.initialInputMode);
		}
		EventSystem.current.SetSelectedGameObject(null);
		this.OnHide();
	}

	// Token: 0x060022A6 RID: 8870 RVA: 0x000FE7B6 File Offset: 0x000FCBB6
	public void DidTapCancelButton()
	{
		this.doCloseDialog();
	}

	// Token: 0x04001DA1 RID: 7585
	public DialogTypeEnum dialogType;

	// Token: 0x04001DA2 RID: 7586
	public Action doCloseDialog;

	// Token: 0x04001DA3 RID: 7587
	public bool isParameterEditor;

	// Token: 0x04001DA4 RID: 7588
	public bool isSmallScreenVersion;

	// Token: 0x04001DA5 RID: 7589
	public int smallScreenMaxHeight;

	// Token: 0x04001DA6 RID: 7590
	public Selectable defaultSelectable;

	// Token: 0x04001DA7 RID: 7591
	private MappableInputMode initialInputMode;
}
