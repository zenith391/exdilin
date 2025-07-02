using System.Collections.Generic;
using Blocks;
using UnityEngine;
using UnityEngine.UI;

public class UIProfileSelection : MonoBehaviour
{
	public UIProfileCardInfo[] profileInfo;

	public UIProfileCard cardTemplate;

	public ScrollRect scrollRect;

	public Button closeButton;

	public Button cancelButton;

	private List<UIProfileCard> _cards;

	private ProfileType _selectedType;

	private ProfileType _originalType;

	private List<List<Tile>> _originalTiles;

	private Block _characterBlock;

	private float _scrollPos;

	public void Init()
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			CharacterEditor.Instance.Exit();
		}
		base.gameObject.SetActive(value: true);
		if (_cards == null)
		{
			_cards = new List<UIProfileCard>();
			RectTransform parent = (RectTransform)cardTemplate.transform.parent;
			Dictionary<ProfileType, UIProfileCardInfo> dictionary = new Dictionary<ProfileType, UIProfileCardInfo>();
			UIProfileCardInfo[] array = profileInfo;
			foreach (UIProfileCardInfo uIProfileCardInfo in array)
			{
				dictionary[uIProfileCardInfo.type] = uIProfileCardInfo;
			}
			foreach (ProfileType key in dictionary.Keys)
			{
				bool unlocked = IsUnlocked(key);
				UIProfileCard uIProfileCard = Object.Instantiate(cardTemplate);
				uIProfileCard.Setup(this, dictionary[key], unlocked);
				uIProfileCard.SetSelected(selected: false);
				RectTransform rectTransform = (RectTransform)uIProfileCard.transform;
				rectTransform.SetParent(parent, worldPositionStays: false);
				_cards.Add(uIProfileCard);
			}
			cardTemplate.gameObject.SetActive(value: false);
			closeButton.onClick.AddListener(ApplySelected);
			cancelButton.onClick.AddListener(Cancel);
		}
		_characterBlock = ProfileBlocksterUtils.GetProfileCharacterBlock();
		float num = 4.5f;
		Vector3 position = _characterBlock.GetPosition();
		Vector3 vector = position - 0.5f * Vector3.up;
		Vector3 vector2 = position + _characterBlock.goT.forward * num + 1.25f * Vector3.up;
		Vector3 vector3 = vector - vector2;
		Vector3 lhs = Vector3.Cross(vector3, Vector3.up);
		Vector3 upwards = Vector3.Cross(lhs, vector3);
		Quaternion quaternion = Quaternion.LookRotation(vector3, upwards);
		Blocksworld.blocksworldCamera.Store();
		Blocksworld.blocksworldCamera.Unfollow();
		Blocksworld.blocksworldCamera.PlaceCamera(quaternion.eulerAngles, vector2);
		Blocksworld.blocksworldCamera.SetTargetDistance(num);
		Blocksworld.blocksworldCamera.SetTargetPosition(vector);
		_originalType = ProfileBlocksterUtils.GetProfileCharacterType(_characterBlock);
		_originalTiles = Blocksworld.CloneBlockTiles(_characterBlock);
		WorldSession.current.profileWorldAnimatedBlockster = null;
		TBox.Show(show: false);
		scrollRect.horizontalNormalizedPosition = _scrollPos;
		if (WorldSession.current.config.isNewProfile)
		{
			cancelButton.gameObject.SetActive(value: false);
		}
	}

	public void SelectProfileType(ProfileType type)
	{
		foreach (UIProfileCard card in _cards)
		{
			if (card.ProfileType() == type)
			{
				card.SetSelected(selected: true);
			}
			else
			{
				card.SetSelected(selected: false);
			}
		}
		if (_selectedType != type)
		{
			_selectedType = type;
			if (_selectedType == _originalType)
			{
				_characterBlock = ProfileBlocksterUtils.RestoreProfileCharacter(_originalTiles);
			}
			else
			{
				_characterBlock = ProfileBlocksterUtils.ReplaceProfileCharacter(_selectedType);
			}
		}
	}

	public void ScrollToType(ProfileType type)
	{
		UIProfileCard uIProfileCard = null;
		int num = 0;
		for (int i = 0; i < _cards.Count; i++)
		{
			UIProfileCard uIProfileCard2 = _cards[i];
			if (uIProfileCard2.ProfileType() == type)
			{
				uIProfileCard = uIProfileCard2;
				num = i;
			}
		}
		RectTransform rectTransform = (RectTransform)uIProfileCard.transform;
		float width = scrollRect.viewport.rect.width;
		float x = rectTransform.localPosition.x;
		float width2 = rectTransform.rect.width;
		if (x < (0f - width) / 2f + width2 || x > width / 2f - width2)
		{
			scrollRect.horizontalNormalizedPosition = (float)num / ((float)_cards.Count - 1f);
		}
		SelectProfileType(type);
	}

	private void ApplySelected()
	{
		Close();
	}

	private void Cancel()
	{
		_characterBlock = ProfileBlocksterUtils.RestoreProfileCharacter(_originalTiles);
		Close();
	}

	private void Close()
	{
		WorldSession.current.config.isNewProfile = false;
		_scrollPos = scrollRect.horizontalNormalizedPosition;
		if (_characterBlock is BlockAnimatedCharacter)
		{
			WorldSession.current.profileWorldAnimatedBlockster = _characterBlock as BlockAnimatedCharacter;
		}
		else
		{
			WorldSession.current.profileWorldAnimatedBlockster = null;
		}
		Blocksworld.UI.HideProfileSelectionScreen();
		TBox.Show(show: false);
		Blocksworld.blocksworldCamera.Restore();
	}

	private bool IsUnlocked(ProfileType type)
	{
		string blockItemIdentifier = ProfileBlocksterUtils.BlockItemIdentifierForProfileType(type);
		return WorldSession.current.BlockIsAvailable(blockItemIdentifier);
	}
}
