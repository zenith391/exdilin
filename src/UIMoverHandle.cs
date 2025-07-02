using UnityEngine;
using UnityEngine.UI;

public class UIMoverHandle : MonoBehaviour
{
	private Image _image;

	private RectTransform _rt;

	public Sprite normalSprite;

	public Sprite pressedSprite;

	public void Init()
	{
		_image = GetComponent<Image>();
		_rt = (RectTransform)base.transform;
	}

	public void MoveTo(Vector2 offset)
	{
		_rt.anchoredPosition = offset;
	}

	public void SetImage(Sprite sprite)
	{
		_image.sprite = sprite;
		normalSprite = sprite;
		pressedSprite = sprite;
	}

	public void Press(bool press)
	{
		_image.sprite = ((!press) ? normalSprite : pressedSprite);
	}

	public void SetColor(Color c)
	{
		_image.color = c;
	}

	public void SetAlpha(float a)
	{
		Color color = _image.color;
		color.a = a;
		_image.color = color;
	}
}
