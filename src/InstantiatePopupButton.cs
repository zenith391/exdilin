using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InstantiatePopupButton : MonoBehaviour, IPopupDelegate
{
	public UIPopup prefab;

	public RectTransform parentTo;

	public int siblingIndex;

	public Animator backgroundHideAnimator;

	public string hideBackgroundTrigger = "Disable";

	public string showBackgroundTrigger = "Enable";

	private Button button;

	private GameObject instantiatedObject;

	private UIPopup popup;

	private void OnEnable()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(Spawn);
	}

	private void OnDisable()
	{
		button.onClick.RemoveListener(Spawn);
	}

	public void ClosePopup()
	{
		if (backgroundHideAnimator != null)
		{
			backgroundHideAnimator.SetTrigger(showBackgroundTrigger);
		}
		Object.Destroy(instantiatedObject);
		instantiatedObject = null;
	}

	private void Spawn()
	{
		if (!(instantiatedObject != null))
		{
			if (backgroundHideAnimator != null)
			{
				backgroundHideAnimator.SetTrigger(hideBackgroundTrigger);
			}
			instantiatedObject = Object.Instantiate(prefab.gameObject);
			popup = instantiatedObject.GetComponent<UIPopup>();
			if (parentTo != null)
			{
				RectTransform rectTransform = (RectTransform)instantiatedObject.transform;
				rectTransform.SetParent(parentTo, worldPositionStays: false);
				rectTransform.SetSiblingIndex(siblingIndex);
			}
			popup.Show(this);
		}
	}
}
