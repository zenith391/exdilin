using System;
using System.Collections;
using UnityEngine;

namespace TMPro.Examples
{
	// Token: 0x02000391 RID: 913
	public class TextConsoleSimulator : MonoBehaviour
	{
		// Token: 0x060027FA RID: 10234 RVA: 0x00126261 File Offset: 0x00124661
		private void Awake()
		{
			this.m_TextComponent = base.gameObject.GetComponent<TMP_Text>();
		}

		// Token: 0x060027FB RID: 10235 RVA: 0x00126274 File Offset: 0x00124674
		private void Start()
		{
			base.StartCoroutine(this.RevealCharacters(this.m_TextComponent));
		}

		// Token: 0x060027FC RID: 10236 RVA: 0x00126289 File Offset: 0x00124689
		private void OnEnable()
		{
			TMPro_EventManager.TEXT_CHANGED_EVENT.Add(new Action<UnityEngine.Object>(this.ON_TEXT_CHANGED));
		}

		// Token: 0x060027FD RID: 10237 RVA: 0x001262A1 File Offset: 0x001246A1
		private void OnDisable()
		{
			TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(new Action<UnityEngine.Object>(this.ON_TEXT_CHANGED));
		}

		// Token: 0x060027FE RID: 10238 RVA: 0x001262B9 File Offset: 0x001246B9
		private void ON_TEXT_CHANGED(UnityEngine.Object obj)
		{
			this.hasTextChanged = true;
		}

		// Token: 0x060027FF RID: 10239 RVA: 0x001262C4 File Offset: 0x001246C4
		private IEnumerator RevealCharacters(TMP_Text textComponent)
		{
			textComponent.ForceMeshUpdate();
			TMP_TextInfo textInfo = textComponent.textInfo;
			int totalVisibleCharacters = textInfo.characterCount;
			int visibleCount = 0;
			for (;;)
			{
				if (this.hasTextChanged)
				{
					totalVisibleCharacters = textInfo.characterCount;
					this.hasTextChanged = false;
				}
				if (visibleCount > totalVisibleCharacters)
				{
					yield return new WaitForSeconds(1f);
					visibleCount = 0;
				}
				textComponent.maxVisibleCharacters = visibleCount;
				visibleCount++;
				yield return null;
			}
			yield break;
		}

		// Token: 0x06002800 RID: 10240 RVA: 0x001262E8 File Offset: 0x001246E8
		private IEnumerator RevealWords(TMP_Text textComponent)
		{
			textComponent.ForceMeshUpdate();
			int totalWordCount = textComponent.textInfo.wordCount;
			int totalVisibleCharacters = textComponent.textInfo.characterCount;
			int counter = 0;
			int currentWord = 0;
			int visibleCount = 0;
			for (;;)
			{
				currentWord = counter % (totalWordCount + 1);
				if (currentWord == 0)
				{
					visibleCount = 0;
				}
				else if (currentWord < totalWordCount)
				{
					visibleCount = textComponent.textInfo.wordInfo[currentWord - 1].lastCharacterIndex + 1;
				}
				else if (currentWord == totalWordCount)
				{
					visibleCount = totalVisibleCharacters;
				}
				textComponent.maxVisibleCharacters = visibleCount;
				if (visibleCount >= totalVisibleCharacters)
				{
					yield return new WaitForSeconds(1f);
				}
				counter++;
				yield return new WaitForSeconds(0.1f);
			}
			yield break;
		}

		// Token: 0x0400230F RID: 8975
		private TMP_Text m_TextComponent;

		// Token: 0x04002310 RID: 8976
		private bool hasTextChanged;
	}
}
