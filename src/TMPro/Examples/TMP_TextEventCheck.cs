using System;
using UnityEngine;
using UnityEngine.Events;

namespace TMPro.Examples
{
	// Token: 0x02000383 RID: 899
	public class TMP_TextEventCheck : MonoBehaviour
	{
		// Token: 0x060027C2 RID: 10178 RVA: 0x001240A0 File Offset: 0x001224A0
		private void OnEnable()
		{
			if (this.TextEventHandler != null)
			{
				this.TextEventHandler.onCharacterSelection.AddListener(new UnityAction<char, int>(this.OnCharacterSelection));
				this.TextEventHandler.onWordSelection.AddListener(new UnityAction<string, int, int>(this.OnWordSelection));
				this.TextEventHandler.onLineSelection.AddListener(new UnityAction<string, int, int>(this.OnLineSelection));
				this.TextEventHandler.onLinkSelection.AddListener(new UnityAction<string, string, int>(this.OnLinkSelection));
			}
		}

		// Token: 0x060027C3 RID: 10179 RVA: 0x00124130 File Offset: 0x00122530
		private void OnDisable()
		{
			if (this.TextEventHandler != null)
			{
				this.TextEventHandler.onCharacterSelection.RemoveListener(new UnityAction<char, int>(this.OnCharacterSelection));
				this.TextEventHandler.onWordSelection.RemoveListener(new UnityAction<string, int, int>(this.OnWordSelection));
				this.TextEventHandler.onLineSelection.RemoveListener(new UnityAction<string, int, int>(this.OnLineSelection));
				this.TextEventHandler.onLinkSelection.RemoveListener(new UnityAction<string, string, int>(this.OnLinkSelection));
			}
		}

		// Token: 0x060027C4 RID: 10180 RVA: 0x001241BE File Offset: 0x001225BE
		private void OnCharacterSelection(char c, int index)
		{
			Debug.Log(string.Concat(new object[]
			{
				"Character [",
				c,
				"] at Index: ",
				index,
				" has been selected."
			}));
		}

		// Token: 0x060027C5 RID: 10181 RVA: 0x001241FC File Offset: 0x001225FC
		private void OnWordSelection(string word, int firstCharacterIndex, int length)
		{
			Debug.Log(string.Concat(new object[]
			{
				"Word [",
				word,
				"] with first character index of ",
				firstCharacterIndex,
				" and length of ",
				length,
				" has been selected."
			}));
		}

		// Token: 0x060027C6 RID: 10182 RVA: 0x00124250 File Offset: 0x00122650
		private void OnLineSelection(string lineText, int firstCharacterIndex, int length)
		{
			Debug.Log(string.Concat(new object[]
			{
				"Line [",
				lineText,
				"] with first character index of ",
				firstCharacterIndex,
				" and length of ",
				length,
				" has been selected."
			}));
		}

		// Token: 0x060027C7 RID: 10183 RVA: 0x001242A4 File Offset: 0x001226A4
		private void OnLinkSelection(string linkID, string linkText, int linkIndex)
		{
			Debug.Log(string.Concat(new object[]
			{
				"Link Index: ",
				linkIndex,
				" with ID [",
				linkID,
				"] and Text \"",
				linkText,
				"\" has been selected."
			}));
		}

		// Token: 0x040022CA RID: 8906
		public TMP_TextEventHandler TextEventHandler;
	}
}
