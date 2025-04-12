using System;
using UnityEngine;

// Token: 0x02000210 RID: 528
public class LeaderboardData : MonoBehaviour
{
	// Token: 0x1700007B RID: 123
	// (get) Token: 0x06001A45 RID: 6725 RVA: 0x000C1A64 File Offset: 0x000BFE64
	private UILeaderboardController GetLeaderboardController
	{
		get
		{
			return Blocksworld.UI.Leaderboard;
		}
	}

	// Token: 0x06001A46 RID: 6726 RVA: 0x000C1A70 File Offset: 0x000BFE70
	public void SetupLeaderboard()
	{
		this.isReset = false;
		this.increaseTimer = WorldSession.canShowLeaderboard();
		if (this.GetLeaderboardController != null)
		{
			if (this.increaseTimer)
			{
				this.GetLeaderboardController.StartSetup();
				this.LoadLeaderboard();
			}
			else
			{
				this.GetLeaderboardController.ClearLeaderboard();
			}
		}
	}

	// Token: 0x06001A47 RID: 6727 RVA: 0x000C1ACC File Offset: 0x000BFECC
	public void UpdateGUI()
	{
		if (this.increaseTimer && Blocksworld.CurrentState == State.Play && this.GetLeaderboardController.readyForTimer)
		{
			if (!this.timePaused)
			{
				this.leaderboardTime += Blocksworld.deltaTime;
			}
			if (!this.temporarilyDisableTimer && this.GetLeaderboardController != null)
			{
				string ourText = this.FormatTime(this.leaderboardTime);
				this.GetLeaderboardController.UpdateTimer(ourText);
			}
		}
	}

	// Token: 0x06001A48 RID: 6728 RVA: 0x000C1B54 File Offset: 0x000BFF54
	public string FormatTime(float time)
	{
		string text = string.Empty;
		int[] array = new int[3];
		TimeTileParameter.CalculateTimeComponents(time, array);
		int num = Mathf.FloorToInt(1000f * time) % 1000;
		array[0] = num;
		string text2 = "0";
		for (int i = 2; i >= 0; i--)
		{
			text += array[i].ToString(text2);
			string text3 = (i != 0) ? ":" : string.Empty;
			text3 = ((i != 1) ? text3 : ".");
			text += text3;
			text2 += "0";
		}
		return text;
	}

	// Token: 0x06001A49 RID: 6729 RVA: 0x000C1C02 File Offset: 0x000C0002
	public void LoadLeaderboard()
	{
		if (WorldSession.isCommunitySession())
		{
			Leaderboard_API.LoadFromRemote(new Leaderboard.LoadFromRemoteSuccessHandler(this.DidLoadLeaderboardSuccess), new Leaderboard.LoadFromRemoteFailureHandler(this.DidLoadLeaderboardFailure));
		}
	}

	// Token: 0x06001A4A RID: 6730 RVA: 0x000C1C2C File Offset: 0x000C002C
	public void DidLoadLeaderboardSuccess(Leaderboard leaderboard)
	{
		if (this.isReset)
		{
			return;
		}
		string builderTime = this.FormatTime(leaderboard.authorTime);
		this.GetLeaderboardController.LoadSetup(leaderboard.leaderboardType, leaderboard.authorUsername, leaderboard.authorId, leaderboard.authorStatus, builderTime, leaderboard.periodicRecords, leaderboard.records);
	}

	// Token: 0x06001A4B RID: 6731 RVA: 0x000C1C82 File Offset: 0x000C0082
	public bool IsAuthor(Leaderboard leaderboard)
	{
		return WorldSession.current.config.currentUserId == leaderboard.authorId;
	}

	// Token: 0x06001A4C RID: 6732 RVA: 0x000C1C9C File Offset: 0x000C009C
	public int GetCurrentUserRank(LeaderboardRecord[] records)
	{
		int result = -1;
		int currentUserId = WorldSession.current.config.currentUserId;
		for (int i = 0; i < records.Length; i++)
		{
			if (records[i].userId == currentUserId)
			{
				if (records[i].time > 0f)
				{
					result = records[i].rank;
				}
				break;
			}
		}
		return result;
	}

	// Token: 0x06001A4D RID: 6733 RVA: 0x000C1D0C File Offset: 0x000C010C
	public void FinishLeaderboard(string winMessage)
	{
		if (this.isReset)
		{
			return;
		}
		if (!this.hasSentData)
		{
			this.gameWinMessage = winMessage;
			this.increaseTimer = false;
			this.hasSentData = true;
			this.GetLeaderboardController.WinScreen(winMessage);
			if (WorldSession.isCommunitySession())
			{
				Leaderboard_API.ReportNewTimeRemote(this.leaderboardTime, new Leaderboard.ReportNewTimeRemoteSuccessHandler(this.DidFinishReportLeaderboarNewTimeSuccess), new Leaderboard.LoadFromRemoteFailureHandler(this.DidLoadLeaderboardFailure));
			}
		}
	}

	// Token: 0x06001A4E RID: 6734 RVA: 0x000C1D7E File Offset: 0x000C017E
	public void LoseCondition()
	{
		this.increaseTimer = false;
		this.GetLeaderboardController.StopTimer();
	}

	// Token: 0x06001A4F RID: 6735 RVA: 0x000C1D94 File Offset: 0x000C0194
	public void TryAgain(bool onLoad)
	{
		if (onLoad)
		{
			Leaderboard_API.LoadFromRemote(new Leaderboard.LoadFromRemoteSuccessHandler(this.DidLoadLeaderboardSuccess), new Leaderboard.LoadFromRemoteFailureHandler(this.DidLoadLeaderboardFailure));
		}
		else if (WorldSession.isCommunitySession())
		{
			Leaderboard_API.ReportNewTimeRemote(this.leaderboardTime, new Leaderboard.ReportNewTimeRemoteSuccessHandler(this.DidFinishReportLeaderboarNewTimeSuccess), new Leaderboard.LoadFromRemoteFailureHandler(this.DidLoadLeaderboardFailure));
		}
	}

	// Token: 0x06001A50 RID: 6736 RVA: 0x000C1DF8 File Offset: 0x000C01F8
	public void DidFinishReportLeaderboarNewTimeSuccess(Leaderboard leaderboard, bool timeImproved)
	{
		if (this.isReset)
		{
			return;
		}
		string playerTime = this.FormatTime(this.leaderboardTime);
		string builderTime = (leaderboard.authorTime != 0f) ? this.FormatTime(leaderboard.authorTime) : string.Empty;
		this.GetLeaderboardController.WinSetup(playerTime, leaderboard.authorUsername, leaderboard.authorId, leaderboard.authorStatus, builderTime, leaderboard.periodicRecords, leaderboard.records);
		if (WorldSession.isCommunitySession() && !this.IsAuthor(leaderboard))
		{
			this.TrackUserAchievement(leaderboard);
		}
	}

	// Token: 0x06001A51 RID: 6737 RVA: 0x000C1E90 File Offset: 0x000C0290
	private void TrackUserAchievement(Leaderboard leaderboard)
	{
		if (leaderboard.periodicRecords != null && leaderboard.periodicRecords.Length >= 100)
		{
			int currentUserRank = this.GetCurrentUserRank(leaderboard.periodicRecords);
			if (currentUserRank > 0 && currentUserRank <= 10)
			{
				WorldSession.platformDelegate.TrackAchievementIncrease("top_ten", 1);
				WorldSession.platformDelegate.TrackAchievementIncrease("checkered_flag", 1);
				return;
			}
		}
		int currentUserRank2 = this.GetCurrentUserRank(leaderboard.records);
		if (currentUserRank2 != 0)
		{
			if (leaderboard.records.Length >= 100 && currentUserRank2 > 0 && currentUserRank2 <= 10)
			{
				WorldSession.platformDelegate.TrackAchievementIncrease("top_ten", 1);
			}
			WorldSession.platformDelegate.TrackAchievementIncrease("checkered_flag", 1);
		}
	}

	// Token: 0x06001A52 RID: 6738 RVA: 0x000C1F45 File Offset: 0x000C0345
	public void DidLoadLeaderboardFailure(string errorMessage)
	{
		if (this.isReset)
		{
			return;
		}
		this.GetLeaderboardController.SendError("Could Not Load Leaderboard", errorMessage);
	}

	// Token: 0x06001A53 RID: 6739 RVA: 0x000C1F64 File Offset: 0x000C0364
	public void PauseLeaderboard(bool isPaused)
	{
		if (this.GetLeaderboardController != null && this.increaseTimer)
		{
			this.GetLeaderboardController.PauseLeaderboard(isPaused);
		}
	}

	// Token: 0x06001A54 RID: 6740 RVA: 0x000C1F8E File Offset: 0x000C038E
	public void ClearWorldSession()
	{
		if (this.GetLeaderboardController != null)
		{
			this.GetLeaderboardController.ClearWorldSessionName();
		}
	}

	// Token: 0x06001A55 RID: 6741 RVA: 0x000C1FAC File Offset: 0x000C03AC
	public void Reset()
	{
		this.isReset = true;
		this.increaseTimer = false;
		this.leaderboardTime = 0f;
		this.hasSentData = false;
		this.hasFaded = false;
		if (this.GetLeaderboardController != null)
		{
			this.GetLeaderboardController.Reset();
		}
	}

	// Token: 0x06001A56 RID: 6742 RVA: 0x000C1FFC File Offset: 0x000C03FC
	public bool IsTimePaused()
	{
		return this.timePaused;
	}

	// Token: 0x06001A57 RID: 6743 RVA: 0x000C2004 File Offset: 0x000C0404
	public void PauseTime(bool pause)
	{
		this.timePaused = pause;
	}

	// Token: 0x06001A58 RID: 6744 RVA: 0x000C200D File Offset: 0x000C040D
	public void AddTime(float time)
	{
		this.leaderboardTime += time;
		this.leaderboardTime = Mathf.Max(this.leaderboardTime, 0f);
	}

	// Token: 0x040015E2 RID: 5602
	public bool temporarilyDisableTimer = true;

	// Token: 0x040015E3 RID: 5603
	private bool increaseTimer;

	// Token: 0x040015E4 RID: 5604
	private bool timePaused;

	// Token: 0x040015E5 RID: 5605
	private float leaderboardTime;

	// Token: 0x040015E6 RID: 5606
	private bool hasSentData;

	// Token: 0x040015E7 RID: 5607
	private bool hasFaded;

	// Token: 0x040015E8 RID: 5608
	private bool isReset = true;

	// Token: 0x040015E9 RID: 5609
	private string timeText = string.Empty;

	// Token: 0x040015EA RID: 5610
	private string gameWinMessage = string.Empty;
}
