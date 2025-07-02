using UnityEngine.Events;
using UnityEngine.UI;

public class UIPopupConfirmation : UIPopup
{
	public Text titleText;

	public LayoutElement titleLayout;

	public Text yesText;

	public Text noText;

	public UnityAction yesAction;

	public void SetTitleText(string titleTextStr)
	{
		titleLayout.enabled = true;
		titleText.text = titleTextStr;
	}

	public void Confirm()
	{
		Hide();
		if (yesAction != null)
		{
			yesAction();
		}
	}
}
