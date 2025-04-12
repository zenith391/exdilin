using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000428 RID: 1064
[RequireComponent(typeof(UIStarRating))]
public class UIPanelElementWorldStarRating : UIPanelElement
{
	// Token: 0x06002DCC RID: 11724 RVA: 0x00145D41 File Offset: 0x00144141
	private void Awake()
	{
		this.starRatingUI = base.GetComponent<UIStarRating>();
		this.Clear();
	}

	// Token: 0x06002DCD RID: 11725 RVA: 0x00145D55 File Offset: 0x00144155
	public override void Clear()
	{
		base.Clear();
		this.worldRatingInfo = new BWStarRatingInfo();
		this.starRatingUI.UpdateStarRating(this.worldRatingInfo.averageRating, this.worldRatingInfo.currentUserRating);
	}

	// Token: 0x06002DCE RID: 11726 RVA: 0x00145D89 File Offset: 0x00144189
	public void OnEnable()
	{
		this.starRatingUI.AddUserStarRatingListener(new StarRatingChangedHandler(this.UserSetRating));
	}

	// Token: 0x06002DCF RID: 11727 RVA: 0x00145DA2 File Offset: 0x001441A2
	public void OnDisable()
	{
		this.starRatingUI.RemoveUserStarRatingListener(new StarRatingChangedHandler(this.UserSetRating));
	}

	// Token: 0x06002DD0 RID: 11728 RVA: 0x00145DBC File Offset: 0x001441BC
	public override void Fill(Dictionary<string, string> data)
	{
		this.worldRatingInfo = new BWStarRatingInfo();
		if (data.ContainsKey(this.averageRatingKey))
		{
			string s = data[this.averageRatingKey];
			float averageRating;
			if (float.TryParse(s, out averageRating))
			{
				this.worldRatingInfo.averageRating = averageRating;
			}
		}
		if (data.ContainsKey(this.worldIDKey))
		{
			this.worldID = data[this.worldIDKey];
		}
		base.StartCoroutine(this.ShowRating());
	}

	// Token: 0x06002DD1 RID: 11729 RVA: 0x00145E3B File Offset: 0x0014423B
	private void UserSetRating(int rating)
	{
		base.StartCoroutine(this.UpdateUserRating(rating));
	}

	// Token: 0x06002DD2 RID: 11730 RVA: 0x00145E4C File Offset: 0x0014424C
	private IEnumerator ShowRating()
	{
		float averageRatingBefore = this.worldRatingInfo.averageRating;
		yield return base.StartCoroutine(BWStarRatingManager.Instance.GetUserRatingForWorld(this.worldID, this.worldRatingInfo));
		if (Mathf.Abs(this.worldRatingInfo.averageRating - averageRatingBefore) > 0.125f)
		{
			this.parentPanel.ElementEditedFloat(this.averageRatingKey, this.worldRatingInfo.averageRating);
		}
		this.starRatingUI.UpdateStarRating(this.worldRatingInfo.averageRating, this.worldRatingInfo.currentUserRating);
		yield break;
	}

	// Token: 0x06002DD3 RID: 11731 RVA: 0x00145E68 File Offset: 0x00144268
	private IEnumerator UpdateUserRating(int rating)
	{
		yield return base.StartCoroutine(BWStarRatingManager.Instance.RateWorld(this.worldID, rating, this.worldRatingInfo));
		this.parentPanel.ElementEditedFloat(this.averageRatingKey, this.worldRatingInfo.averageRating);
		this.starRatingUI.UpdateStarRating(this.worldRatingInfo.averageRating, this.worldRatingInfo.currentUserRating);
		yield break;
	}

	// Token: 0x0400264C RID: 9804
	public UIStarRating starRatingUI;

	// Token: 0x0400264D RID: 9805
	public Text noAverageRatingText;

	// Token: 0x0400264E RID: 9806
	private BWStarRatingInfo worldRatingInfo;

	// Token: 0x0400264F RID: 9807
	private string worldIDKey = "world_id";

	// Token: 0x04002650 RID: 9808
	private string averageRatingKey = "average_star_rating";

	// Token: 0x04002651 RID: 9809
	private string worldID;
}
