using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000426 RID: 1062
[RequireComponent(typeof(UIStarRating))]
public class UIPanelElementU2UModelStarRating : UIPanelElement
{
	// Token: 0x06002DC0 RID: 11712 RVA: 0x001458FE File Offset: 0x00143CFE
	private void Awake()
	{
		this.starRatingUI = base.GetComponent<UIStarRating>();
		this.Clear();
	}

	// Token: 0x06002DC1 RID: 11713 RVA: 0x00145912 File Offset: 0x00143D12
	public override void Clear()
	{
		base.Clear();
		this.ratingInfo = new BWStarRatingInfo();
		this.starRatingUI.UpdateStarRating(this.ratingInfo.averageRating, this.ratingInfo.currentUserRating);
	}

	// Token: 0x06002DC2 RID: 11714 RVA: 0x00145946 File Offset: 0x00143D46
	public void OnEnable()
	{
		this.starRatingUI.AddUserStarRatingListener(new StarRatingChangedHandler(this.UserSetRating));
	}

	// Token: 0x06002DC3 RID: 11715 RVA: 0x0014595F File Offset: 0x00143D5F
	public void OnDisable()
	{
		this.starRatingUI.RemoveUserStarRatingListener(new StarRatingChangedHandler(this.UserSetRating));
	}

	// Token: 0x06002DC4 RID: 11716 RVA: 0x00145978 File Offset: 0x00143D78
	public override void Fill(Dictionary<string, string> data)
	{
		this.ratingInfo = new BWStarRatingInfo();
		if (data.ContainsKey(this.averageRatingKey))
		{
			string s = data[this.averageRatingKey];
			float averageRating;
			if (float.TryParse(s, out averageRating))
			{
				this.ratingInfo.averageRating = averageRating;
			}
		}
		if (data.ContainsKey(this.modelIDKey))
		{
			this.modelID = data[this.modelIDKey];
		}
		base.StartCoroutine(this.ShowRating());
	}

	// Token: 0x06002DC5 RID: 11717 RVA: 0x001459F7 File Offset: 0x00143DF7
	private void UserSetRating(int rating)
	{
		base.StartCoroutine(this.UpdateUserRating(rating));
	}

	// Token: 0x06002DC6 RID: 11718 RVA: 0x00145A08 File Offset: 0x00143E08
	private IEnumerator ShowRating()
	{
		float averageRatingBefore = this.ratingInfo.averageRating;
		yield return base.StartCoroutine(BWStarRatingManager.Instance.GetUserRatingForModel(this.modelID, this.ratingInfo));
		if (Mathf.Abs(this.ratingInfo.averageRating - averageRatingBefore) > 0.125f)
		{
			this.parentPanel.ElementEditedFloat(this.averageRatingKey, this.ratingInfo.averageRating);
		}
		this.starRatingUI.UpdateStarRating(this.ratingInfo.averageRating, this.ratingInfo.currentUserRating);
		yield break;
	}

	// Token: 0x06002DC7 RID: 11719 RVA: 0x00145A24 File Offset: 0x00143E24
	private IEnumerator UpdateUserRating(int rating)
	{
		yield return base.StartCoroutine(BWStarRatingManager.Instance.RateModel(this.modelID, rating, this.ratingInfo));
		this.parentPanel.ElementEditedFloat(this.averageRatingKey, this.ratingInfo.averageRating);
		this.starRatingUI.UpdateStarRating(this.ratingInfo.averageRating, this.ratingInfo.currentUserRating);
		yield break;
	}

	// Token: 0x04002645 RID: 9797
	public UIStarRating starRatingUI;

	// Token: 0x04002646 RID: 9798
	public Text noAverageRatingText;

	// Token: 0x04002647 RID: 9799
	private BWStarRatingInfo ratingInfo;

	// Token: 0x04002648 RID: 9800
	private string modelIDKey = "model_id";

	// Token: 0x04002649 RID: 9801
	private string averageRatingKey = "average_star_rating";

	// Token: 0x0400264A RID: 9802
	private string modelID;
}
