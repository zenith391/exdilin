using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000377 RID: 887
public class ChatController : MonoBehaviour
{
	// Token: 0x060027A2 RID: 10146 RVA: 0x00122CB2 File Offset: 0x001210B2
	private void OnEnable()
	{
		this.TMP_ChatInput.onSubmit.AddListener(new UnityAction<string>(this.AddToChatOutput));
	}

	// Token: 0x060027A3 RID: 10147 RVA: 0x00122CD0 File Offset: 0x001210D0
	private void OnDisable()
	{
		this.TMP_ChatInput.onSubmit.RemoveListener(new UnityAction<string>(this.AddToChatOutput));
	}

	// Token: 0x060027A4 RID: 10148 RVA: 0x00122CF0 File Offset: 0x001210F0
	private void AddToChatOutput(string newText)
	{
		this.TMP_ChatInput.text = string.Empty;
		DateTime now = DateTime.Now;
		TMP_Text tmp_ChatOutput = this.TMP_ChatOutput;
		string text = tmp_ChatOutput.text;
		tmp_ChatOutput.text = string.Concat(new string[]
		{
			text,
			"[<#FFFF80>",
			now.Hour.ToString("d2"),
			":",
			now.Minute.ToString("d2"),
			":",
			now.Second.ToString("d2"),
			"</color>] ",
			newText,
			"\n"
		});
		this.TMP_ChatInput.ActivateInputField();
		this.ChatScrollbar.value = 0f;
	}

	// Token: 0x04002294 RID: 8852
	public TMP_InputField TMP_ChatInput;

	// Token: 0x04002295 RID: 8853
	public TMP_Text TMP_ChatOutput;

	// Token: 0x04002296 RID: 8854
	public Scrollbar ChatScrollbar;
}
