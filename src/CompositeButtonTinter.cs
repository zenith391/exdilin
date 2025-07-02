using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CompositeButtonTinter : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	private enum ButtonState
	{
		Normal,
		Highlighted,
		Pressed,
		Ghosted
	}

	public Color normalTint = Color.white;

	public Color highlightTint = new Color(0.9f, 0.9f, 0.9f, 1f);

	public Color pressedTint = new Color(0.7f, 0.7f, 0.8f, 1f);

	public Color ghostedTint = new Color(1f, 1f, 1f, 0.5f);

	private bool _pointerOver;

	private ButtonState _currentState;

	private List<Image> _images = new List<Image>();

	private List<Text> _texts = new List<Text>();

	private Button _button;

	private bool Pressed => _currentState == ButtonState.Pressed;

	private bool Ghosted => _currentState == ButtonState.Ghosted;

	private void Awake()
	{
		for (int i = 0; i != base.transform.GetChildCount(); i++)
		{
			_images.AddRange(base.transform.GetChild(i).GetComponentsInChildren<Image>());
			_texts.AddRange(base.transform.GetChild(i).GetComponentsInChildren<Text>());
		}
		_button = GetComponent<Button>();
		if (_button != null)
		{
			_button.onClick.AddListener(OnButtonClick);
		}
		EnterState(ButtonState.Normal);
	}

	private void EnterState(ButtonState state)
	{
		switch (state)
		{
		case ButtonState.Normal:
			SetChildrenTint(normalTint);
			break;
		case ButtonState.Highlighted:
			SetChildrenTint(highlightTint);
			break;
		case ButtonState.Pressed:
			SetChildrenTint(pressedTint);
			break;
		case ButtonState.Ghosted:
			SetChildrenTint(ghostedTint);
			break;
		}
	}

	private void SetChildrenTint(Color color)
	{
		for (int i = 0; i != _images.Count; i++)
		{
			_images[i].color = color;
		}
		for (int j = 0; j != _texts.Count; j++)
		{
			_texts[j].color = color;
		}
	}

	public void SetGhosted(bool status)
	{
		if (status)
		{
			Debug.Log("Ghost!");
			EnterState(ButtonState.Ghosted);
		}
		else
		{
			EnterState(ButtonState.Normal);
		}
	}

	public void Reset()
	{
		EnterState(ButtonState.Normal);
		_pointerOver = false;
	}

	public void OnDisable()
	{
		Reset();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!Ghosted && !Pressed)
		{
			EnterState(ButtonState.Pressed);
		}
	}

	private void OnButtonClick()
	{
		EnterState(ButtonState.Normal);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!Ghosted && Pressed)
		{
			if (_pointerOver)
			{
				EnterState(ButtonState.Highlighted);
			}
			else
			{
				EnterState(ButtonState.Normal);
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_pointerOver = true;
		if (!Pressed && !Ghosted)
		{
			EnterState(ButtonState.Highlighted);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!Ghosted)
		{
			EnterState(ButtonState.Normal);
		}
		_pointerOver = false;
	}
}
