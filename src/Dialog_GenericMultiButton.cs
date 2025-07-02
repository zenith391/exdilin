using System;
using UnityEngine;

public class Dialog_GenericMultiButton : UIDialogPanel
{
	public UIEditableText[] buttonTexts;

	public GameObject[] buttonObjects;

	public UIEditableText mainText;

	public UITileScrollView tileScrollView;

	private Action[] _buttonActions;

	private int _buttonCount;

	public override void Init()
	{
		mainText.Init();
		UIEditableText[] array = buttonTexts;
		foreach (UIEditableText uIEditableText in array)
		{
			uIEditableText.Init();
		}
		if (tileScrollView != null)
		{
			tileScrollView.Init();
		}
	}

	protected override void OnHide()
	{
		if (tileScrollView != null)
		{
			tileScrollView.ClearTiles();
		}
	}

	public void Setup(string mainTextStr, string[] buttonLabels, Action[] buttonActions)
	{
		mainText.Set(mainTextStr);
		_buttonActions = buttonActions;
		if (buttonLabels.Length != buttonActions.Length)
		{
			BWLog.Error("argument mismatch");
		}
		if (buttonLabels.Length > buttonObjects.Length || buttonLabels.Length > buttonTexts.Length)
		{
			BWLog.Info("Too many buttons!");
		}
		_buttonCount = Mathf.Min(buttonLabels.Length, buttonObjects.Length);
		for (int i = 0; i < buttonObjects.Length; i++)
		{
			if (i < _buttonCount)
			{
				buttonObjects[i].SetActive(value: true);
				buttonTexts[i].Set(buttonLabels[i]);
			}
			else
			{
				buttonObjects[i].SetActive(value: false);
			}
		}
	}

	public void DidTapButton(int buttonID)
	{
		doCloseDialog();
		if (buttonID >= _buttonCount)
		{
			BWLog.Error("invalid button");
		}
		else
		{
			_buttonActions[buttonID]?.Invoke();
		}
	}
}
