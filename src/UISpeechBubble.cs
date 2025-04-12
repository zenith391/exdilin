using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000319 RID: 793
public class UISpeechBubble : MonoBehaviour
{
	// Token: 0x060023DD RID: 9181 RVA: 0x00107788 File Offset: 0x00105B88
	public void Init()
	{
		this.bubbleTransform = base.GetComponent<RectTransform>();
		this._defaultWidth = this.bubble.rectTransform.sizeDelta.x;
		this._width = this._defaultWidth;
		this.UpdateScale();
		this._firstTime = true;
	}

	// Token: 0x060023DE RID: 9182 RVA: 0x001077D8 File Offset: 0x00105BD8
	public void Reset()
	{
		for (int i = 0; i < this._buttons.Count; i++)
		{
			UnityEngine.Object.Destroy(this._buttons[i].gameObject);
		}
		this._width = this._defaultWidth;
		this._buttonPressed = false;
		this._buttonPressedStates.Clear();
		this._buttonCommands.Clear();
		this._buttons.Clear();
		this._firstTime = true;
	}

	// Token: 0x060023DF RID: 9183 RVA: 0x00107852 File Offset: 0x00105C52
	public void SetText(string str)
	{
		if (str == this._currentTextStr)
		{
			return;
		}
		this._currentTextStr = str;
		this.text.text = str;
	}

	// Token: 0x060023E0 RID: 9184 RVA: 0x0010787C File Offset: 0x00105C7C
	public void AddButton(UIButton button, string buttonName, string buttonCommand)
	{
		button.Init(false);
		button.SetText(buttonName);
		button.transform.SetParent(this.buttonParent, false);
		button.Show();
		this._buttons.Add(button);
		this._buttonPressedStates.Add(false);
		this._buttonCommands.Add(buttonCommand);
		int buttonIndex = this._buttonPressedStates.Count - 1;
		button.clickAction = delegate()
		{
			this.ButtonPressedCallback(buttonIndex);
		};
	}

	// Token: 0x060023E1 RID: 9185 RVA: 0x00107905 File Offset: 0x00105D05
	public void SetWidth(float width)
	{
		this._width = width * NormalizedScreen.scale;
	}

	// Token: 0x060023E2 RID: 9186 RVA: 0x00107914 File Offset: 0x00105D14
	public void Layout()
	{
		float y = this.text.rectTransform.anchoredPosition.y;
		float num = -2f * y;
		float num2 = 0f;
		if (this.HasButtons())
		{
			num += this.buttonParent.sizeDelta.y;
			for (int i = 0; i < this._buttons.Count; i++)
			{
				num2 += this._buttons[i].PreferredWidth - this._buttons[i].DefaultWidth;
			}
			this.buttonParent.sizeDelta = new Vector2(this._width + num2, this.buttonParent.sizeDelta.y);
		}
		this.bubble.rectTransform.sizeDelta = new Vector2(this._width + num2, this.text.preferredHeight + num);
	}

	// Token: 0x060023E3 RID: 9187 RVA: 0x00107A08 File Offset: 0x00105E08
	private void ButtonPressedCallback(int index)
	{
		if (index < this._buttonPressedStates.Count)
		{
			this._buttonPressedStates[index] = true;
		}
	}

	// Token: 0x060023E4 RID: 9188 RVA: 0x00107A28 File Offset: 0x00105E28
	private void Update()
	{
		if (MappedInput.InputDown(MappableInput.MENU_SUBMIT) && this._buttonPressedStates.Count > 0)
		{
			this._buttonPressedStates[0] = true;
		}
	}

	// Token: 0x060023E5 RID: 9189 RVA: 0x00107A54 File Offset: 0x00105E54
	public bool ButtonPressed()
	{
		return this._buttonPressed;
	}

	// Token: 0x060023E6 RID: 9190 RVA: 0x00107A5C File Offset: 0x00105E5C
	public bool HasButtons()
	{
		return this._buttons.Count > 0;
	}

	// Token: 0x060023E7 RID: 9191 RVA: 0x00107A6C File Offset: 0x00105E6C
	public void UpdatePosition(Vector2 screenPos)
	{
		this.bubbleTransform.anchoredPosition = screenPos * NormalizedScreen.pixelScale;
	}

	// Token: 0x060023E8 RID: 9192 RVA: 0x00107A84 File Offset: 0x00105E84
	public void UpdatePosition(Vector3 speakerPos)
	{
		base.transform.position = speakerPos;
		base.transform.localScale = 0.01f * Vector3.one;
		base.transform.forward = (speakerPos - Blocksworld.mainCamera.transform.position).normalized;
	}

	// Token: 0x060023E9 RID: 9193 RVA: 0x00107AE0 File Offset: 0x00105EE0
	public void UpdatePosition(Vector3 speakerScreenPos, List<Rect> protectedRects)
	{
		float minX = 10f * NormalizedScreen.widthScaleRatio;
		float maxX = (float)NormalizedScreen.width - 10f * NormalizedScreen.widthScaleRatio;
		float minY = 10f * NormalizedScreen.heightScaleRatio;
		float maxY = (float)NormalizedScreen.height - 10f * NormalizedScreen.heightScaleRatio;
		float x = speakerScreenPos.x;
		float y = speakerScreenPos.y;
		if (this._firstTime)
		{
			this.bubbleTransform.anchoredPosition = new Vector2(x, y);
			this._firstTime = false;
		}
		else
		{
			this.AvoidRects(protectedRects, minX, maxX, minY, maxY);
			this.bubbleTransform.anchoredPosition = new Vector2(x + this._avoidOffset.x, y + this._avoidOffset.y);
		}
	}

	// Token: 0x060023EA RID: 9194 RVA: 0x00107BA0 File Offset: 0x00105FA0
	public string ProcessButtons()
	{
		string result = null;
		for (int i = 0; i < this._buttonPressedStates.Count; i++)
		{
			if (this._buttonPressedStates[i])
			{
				result = this._buttonCommands[i];
				this._buttonPressed = true;
			}
			this._buttonPressedStates[i] = false;
		}
		return result;
	}

	// Token: 0x060023EB RID: 9195 RVA: 0x00107C00 File Offset: 0x00106000
	public void UpdateScale()
	{
		this.bubbleTransform.localScale = Vector3.one * NormalizedScreen.pixelScale;
		this._avoidOffset = (this._avoidOffsetTarget = Vector2.zero);
	}

	// Token: 0x17000171 RID: 369
	// (get) Token: 0x060023EC RID: 9196 RVA: 0x00107C3C File Offset: 0x0010603C
	private float bubbleWidth
	{
		get
		{
			return this.bubble.rectTransform.sizeDelta.x;
		}
	}

	// Token: 0x17000172 RID: 370
	// (get) Token: 0x060023ED RID: 9197 RVA: 0x00107C64 File Offset: 0x00106064
	private float bubbleHeight
	{
		get
		{
			return this.bubble.rectTransform.sizeDelta.y;
		}
	}

	// Token: 0x060023EE RID: 9198 RVA: 0x00107C8C File Offset: 0x0010608C
	public Rect GetScreenRect()
	{
		float num = this.bubbleTransform.anchoredPosition.x + this.bubble.rectTransform.anchoredPosition.x * NormalizedScreen.pixelScale;
		float num2 = this.bubbleTransform.anchoredPosition.y + this.bubble.rectTransform.anchoredPosition.y * NormalizedScreen.pixelScale;
		float num3 = this.bubble.rectTransform.rect.width * NormalizedScreen.pixelScale;
		float num4 = this.bubble.rectTransform.rect.height * NormalizedScreen.pixelScale;
		num /= NormalizedScreen.scale;
		num2 /= NormalizedScreen.scale;
		num3 /= NormalizedScreen.scale;
		num4 /= NormalizedScreen.scale;
		return new Rect(num, num2, num3, num4);
	}

	// Token: 0x060023EF RID: 9199 RVA: 0x00107D72 File Offset: 0x00106172
	public Rect GetWorldRect()
	{
		return Util.GetWorldRectForRectTransform(this.bubble.rectTransform);
	}

	// Token: 0x060023F0 RID: 9200 RVA: 0x00107D84 File Offset: 0x00106184
	private void AvoidRects(List<Rect> protectedRects, float minX, float maxX, float minY, float maxY)
	{
		Rect worldRect = this.GetWorldRect();
		Rect screenRect = this.GetScreenRect();
		float d = 0.99f;
		for (int i = 0; i < Mathf.Min(new int[]
		{
			protectedRects.Count
		}); i++)
		{
			Rect r = protectedRects[i];
			if (r.Intersects(worldRect))
			{
				float num = Mathf.Max(worldRect.xMin, r.xMin);
				float num2 = Mathf.Min(worldRect.xMax, r.xMax);
				float num3 = Mathf.Max(worldRect.yMin, r.yMin);
				float num4 = Mathf.Min(worldRect.yMax, r.yMax);
				float num5 = (num2 - num) * (num4 - num3);
				float num6 = Mathf.Min(new float[]
				{
					worldRect.height * worldRect.width + r.height * r.width
				});
				float d2 = num5 / num6;
				Vector2 vector = worldRect.center - r.center;
				float num7 = worldRect.width / worldRect.height + r.width / r.height;
				vector.x /= num7;
				if (vector.sqrMagnitude > 0.01f)
				{
					Vector2 b = vector.normalized * 60f * d2;
					this._avoidOffsetTarget += b;
				}
			}
		}
		float num8 = 0.5f;
		if (screenRect.y < minY)
		{
			this._avoidOffsetTarget.y = this._avoidOffsetTarget.y + (minY - screenRect.y) * num8;
		}
		if (screenRect.x < minX)
		{
			this._avoidOffsetTarget.x = this._avoidOffsetTarget.x + (minX - screenRect.x) * num8;
		}
		if (screenRect.yMax > maxY)
		{
			this._avoidOffsetTarget.y = this._avoidOffsetTarget.y + (maxY - screenRect.yMax) * num8;
		}
		if (screenRect.xMax > maxX)
		{
			this._avoidOffsetTarget.x = this._avoidOffsetTarget.x + (maxX - screenRect.xMax) * num8;
		}
		this._avoidOffsetTarget *= d;
		this._avoidOffset = this._avoidOffset * 0.9f + this._avoidOffsetTarget * 0.1f;
	}

	// Token: 0x04001EF7 RID: 7927
	public Text text;

	// Token: 0x04001EF8 RID: 7928
	public Image bubble;

	// Token: 0x04001EF9 RID: 7929
	public RectTransform buttonParent;

	// Token: 0x04001EFA RID: 7930
	private float _defaultWidth = 400f;

	// Token: 0x04001EFB RID: 7931
	private float _width;

	// Token: 0x04001EFC RID: 7932
	public bool isConnected;

	// Token: 0x04001EFD RID: 7933
	public string rawText;

	// Token: 0x04001EFE RID: 7934
	public int tailDirection;

	// Token: 0x04001EFF RID: 7935
	private string _currentTextStr;

	// Token: 0x04001F00 RID: 7936
	private bool _buttonPressed;

	// Token: 0x04001F01 RID: 7937
	private bool _firstTime;

	// Token: 0x04001F02 RID: 7938
	private List<UIButton> _buttons = new List<UIButton>();

	// Token: 0x04001F03 RID: 7939
	private List<bool> _buttonPressedStates = new List<bool>();

	// Token: 0x04001F04 RID: 7940
	private List<string> _buttonCommands = new List<string>();

	// Token: 0x04001F05 RID: 7941
	private const float screenBorder = 10f;

	// Token: 0x04001F06 RID: 7942
	private Vector2 _avoidOffset;

	// Token: 0x04001F07 RID: 7943
	private Vector2 _avoidOffsetTarget;

	// Token: 0x04001F08 RID: 7944
	private RectTransform bubbleTransform;
}
