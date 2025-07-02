using UnityEngine;
using UnityEngine.EventSystems;

public class UISound : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	public string hoverEnter;

	public string hoverExit;

	public string pressDown = "DefaultButtonClick";

	public string release;

	public string buttonSuccess = "DefaultButtonClick";

	public string buttonFailure;

	public float volume = 1f;

	private void OnEnable()
	{
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(hoverEnter))
		{
			UISoundPlayer.Instance.PlayClip(hoverEnter, volume);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(hoverExit))
		{
			UISoundPlayer.Instance.PlayClip(hoverExit, volume);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(pressDown))
		{
			UISoundPlayer.Instance.PlayClip(pressDown, volume);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(release))
		{
			UISoundPlayer.Instance.PlayClip(release, volume);
		}
	}

	public void PlayMessageButtonSound(bool success)
	{
		if (success && !string.IsNullOrEmpty(buttonSuccess))
		{
			UISoundPlayer.Instance.PlayClip(buttonSuccess, volume);
		}
		else if (!success && !string.IsNullOrEmpty(buttonFailure))
		{
			UISoundPlayer.Instance.PlayClip(buttonFailure, volume);
		}
	}
}
