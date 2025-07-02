using UnityEngine;
using UnityEngine.UI;

public class UIArrow : MonoBehaviour
{
	public Vector2 debugFrom = Vector2.zero;

	public Vector2 debugTo = new Vector2(-256f, 0f);

	public RectTransform shaftRT;

	public RectTransform headRT;

	public Sprite defaultShaftSprite;

	public Sprite swipeShaftSprite;

	public Sprite doubleSwiptShaftSprite;

	public Sprite defaultHeadSprite;

	public Sprite swipeHeadSprite;

	private RectTransform _rt;

	private CanvasGroup _canvasGroup;

	private Vector2 _defaultHeadSize;

	private Vector2 _defaultShaftSize;

	private Image _shaftImage;

	private Image _headImage;

	private bool _invisible;

	private const float kMinScale = 0.75f;

	private const float kLengthForDefaultScale = 80f;

	private const float kFadeStartLength = 10f;

	private const float kHeadGap = 40f;

	public void Init()
	{
		_rt = (RectTransform)base.transform;
		_canvasGroup = GetComponent<CanvasGroup>();
		_defaultHeadSize = headRT.sizeDelta;
		_defaultShaftSize = shaftRT.sizeDelta;
		_shaftImage = shaftRT.GetComponent<Image>();
		_headImage = headRT.GetComponent<Image>();
	}

	public void Show(bool show)
	{
		base.gameObject.SetActiveRecursively(show);
	}

	public bool IsShowing()
	{
		return base.gameObject.activeSelf;
	}

	public void SetSwipeMode(int mode)
	{
		switch (mode)
		{
		case 0:
			_shaftImage.sprite = defaultShaftSprite;
			_headImage.sprite = defaultHeadSprite;
			_invisible = false;
			break;
		case 1:
			_shaftImage.sprite = swipeShaftSprite;
			_headImage.sprite = swipeHeadSprite;
			_invisible = false;
			break;
		case 2:
			_shaftImage.sprite = doubleSwiptShaftSprite;
			_headImage.sprite = swipeHeadSprite;
			_invisible = false;
			break;
		case 3:
			_shaftImage.sprite = defaultShaftSprite;
			_headImage.sprite = defaultHeadSprite;
			_invisible = true;
			_canvasGroup.alpha = 0f;
			break;
		}
	}

	public void SetEndpoints(Vector2 fromPos, Vector2 toPos)
	{
		Vector2 vector = toPos - fromPos;
		float magnitude = vector.magnitude;
		_rt.anchoredPosition = fromPos;
		headRT.anchoredPosition = magnitude * Vector2.right;
		float num = Mathf.Clamp(magnitude / 80f, 0.75f, 1f);
		float num2 = ((magnitude >= 10f) ? 1f : 0f);
		_canvasGroup.alpha = ((!_invisible) ? num2 : 0f);
		shaftRT.sizeDelta = new Vector2(magnitude - num * 40f, num * _defaultShaftSize.y);
		headRT.sizeDelta = num * _defaultHeadSize;
		float z = Mathf.Sign(vector.y) * Mathf.Abs(Vector2.Angle(vector, Vector2.right));
		_rt.localEulerAngles = new Vector3(0f, 0f, z);
	}
}
