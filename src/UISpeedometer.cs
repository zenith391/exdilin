using UnityEngine;

public class UISpeedometer : MonoBehaviour
{
	private UIEditableText text;

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetSpeed(float speed)
	{
		if (text == null)
		{
			text = GetComponent<UIEditableText>();
		}
		text.Set(speed.ToString("0") + " B P S");
	}
}
