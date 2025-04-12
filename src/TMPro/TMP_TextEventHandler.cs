using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TMPro
{
	// Token: 0x02000384 RID: 900
	public class TMP_TextEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
	{
		// Token: 0x17000186 RID: 390
		// (get) Token: 0x060027C9 RID: 10185 RVA: 0x0012434F File Offset: 0x0012274F
		// (set) Token: 0x060027CA RID: 10186 RVA: 0x00124357 File Offset: 0x00122757
		public TMP_TextEventHandler.CharacterSelectionEvent onCharacterSelection
		{
			get
			{
				return this.m_OnCharacterSelection;
			}
			set
			{
				this.m_OnCharacterSelection = value;
			}
		}

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x060027CB RID: 10187 RVA: 0x00124360 File Offset: 0x00122760
		// (set) Token: 0x060027CC RID: 10188 RVA: 0x00124368 File Offset: 0x00122768
		public TMP_TextEventHandler.WordSelectionEvent onWordSelection
		{
			get
			{
				return this.m_OnWordSelection;
			}
			set
			{
				this.m_OnWordSelection = value;
			}
		}

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x060027CD RID: 10189 RVA: 0x00124371 File Offset: 0x00122771
		// (set) Token: 0x060027CE RID: 10190 RVA: 0x00124379 File Offset: 0x00122779
		public TMP_TextEventHandler.LineSelectionEvent onLineSelection
		{
			get
			{
				return this.m_OnLineSelection;
			}
			set
			{
				this.m_OnLineSelection = value;
			}
		}

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x060027CF RID: 10191 RVA: 0x00124382 File Offset: 0x00122782
		// (set) Token: 0x060027D0 RID: 10192 RVA: 0x0012438A File Offset: 0x0012278A
		public TMP_TextEventHandler.LinkSelectionEvent onLinkSelection
		{
			get
			{
				return this.m_OnLinkSelection;
			}
			set
			{
				this.m_OnLinkSelection = value;
			}
		}

		// Token: 0x060027D1 RID: 10193 RVA: 0x00124394 File Offset: 0x00122794
		private void Awake()
		{
			this.m_TextComponent = base.gameObject.GetComponent<TMP_Text>();
			if (this.m_TextComponent.GetType() == typeof(TextMeshProUGUI))
			{
				this.m_Canvas = base.gameObject.GetComponentInParent<Canvas>();
				if (this.m_Canvas != null)
				{
					if (this.m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
					{
						this.m_Camera = null;
					}
					else
					{
						this.m_Camera = this.m_Canvas.worldCamera;
					}
				}
			}
			else
			{
				this.m_Camera = Camera.main;
			}
		}

		// Token: 0x060027D2 RID: 10194 RVA: 0x0012442C File Offset: 0x0012282C
		private void LateUpdate()
		{
			if (TMP_TextUtilities.IsIntersectingRectTransform(this.m_TextComponent.rectTransform, Input.mousePosition, this.m_Camera))
			{
				int num = TMP_TextUtilities.FindIntersectingCharacter(this.m_TextComponent, Input.mousePosition, this.m_Camera, true);
				if (num != -1 && num != this.m_lastCharIndex)
				{
					this.m_lastCharIndex = num;
					this.SendOnCharacterSelection(this.m_TextComponent.textInfo.characterInfo[num].character, num);
				}
				int num2 = TMP_TextUtilities.FindIntersectingWord(this.m_TextComponent, Input.mousePosition, this.m_Camera);
				if (num2 != -1 && num2 != this.m_lastWordIndex)
				{
					this.m_lastWordIndex = num2;
					TMP_WordInfo tmp_WordInfo = this.m_TextComponent.textInfo.wordInfo[num2];
					this.SendOnWordSelection(tmp_WordInfo.GetWord(), tmp_WordInfo.firstCharacterIndex, tmp_WordInfo.characterCount);
				}
				int num3 = TMP_TextUtilities.FindIntersectingLine(this.m_TextComponent, Input.mousePosition, this.m_Camera);
				if (num3 != -1 && num3 != this.m_lastLineIndex)
				{
					this.m_lastLineIndex = num3;
					TMP_LineInfo tmp_LineInfo = this.m_TextComponent.textInfo.lineInfo[num3];
					char[] array = new char[tmp_LineInfo.characterCount];
					int num4 = 0;
					while (num4 < tmp_LineInfo.characterCount && num4 < this.m_TextComponent.textInfo.characterInfo.Length)
					{
						array[num4] = this.m_TextComponent.textInfo.characterInfo[num4 + tmp_LineInfo.firstCharacterIndex].character;
						num4++;
					}
					string line = new string(array);
					this.SendOnLineSelection(line, tmp_LineInfo.firstCharacterIndex, tmp_LineInfo.characterCount);
				}
				int num5 = TMP_TextUtilities.FindIntersectingLink(this.m_TextComponent, Input.mousePosition, this.m_Camera);
				if (num5 != -1 && num5 != this.m_selectedLink)
				{
					this.m_selectedLink = num5;
					TMP_LinkInfo tmp_LinkInfo = this.m_TextComponent.textInfo.linkInfo[num5];
					this.SendOnLinkSelection(tmp_LinkInfo.GetLinkID(), tmp_LinkInfo.GetLinkText(), num5);
				}
			}
		}

		// Token: 0x060027D3 RID: 10195 RVA: 0x00124657 File Offset: 0x00122A57
		public void OnPointerEnter(PointerEventData eventData)
		{
		}

		// Token: 0x060027D4 RID: 10196 RVA: 0x00124659 File Offset: 0x00122A59
		public void OnPointerExit(PointerEventData eventData)
		{
		}

		// Token: 0x060027D5 RID: 10197 RVA: 0x0012465B File Offset: 0x00122A5B
		private void SendOnCharacterSelection(char character, int characterIndex)
		{
			if (this.onCharacterSelection != null)
			{
				this.onCharacterSelection.Invoke(character, characterIndex);
			}
		}

		// Token: 0x060027D6 RID: 10198 RVA: 0x00124675 File Offset: 0x00122A75
		private void SendOnWordSelection(string word, int charIndex, int length)
		{
			if (this.onWordSelection != null)
			{
				this.onWordSelection.Invoke(word, charIndex, length);
			}
		}

		// Token: 0x060027D7 RID: 10199 RVA: 0x00124690 File Offset: 0x00122A90
		private void SendOnLineSelection(string line, int charIndex, int length)
		{
			if (this.onLineSelection != null)
			{
				this.onLineSelection.Invoke(line, charIndex, length);
			}
		}

		// Token: 0x060027D8 RID: 10200 RVA: 0x001246AB File Offset: 0x00122AAB
		private void SendOnLinkSelection(string linkID, string linkText, int linkIndex)
		{
			if (this.onLinkSelection != null)
			{
				this.onLinkSelection.Invoke(linkID, linkText, linkIndex);
			}
		}

		// Token: 0x040022CB RID: 8907
		[SerializeField]
		private TMP_TextEventHandler.CharacterSelectionEvent m_OnCharacterSelection = new TMP_TextEventHandler.CharacterSelectionEvent();

		// Token: 0x040022CC RID: 8908
		[SerializeField]
		private TMP_TextEventHandler.WordSelectionEvent m_OnWordSelection = new TMP_TextEventHandler.WordSelectionEvent();

		// Token: 0x040022CD RID: 8909
		[SerializeField]
		private TMP_TextEventHandler.LineSelectionEvent m_OnLineSelection = new TMP_TextEventHandler.LineSelectionEvent();

		// Token: 0x040022CE RID: 8910
		[SerializeField]
		private TMP_TextEventHandler.LinkSelectionEvent m_OnLinkSelection = new TMP_TextEventHandler.LinkSelectionEvent();

		// Token: 0x040022CF RID: 8911
		private TMP_Text m_TextComponent;

		// Token: 0x040022D0 RID: 8912
		private Camera m_Camera;

		// Token: 0x040022D1 RID: 8913
		private Canvas m_Canvas;

		// Token: 0x040022D2 RID: 8914
		private int m_selectedLink = -1;

		// Token: 0x040022D3 RID: 8915
		private int m_lastCharIndex = -1;

		// Token: 0x040022D4 RID: 8916
		private int m_lastWordIndex = -1;

		// Token: 0x040022D5 RID: 8917
		private int m_lastLineIndex = -1;

		// Token: 0x02000385 RID: 901
		[Serializable]
		public class CharacterSelectionEvent : UnityEvent<char, int>
		{
		}

		// Token: 0x02000386 RID: 902
		[Serializable]
		public class WordSelectionEvent : UnityEvent<string, int, int>
		{
		}

		// Token: 0x02000387 RID: 903
		[Serializable]
		public class LineSelectionEvent : UnityEvent<string, int, int>
		{
		}

		// Token: 0x02000388 RID: 904
		[Serializable]
		public class LinkSelectionEvent : UnityEvent<string, string, int>
		{
		}
	}
}
