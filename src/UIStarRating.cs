using System;
using System.Collections;
using UnityEngine;

// Token: 0x0200045F RID: 1119
public class UIStarRating : MonoBehaviour
{
	// Token: 0x14000022 RID: 34
	// (add) Token: 0x06002F4F RID: 12111 RVA: 0x0014EEF0 File Offset: 0x0014D2F0
	// (remove) Token: 0x06002F50 RID: 12112 RVA: 0x0014EF28 File Offset: 0x0014D328
	private event StarRatingChangedHandler UserStarRatingChanged;

	// Token: 0x06002F51 RID: 12113 RVA: 0x0014EF60 File Offset: 0x0014D360
	public void PointerEnteredStar(int starNumber)
	{
		for (int i = 1; i <= 5; i++)
		{
			this.hoverFlags[i - 1] = (i == starNumber);
		}
		this.userStarHoverOn = true;
		this.UpdateUI();
	}

	// Token: 0x06002F52 RID: 12114 RVA: 0x0014EF9E File Offset: 0x0014D39E
	public void PointerLeftStar(int starNumber)
	{
		this.hoverFlags[starNumber - 1] = false;
		this.UpdateUI();
	}

	// Token: 0x06002F53 RID: 12115 RVA: 0x0014EFB5 File Offset: 0x0014D3B5
	public void ClickedOnStar(int starNumber)
	{
		this.currentUserRating = starNumber;
		if (this.UserStarRatingChanged != null)
		{
			this.UserStarRatingChanged(this.currentUserRating);
		}
		this.userStarHoverOn = false;
		this.UpdateUI();
	}

	// Token: 0x06002F54 RID: 12116 RVA: 0x0014EFE7 File Offset: 0x0014D3E7
	public void AddUserStarRatingListener(StarRatingChangedHandler listener)
	{
		this.UserStarRatingChanged -= listener;
		this.UserStarRatingChanged += listener;
	}

	// Token: 0x06002F55 RID: 12117 RVA: 0x0014EFF7 File Offset: 0x0014D3F7
	public void RemoveUserStarRatingListener(StarRatingChangedHandler listener)
	{
		this.UserStarRatingChanged -= listener;
	}

	// Token: 0x06002F56 RID: 12118 RVA: 0x0014F000 File Offset: 0x0014D400
	public void UpdateStarRating(float averageRating, int userRating)
	{
		this.averagePublicRating = averageRating;
		this.currentUserRating = userRating;
		this.UpdateUI();
	}

	// Token: 0x06002F57 RID: 12119 RVA: 0x0014F018 File Offset: 0x0014D418
	private int HoverStar()
	{
		for (int i = 1; i <= 5; i++)
		{
			if (this.hoverFlags[i - 1])
			{
				return i;
			}
		}
		return 0;
	}

	// Token: 0x06002F58 RID: 12120 RVA: 0x0014F050 File Offset: 0x0014D450
	private void UpdateUI()
	{
		bool flag = this.averagePublicRating < 1f;
		bool flag2 = (float)this.currentUserRating < 1f;
		int num = this.HoverStar();
		this.userStarHoverOn &= (num > 0);
		if (flag)
		{
			if (flag2)
			{
				this.unrated.gameObject.SetActive(true);
				this.publicRating.gameObject.SetActive(false);
				this.userRating.gameObject.SetActive(false);
				this.outline.gameObject.SetActive(false);
				if (this.userStarHoverOn)
				{
					this.userRatingHover.gameObject.SetActive(true);
					this.SetMaskValue(this.userRatingHover, (float)num);
				}
				else
				{
					this.userRatingHover.gameObject.SetActive(false);
				}
			}
			else
			{
				this.unrated.gameObject.SetActive(false);
				this.outline.gameObject.SetActive(true);
				this.publicRating.gameObject.SetActive(false);
				if (this.userStarHoverOn)
				{
					this.userRatingHover.gameObject.SetActive(true);
					this.userRating.gameObject.SetActive(false);
					this.SetMaskValue(this.userRatingHover, (float)num);
				}
				else
				{
					this.userRatingHover.gameObject.SetActive(false);
					this.userRating.gameObject.SetActive(true);
					this.SetMaskValue(this.userRating, (float)this.currentUserRating);
				}
			}
		}
		else if (flag2)
		{
			this.unrated.gameObject.SetActive(false);
			this.outline.gameObject.SetActive(true);
			this.publicRating.gameObject.SetActive(true);
			this.userRating.gameObject.SetActive(false);
			this.userRatingHover.gameObject.SetActive(true);
			this.SetMaskValue(this.publicRating, this.averagePublicRating);
			this.SetMaskValue(this.userRatingHover, (float)num);
		}
		else
		{
			this.unrated.gameObject.SetActive(false);
			this.outline.gameObject.SetActive(true);
			this.publicRating.gameObject.SetActive(true);
			this.SetMaskValue(this.publicRating, this.averagePublicRating);
			if (this.userStarHoverOn)
			{
				this.userRating.gameObject.SetActive(false);
				this.userRatingHover.gameObject.SetActive(true);
				this.SetMaskValue(this.userRatingHover, (float)num);
			}
			else
			{
				this.userRating.gameObject.SetActive(true);
				this.userRatingHover.gameObject.SetActive(false);
				this.SetMaskValue(this.userRating, (float)this.currentUserRating);
			}
		}
	}

	// Token: 0x06002F59 RID: 12121 RVA: 0x0014F310 File Offset: 0x0014D710
	private void SetMaskValue(RectTransform maskTransform, float starValue)
	{
		starValue = Mathf.Round(starValue * 4f) / 4f;
		starValue = Mathf.Max(0.01f, starValue);
		if (this.sizeTransform == null)
		{
			this.sizeTransform = (RectTransform)base.transform;
		}
		Vector2 sizeDelta = new Vector2(starValue * this.sizeTransform.sizeDelta.x / 5f, this.sizeTransform.sizeDelta.y);
		maskTransform.sizeDelta = sizeDelta;
	}

	// Token: 0x06002F5A RID: 12122 RVA: 0x0014F39C File Offset: 0x0014D79C
	private void OnUserRatingChanged()
	{
		if (this.UserStarRatingChanged != null)
		{
			this.UserStarRatingChanged(this.currentUserRating);
		}
	}

	// Token: 0x040027B0 RID: 10160
	public RectTransform unrated;

	// Token: 0x040027B1 RID: 10161
	public RectTransform outline;

	// Token: 0x040027B2 RID: 10162
	public RectTransform publicRating;

	// Token: 0x040027B3 RID: 10163
	public RectTransform userRating;

	// Token: 0x040027B4 RID: 10164
	public RectTransform userRatingHover;

	// Token: 0x040027B5 RID: 10165
	public Color userStarColor;

	// Token: 0x040027B6 RID: 10166
	public Color userStarHoverColor;

	// Token: 0x040027B7 RID: 10167
	private bool userStarHoverOn;

	// Token: 0x040027B8 RID: 10168
	private int currentUserRating;

	// Token: 0x040027B9 RID: 10169
	private float averagePublicRating;

	// Token: 0x040027BA RID: 10170
	private RectTransform sizeTransform;

	// Token: 0x040027BB RID: 10171
	private BitArray hoverFlags = new BitArray(5, false);
}
