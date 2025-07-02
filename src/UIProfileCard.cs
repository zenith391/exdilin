using UnityEngine;
using UnityEngine.UI;

public class UIProfileCard : MonoBehaviour
{
	public Button cardButton;

	public Image selectionBorder;

	public Image lockedOverlay;

	private UIProfileSelection _selectionUI;

	private ProfileType _profileType;

	private bool _unlocked;

	public void Setup(UIProfileSelection selectionUI, UIProfileCardInfo cardInfo, bool unlocked)
	{
		_selectionUI = selectionUI;
		_profileType = cardInfo.type;
		_unlocked = unlocked;
		cardButton.image.sprite = cardInfo.image;
		cardButton.onClick.AddListener(CardTap);
		lockedOverlay.enabled = !unlocked;
	}

	public ProfileType ProfileType()
	{
		return _profileType;
	}

	public void SetSelected(bool selected)
	{
		selectionBorder.enabled = selected;
	}

	private void CardTap()
	{
		if (_unlocked)
		{
			_selectionUI.SelectProfileType(_profileType);
		}
	}
}
