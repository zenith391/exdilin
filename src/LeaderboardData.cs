using UnityEngine;

public class LeaderboardData : MonoBehaviour
{
	public bool temporarilyDisableTimer = true;

	private bool increaseTimer;

	private bool timePaused;

	private float leaderboardTime;

	private bool hasSentData;

	private bool hasFaded;

	private bool isReset = true;

	private string timeText = string.Empty;

	private string gameWinMessage = string.Empty;

	private UILeaderboardController GetLeaderboardController => Blocksworld.UI.Leaderboard;

	public void SetupLeaderboard()
	{
		isReset = false;
		increaseTimer = WorldSession.canShowLeaderboard();
		if (GetLeaderboardController != null)
		{
			if (increaseTimer)
			{
				GetLeaderboardController.StartSetup();
				LoadLeaderboard();
			}
			else
			{
				GetLeaderboardController.ClearLeaderboard();
			}
		}
	}

	public void UpdateGUI()
	{
		if (increaseTimer && Blocksworld.CurrentState == State.Play && GetLeaderboardController.readyForTimer)
		{
			if (!timePaused)
			{
				leaderboardTime += Blocksworld.deltaTime;
			}
			if (!temporarilyDisableTimer && GetLeaderboardController != null)
			{
				string ourText = FormatTime(leaderboardTime);
				GetLeaderboardController.UpdateTimer(ourText);
			}
		}
	}

	public string FormatTime(float time)
	{
		string text = string.Empty;
		int[] array = new int[3];
		TimeTileParameter.CalculateTimeComponents(time, array);
		int num = Mathf.FloorToInt(1000f * time) % 1000;
		array[0] = num;
		string text2 = "0";
		for (int num2 = 2; num2 >= 0; num2--)
		{
			text += array[num2].ToString(text2);
			string text3 = ((num2 != 0) ? ":" : string.Empty);
			text3 = ((num2 != 1) ? text3 : ".");
			text += text3;
			text2 += "0";
		}
		return text;
	}

	public void LoadLeaderboard()
	{
		if (WorldSession.isCommunitySession())
		{
			Leaderboard_API.LoadFromRemote(DidLoadLeaderboardSuccess, DidLoadLeaderboardFailure);
		}
	}

	public void DidLoadLeaderboardSuccess(Leaderboard leaderboard)
	{
		if (!isReset)
		{
			string builderTime = FormatTime(leaderboard.authorTime);
			GetLeaderboardController.LoadSetup(leaderboard.leaderboardType, leaderboard.authorUsername, leaderboard.authorId, leaderboard.authorStatus, builderTime, leaderboard.periodicRecords, leaderboard.records);
		}
	}

	public bool IsAuthor(Leaderboard leaderboard)
	{
		return WorldSession.current.config.currentUserId == leaderboard.authorId;
	}

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

	public void FinishLeaderboard(string winMessage)
	{
		if (!isReset && !hasSentData)
		{
			gameWinMessage = winMessage;
			increaseTimer = false;
			hasSentData = true;
			GetLeaderboardController.WinScreen(winMessage);
			if (WorldSession.isCommunitySession())
			{
				Leaderboard_API.ReportNewTimeRemote(leaderboardTime, DidFinishReportLeaderboarNewTimeSuccess, DidLoadLeaderboardFailure);
			}
		}
	}

	public void LoseCondition()
	{
		increaseTimer = false;
		GetLeaderboardController.StopTimer();
	}

	public void TryAgain(bool onLoad)
	{
		if (onLoad)
		{
			Leaderboard_API.LoadFromRemote(DidLoadLeaderboardSuccess, DidLoadLeaderboardFailure);
		}
		else if (WorldSession.isCommunitySession())
		{
			Leaderboard_API.ReportNewTimeRemote(leaderboardTime, DidFinishReportLeaderboarNewTimeSuccess, DidLoadLeaderboardFailure);
		}
	}

	public void DidFinishReportLeaderboarNewTimeSuccess(Leaderboard leaderboard, bool timeImproved)
	{
		if (!isReset)
		{
			string playerTime = FormatTime(leaderboardTime);
			string builderTime = ((leaderboard.authorTime != 0f) ? FormatTime(leaderboard.authorTime) : string.Empty);
			GetLeaderboardController.WinSetup(playerTime, leaderboard.authorUsername, leaderboard.authorId, leaderboard.authorStatus, builderTime, leaderboard.periodicRecords, leaderboard.records);
			if (WorldSession.isCommunitySession() && !IsAuthor(leaderboard))
			{
				TrackUserAchievement(leaderboard);
			}
		}
	}

	private void TrackUserAchievement(Leaderboard leaderboard)
	{
		if (leaderboard.periodicRecords != null && leaderboard.periodicRecords.Length >= 100)
		{
			int currentUserRank = GetCurrentUserRank(leaderboard.periodicRecords);
			if (currentUserRank > 0 && currentUserRank <= 10)
			{
				WorldSession.platformDelegate.TrackAchievementIncrease("top_ten", 1);
				WorldSession.platformDelegate.TrackAchievementIncrease("checkered_flag", 1);
				return;
			}
		}
		int currentUserRank2 = GetCurrentUserRank(leaderboard.records);
		if (currentUserRank2 != 0)
		{
			if (leaderboard.records.Length >= 100 && currentUserRank2 > 0 && currentUserRank2 <= 10)
			{
				WorldSession.platformDelegate.TrackAchievementIncrease("top_ten", 1);
			}
			WorldSession.platformDelegate.TrackAchievementIncrease("checkered_flag", 1);
		}
	}

	public void DidLoadLeaderboardFailure(string errorMessage)
	{
		if (!isReset)
		{
			GetLeaderboardController.SendError("Could Not Load Leaderboard", errorMessage);
		}
	}

	public void PauseLeaderboard(bool isPaused)
	{
		if (GetLeaderboardController != null && increaseTimer)
		{
			GetLeaderboardController.PauseLeaderboard(isPaused);
		}
	}

	public void ClearWorldSession()
	{
		if (GetLeaderboardController != null)
		{
			GetLeaderboardController.ClearWorldSessionName();
		}
	}

	public void Reset()
	{
		isReset = true;
		increaseTimer = false;
		leaderboardTime = 0f;
		hasSentData = false;
		hasFaded = false;
		if (GetLeaderboardController != null)
		{
			GetLeaderboardController.Reset();
		}
	}

	public bool IsTimePaused()
	{
		return timePaused;
	}

	public void PauseTime(bool pause)
	{
		timePaused = pause;
	}

	public void AddTime(float time)
	{
		leaderboardTime += time;
		leaderboardTime = Mathf.Max(leaderboardTime, 0f);
	}
}
