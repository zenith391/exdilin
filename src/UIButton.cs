using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
	public UnityAction clickAction;

	public UIEditableText buttonText;

	public float maxWidth = 220f;

	private float _defaultWidth = 155f;

	private float _defaultHeight = 55f;

	private float _preferredWidth = 155f;

	private RectTransform _rt;

	private LayoutElement _layoutElement;

	private bool _alphaMode;

	private Button _buttonComponent;

	private TapStartButton _tapStartComponent;

	public float DefaultWidth => _defaultWidth;

	public float PreferredWidth => _preferredWidth;

	public void Init(bool alphaMode = false)
	{
		_rt = (RectTransform)base.transform;
		if (buttonText != null)
		{
			buttonText.Init();
		}
		_defaultWidth = _rt.sizeDelta.x;
		_defaultHeight = _rt.sizeDelta.y;
		_preferredWidth = _defaultWidth;
		_layoutElement = GetComponent<LayoutElement>();
		_alphaMode = alphaMode;
		_tapStartComponent = _rt.GetComponent<TapStartButton>();
		if (_tapStartComponent != null)
		{
			_tapStartComponent.tapAction = delegate
			{
				DidClick();
			};
		}
		else
		{
			_buttonComponent = _rt.GetComponent<Button>();
		}
		if (_alphaMode && !base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public void Show()
	{
		if (_alphaMode)
		{
			if (_buttonComponent != null)
			{
				_buttonComponent.interactable = true;
			}
			else if (_tapStartComponent != null)
			{
				_tapStartComponent.interactable = true;
			}
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public void Hide()
	{
		if (_alphaMode)
		{
			if (_buttonComponent != null)
			{
				_buttonComponent.interactable = false;
			}
			else if (_tapStartComponent != null)
			{
				_tapStartComponent.interactable = false;
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public bool Hit(Vector3 pos)
	{
		if (base.gameObject.activeInHierarchy)
		{
			return Util.RectTransformContains(_rt, pos);
		}
		return false;
	}

	public void SetText(string text)
	{
		_preferredWidth = _defaultWidth;
		Vector2 sizeDelta = new Vector2(_defaultWidth, _defaultHeight);
		if (buttonText != null)
		{
			buttonText.Set(text);
			_preferredWidth = Mathf.Max(_defaultWidth, buttonText.PreferredWidth + 30f * NormalizedScreen.scale);
			sizeDelta = new Vector2(_preferredWidth, _defaultHeight);
		}
		((RectTransform)base.transform).sizeDelta = sizeDelta;
		if (_layoutElement != null)
		{
			_layoutElement.preferredWidth = _preferredWidth;
		}
	}

	public void DidClick()
	{
		if (clickAction != null)
		{
			clickAction();
		}
	}
}
