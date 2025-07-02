using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInputControl : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public enum ControlType
	{
		Left,
		Right,
		Up,
		Down,
		L,
		R
	}

	public enum ControlVariant
	{
		Default,
		Action,
		Attack,
		Explode,
		Help,
		Jump,
		Laser,
		Missile,
		Mode,
		Speak,
		Speed
	}

	public static int controlTypeCount = 6;

	private static List<ControlType> _allButtonTypes = new List<ControlType>
	{
		ControlType.Left,
		ControlType.Right,
		ControlType.Up,
		ControlType.Down,
		ControlType.L,
		ControlType.R
	};

	private static List<ControlVariant> _allButtonVariants = new List<ControlVariant>
	{
		ControlVariant.Default,
		ControlVariant.Action,
		ControlVariant.Attack,
		ControlVariant.Explode,
		ControlVariant.Help,
		ControlVariant.Jump,
		ControlVariant.Laser,
		ControlVariant.Missile,
		ControlVariant.Mode,
		ControlVariant.Speak,
		ControlVariant.Speed
	};

	private static Dictionary<string, ControlType> _controlTypeFromString;

	private static Dictionary<string, ControlVariant> _controlVariantFromString;

	public ControlType controlType;

	public Image mainImage;

	public Sprite defaultSprite;

	public Sprite defaultPressedSprite;

	public GameObject keyImageObject;

	public bool useMappableInput;

	public MappableInput mappableInput;

	public Text mappedKeyIndicatorText;

	public int visibilityGroup;

	private bool _isPressed;

	private bool _isPressedByPointer;

	private bool _isPressedByKey;

	private bool _enabled;

	private Image _image;

	private Button _button;

	private Sprite _defaultSprite;

	private Sprite _defaultPressedSprite;

	private Image _keyImage;

	private Sprite _unpressedSprite;

	private Sprite _pressedSprite;

	private RectTransform _rectT;

	private CanvasGroup _canvasGroup;

	private List<Text> _mappedKeyIndicatorTexts;

	private bool _pointerControlEnabled = true;

	private int _pointerId = -1;

	public static List<ControlType> allButtonTypes => _allButtonTypes;

	public static List<ControlVariant> allButtonVariants => _allButtonVariants;

	public static Dictionary<string, ControlType> controlTypeFromString
	{
		get
		{
			if (_controlTypeFromString == null)
			{
				_controlTypeFromString = new Dictionary<string, ControlType>();
				for (int i = 0; i < allButtonTypes.Count; i++)
				{
					string text = allButtonTypes[i].ToString();
					_controlTypeFromString[text] = allButtonTypes[i];
					for (int j = 0; j < allButtonVariants.Count; j++)
					{
						string key = text + " " + allButtonVariants[j];
						_controlTypeFromString[key] = allButtonTypes[i];
					}
				}
			}
			return _controlTypeFromString;
		}
	}

	public static Dictionary<string, ControlVariant> controlVariantFromString
	{
		get
		{
			if (_controlVariantFromString == null)
			{
				_controlVariantFromString = new Dictionary<string, ControlVariant>();
				for (int i = 0; i < allButtonTypes.Count; i++)
				{
					string text = allButtonTypes[i].ToString();
					_controlVariantFromString[text] = ControlVariant.Default;
					for (int j = 0; j < allButtonVariants.Count; j++)
					{
						string key = text + " " + allButtonVariants[j];
						_controlVariantFromString[key] = allButtonVariants[j];
					}
				}
			}
			return _controlVariantFromString;
		}
	}

	public void Init()
	{
		if (mainImage != null)
		{
			_image = mainImage;
		}
		else
		{
			_image = GetComponent<Image>();
			if (_image == null && base.transform.childCount > 0)
			{
				_image = base.transform.GetChild(0).GetComponent<Image>();
			}
		}
		_button = GetComponent<Button>();
		_defaultSprite = defaultSprite;
		_defaultPressedSprite = defaultPressedSprite;
		if (keyImageObject != null)
		{
			_keyImage = keyImageObject.GetComponent<Image>();
			HideKeySprite();
		}
		_button.transition = Selectable.Transition.None;
		_canvasGroup = GetComponent<CanvasGroup>();
		_pressedSprite = _defaultPressedSprite;
		_unpressedSprite = _defaultSprite;
		_rectT = (RectTransform)base.transform;
		_isPressed = false;
		_isPressedByKey = false;
		_isPressedByPointer = false;
		_enabled = true;
		if (mappedKeyIndicatorText != null)
		{
			_mappedKeyIndicatorTexts = new List<Text>();
			_mappedKeyIndicatorTexts.AddRange(mappedKeyIndicatorText.GetComponentsInChildren<Text>());
			_mappedKeyIndicatorTexts.AddRange(mappedKeyIndicatorText.GetComponentsInChildren<HighlightableText>());
		}
		UpdateState();
	}

	public void ResetInputControl()
	{
		SetEnabled(enabled: true);
		HideKeySprite();
	}

	public bool IsPressed()
	{
		return _isPressed;
	}

	public void Show(bool show)
	{
		if (show)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void Show()
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
			UpdateMappedKeyIndicator();
		}
	}

	public void OnDisable()
	{
		_isPressed = false;
		_isPressedByKey = false;
		_isPressedByPointer = false;
	}

	public bool IsVisible()
	{
		return base.gameObject.activeSelf;
	}

	public void Hide()
	{
		_isPressed = false;
		_isPressedByKey = false;
		_isPressedByPointer = false;
		EnterUnpressedState();
		base.gameObject.SetActive(value: false);
	}

	public bool GetEnabled()
	{
		return _enabled;
	}

	public void SetEnabled(bool enabled)
	{
		if (enabled != _enabled)
		{
			_enabled = enabled;
			_button.interactable = enabled;
			if (!enabled)
			{
				EnterUnpressedState();
			}
			if (_canvasGroup != null)
			{
				float num = 0.5f;
				_canvasGroup.alpha = ((!_enabled) ? num : 1f);
				_canvasGroup.interactable = enabled;
				_canvasGroup.blocksRaycasts = enabled;
			}
		}
	}

	public void SetPointerControlEnabled(bool enabled)
	{
		_pointerControlEnabled = enabled;
	}

	public void OverrideSprite(Sprite sprite)
	{
		_unpressedSprite = sprite;
		if (!_isPressed)
		{
			_image.sprite = sprite;
		}
	}

	public void OverridePressedSprite(Sprite sprite)
	{
		_pressedSprite = sprite;
		if (_isPressed)
		{
			_image.sprite = sprite;
		}
	}

	public void HideKeySprite()
	{
		if (_keyImage != null)
		{
			_keyImage.enabled = false;
		}
	}

	public void AssignKeySprite(Sprite sprite)
	{
		if (_keyImage != null)
		{
			_keyImage.enabled = true;
			_keyImage.sprite = sprite;
		}
	}

	public void ResetDefaultSprites()
	{
		_unpressedSprite = _defaultSprite;
		_pressedSprite = _defaultPressedSprite;
		_image.sprite = ((!_isPressed) ? _unpressedSprite : _pressedSprite);
	}

	public bool OwnsTouch(int touchId)
	{
		if (Input.touchCount <= touchId)
		{
			return false;
		}
		Touch touch = Input.GetTouch(touchId);
		bool flag = BW.Options.useTouch() && touch.fingerId == _pointerId;
		return _pointerControlEnabled && _isPressed && flag;
	}

	public RectTransform GetRectTransform()
	{
		return _rectT;
	}

	public void UpdateMappedKeyIndicator()
	{
		if (mappedKeyIndicatorText == null)
		{
			return;
		}
		string text = null;
		if (useMappableInput)
		{
			text = MappedInput.GetLabel(mappableInput, firstInputOnly: true);
		}
		bool flag = !string.IsNullOrEmpty(text);
		for (int i = 0; i < _mappedKeyIndicatorTexts.Count; i++)
		{
			if (flag)
			{
				_mappedKeyIndicatorTexts[i].text = text;
				_mappedKeyIndicatorTexts[i].enabled = true;
			}
			else
			{
				_mappedKeyIndicatorTexts[i].enabled = false;
			}
		}
	}

	public void UpdateKeyboardInput()
	{
		if (Blocksworld.lockInput || !_enabled)
		{
			_isPressedByKey = false;
		}
		else
		{
			_isPressedByKey = MappedInput.InputPressed(mappableInput);
		}
		UpdateState();
	}

	private void UpdateState()
	{
		if (_isPressedByPointer || _isPressedByKey)
		{
			EnterPressedState();
		}
		else
		{
			EnterUnpressedState();
		}
	}

	private void EnterPressedState()
	{
		if (_isPressed)
		{
			return;
		}
		_isPressed = true;
		_image.sprite = _pressedSprite;
		if (_mappedKeyIndicatorTexts == null)
		{
			return;
		}
		for (int i = 0; i < _mappedKeyIndicatorTexts.Count; i++)
		{
			if (_mappedKeyIndicatorTexts[i] is HighlightableText)
			{
				((HighlightableText)_mappedKeyIndicatorTexts[i]).Highlight(highlight: true);
			}
		}
	}

	private void EnterUnpressedState()
	{
		if (!_isPressed)
		{
			return;
		}
		_isPressedByPointer = false;
		_isPressedByKey = false;
		_isPressed = false;
		_image.sprite = _unpressedSprite;
		if (_mappedKeyIndicatorTexts == null)
		{
			return;
		}
		for (int i = 0; i < _mappedKeyIndicatorTexts.Count; i++)
		{
			if (_mappedKeyIndicatorTexts[i] is HighlightableText)
			{
				((HighlightableText)_mappedKeyIndicatorTexts[i]).Highlight(highlight: false);
			}
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (_enabled && _pointerControlEnabled)
		{
			_isPressedByPointer = true;
			_pointerId = eventData.pointerId;
			UpdateState();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (_enabled && _pointerControlEnabled && eventData.pointerId == _pointerId)
		{
			_isPressedByPointer = false;
			_pointerId = -1;
			UpdateState();
		}
	}
}
