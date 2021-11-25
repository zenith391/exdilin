using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000306 RID: 774
public class UILeaderboardController : MonoBehaviour
{
	// Token: 0x060022CF RID: 8911 RVA: 0x001029A4 File Offset: 0x00100DA4
	public void Init()
	{
		this.lbOverlay.gameObject.SetActive(false);
		this.timerDisplay.gameObject.SetActive(true);
		this.yStart = this.timerBackground.rectTransform.rect.height;
		this.timerDisplay.anchoredPosition = new Vector3(0f, this.yStart, 0f);
		this.timerDisplay.gameObject.SetActive(false);
		UILeaderboardController.UIState = LBGUIState.Reset;
		this.LoadState = LBGUILoad.Loading;
		for (int i = 0; i < this.builderNameTexts.Length; i++)
		{
			Button component = this.builderNameTexts[i].GetComponent<Button>();
			if (component != null)
			{
				component.onClick.AddListener(new UnityAction(this.VisitCreatorProfile));
			}
		}
		this.canvasScaler = base.GetComponent<CanvasScaler>();
		this.SetScale();
		ViewportWatchdog.AddListener(new ViewportWatchdog.ViewportSizeChangedAction(this.ViewportSizeDidChange));
	}

	// Token: 0x060022D0 RID: 8912 RVA: 0x00102AA1 File Offset: 0x00100EA1
	public bool IsVisible()
	{
		return UILeaderboardController.UIState != LBGUIState.Reset;
	}

	// Token: 0x060022D1 RID: 8913 RVA: 0x00102AAE File Offset: 0x00100EAE
	private void ViewportSizeDidChange()
	{
		this.SetScale();
	}

	// Token: 0x060022D2 RID: 8914 RVA: 0x00102AB6 File Offset: 0x00100EB6
	private void SetScale()
	{
		this.canvasScaler.scaleFactor = NormalizedScreen.pixelScale;
	}

	// Token: 0x060022D3 RID: 8915 RVA: 0x00102AC8 File Offset: 0x00100EC8
	public void GetUIObjects(List<GameObject> objectList)
	{
		objectList.Add(this.lbOverlay.gameObject);
	}

	// Token: 0x060022D4 RID: 8916 RVA: 0x00102ADB File Offset: 0x00100EDB
	public void SendError(string serverErrorTitle, string serverMessage)
	{
		this.LoadState = LBGUILoad.Error;
		this.errorTitleText.text = serverErrorTitle;
		this.errorMessageText.text = serverMessage;
		this.WipeLeaderboard();
		this.UpdateNetworkLoading();
	}

	// Token: 0x060022D5 RID: 8917 RVA: 0x00102B08 File Offset: 0x00100F08
	public void ClearLeaderboard()
	{
		this.LoadState = LBGUILoad.Done;
		UILeaderboardController.UIState = LBGUIState.Reset;
		this.UpdateScreens();
	}

	// Token: 0x060022D6 RID: 8918 RVA: 0x00102B20 File Offset: 0x00100F20
	public void StartSetup()
	{
		this.isAuthor = WorldSession.current.isWorldAuthorCurrentUser;
		this.LoadState = LBGUILoad.Loading;
		this.worldNameText.text = WorldSession.current.worldTitle;
		Blocksworld.UI.SetControlsCanvasVisible(true);
		if (this.lastWorldID == WorldSession.current.worldId)
		{
			UILeaderboardController.UIState = LBGUIState.Game;
			this.UpdateScreens();
		}
		else
		{
			base.StartCoroutine("PauseWorld");
		}
	}

	// Token: 0x060022D7 RID: 8919 RVA: 0x00102B9C File Offset: 0x00100F9C
	private IEnumerator PauseWorld()
	{
		for (int i = 0; i < 2; i++)
		{
			if (UILeaderboardController.UIState == LBGUIState.WinScreen)
			{
				yield break;
			}
			yield return 0;
		}
		UILeaderboardController.UIState = LBGUIState.Start;
		WorldSession.current.EnterLeaderboardStartScreen();
		this.UpdateScreens();
		yield break;
	}

	// Token: 0x060022D8 RID: 8920 RVA: 0x00102BB8 File Offset: 0x00100FB8
	private void UpdateBuilderInfo(string builderName, int builderId, int builderBlocksworldPremium, string builderTime)
	{
		this.worldCreatorName = builderName;
		this.worldCreatorId = builderId;
		string text = (builderBlocksworldPremium <= 0) ? "lightblue" : "lightblue";
		builderName = ((!string.IsNullOrEmpty(builderName)) ? string.Concat(new string[]
		{
			"Creator <color=",
			text,
			">",
			builderName,
			"</color>"
		}) : ("<color=" + text + ">Creator</color>"));
		string text2 = (!string.IsNullOrEmpty(builderTime) && !(builderTime == "0:00.000")) ? (builderName + "'s best time is <color=lightblue>" + builderTime + "</color>") : (builderName + " hasn’t posted a time.");
		for (int i = 0; i < this.builderNameTexts.Length; i++)
		{
			this.builderNameTexts[i].text = text2;
		}
	}

	// Token: 0x060022D9 RID: 8921 RVA: 0x00102CA0 File Offset: 0x001010A0
	private void VisitCreatorProfile()
	{
		if (string.IsNullOrEmpty(this.worldCreatorName) || this.worldCreatorId == 0)
		{
			return;
		}
		UIDialog dialog = Blocksworld.UI.Dialog;
		string mainText = string.Format("Visit {0}'s page?", this.worldCreatorName);
		string[] buttonLabels = new string[]
		{
			"Ok",
			"No thanks"
		};
		Action[] array = new Action[2];
		array[0] = delegate()
		{
			WorldSession.QuitWithDeepLink(string.Format("deep-link/profile/{0}", this.worldCreatorId.ToString()));
		};
		array[1] = delegate()
		{
			Blocksworld.UI.Dialog.CloseActiveDialog();
		};
		dialog.ShowGenericDialog(mainText, buttonLabels, array);
	}

	// Token: 0x060022DA RID: 8922 RVA: 0x00102D33 File Offset: 0x00101133
	private void UpdateLeaderboardType(LeaderboardType leaderboardType)
	{
		if (leaderboardType != LeaderboardType.LongestTime)
		{
			if (leaderboardType != LeaderboardType.ShortestTime)
			{
			}
			this.leaderboardTypeText.text = "Fastest Time";
		}
		else
		{
			this.leaderboardTypeText.text = "Longest Time";
		}
	}

	// Token: 0x060022DB RID: 8923 RVA: 0x00102D71 File Offset: 0x00101171
	public void LoadSetup(LeaderboardType leaderboardType, string builderName, int builderId, int builderBlocksworldPremium, string builderTime, LeaderboardRecord[] weeklyRecords, LeaderboardRecord[] allTimeRecords)
	{
		this.LoadState = LBGUILoad.Done;
		this.UpdateLeaderboardType(leaderboardType);
		this.UpdateBuilderInfo(builderName, builderId, builderBlocksworldPremium, builderTime);
		this.storedWeeklyRecords = weeklyRecords;
		this.storedAllTimeRecords = allTimeRecords;
		this.CheckWeeklyRecordForNull();
		this.UpdateNetworkLoading();
	}

	// Token: 0x060022DC RID: 8924 RVA: 0x00102DAC File Offset: 0x001011AC
	public void WinScreen(string ourWinMessage)
	{
		Blocksworld.SetVRMode(false);
		this.WipeLeaderboard();
		Blocksworld.UI.SetControlsCanvasVisible(false);
		this.LoadState = LBGUILoad.Loading;
		string text = this.timerTimecode.text;
		this.winningTitle.text = "Awesome! Your time is <color=lightblue>" + text + "</color>";
		UILeaderboardController.UIState = LBGUIState.WinScreen;
		this.UpdateScreens();
		WorldSession.current.EnterLeaderboardWinScreen();
		this.winFadeGroup.gameObject.SetActive(true);
		base.StartCoroutine("FadeInGUI");
	}

	// Token: 0x060022DD RID: 8925 RVA: 0x00102E31 File Offset: 0x00101231
	public void WinSetup(string playerTime, string builderName, int builderId, int builderBlocksworldPremium, string builderTime, LeaderboardRecord[] weeklyRecords, LeaderboardRecord[] allTimeRecords)
	{
		this.LoadState = LBGUILoad.Done;
		this.UpdateBuilderInfo(builderName, builderId, builderBlocksworldPremium, builderTime);
		this.storedWeeklyRecords = weeklyRecords;
		this.storedAllTimeRecords = allTimeRecords;
		this.CheckWeeklyRecordForNull();
		this.UpdateNetworkLoading();
	}

	// Token: 0x060022DE RID: 8926 RVA: 0x00102E64 File Offset: 0x00101264
	private void CheckWeeklyRecordForNull()
	{
		bool flag = this.storedWeeklyRecords == null || this.storedWeeklyRecords.Length == 0;
		LeaderboardRecord[] leaderboardRecords = (!flag) ? this.storedWeeklyRecords : this.storedAllTimeRecords;
		this.UpdateTabs(!flag);
		this.UpdateScrollableLeaderboard(leaderboardRecords);
	}

	// Token: 0x060022DF RID: 8927 RVA: 0x00102EB4 File Offset: 0x001012B4
	private void UpdateNetworkLoading()
	{
		this.loadingSpinner.gameObject.SetActive(this.LoadState == LBGUILoad.Loading);
		this.errorPanel.SetActive(this.LoadState == LBGUILoad.Error);
	}

	// Token: 0x060022E0 RID: 8928 RVA: 0x00102EE3 File Offset: 0x001012E3
	private bool AnyGameControlsActive()
	{
		return Blocksworld.UI.Controls.AnyControlActive() || Blocksworld.bw.IsPullingObject() || Blocksworld.bw.HadObjectTapping();
	}

	// Token: 0x060022E1 RID: 8929 RVA: 0x00102F18 File Offset: 0x00101318
	public void Update()
	{
		if (UILeaderboardController.UIState == LBGUIState.Start && Blocksworld.UI.Controls != null && Blocksworld.UI.Controls.AnyControlActive())
		{
			this.ProcessButtonPress("RealPlay");
		}
		else if (UILeaderboardController.UIState == LBGUIState.Start && Input.anyKeyDown && !Input.GetMouseButton(0) && Input.touches.Length == 0)
		{
			this.ProcessButtonPress("RealPlay");
		}
		if (!this.readyForTimer && UILeaderboardController.UIState == LBGUIState.Game && this.AnyGameControlsActive())
		{
			this.readyForTimer = true;
		}
	}

	// Token: 0x060022E2 RID: 8930 RVA: 0x00102FC8 File Offset: 0x001013C8
	private void UpdateScreens()
	{
		this.UpdateNetworkLoading();
		bool flag = UILeaderboardController.UIState == LBGUIState.Start;
		bool flag2 = UILeaderboardController.UIState == LBGUIState.Pause;
		bool flag3 = UILeaderboardController.UIState == LBGUIState.WinScreen;
		bool flag4 = UILeaderboardController.UIState == LBGUIState.Share;
		bool active = flag || flag3 || flag2;
		this.buttonBackground.SetActive(flag);
		this.leaderboardScrollParent.SetActive(active);
		this.startGroup.SetActive(flag);
		this.resultPanel.SetActive(flag3);
		this.sharePanel.SetActive(flag4);
		this.fixedButtons.SetActive(flag4 || flag3);
		this.lbOverlay.gameObject.SetActive(flag3);
		Canvas.ForceUpdateCanvases();
	}

	// Token: 0x060022E3 RID: 8931 RVA: 0x00103079 File Offset: 0x00101479
	public void StopTimer()
	{
		base.StopCoroutine("SlideInTimer");
		base.StartCoroutine("SlideInTimer", false);
	}

	// Token: 0x060022E4 RID: 8932 RVA: 0x00103098 File Offset: 0x00101498
	public IEnumerator SlideInTimer(bool down)
	{
		this.timerDisplay.gameObject.SetActive(true);
		float directionMod = (!down) ? -100f : 100f;
		float yGoal = (!down) ? this.yStart : 0f;
		while (this.timerDisplay.anchoredPosition.y != yGoal)
		{
			if (this.timerDisplay.gameObject.activeSelf)
			{
				Vector2 timerPos = Vector2.up * (Blocksworld.deltaTime * directionMod);
				Vector2 timerPosVec = this.timerDisplay.anchoredPosition - timerPos;
				timerPosVec.y = Mathf.Clamp(timerPosVec.y, 0f, this.yStart);
				this.timerDisplay.anchoredPosition = timerPosVec;
				yield return null;
			}
		}
		if (!down)
		{
			this.timerDisplay.gameObject.SetActive(false);
		}
		yield break;
	}

	// Token: 0x060022E5 RID: 8933 RVA: 0x001030BC File Offset: 0x001014BC
	public IEnumerator FadeInGUI()
	{
		this.winFadeGroup.alpha = 0f;
		this.lbOverlay.color = new Color(0f, 0f, 0f, 0f);
		yield return new WaitForSeconds(1f);
		base.StopCoroutine("SlideInTimer");
		base.StartCoroutine("SlideInTimer", false);
		while (this.winFadeGroup.alpha < 1f)
		{
			float alphaAdd = Blocksworld.deltaTime;
			this.winFadeGroup.alpha += alphaAdd;
			this.lbOverlay.color = new Color(0f, 0f, 0f, this.winFadeGroup.alpha * 0.75f);
			yield return null;
		}
		yield break;
	}

	// Token: 0x060022E6 RID: 8934 RVA: 0x001030D8 File Offset: 0x001014D8
	private void UpdateTabs(bool isWeek)
	{
		this.onWeekTab = isWeek;
		if (!this.tabRoot.activeSelf)
		{
			this.tabRoot.SetActive(true);
		}
		Color color = (!isWeek) ? new Color(1f, 0.4f, 0f) : new Color(0.17f, 0.77f, 1f);
		this.leaderboardOutline.color = color;
	}

	// Token: 0x060022E7 RID: 8935 RVA: 0x00103148 File Offset: 0x00101548
	public void ProcessButtonPress(string buttonCommand)
	{
		if (!WorldSession.isProfileBuildSession())
		{
			switch (buttonCommand)
			{
			case "AllTimeTab":
				if (this.onWeekTab)
				{
					this.UpdateTabs(false);
					this.UpdateScrollableLeaderboard(this.storedAllTimeRecords);
				}
				return;
			case "WeeklyTab":
				if (!this.onWeekTab)
				{
					this.UpdateTabs(true);
					this.UpdateScrollableLeaderboard(this.storedWeeklyRecords);
				}
				return;
			case "RealPlay":
				UILeaderboardController.UIState = LBGUIState.Game;
				this.UpdateScreens();
				WorldSession.current.ExitLeaderboardStartScreen();
				return;
			case "Share":
			case "Facebook":
			case "SMS":
			case "Email":
				if (this.LoadState != LBGUILoad.Error)
				{
					string shareMessage = this.GetShareMessage();
					Canvas component = base.GetComponent<Canvas>();
					Rect shareRect = RectTransformUtility.PixelAdjustRect(this.shareButton, component);
					shareRect.center = this.shareButton.anchoredPosition;
					WorldSession.platformDelegate.ShowSharingPopupWithMessage(shareRect, shareMessage);
					BWLog.Info("Sending share popup message: " + shareMessage);
				}
				return;
			case "Retry":
				this.LoadState = LBGUILoad.Loading;
				this.UpdateNetworkLoading();
				if (Blocksworld.leaderboardData != null)
				{
					Blocksworld.leaderboardData.TryAgain(UILeaderboardController.UIState == LBGUIState.Start || UILeaderboardController.UIState == LBGUIState.Pause);
				}
				else
				{
					BWLog.Info("Couldn't find leaderboard data");
				}
				return;
			case "Leaderboard":
				UILeaderboardController.UIState = LBGUIState.WinScreen;
				this.UpdateScreens();
				return;
			case "Restart":
				Blocksworld.bw.ButtonRestartTapped();
				return;
			}
			BWLog.Info("No button functionality for: " + buttonCommand);
		}
	}

	// Token: 0x060022E8 RID: 8936 RVA: 0x00103374 File Offset: 0x00101774
	private void UpdateScrollableLeaderboard(LeaderboardRecord[] leaderboardRecords)
	{
		this.WipeScoreText();
		this.firstTimeText.text = ((!this.isAuthor) ? this.firstTimeString : (this.firstTimeString + "\n" + this.authorFirstTimeString));
		if (leaderboardRecords == null || leaderboardRecords.Length == 0)
		{
			this.firstTimeText.gameObject.SetActive(true);
			this.winningTitle.text = "Awesome! Your time is <color=lightblue>" + this.timerTimecode.text + "</color>";
			return;
		}
		this.firstTimeText.gameObject.SetActive(leaderboardRecords.Length == 0);
		int num = Blocksworld.leaderboardData.GetCurrentUserRank(leaderboardRecords);
		string str = (num != 1) ? "Awesome!" : "You got first place!";
		string text = this.timerTimecode.text;
		this.winningTitle.text = str + " Your time is <color=lightblue>" + text + "</color>";
		Color color = new Color(0.6f, 0.9f, 1f);
		Color color2 = new Color(0.8f, 0.8f, 0.8f);
		for (int i = 0; i < leaderboardRecords.Length; i++)
		{
			LeaderboardRecord leaderboardRecord = leaderboardRecords[i];
			bool flag = i == num - 1;
			bool flag2 = i == leaderboardRecords.Length - 1;
			UILeaderboardEntry uileaderboardEntry = UnityEngine.Object.Instantiate<UILeaderboardEntry>(this.leaderboardEntryTemplate);
			UIUserProfileLink userProfileLink = uileaderboardEntry.userProfileLink;
			bool flag3 = i < 5 || flag;
			userProfileLink.SetupForUser(leaderboardRecord.username, leaderboardRecord.userId, flag);
			uileaderboardEntry.gameObject.SetActive(true);
			RectTransform rectTransform = (RectTransform)uileaderboardEntry.transform;
			rectTransform.SetParent(this.contentRoot, true);
			rectTransform.localScale = Vector3.one;
			if (flag3)
			{
				userProfileLink.LoadProfileImage(leaderboardRecord.userProfileImageUrl);
			}
			this.leaderboardEntries.Add(uileaderboardEntry);
			uileaderboardEntry.userRankText.color = ((!flag) ? color2 : Color.white);
			uileaderboardEntry.userRankText.text = (i + 1).ToString();
			uileaderboardEntry.userScoreText.color = ((!flag) ? color : Color.white);
			uileaderboardEntry.userScoreText.text = Blocksworld.leaderboardData.FormatTime(leaderboardRecord.time);
			uileaderboardEntry.playerBackground.gameObject.SetActive(false);
		}
		float num2 = (float)Mathf.Max(1, leaderboardRecords.Length);
		float startScrollVal = 1f;
		float rankVal = -1f;
		if (num > 0 && num <= this.leaderboardEntries.Count)
		{
			this.leaderboardEntries[num - 1].playerBackground.gameObject.SetActive(true);
			this.leaderboardEntries[num - 1].playerBackground.color = this.leaderboardOutline.color;
			num = Mathf.Max(0, num - 1);
			startScrollVal = Mathf.Abs((float)(num + 1) - num2) / (num2 - 1f);
			float num3 = num2 + 0.625f;
			rankVal = (float)num / num3;
		}
		base.StartCoroutine(this.UpdateScrollbar(startScrollVal, rankVal));
	}

	// Token: 0x060022E9 RID: 8937 RVA: 0x001036A0 File Offset: 0x00101AA0
	private IEnumerator UpdateScrollbar(float startScrollVal, float rankVal)
	{
		yield return new WaitForEndOfFrame();
		this.leaderboardScrollbar.value = startScrollVal;
		yield break;
	}

	// Token: 0x060022EA RID: 8938 RVA: 0x001036C4 File Offset: 0x00101AC4
	public void UpdateTimer(string ourText)
	{
		if (!this.loadingSpinner.gameObject.activeSelf)
		{
			this.loadingSpinner.Rotate(0f, 0f, Time.deltaTime);
		}
		if (!this.timerDisplay.gameObject.activeSelf)
		{
			this.timerDisplay.gameObject.SetActive(true);
			this.timerDisplay.anchoredPosition = new Vector2(0f, this.yStart);
			base.StartCoroutine("SlideInTimer", true);
			Blocksworld.UI.Controls.SetupActiveControlsTimer();
		}
		this.timerTimecode.text = ourText;
	}

	// Token: 0x060022EB RID: 8939 RVA: 0x00103770 File Offset: 0x00101B70
	public void PauseLeaderboard(bool isPaused)
	{
		if (UILeaderboardController.UIState == LBGUIState.Reset)
		{
			return;
		}
		if (isPaused)
		{
			this.StoreUIState = UILeaderboardController.UIState;
			UILeaderboardController.UIState = LBGUIState.Pause;
		}
		else
		{
			UILeaderboardController.UIState = this.StoreUIState;
		}
		if (UILeaderboardController.UIState == LBGUIState.WinScreen && !isPaused)
		{
			Blocksworld.UI.SetControlsCanvasVisible(false);
		}
		this.UpdateScreens();
	}

	// Token: 0x060022EC RID: 8940 RVA: 0x001037D4 File Offset: 0x00101BD4
	public void Reset()
	{
		this.lastWorldID = WorldSession.current.worldId;
		UILeaderboardController.UIState = LBGUIState.Reset;
		this.LoadState = LBGUILoad.Done;
		Vector3 v = Vector3.up * this.yStart;
		base.StopAllCoroutines();
		this.timerDisplay.anchoredPosition = v;
		this.timerDisplay.gameObject.SetActive(false);
		this.readyForTimer = false;
		this.WipeLeaderboard();
		this.UpdateScreens();
		this.isAuthor = false;
	}

	// Token: 0x060022ED RID: 8941 RVA: 0x00103851 File Offset: 0x00101C51
	public void ClearWorldSessionName()
	{
		this.lastWorldID = string.Empty;
		UILeaderboardController.UIState = LBGUIState.Reset;
		this.UpdateScreens();
	}

	// Token: 0x060022EE RID: 8942 RVA: 0x0010386C File Offset: 0x00101C6C
	private string GetFormattedPlace(int ourRank)
	{
		int num = ourRank % 10;
		bool flag = ourRank / 10 == 1;
		string str = (!flag) ? ((num != 1) ? ((num != 2) ? ((num != 3) ? "th" : "rd") : "nd") : "st") : "th";
		return ourRank.ToString() + str;
	}

	// Token: 0x060022EF RID: 8943 RVA: 0x001038E4 File Offset: 0x00101CE4
	private string GetShareMessage()
	{
		string worldTitle = WorldSession.current.worldTitle;
		string result = string.Empty;
		LeaderboardRecord[] array = (!this.onWeekTab) ? this.storedAllTimeRecords : this.storedWeeklyRecords;
		int num = (!(Blocksworld.leaderboardData != null) || array == null) ? -1 : Blocksworld.leaderboardData.GetCurrentUserRank(array);
		if (array != null && num != -1)
		{
			if (array.Length > num && !string.IsNullOrEmpty(array[num].username))
			{
				result = string.Concat(new string[]
				{
					"I got ",
					this.GetFormattedPlace(num),
					" place and beat ",
					array[num].username,
					" in ",
					worldTitle,
					"!"
				});
			}
			else
			{
				result = string.Concat(new string[]
				{
					"I got ",
					this.GetFormattedPlace(num),
					" place in ",
					worldTitle,
					"!"
				});
			}
		}
		else
		{
			result = "I finished " + worldTitle + " in " + this.timerTimecode.text;
		}
		return result;
	}

	// Token: 0x060022F0 RID: 8944 RVA: 0x00103A15 File Offset: 0x00101E15
	private void WipeLeaderboard()
	{
		this.firstTimeText.gameObject.SetActive(false);
		this.onWeekTab = true;
		this.tabRoot.SetActive(false);
		this.storedWeeklyRecords = null;
		this.storedAllTimeRecords = null;
		this.WipeScoreText();
		this.WipeBuilderText();
	}

	// Token: 0x060022F1 RID: 8945 RVA: 0x00103A58 File Offset: 0x00101E58
	private void WipeScoreText()
	{
		foreach (UILeaderboardEntry uileaderboardEntry in this.leaderboardEntries)
		{
			UnityEngine.Object.Destroy(uileaderboardEntry.gameObject);
		}
		this.leaderboardEntries.Clear();
	}

	// Token: 0x060022F2 RID: 8946 RVA: 0x00103AC4 File Offset: 0x00101EC4
	private void WipeBuilderText()
	{
		for (int i = 0; i < this.builderNameTexts.Length; i++)
		{
			this.builderNameTexts[i].text = string.Empty;
		}
	}

	// Token: 0x060022F3 RID: 8947 RVA: 0x00103AFC File Offset: 0x00101EFC
	private string TruncateName(string longName)
	{
		string text = longName;
		if (text.Length > 11)
		{
			text = text.Remove(10);
			text = text.Insert(text.Length, "...");
		}
		return text;
	}

	// Token: 0x04001DE9 RID: 7657
	public bool readyForTimer;

	// Token: 0x04001DEA RID: 7658
	private CanvasScaler canvasScaler;

	// Token: 0x04001DEB RID: 7659
	private static LBGUIState UIState = LBGUIState.Reset;

	// Token: 0x04001DEC RID: 7660
	private LBGUIState StoreUIState = LBGUIState.Reset;

	// Token: 0x04001DED RID: 7661
	private LBGUILoad LoadState;

	// Token: 0x04001DEE RID: 7662
	public RectTransform timerDisplay;

	// Token: 0x04001DEF RID: 7663
	public Text timerTimecode;

	// Token: 0x04001DF0 RID: 7664
	public Image timerBackground;

	// Token: 0x04001DF1 RID: 7665
	private float yStart;

	// Token: 0x04001DF2 RID: 7666
	public Image lbOverlay;

	// Token: 0x04001DF3 RID: 7667
	public GameObject buttonBackground;

	// Token: 0x04001DF4 RID: 7668
	public GameObject leaderboardScrollParent;

	// Token: 0x04001DF5 RID: 7669
	public Scrollbar leaderboardScrollbar;

	// Token: 0x04001DF6 RID: 7670
	public RectTransform contentRoot;

	// Token: 0x04001DF7 RID: 7671
	public GameObject errorPanel;

	// Token: 0x04001DF8 RID: 7672
	public Text errorTitleText;

	// Token: 0x04001DF9 RID: 7673
	public Text errorMessageText;

	// Token: 0x04001DFA RID: 7674
	public RectTransform loadingSpinner;

	// Token: 0x04001DFB RID: 7675
	public Text firstTimeText;

	// Token: 0x04001DFC RID: 7676
	public GameObject resultPanel;

	// Token: 0x04001DFD RID: 7677
	public Text winningTitle;

	// Token: 0x04001DFE RID: 7678
	public GameObject sharePanel;

	// Token: 0x04001DFF RID: 7679
	public RectTransform shareButton;

	// Token: 0x04001E00 RID: 7680
	public RectTransform _sharePopupSourceRect;

	// Token: 0x04001E01 RID: 7681
	public GameObject fixedButtons;

	// Token: 0x04001E02 RID: 7682
	public GameObject startGroup;

	// Token: 0x04001E03 RID: 7683
	public Text worldNameText;

	// Token: 0x04001E04 RID: 7684
	public Text[] builderNameTexts;

	// Token: 0x04001E05 RID: 7685
	public CanvasGroup winFadeGroup;

	// Token: 0x04001E06 RID: 7686
	public GameObject tabRoot;

	// Token: 0x04001E07 RID: 7687
	public Image leaderboardOutline;

	// Token: 0x04001E08 RID: 7688
	public UILeaderboardEntry leaderboardEntryTemplate;

	// Token: 0x04001E09 RID: 7689
	private List<UILeaderboardEntry> leaderboardEntries = new List<UILeaderboardEntry>();

	// Token: 0x04001E0A RID: 7690
	public Text leaderboardTypeText;

	// Token: 0x04001E0B RID: 7691
	private bool onWeekTab = true;

	// Token: 0x04001E0C RID: 7692
	private LeaderboardRecord[] storedWeeklyRecords;

	// Token: 0x04001E0D RID: 7693
	private LeaderboardRecord[] storedAllTimeRecords;

	// Token: 0x04001E0E RID: 7694
	private string lastWorldID = string.Empty;

	// Token: 0x04001E0F RID: 7695
	private bool isAuthor;

	// Token: 0x04001E10 RID: 7696
	private string firstTimeString = "Be the first to set a time!";

	// Token: 0x04001E11 RID: 7697
	private string authorFirstTimeString = "You can’t place on your own leaderboard!";

	// Token: 0x04001E12 RID: 7698
	private string worldCreatorName;

	// Token: 0x04001E13 RID: 7699
	private int worldCreatorId;

	// Token: 0x04001E14 RID: 7700
	private const float fadeWait = 1f;

	// Token: 0x04001E15 RID: 7701
	private const string colorTagStartWhite = "<color=white>";

	// Token: 0x04001E16 RID: 7702
	private const string colorTagEnd = "</color>";

	// Token: 0x04001E17 RID: 7703
	private const int maxNameLength = 11;
}
