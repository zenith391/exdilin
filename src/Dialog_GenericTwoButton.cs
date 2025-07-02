using System;

public class Dialog_GenericTwoButton : UIDialogPanel
{
	public Action onButtonA;

	public Action onButtonB;

	public UIEditableText mainText;

	public UIEditableText buttonAText;

	public UIEditableText buttonBText;

	public override void Init()
	{
		mainText.Init();
		buttonAText.Init();
		buttonBText.Init();
	}

	public void Setup(string mainTextStr, string buttonATextStr, string buttonBTextStr, Action buttonAAction, Action buttonBAction)
	{
		mainText.Set(mainTextStr);
		buttonAText.Set(buttonATextStr);
		buttonBText.Set(buttonBTextStr);
		onButtonA = buttonAAction;
		onButtonB = buttonBAction;
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
