using System.Collections;
using UnityEngine;

public class UIStarRating : MonoBehaviour
{
	public RectTransform unrated;

	public RectTransform outline;

	public RectTransform publicRating;

	public RectTransform userRating;

	public RectTransform userRatingHover;

	public Color userStarColor;

	public Color userStarHoverColor;

	private bool userStarHoverOn;

	private int currentUserRating;

	private float averagePublicRating;

	private RectTransform sizeTransform;

	private BitArray hoverFlags = new BitArray(5, defaultValue: false);

	private event StarRatingChangedHandler UserStarRatingChanged;

	public void PointerEnteredStar(int starNumber)
	{
		for (int i = 1; i <= 5; i++)
		{
			hoverFlags[i - 1] = i == starNumber;
		}
		userStarHoverOn = true;
		UpdateUI();
	}

	public void PointerLeftStar(int starNumber)
	{
		hoverFlags[starNumber - 1] = false;
		UpdateUI();
	}

	public void ClickedOnStar(int starNumber)
	{
		currentUserRating = starNumber;
		if (this.UserStarRatingChanged != null)
		{
			this.UserStarRatingChanged(currentUserRating);
		}
		userStarHoverOn = false;
		UpdateUI();
	}

	public void AddUserStarRatingListener(StarRatingChangedHandler listener)
	{
		UserStarRatingChanged -= listener;
		UserStarRatingChanged += listener;
	}

	public void RemoveUserStarRatingListener(StarRatingChangedHandler listener)
	{
		UserStarRatingChanged -= listener;
	}

	public void UpdateStarRating(float averageRating, int userRating)
	{
		averagePublicRating = averageRating;
		currentUserRating = userRating;
		UpdateUI();
	}

	private int HoverStar()
	{
		for (int i = 1; i <= 5; i++)
		{
			if (hoverFlags[i - 1])
			{
				return i;
			}
		}
		return 0;
	}

	private void UpdateUI()
	{
		bool flag = averagePublicRating < 1f;
		bool flag2 = (float)currentUserRating < 1f;
		int num = HoverStar();
		userStarHoverOn &= num > 0;
		if (flag)
		{
			if (flag2)
			{
				unrated.gameObject.SetActive(value: true);
				publicRating.gameObject.SetActive(value: false);
				userRating.gameObject.SetActive(value: false);
				outline.gameObject.SetActive(value: false);
				if (userStarHoverOn)
				{
					userRatingHover.gameObject.SetActive(value: true);
					SetMaskValue(userRatingHover, num);
				}
				else
				{
					userRatingHover.gameObject.SetActive(value: false);
				}
				return;
			}
			unrated.gameObject.SetActive(value: false);
			outline.gameObject.SetActive(value: true);
			publicRating.gameObject.SetActive(value: false);
			if (userStarHoverOn)
			{
				userRatingHover.gameObject.SetActive(value: true);
				userRating.gameObject.SetActive(value: false);
				SetMaskValue(userRatingHover, num);
			}
			else
			{
				userRatingHover.gameObject.SetActive(value: false);
				userRating.gameObject.SetActive(value: true);
				SetMaskValue(userRating, currentUserRating);
			}
		}
		else if (flag2)
		{
			unrated.gameObject.SetActive(value: false);
			outline.gameObject.SetActive(value: true);
			publicRating.gameObject.SetActive(value: true);
			userRating.gameObject.SetActive(value: false);
			userRatingHover.gameObject.SetActive(value: true);
			SetMaskValue(publicRating, averagePublicRating);
			SetMaskValue(userRatingHover, num);
		}
		else
		{
			unrated.gameObject.SetActive(value: false);
			outline.gameObject.SetActive(value: true);
			publicRating.gameObject.SetActive(value: true);
			SetMaskValue(publicRating, averagePublicRating);
			if (userStarHoverOn)
			{
				userRating.gameObject.SetActive(value: false);
				userRatingHover.gameObject.SetActive(value: true);
				SetMaskValue(userRatingHover, num);
			}
			else
			{
				userRating.gameObject.SetActive(value: true);
				userRatingHover.gameObject.SetActive(value: false);
				SetMaskValue(userRating, currentUserRating);
			}
		}
	}

	private void SetMaskValue(RectTransform maskTransform, float starValue)
	{
		starValue = Mathf.Round(starValue * 4f) / 4f;
		starValue = Mathf.Max(0.01f, starValue);
		if (sizeTransform == null)
		{
			sizeTransform = (RectTransform)base.transform;
		}
		Vector2 sizeDelta = new Vector2(starValue * sizeTransform.sizeDelta.x / 5f, sizeTransform.sizeDelta.y);
		maskTransform.sizeDelta = sizeDelta;
	}

	private void OnUserRatingChanged()
	{
		if (this.UserStarRatingChanged != null)
		{
			this.UserStarRatingChanged(currentUserRating);
		}
	}
}
