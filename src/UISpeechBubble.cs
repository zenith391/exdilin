using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISpeechBubble : MonoBehaviour
{
	public Text text;

	public Image bubble;

	public RectTransform buttonParent;

	private float _defaultWidth = 400f;

	private float _width;

	public bool isConnected;

	public string rawText;

	public int tailDirection;

	private string _currentTextStr;

	private bool _buttonPressed;

	private bool _firstTime;

	private List<UIButton> _buttons = new List<UIButton>();

	private List<bool> _buttonPressedStates = new List<bool>();

	private List<string> _buttonCommands = new List<string>();

	private const float screenBorder = 10f;

	private Vector2 _avoidOffset;

	private Vector2 _avoidOffsetTarget;

	private RectTransform bubbleTransform;

	private float bubbleWidth => bubble.rectTransform.sizeDelta.x;

	private float bubbleHeight => bubble.rectTransform.sizeDelta.y;

	public void Init()
	{
		bubbleTransform = GetComponent<RectTransform>();
		_defaultWidth = bubble.rectTransform.sizeDelta.x;
		_width = _defaultWidth;
		UpdateScale();
		_firstTime = true;
	}

	public void Reset()
	{
		for (int i = 0; i < _buttons.Count; i++)
		{
			Object.Destroy(_buttons[i].gameObject);
		}
		_width = _defaultWidth;
		_buttonPressed = false;
		_buttonPressedStates.Clear();
		_buttonCommands.Clear();
		_buttons.Clear();
		_firstTime = true;
	}

	public void SetText(string str)
	{
		if (!(str == _currentTextStr))
		{
			_currentTextStr = str;
			text.text = str;
		}
	}

	public void AddButton(UIButton button, string buttonName, string buttonCommand)
	{
		button.Init();
		button.SetText(buttonName);
		button.transform.SetParent(buttonParent, worldPositionStays: false);
		button.Show();
		_buttons.Add(button);
		_buttonPressedStates.Add(item: false);
		_buttonCommands.Add(buttonCommand);
		int buttonIndex = _buttonPressedStates.Count - 1;
		button.clickAction = delegate
		{
			ButtonPressedCallback(buttonIndex);
		};
	}

	public void SetWidth(float width)
	{
		_width = width * NormalizedScreen.scale;
	}

	public void Layout()
	{
		float y = text.rectTransform.anchoredPosition.y;
		float num = -2f * y;
		float num2 = 0f;
		if (HasButtons())
		{
			num += buttonParent.sizeDelta.y;
			for (int i = 0; i < _buttons.Count; i++)
			{
				num2 += _buttons[i].PreferredWidth - _buttons[i].DefaultWidth;
			}
			buttonParent.sizeDelta = new Vector2(_width + num2, buttonParent.sizeDelta.y);
		}
		bubble.rectTransform.sizeDelta = new Vector2(_width + num2, text.preferredHeight + num);
	}

	private void ButtonPressedCallback(int index)
	{
		if (index < _buttonPressedStates.Count)
		{
			_buttonPressedStates[index] = true;
		}
	}

	private void Update()
	{
		if (MappedInput.InputDown(MappableInput.MENU_SUBMIT) && _buttonPressedStates.Count > 0)
		{
			_buttonPressedStates[0] = true;
		}
	}

	public bool ButtonPressed()
	{
		return _buttonPressed;
	}

	public bool HasButtons()
	{
		return _buttons.Count > 0;
	}

	public void UpdatePosition(Vector2 screenPos)
	{
		bubbleTransform.anchoredPosition = screenPos * NormalizedScreen.pixelScale;
	}

	public void UpdatePosition(Vector3 speakerPos)
	{
		base.transform.position = speakerPos;
		base.transform.localScale = 0.01f * Vector3.one;
		base.transform.forward = (speakerPos - Blocksworld.mainCamera.transform.position).normalized;
	}

	public void UpdatePosition(Vector3 speakerScreenPos, List<Rect> protectedRects)
	{
		float minX = 10f * NormalizedScreen.widthScaleRatio;
		float maxX = (float)NormalizedScreen.width - 10f * NormalizedScreen.widthScaleRatio;
		float minY = 10f * NormalizedScreen.heightScaleRatio;
		float maxY = (float)NormalizedScreen.height - 10f * NormalizedScreen.heightScaleRatio;
		float x = speakerScreenPos.x;
		float y = speakerScreenPos.y;
		if (_firstTime)
		{
			bubbleTransform.anchoredPosition = new Vector2(x, y);
			_firstTime = false;
		}
		else
		{
			AvoidRects(protectedRects, minX, maxX, minY, maxY);
			bubbleTransform.anchoredPosition = new Vector2(x + _avoidOffset.x, y + _avoidOffset.y);
		}
	}

	public string ProcessButtons()
	{
		string result = null;
		for (int i = 0; i < _buttonPressedStates.Count; i++)
		{
			if (_buttonPressedStates[i])
			{
				result = _buttonCommands[i];
				_buttonPressed = true;
			}
			_buttonPressedStates[i] = false;
		}
		return result;
	}

	public void UpdateScale()
	{
		bubbleTransform.localScale = Vector3.one * NormalizedScreen.pixelScale;
		_avoidOffset = (_avoidOffsetTarget = Vector2.zero);
	}

	public Rect GetScreenRect()
	{
		float num = bubbleTransform.anchoredPosition.x + bubble.rectTransform.anchoredPosition.x * NormalizedScreen.pixelScale;
		float num2 = bubbleTransform.anchoredPosition.y + bubble.rectTransform.anchoredPosition.y * NormalizedScreen.pixelScale;
		float num3 = bubble.rectTransform.rect.width * NormalizedScreen.pixelScale;
		float num4 = bubble.rectTransform.rect.height * NormalizedScreen.pixelScale;
		num /= NormalizedScreen.scale;
		num2 /= NormalizedScreen.scale;
		num3 /= NormalizedScreen.scale;
		num4 /= NormalizedScreen.scale;
		return new Rect(num, num2, num3, num4);
	}

	public Rect GetWorldRect()
	{
		return Util.GetWorldRectForRectTransform(bubble.rectTransform);
	}

	private void AvoidRects(List<Rect> protectedRects, float minX, float maxX, float minY, float maxY)
	{
		Rect worldRect = GetWorldRect();
		Rect screenRect = GetScreenRect();
		float num = 0.99f;
		for (int num2 = 0; num2 < Mathf.Min(protectedRects.Count); num2++)
		{
			Rect r = protectedRects[num2];
			if (r.Intersects(worldRect))
			{
				float num3 = Mathf.Max(worldRect.xMin, r.xMin);
				float num4 = Mathf.Min(worldRect.xMax, r.xMax);
				float num5 = Mathf.Max(worldRect.yMin, r.yMin);
				float num6 = Mathf.Min(worldRect.yMax, r.yMax);
				float num7 = (num4 - num3) * (num6 - num5);
				float num8 = Mathf.Min(worldRect.height * worldRect.width + r.height * r.width);
				float num9 = num7 / num8;
				Vector2 vector = worldRect.center - r.center;
				float num10 = worldRect.width / worldRect.height + r.width / r.height;
				vector.x /= num10;
				if (vector.sqrMagnitude > 0.01f)
				{
					Vector2 vector2 = vector.normalized * 60f * num9;
					_avoidOffsetTarget += vector2;
				}
			}
		}
		float num11 = 0.5f;
		if (screenRect.y < minY)
		{
			_avoidOffsetTarget.y += (minY - screenRect.y) * num11;
		}
		if (screenRect.x < minX)
		{
			_avoidOffsetTarget.x += (minX - screenRect.x) * num11;
		}
		if (screenRect.yMax > maxY)
		{
			_avoidOffsetTarget.y += (maxY - screenRect.yMax) * num11;
		}
		if (screenRect.xMax > maxX)
		{
			_avoidOffsetTarget.x += (maxX - screenRect.xMax) * num11;
		}
		_avoidOffsetTarget *= num;
		_avoidOffset = _avoidOffset * 0.9f + _avoidOffsetTarget * 0.1f;
	}
}
