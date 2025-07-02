using System;
using System.Collections;
using UnityEngine.UI;

public class Dialog_TextInput : UIDialogPanel
{
	public Text message;

	public Text messageShadow;

	public InputField inputField;

	public Action completion;

	public Action<string> textEditAction;

	public bool limitToTileWidth;

	public bool autoFocus = true;

	protected override void OnShow()
	{
		inputField.text = string.Empty;
		if (autoFocus)
		{
			FocusOnInputField();
			return;
		}
		inputField.enabled = true;
		inputField.interactable = true;
	}

	private void HideCursor()
	{
		inputField.DeactivateInputField();
		inputField.enabled = false;
	}

	public void Setup(Action completionAction, Action<string> textAction)
	{
		completion = completionAction;
		textEditAction = textAction;
	}

	public void SetPromptText(string promptText)
	{
		if (message != null)
		{
			message.text = promptText;
			if (messageShadow != null)
			{
				messageShadow.text = promptText;
			}
		}
	}

	public void SetText(string text)
	{
		inputField.text = text;
	}

	public void FocusOnInputField()
	{
		inputField.enabled = true;
		string text = inputField.text;
		inputField.text = text;
		inputField.interactable = true;
		inputField.Select();
		inputField.ActivateInputField();
		inputField.caretPosition = text.Length;
	}

	public virtual void DidEditText(string text)
	{
		if (limitToTileWidth)
		{
			bool flag = ModelUtils.IsValidShortTitle(text);
			bool flag2 = !flag;
			while (!flag && text.Length > 1)
			{
				text = text.Substring(0, text.Length - 1);
				flag = ModelUtils.IsValidShortTitle(text);
			}
			if (flag2)
			{
				inputField.text = text;
			}
		}
		if (textEditAction != null)
		{
			textEditAction(text);
		}
	}

	public void DidFinishEdit(string text)
	{
		StartCoroutine(CDidFinishEdit(text));
	}

	public void DidTapOk()
	{
		StartCoroutine(CDidFinishEdit(inputField.text));
	}

	private IEnumerator CDidFinishEdit(string text)
	{
		HideCursor();
		yield return null;
		yield return null;
		doCloseDialog();
		DidEditText(text);
		if (completion != null)
		{
			completion();
		}
	}
}
