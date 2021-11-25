using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000316 RID: 790
public class UIProfileSelection : MonoBehaviour
{
	// Token: 0x06002397 RID: 9111 RVA: 0x001065CC File Offset: 0x001049CC
	public void Init()
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			CharacterEditor.Instance.Exit();
		}
		base.gameObject.SetActive(true);
		if (this._cards == null)
		{
			this._cards = new List<UIProfileCard>();
			RectTransform parent = (RectTransform)this.cardTemplate.transform.parent;
			Dictionary<ProfileType, UIProfileCardInfo> dictionary = new Dictionary<ProfileType, UIProfileCardInfo>();
			foreach (UIProfileCardInfo uiprofileCardInfo in this.profileInfo)
			{
				dictionary[uiprofileCardInfo.type] = uiprofileCardInfo;
			}
			foreach (ProfileType profileType in dictionary.Keys)
			{
				bool unlocked = this.IsUnlocked(profileType);
				UIProfileCard uiprofileCard = UnityEngine.Object.Instantiate<UIProfileCard>(this.cardTemplate);
				uiprofileCard.Setup(this, dictionary[profileType], unlocked);
				uiprofileCard.SetSelected(false);
				RectTransform rectTransform = (RectTransform)uiprofileCard.transform;
				rectTransform.SetParent(parent, false);
				this._cards.Add(uiprofileCard);
			}
			this.cardTemplate.gameObject.SetActive(false);
			this.closeButton.onClick.AddListener(new UnityAction(this.ApplySelected));
			this.cancelButton.onClick.AddListener(new UnityAction(this.Cancel));
		}
		this._characterBlock = ProfileBlocksterUtils.GetProfileCharacterBlock();
		float num = 4.5f;
		Vector3 position = this._characterBlock.GetPosition();
		Vector3 vector = position - 0.5f * Vector3.up;
		Vector3 vector2 = position + this._characterBlock.goT.forward * num + 1.25f * Vector3.up;
		Vector3 vector3 = vector - vector2;
		Vector3 lhs = Vector3.Cross(vector3, Vector3.up);
		Vector3 upwards = Vector3.Cross(lhs, vector3);
		Quaternion quaternion = Quaternion.LookRotation(vector3, upwards);
		Blocksworld.blocksworldCamera.Store();
		Blocksworld.blocksworldCamera.Unfollow();
		Blocksworld.blocksworldCamera.PlaceCamera(quaternion.eulerAngles, vector2);
		Blocksworld.blocksworldCamera.SetTargetDistance(num);
		Blocksworld.blocksworldCamera.SetTargetPosition(vector);
		this._originalType = ProfileBlocksterUtils.GetProfileCharacterType(this._characterBlock);
		this._originalTiles = Blocksworld.CloneBlockTiles(this._characterBlock, false, false);
		WorldSession.current.profileWorldAnimatedBlockster = null;
		TBox.Show(false);
		this.scrollRect.horizontalNormalizedPosition = this._scrollPos;
		bool isNewProfile = WorldSession.current.config.isNewProfile;
		if (isNewProfile)
		{
			this.cancelButton.gameObject.SetActive(false);
		}
	}

	// Token: 0x06002398 RID: 9112 RVA: 0x00106894 File Offset: 0x00104C94
	public void SelectProfileType(ProfileType type)
	{
		foreach (UIProfileCard uiprofileCard in this._cards)
		{
			if (uiprofileCard.ProfileType() == type)
			{
				uiprofileCard.SetSelected(true);
			}
			else
			{
				uiprofileCard.SetSelected(false);
			}
		}
		if (this._selectedType != type)
		{
			this._selectedType = type;
			if (this._selectedType == this._originalType)
			{
				this._characterBlock = ProfileBlocksterUtils.RestoreProfileCharacter(this._originalTiles);
			}
			else
			{
				this._characterBlock = ProfileBlocksterUtils.ReplaceProfileCharacter(this._selectedType);
			}
		}
	}

	// Token: 0x06002399 RID: 9113 RVA: 0x00106958 File Offset: 0x00104D58
	public void ScrollToType(ProfileType type)
	{
		UIProfileCard uiprofileCard = null;
		int num = 0;
		for (int i = 0; i < this._cards.Count; i++)
		{
			UIProfileCard uiprofileCard2 = this._cards[i];
			if (uiprofileCard2.ProfileType() == type)
			{
				uiprofileCard = uiprofileCard2;
				num = i;
			}
		}
		RectTransform rectTransform = (RectTransform)uiprofileCard.transform;
		float width = this.scrollRect.viewport.rect.width;
		float x = rectTransform.localPosition.x;
		float width2 = rectTransform.rect.width;
		if (x < -width / 2f + width2 || x > width / 2f - width2)
		{
			this.scrollRect.horizontalNormalizedPosition = (float)num / ((float)this._cards.Count - 1f);
		}
		this.SelectProfileType(type);
	}

	// Token: 0x0600239A RID: 9114 RVA: 0x00106A3A File Offset: 0x00104E3A
	private void ApplySelected()
	{
		this.Close();
	}

	// Token: 0x0600239B RID: 9115 RVA: 0x00106A42 File Offset: 0x00104E42
	private void Cancel()
	{
		this._characterBlock = ProfileBlocksterUtils.RestoreProfileCharacter(this._originalTiles);
		this.Close();
	}

	// Token: 0x0600239C RID: 9116 RVA: 0x00106A5C File Offset: 0x00104E5C
	private void Close()
	{
		WorldSession.current.config.isNewProfile = false;
		this._scrollPos = this.scrollRect.horizontalNormalizedPosition;
		if (this._characterBlock is BlockAnimatedCharacter)
		{
			WorldSession.current.profileWorldAnimatedBlockster = (this._characterBlock as BlockAnimatedCharacter);
		}
		else
		{
			WorldSession.current.profileWorldAnimatedBlockster = null;
		}
		Blocksworld.UI.HideProfileSelectionScreen();
		TBox.Show(false);
		Blocksworld.blocksworldCamera.Restore();
	}

	// Token: 0x0600239D RID: 9117 RVA: 0x00106ADC File Offset: 0x00104EDC
	private bool IsUnlocked(ProfileType type)
	{
		string blockItemIdentifier = ProfileBlocksterUtils.BlockItemIdentifierForProfileType(type);
		return WorldSession.current.BlockIsAvailable(blockItemIdentifier);
	}

	// Token: 0x04001ECE RID: 7886
	public UIProfileCardInfo[] profileInfo;

	// Token: 0x04001ECF RID: 7887
	public UIProfileCard cardTemplate;

	// Token: 0x04001ED0 RID: 7888
	public ScrollRect scrollRect;

	// Token: 0x04001ED1 RID: 7889
	public Button closeButton;

	// Token: 0x04001ED2 RID: 7890
	public Button cancelButton;

	// Token: 0x04001ED3 RID: 7891
	private List<UIProfileCard> _cards;

	// Token: 0x04001ED4 RID: 7892
	private ProfileType _selectedType;

	// Token: 0x04001ED5 RID: 7893
	private ProfileType _originalType;

	// Token: 0x04001ED6 RID: 7894
	private List<List<Tile>> _originalTiles;

	// Token: 0x04001ED7 RID: 7895
	private Block _characterBlock;

	// Token: 0x04001ED8 RID: 7896
	private float _scrollPos;
}
