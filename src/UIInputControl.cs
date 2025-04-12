using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x02000301 RID: 769
public class UIInputControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IEventSystemHandler
{
	// Token: 0x17000162 RID: 354
	// (get) Token: 0x060022B0 RID: 8880 RVA: 0x001020A2 File Offset: 0x001004A2
	public static List<UIInputControl.ControlType> allButtonTypes
	{
		get
		{
			return UIInputControl._allButtonTypes;
		}
	}

	// Token: 0x17000163 RID: 355
	// (get) Token: 0x060022B1 RID: 8881 RVA: 0x001020A9 File Offset: 0x001004A9
	public static List<UIInputControl.ControlVariant> allButtonVariants
	{
		get
		{
			return UIInputControl._allButtonVariants;
		}
	}

	// Token: 0x17000164 RID: 356
	// (get) Token: 0x060022B2 RID: 8882 RVA: 0x001020B0 File Offset: 0x001004B0
	public static Dictionary<string, UIInputControl.ControlType> controlTypeFromString
	{
		get
		{
			if (UIInputControl._controlTypeFromString == null)
			{
				UIInputControl._controlTypeFromString = new Dictionary<string, UIInputControl.ControlType>();
				for (int i = 0; i < UIInputControl.allButtonTypes.Count; i++)
				{
					string text = UIInputControl.allButtonTypes[i].ToString();
					UIInputControl._controlTypeFromString[text] = UIInputControl.allButtonTypes[i];
					for (int j = 0; j < UIInputControl.allButtonVariants.Count; j++)
					{
						string key = text + " " + UIInputControl.allButtonVariants[j].ToString();
						UIInputControl._controlTypeFromString[key] = UIInputControl.allButtonTypes[i];
					}
				}
			}
			return UIInputControl._controlTypeFromString;
		}
	}

	// Token: 0x17000165 RID: 357
	// (get) Token: 0x060022B3 RID: 8883 RVA: 0x0010217C File Offset: 0x0010057C
	public static Dictionary<string, UIInputControl.ControlVariant> controlVariantFromString
	{
		get
		{
			if (UIInputControl._controlVariantFromString == null)
			{
				UIInputControl._controlVariantFromString = new Dictionary<string, UIInputControl.ControlVariant>();
				for (int i = 0; i < UIInputControl.allButtonTypes.Count; i++)
				{
					string text = UIInputControl.allButtonTypes[i].ToString();
					UIInputControl._controlVariantFromString[text] = UIInputControl.ControlVariant.Default;
					for (int j = 0; j < UIInputControl.allButtonVariants.Count; j++)
					{
						string key = text + " " + UIInputControl.allButtonVariants[j].ToString();
						UIInputControl._controlVariantFromString[key] = UIInputControl.allButtonVariants[j];
					}
				}
			}
			return UIInputControl._controlVariantFromString;
		}
	}

	// Token: 0x060022B4 RID: 8884 RVA: 0x0010223C File Offset: 0x0010063C
	public void Init()
	{
		if (this.mainImage != null)
		{
			this._image = this.mainImage;
		}
		else
		{
			this._image = base.GetComponent<Image>();
			if (this._image == null && base.transform.childCount > 0)
			{
				this._image = base.transform.GetChild(0).GetComponent<Image>();
			}
		}
		this._button = base.GetComponent<Button>();
		this._defaultSprite = this.defaultSprite;
		this._defaultPressedSprite = this.defaultPressedSprite;
		if (this.keyImageObject != null)
		{
			this._keyImage = this.keyImageObject.GetComponent<Image>();
			this.HideKeySprite();
		}
		this._button.transition = Selectable.Transition.None;
		this._canvasGroup = base.GetComponent<CanvasGroup>();
		this._pressedSprite = this._defaultPressedSprite;
		this._unpressedSprite = this._defaultSprite;
		this._rectT = (RectTransform)base.transform;
		this._isPressed = false;
		this._isPressedByKey = false;
		this._isPressedByPointer = false;
		this._enabled = true;
		if (this.mappedKeyIndicatorText != null)
		{
			this._mappedKeyIndicatorTexts = new List<Text>();
			this._mappedKeyIndicatorTexts.AddRange(this.mappedKeyIndicatorText.GetComponentsInChildren<Text>());
			this._mappedKeyIndicatorTexts.AddRange(this.mappedKeyIndicatorText.GetComponentsInChildren<HighlightableText>());
		}
		this.UpdateState();
	}

	// Token: 0x060022B5 RID: 8885 RVA: 0x001023A7 File Offset: 0x001007A7
	public void ResetInputControl()
	{
		this.SetEnabled(true);
		this.HideKeySprite();
	}

	// Token: 0x060022B6 RID: 8886 RVA: 0x001023B6 File Offset: 0x001007B6
	public bool IsPressed()
	{
		return this._isPressed;
	}

	// Token: 0x060022B7 RID: 8887 RVA: 0x001023BE File Offset: 0x001007BE
	public void Show(bool show)
	{
		if (show)
		{
			this.Show();
		}
		else
		{
			this.Hide();
		}
	}

	// Token: 0x060022B8 RID: 8888 RVA: 0x001023D7 File Offset: 0x001007D7
	public void Show()
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
			this.UpdateMappedKeyIndicator();
		}
	}

	// Token: 0x060022B9 RID: 8889 RVA: 0x001023FB File Offset: 0x001007FB
	public void OnDisable()
	{
		this._isPressed = false;
		this._isPressedByKey = false;
		this._isPressedByPointer = false;
	}

	// Token: 0x060022BA RID: 8890 RVA: 0x00102412 File Offset: 0x00100812
	public bool IsVisible()
	{
		return base.gameObject.activeSelf;
	}

	// Token: 0x060022BB RID: 8891 RVA: 0x0010241F File Offset: 0x0010081F
	public void Hide()
	{
		this._isPressed = false;
		this._isPressedByKey = false;
		this._isPressedByPointer = false;
		this.EnterUnpressedState();
		base.gameObject.SetActive(false);
	}

	// Token: 0x060022BC RID: 8892 RVA: 0x00102448 File Offset: 0x00100848
	public bool GetEnabled()
	{
		return this._enabled;
	}

	// Token: 0x060022BD RID: 8893 RVA: 0x00102450 File Offset: 0x00100850
	public void SetEnabled(bool enabled)
	{
		if (enabled == this._enabled)
		{
			return;
		}
		this._enabled = enabled;
		this._button.interactable = enabled;
		if (!enabled)
		{
			this.EnterUnpressedState();
		}
		if (this._canvasGroup != null)
		{
			float num = 0.5f;
			this._canvasGroup.alpha = ((!this._enabled) ? num : 1f);
			this._canvasGroup.interactable = enabled;
			this._canvasGroup.blocksRaycasts = enabled;
		}
	}

	// Token: 0x060022BE RID: 8894 RVA: 0x001024D9 File Offset: 0x001008D9
	public void SetPointerControlEnabled(bool enabled)
	{
		this._pointerControlEnabled = enabled;
	}

	// Token: 0x060022BF RID: 8895 RVA: 0x001024E2 File Offset: 0x001008E2
	public void OverrideSprite(Sprite sprite)
	{
		this._unpressedSprite = sprite;
		if (!this._isPressed)
		{
			this._image.sprite = sprite;
		}
	}

	// Token: 0x060022C0 RID: 8896 RVA: 0x00102502 File Offset: 0x00100902
	public void OverridePressedSprite(Sprite sprite)
	{
		this._pressedSprite = sprite;
		if (this._isPressed)
		{
			this._image.sprite = sprite;
		}
	}

	// Token: 0x060022C1 RID: 8897 RVA: 0x00102522 File Offset: 0x00100922
	public void HideKeySprite()
	{
		if (this._keyImage != null)
		{
			this._keyImage.enabled = false;
		}
	}

	// Token: 0x060022C2 RID: 8898 RVA: 0x00102541 File Offset: 0x00100941
	public void AssignKeySprite(Sprite sprite)
	{
		if (this._keyImage != null)
		{
			this._keyImage.enabled = true;
			this._keyImage.sprite = sprite;
		}
	}

	// Token: 0x060022C3 RID: 8899 RVA: 0x0010256C File Offset: 0x0010096C
	public void ResetDefaultSprites()
	{
		this._unpressedSprite = this._defaultSprite;
		this._pressedSprite = this._defaultPressedSprite;
		this._image.sprite = ((!this._isPressed) ? this._unpressedSprite : this._pressedSprite);
	}

	// Token: 0x060022C4 RID: 8900 RVA: 0x001025B8 File Offset: 0x001009B8
	public bool OwnsTouch(int touchId)
	{
		if (Input.touchCount <= touchId)
		{
			return false;
		}
		Touch touch = Input.GetTouch(touchId);
		bool flag = BW.Options.useTouch() && touch.fingerId == this._pointerId;
		return this._pointerControlEnabled && this._isPressed && flag;
	}

	// Token: 0x060022C5 RID: 8901 RVA: 0x00102615 File Offset: 0x00100A15
	public RectTransform GetRectTransform()
	{
		return this._rectT;
	}

	// Token: 0x060022C6 RID: 8902 RVA: 0x00102620 File Offset: 0x00100A20
	public void UpdateMappedKeyIndicator()
	{
		if (this.mappedKeyIndicatorText == null)
		{
			return;
		}
		string text = null;
		if (this.useMappableInput)
		{
			text = MappedInput.GetLabel(this.mappableInput, true);
		}
		bool flag = !string.IsNullOrEmpty(text);
		for (int i = 0; i < this._mappedKeyIndicatorTexts.Count; i++)
		{
			if (flag)
			{
				this._mappedKeyIndicatorTexts[i].text = text;
				this._mappedKeyIndicatorTexts[i].enabled = true;
			}
			else
			{
				this._mappedKeyIndicatorTexts[i].enabled = false;
			}
		}
	}

	// Token: 0x060022C7 RID: 8903 RVA: 0x001026C0 File Offset: 0x00100AC0
	public void UpdateKeyboardInput()
	{
		if (Blocksworld.lockInput || !this._enabled)
		{
			this._isPressedByKey = false;
		}
		else
		{
			this._isPressedByKey = MappedInput.InputPressed(this.mappableInput);
		}
		this.UpdateState();
	}

	// Token: 0x060022C8 RID: 8904 RVA: 0x001026FA File Offset: 0x00100AFA
	private void UpdateState()
	{
		if (this._isPressedByPointer || this._isPressedByKey)
		{
			this.EnterPressedState();
		}
		else
		{
			this.EnterUnpressedState();
		}
	}

	// Token: 0x060022C9 RID: 8905 RVA: 0x00102724 File Offset: 0x00100B24
	private void EnterPressedState()
	{
		if (this._isPressed)
		{
			return;
		}
		this._isPressed = true;
		this._image.sprite = this._pressedSprite;
		if (this._mappedKeyIndicatorTexts != null)
		{
			for (int i = 0; i < this._mappedKeyIndicatorTexts.Count; i++)
			{
				if (this._mappedKeyIndicatorTexts[i] is HighlightableText)
				{
					((HighlightableText)this._mappedKeyIndicatorTexts[i]).Highlight(true);
				}
			}
		}
	}

	// Token: 0x060022CA RID: 8906 RVA: 0x001027AC File Offset: 0x00100BAC
	private void EnterUnpressedState()
	{
		if (!this._isPressed)
		{
			return;
		}
		this._isPressedByPointer = false;
		this._isPressedByKey = false;
		this._isPressed = false;
		this._image.sprite = this._unpressedSprite;
		if (this._mappedKeyIndicatorTexts != null)
		{
			for (int i = 0; i < this._mappedKeyIndicatorTexts.Count; i++)
			{
				if (this._mappedKeyIndicatorTexts[i] is HighlightableText)
				{
					((HighlightableText)this._mappedKeyIndicatorTexts[i]).Highlight(false);
				}
			}
		}
	}

	// Token: 0x060022CB RID: 8907 RVA: 0x0010283F File Offset: 0x00100C3F
	public void OnPointerDown(PointerEventData eventData)
	{
		if (!this._enabled || !this._pointerControlEnabled)
		{
			return;
		}
		this._isPressedByPointer = true;
		this._pointerId = eventData.pointerId;
		this.UpdateState();
	}

	// Token: 0x060022CC RID: 8908 RVA: 0x00102871 File Offset: 0x00100C71
	public void OnPointerUp(PointerEventData eventData)
	{
		if (!this._enabled || !this._pointerControlEnabled)
		{
			return;
		}
		if (eventData.pointerId == this._pointerId)
		{
			this._isPressedByPointer = false;
			this._pointerId = -1;
			this.UpdateState();
		}
	}

	// Token: 0x04001DAD RID: 7597
	public static int controlTypeCount = 6;

	// Token: 0x04001DAE RID: 7598
	private static List<UIInputControl.ControlType> _allButtonTypes = new List<UIInputControl.ControlType>
	{
		UIInputControl.ControlType.Left,
		UIInputControl.ControlType.Right,
		UIInputControl.ControlType.Up,
		UIInputControl.ControlType.Down,
		UIInputControl.ControlType.L,
		UIInputControl.ControlType.R
	};

	// Token: 0x04001DAF RID: 7599
	private static List<UIInputControl.ControlVariant> _allButtonVariants = new List<UIInputControl.ControlVariant>
	{
		UIInputControl.ControlVariant.Default,
		UIInputControl.ControlVariant.Action,
		UIInputControl.ControlVariant.Attack,
		UIInputControl.ControlVariant.Explode,
		UIInputControl.ControlVariant.Help,
		UIInputControl.ControlVariant.Jump,
		UIInputControl.ControlVariant.Laser,
		UIInputControl.ControlVariant.Missile,
		UIInputControl.ControlVariant.Mode,
		UIInputControl.ControlVariant.Speak,
		UIInputControl.ControlVariant.Speed
	};

	// Token: 0x04001DB0 RID: 7600
	private static Dictionary<string, UIInputControl.ControlType> _controlTypeFromString;

	// Token: 0x04001DB1 RID: 7601
	private static Dictionary<string, UIInputControl.ControlVariant> _controlVariantFromString;

	// Token: 0x04001DB2 RID: 7602
	public UIInputControl.ControlType controlType;

	// Token: 0x04001DB3 RID: 7603
	public Image mainImage;

	// Token: 0x04001DB4 RID: 7604
	public Sprite defaultSprite;

	// Token: 0x04001DB5 RID: 7605
	public Sprite defaultPressedSprite;

	// Token: 0x04001DB6 RID: 7606
	public GameObject keyImageObject;

	// Token: 0x04001DB7 RID: 7607
	public bool useMappableInput;

	// Token: 0x04001DB8 RID: 7608
	public MappableInput mappableInput;

	// Token: 0x04001DB9 RID: 7609
	public Text mappedKeyIndicatorText;

	// Token: 0x04001DBA RID: 7610
	public int visibilityGroup;

	// Token: 0x04001DBB RID: 7611
	private bool _isPressed;

	// Token: 0x04001DBC RID: 7612
	private bool _isPressedByPointer;

	// Token: 0x04001DBD RID: 7613
	private bool _isPressedByKey;

	// Token: 0x04001DBE RID: 7614
	private bool _enabled;

	// Token: 0x04001DBF RID: 7615
	private Image _image;

	// Token: 0x04001DC0 RID: 7616
	private Button _button;

	// Token: 0x04001DC1 RID: 7617
	private Sprite _defaultSprite;

	// Token: 0x04001DC2 RID: 7618
	private Sprite _defaultPressedSprite;

	// Token: 0x04001DC3 RID: 7619
	private Image _keyImage;

	// Token: 0x04001DC4 RID: 7620
	private Sprite _unpressedSprite;

	// Token: 0x04001DC5 RID: 7621
	private Sprite _pressedSprite;

	// Token: 0x04001DC6 RID: 7622
	private RectTransform _rectT;

	// Token: 0x04001DC7 RID: 7623
	private CanvasGroup _canvasGroup;

	// Token: 0x04001DC8 RID: 7624
	private List<Text> _mappedKeyIndicatorTexts;

	// Token: 0x04001DC9 RID: 7625
	private bool _pointerControlEnabled = true;

	// Token: 0x04001DCA RID: 7626
	private int _pointerId = -1;

	// Token: 0x02000302 RID: 770
	public enum ControlType
	{
		// Token: 0x04001DCC RID: 7628
		Left,
		// Token: 0x04001DCD RID: 7629
		Right,
		// Token: 0x04001DCE RID: 7630
		Up,
		// Token: 0x04001DCF RID: 7631
		Down,
		// Token: 0x04001DD0 RID: 7632
		L,
		// Token: 0x04001DD1 RID: 7633
		R
	}

	// Token: 0x02000303 RID: 771
	public enum ControlVariant
	{
		// Token: 0x04001DD3 RID: 7635
		Default,
		// Token: 0x04001DD4 RID: 7636
		Action,
		// Token: 0x04001DD5 RID: 7637
		Attack,
		// Token: 0x04001DD6 RID: 7638
		Explode,
		// Token: 0x04001DD7 RID: 7639
		Help,
		// Token: 0x04001DD8 RID: 7640
		Jump,
		// Token: 0x04001DD9 RID: 7641
		Laser,
		// Token: 0x04001DDA RID: 7642
		Missile,
		// Token: 0x04001DDB RID: 7643
		Mode,
		// Token: 0x04001DDC RID: 7644
		Speak,
		// Token: 0x04001DDD RID: 7645
		Speed
	}
}
