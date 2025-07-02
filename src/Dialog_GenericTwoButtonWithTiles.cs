using System;
using UnityEngine;

public class Dialog_GenericTwoButtonWithTiles : UIDialogPanel
{
	public Action onButtonA;

	public Action onButtonB;

	public UIEditableText mainText;

	public UIEditableText buttonAText;

	public UIEditableText buttonBText;

	public UITileScrollView tileScrollView;

	public GameObject buttonA;

	public GameObject buttonB;

	public override void Init()
	{
		mainText.Init();
		buttonAText.Init();
		buttonBText.Init();
		tileScrollView.Init();
	}

	protected override void OnHide()
	{
		tileScrollView.ClearTiles();
	}

	public void Setup(string mainTextStr, string buttonATextStr, string buttonBTextStr, Action buttonAAction, Action buttonBAction)
	{
		mainText.Set(mainTextStr);
		buttonA.SetActive(value: true);
		buttonAText.Set(buttonATextStr);
		buttonBText.Set(buttonBTextStr);
		onButtonA = buttonAAction;
		onButtonB = buttonBAction;
	}

	public void Setup(string mainTextStr, string buttonTextStr, Action buttonAction)
	{
		mainText.Set(mainTextStr);
		buttonA.SetActive(value: false);
		buttonBText.Set(buttonTextStr);
		onButtonB = buttonAction;
		onButtonA = null;
	}

	public void DidTapButtonA()
	{
		doCloseDialog();
		if (onButtonA != null)
		{
			onButtonA();
		}
	}

	public void DidTapButtonB()
	{
		doCloseDialog();
		if (onButtonB != null)
		{
			onButtonB();
		}
	}
}
