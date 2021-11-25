using System;
using System.Collections;
using UnityEngine.UI;

// Token: 0x020002F1 RID: 753
public class Dialog_TextInput : UIDialogPanel
{
	// Token: 0x06002227 RID: 8743 RVA: 0x000FEFFE File Offset: 0x000FD3FE
	protected override void OnShow()
	{
		this.inputField.text = string.Empty;
		if (this.autoFocus)
		{
			this.FocusOnInputField();
		}
		else
		{
			this.inputField.enabled = true;
			this.inputField.interactable = true;
		}
	}

	// Token: 0x06002228 RID: 8744 RVA: 0x000FF03E File Offset: 0x000FD43E
	private void HideCursor()
	{
		this.inputField.DeactivateInputField();
		this.inputField.enabled = false;
	}

	// Token: 0x06002229 RID: 8745 RVA: 0x000FF057 File Offset: 0x000FD457
	public void Setup(Action completionAction, Action<string> textAction)
	{
		this.completion = completionAction;
		this.textEditAction = textAction;
	}

	// Token: 0x0600222A RID: 8746 RVA: 0x000FF067 File Offset: 0x000FD467
	public void SetPromptText(string promptText)
	{
		if (this.message != null)
		{
			this.message.text = promptText;
			if (this.messageShadow != null)
			{
				this.messageShadow.text = promptText;
			}
		}
	}

	// Token: 0x0600222B RID: 8747 RVA: 0x000FF0A3 File Offset: 0x000FD4A3
	public void SetText(string text)
	{
		this.inputField.text = text;
	}

	// Token: 0x0600222C RID: 8748 RVA: 0x000FF0B4 File Offset: 0x000FD4B4
	public void FocusOnInputField()
	{
		this.inputField.enabled = true;
		string text = this.inputField.text;
		this.inputField.text = text;
		this.inputField.interactable = true;
		this.inputField.Select();
		this.inputField.ActivateInputField();
		this.inputField.caretPosition = text.Length;
	}

	// Token: 0x0600222D RID: 8749 RVA: 0x000FF118 File Offset: 0x000FD518
	public virtual void DidEditText(string text)
	{
		if (this.limitToTileWidth)
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
				this.inputField.text = text;
			}
		}
		if (this.textEditAction != null)
		{
			this.textEditAction(text);
		}
	}

	// Token: 0x0600222E RID: 8750 RVA: 0x000FF194 File Offset: 0x000FD594
	public void DidFinishEdit(string text)
	{
		base.StartCoroutine(this.CDidFinishEdit(text));
	}

	// Token: 0x0600222F RID: 8751 RVA: 0x000FF1A4 File Offset: 0x000FD5A4
	public void DidTapOk()
	{
		base.StartCoroutine(this.CDidFinishEdit(this.inputField.text));
	}

	// Token: 0x06002230 RID: 8752 RVA: 0x000FF1C0 File Offset: 0x000FD5C0
	private IEnumerator CDidFinishEdit(string text)
	{
		this.HideCursor();
		yield return null;
		yield return null;
		this.doCloseDialog();
		this.DidEditText(text);
		if (this.completion != null)
		{
			this.completion();
		}
		yield break;
	}

	// Token: 0x04001D2A RID: 7466
	public Text message;

	// Token: 0x04001D2B RID: 7467
	public Text messageShadow;

	// Token: 0x04001D2C RID: 7468
	public InputField inputField;

	// Token: 0x04001D2D RID: 7469
	public Action completion;

	// Token: 0x04001D2E RID: 7470
	public Action<string> textEditAction;

	// Token: 0x04001D2F RID: 7471
	public bool limitToTileWidth;

	// Token: 0x04001D30 RID: 7472
	public bool autoFocus = true;
}
