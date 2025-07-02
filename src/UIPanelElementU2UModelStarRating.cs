using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIStarRating))]
public class UIPanelElementU2UModelStarRating : UIPanelElement
{
	public UIStarRating starRatingUI;

	public Text noAverageRatingText;

	private BWStarRatingInfo ratingInfo;

	private string modelIDKey = "model_id";

	private string averageRatingKey = "average_star_rating";

	private string modelID;

	private void Awake()
	{
		starRatingUI = GetComponent<UIStarRating>();
		Clear();
	}

	public override void Clear()
	{
		base.Clear();
		ratingInfo = new BWStarRatingInfo();
		starRatingUI.UpdateStarRating(ratingInfo.averageRating, ratingInfo.currentUserRating);
	}

	public void OnEnable()
	{
		starRatingUI.AddUserStarRatingListener(UserSetRating);
	}

	public void OnDisable()
	{
		starRatingUI.RemoveUserStarRatingListener(UserSetRating);
	}

	public override void Fill(Dictionary<string, string> data)
	{
		ratingInfo = new BWStarRatingInfo();
		if (data.ContainsKey(averageRatingKey))
		{
			string s = data[averageRatingKey];
			if (float.TryParse(s, out var result))
			{
				ratingInfo.averageRating = result;
			}
		}
		if (data.ContainsKey(modelIDKey))
		{
			modelID = data[modelIDKey];
		}
		StartCoroutine(ShowRating());
	}

	private void UserSetRating(int rating)
	{
		StartCoroutine(UpdateUserRating(rating));
	}

	private IEnumerator ShowRating()
	{
		float averageRatingBefore = ratingInfo.averageRating;
		yield return StartCoroutine(BWStarRatingManager.Instance.GetUserRatingForModel(modelID, ratingInfo));
		if (Mathf.Abs(ratingInfo.averageRating - averageRatingBefore) > 0.125f)
		{
			parentPanel.ElementEditedFloat(averageRatingKey, ratingInfo.averageRating);
		}
		starRatingUI.UpdateStarRating(ratingInfo.averageRating, ratingInfo.currentUserRating);
	}

	private IEnumerator UpdateUserRating(int rating)
	{
		yield return StartCoroutine(BWStarRatingManager.Instance.RateModel(modelID, rating, ratingInfo));
		parentPanel.ElementEditedFloat(averageRatingKey, ratingInfo.averageRating);
		starRatingUI.UpdateStarRating(ratingInfo.averageRating, ratingInfo.currentUserRating);
	}
}
