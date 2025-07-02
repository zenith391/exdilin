using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIStarRating))]
public class UIPanelElementWorldStarRating : UIPanelElement
{
	public UIStarRating starRatingUI;

	public Text noAverageRatingText;

	private BWStarRatingInfo worldRatingInfo;

	private string worldIDKey = "world_id";

	private string averageRatingKey = "average_star_rating";

	private string worldID;

	private void Awake()
	{
		starRatingUI = GetComponent<UIStarRating>();
		Clear();
	}

	public override void Clear()
	{
		base.Clear();
		worldRatingInfo = new BWStarRatingInfo();
		starRatingUI.UpdateStarRating(worldRatingInfo.averageRating, worldRatingInfo.currentUserRating);
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
		worldRatingInfo = new BWStarRatingInfo();
		if (data.ContainsKey(averageRatingKey))
		{
			string s = data[averageRatingKey];
			if (float.TryParse(s, out var result))
			{
				worldRatingInfo.averageRating = result;
			}
		}
		if (data.ContainsKey(worldIDKey))
		{
			worldID = data[worldIDKey];
		}
		StartCoroutine(ShowRating());
	}

	private void UserSetRating(int rating)
	{
		StartCoroutine(UpdateUserRating(rating));
	}

	private IEnumerator ShowRating()
	{
		float averageRatingBefore = worldRatingInfo.averageRating;
		yield return StartCoroutine(BWStarRatingManager.Instance.GetUserRatingForWorld(worldID, worldRatingInfo));
		if (Mathf.Abs(worldRatingInfo.averageRating - averageRatingBefore) > 0.125f)
		{
			parentPanel.ElementEditedFloat(averageRatingKey, worldRatingInfo.averageRating);
		}
		starRatingUI.UpdateStarRating(worldRatingInfo.averageRating, worldRatingInfo.currentUserRating);
	}

	private IEnumerator UpdateUserRating(int rating)
	{
		yield return StartCoroutine(BWStarRatingManager.Instance.RateWorld(worldID, rating, worldRatingInfo));
		parentPanel.ElementEditedFloat(averageRatingKey, worldRatingInfo.averageRating);
		starRatingUI.UpdateStarRating(worldRatingInfo.averageRating, worldRatingInfo.currentUserRating);
	}
}
