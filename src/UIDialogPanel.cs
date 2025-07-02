using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDialogPanel : MonoBehaviour
{
	public DialogTypeEnum dialogType;

	public Action doCloseDialog;

	public bool isParameterEditor;

	public bool isSmallScreenVersion;

	public int smallScreenMaxHeight;

	public Selectable defaultSelectable;

	private MappableInputMode initialInputMode;

	public virtual void Init()
	{
	}

	protected virtual void OnShow()
	{
	}

	protected virtual void OnHide()
	{
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		if (MappedInput.Enabled)
		{
			initialInputMode = MappedInput.inputMode;
			MappedInput.SetMode(MappableInputMode.Menu);
		}
		if (defaultSelectable != null)
		{
			defaultSelectable.Select();
		}
		OnShow();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		if (MappedInput.Enabled)
		{
			MappedInput.SetMode(initialInputMode);
		}
		EventSystem.current.SetSelectedGameObject(null);
		OnHide();
	}

	public void DidTapCancelButton()
	{
		doCloseDialog();
	}
}
