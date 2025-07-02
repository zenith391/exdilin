using System;

public class Dialog_Generic : UIDialogPanel
{
	public Action yesAction;

	public Action noAction;

	public void DidTapNo()
	{
		doCloseDialog();
		if (noAction != null)
		{
			noAction();
		}
	}

	public void DidTapYes()
	{
		doCloseDialog();
		if (yesAction != null)
		{
			yesAction();
		}
	}
}
