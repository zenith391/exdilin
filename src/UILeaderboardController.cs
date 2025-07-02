using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILeaderboardController : MonoBehaviour
{
	public bool readyForTimer;

	private CanvasScaler canvasScaler;

	private static LBGUIState UIState = LBGUIState.Reset;

	private LBGUIState StoreUIState = LBGUIState.Reset;

	private LBGUILoad LoadState;

	public RectTransform timerDisplay;

	public Text timerTimecode;

	public Image timerBackground;

	private float yStart;

	public Image lbOverlay;

	public GameObject buttonBackground;

	public GameObject leaderboardScrollParent;

	public Scrollbar leaderboardScrollbar;

	public RectTransform contentRoot;

	public GameObject errorPanel;

	public Text errorTitleText;

	public Text errorMessageText;

	public RectTransform loadingSpinner;

	public Text firstTimeText;

	public GameObject resultPanel;

	public Text winningTitle;

	public GameObject sharePanel;

	public RectTransform shareButton;

	public RectTransform _sharePopupSourceRect;

	public GameObject fixedButtons;

	public GameObject startGroup;

	public Text worldNameText;

	public Text[] builderNameTexts;

	public CanvasGroup winFadeGroup;

	public GameObject tabRoot;

	public Image leaderboardOutline;

	public UILeaderboardEntry leaderboardEntryTemplate;

	private List<UILeaderboardEntry> leaderboardEntries = new List<UILeaderboardEntry>();

	public Text leaderboardTypeText;

	private bool onWeekTab = true;

	private LeaderboardRecord[] storedWeeklyRecords;

	private LeaderboardRecord[] storedAllTimeRecords;

	private string lastWorldID = string.Empty;

	private bool isAuthor;

	private string firstTimeString = "Be the first to set a time!";

	private string authorFirstTimeString = "You can’t place on your own leaderboard!";

	private string worldCreatorName;

	private int worldCreatorId;

	private const float fadeWait = 1f;

	private const string colorTagStartWhite = "<color=white>";

	private const string colorTagEnd = "</color>";

	private const int maxNameLength = 11;

	public void Init()
	{
		lbOverlay.gameObject.SetActive(value: false);
		timerDisplay.gameObject.SetActive(value: true);
		yStart = timerBackground.rectTransform.rect.height;
		timerDisplay.anchoredPosition = new Vector3(0f, yStart, 0f);
		timerDisplay.gameObject.SetActive(value: false);
		UIState = LBGUIState.Reset;
		LoadState = LBGUILoad.Loading;
		for (int i = 0; i < builderNameTexts.Length; i++)
		{
			Button component = builderNameTexts[i].GetComponent<Button>();
			if (component != null)
			{
				component.onClick.AddListener(VisitCreatorProfile);
			}
		}
		canvasScaler = GetComponent<CanvasScaler>();
		SetScale();
		ViewportWatchdog.AddListener(ViewportSizeDidChange);
	}

	public bool IsVisible()
	{
		return UIState != LBGUIState.Reset;
	}

	private void ViewportSizeDidChange()
	{
		SetScale();
	}

	private void SetScale()
	{
		canvasScaler.scaleFactor = NormalizedScreen.pixelScale;
	}

	public void GetUIObjects(List<GameObject> objectList)
	{
		objectList.Add(lbOverlay.gameObject);
	}

	public void SendError(string serverErrorTitle, string serverMessage)
	{
		LoadState = LBGUILoad.Error;
		errorTitleText.text = serverErrorTitle;
		errorMessageText.text = serverMessage;
		WipeLeaderboard();
		UpdateNetworkLoading();
	}

	public void ClearLeaderboard()
	{
		LoadState = LBGUILoad.Done;
		UIState = LBGUIState.Reset;
		UpdateScreens();
	}

	public void StartSetup()
	{
		isAuthor = WorldSession.current.isWorldAuthorCurrentUser;
		LoadState = LBGUILoad.Loading;
		worldNameText.text = WorldSession.current.worldTitle;
		Blocksworld.UI.SetControlsCanvasVisible(visible: true);
		if (lastWorldID == WorldSession.current.worldId)
		{
			UIState = LBGUIState.Game;
			UpdateScreens();
		}
		else
		{
			StartCoroutine("PauseWorld");
		}
	}

	private IEnumerator PauseWorld()
	{
		for (int i = 0; i < 2; i++)
		{
			if (UIState == LBGUIState.WinScreen)
			{
				yield break;
			}
			yield return 0;
		}
		UIState = LBGUIState.Start;
		WorldSession.current.EnterLeaderboardStartScreen();
		UpdateScreens();
	}

	private void UpdateBuilderInfo(string builderName, int builderId, int builderBlocksworldPremium, string builderTime)
	{
		worldCreatorName = builderName;
		worldCreatorId = builderId;
		string text = ((builderBlocksworldPremium <= 0) ? "lightblue" : "lightblue");
		builderName = ((!string.IsNullOrEmpty(builderName)) ? ("Creator <color=" + text + ">" + builderName + "</color>") : ("<color=" + text + ">Creator</color>"));
		string text2 = ((!string.IsNullOrEmpty(builderTime) && !(builderTime == "0:00.000")) ? (builderName + "'s best time is <color=lightblue>" + builderTime + "</color>") : (builderName + " hasn’t posted a time."));
		for (int i = 0; i < builderNameTexts.Length; i++)
		{
			builderNameTexts[i].text = text2;
		}
	}

	private void VisitCreatorProfile()
	{
		if (!string.IsNullOrEmpty(worldCreatorName) && worldCreatorId != 0)
		{
			UIDialog dialog = Blocksworld.UI.Dialog;
			string mainText = $"Visit {worldCreatorName}'s page?";
			string[] buttonLabels = new string[2] { "Ok", "No thanks" };
			dialog.ShowGenericDialog(mainText, buttonLabels, new Action[2]
			{
				delegate
				{
					WorldSession.QuitWithDeepLink($"deep-link/profile/{worldCreatorId.ToString()}");
				},
				delegate
				{
					Blocksworld.UI.Dialog.CloseActiveDialog();
				}
			});
		}
	}

	private void UpdateLeaderboardType(LeaderboardType leaderboardType)
	{
		if (leaderboardType != LeaderboardType.LongestTime)
		{
			leaderboardTypeText.text = "Fastest Time";
		}
		else
		{
			leaderboardTypeText.text = "Longest Time";
		}
	}

	public void LoadSetup(LeaderboardType leaderboardType, string builderName, int builderId, int builderBlocksworldPremium, string builderTime, LeaderboardRecord[] weeklyRecords, LeaderboardRecord[] allTimeRecords)
	{
		LoadState = LBGUILoad.Done;
		UpdateLeaderboardType(leaderboardType);
		UpdateBuilderInfo(builderName, builderId, builderBlocksworldPremium, builderTime);
		storedWeeklyRecords = weeklyRecords;
		storedAllTimeRecords = allTimeRecords;
		CheckWeeklyRecordForNull();
		UpdateNetworkLoading();
	}

	public void WinScreen(string ourWinMessage)
	{
		Blocksworld.SetVRMode(enabled: false);
		WipeLeaderboard();
		Blocksworld.UI.SetControlsCanvasVisible(visible: false);
		LoadState = LBGUILoad.Loading;
		string text = timerTimecode.text;
		winningTitle.text = "Awesome! Your time is <color=lightblue>" + text + "</color>";
		UIState = LBGUIState.WinScreen;
		UpdateScreens();
		WorldSession.current.EnterLeaderboardWinScreen();
		winFadeGroup.gameObject.SetActive(value: true);
		StartCoroutine("FadeInGUI");
	}

	public void WinSetup(string playerTime, string builderName, int builderId, int builderBlocksworldPremium, string builderTime, LeaderboardRecord[] weeklyRecords, LeaderboardRecord[] allTimeRecords)
	{
		LoadState = LBGUILoad.Done;
		UpdateBuilderInfo(builderName, builderId, builderBlocksworldPremium, builderTime);
		storedWeeklyRecords = weeklyRecords;
		storedAllTimeRecords = allTimeRecords;
		CheckWeeklyRecordForNull();
		UpdateNetworkLoading();
	}

	private void CheckWeeklyRecordForNull()
	{
		bool flag = storedWeeklyRecords == null || storedWeeklyRecords.Length == 0;
		LeaderboardRecord[] leaderboardRecords = ((!flag) ? storedWeeklyRecords : storedAllTimeRecords);
		UpdateTabs(!flag);
		UpdateScrollableLeaderboard(leaderboardRecords);
	}

	private void UpdateNetworkLoading()
	{
		loadingSpinner.gameObject.SetActive(LoadState == LBGUILoad.Loading);
		errorPanel.SetActive(LoadState == LBGUILoad.Error);
	}

	private bool AnyGameControlsActive()
	{
		if (!Blocksworld.UI.Controls.AnyControlActive() && !Blocksworld.bw.IsPullingObject())
		{
			return Blocksworld.bw.HadObjectTapping();
		}
		return true;
	}

	public void Update()
	{
		if (UIState == LBGUIState.Start && Blocksworld.UI.Controls != null && Blocksworld.UI.Controls.AnyControlActive())
		{
			ProcessButtonPress("RealPlay");
		}
		else if (UIState == LBGUIState.Start && Input.anyKeyDown && !Input.GetMouseButton(0) && Input.touches.Length == 0)
		{
			ProcessButtonPress("RealPlay");
		}
		if (!readyForTimer && UIState == LBGUIState.Game && AnyGameControlsActive())
		{
			readyForTimer = true;
		}
	}

	private void UpdateScreens()
	{
		UpdateNetworkLoading();
		bool flag = UIState == LBGUIState.Start;
		bool flag2 = UIState == LBGUIState.Pause;
		bool flag3 = UIState == LBGUIState.WinScreen;
		bool flag4 = UIState == LBGUIState.Share;
		bool active = flag || flag3 || flag2;
		buttonBackground.SetActive(flag);
		leaderboardScrollParent.SetActive(active);
		startGroup.SetActive(flag);
		resultPanel.SetActive(flag3);
		sharePanel.SetActive(flag4);
		fixedButtons.SetActive(flag4 || flag3);
		lbOverlay.gameObject.SetActive(flag3);
		Canvas.ForceUpdateCanvases();
	}

	public void StopTimer()
	{
		StopCoroutine("SlideInTimer");
		StartCoroutine("SlideInTimer", false);
	}

	public IEnumerator SlideInTimer(bool down)
	{
		timerDisplay.gameObject.SetActive(value: true);
		float directionMod = ((!down) ? (-100f) : 100f);
		float yGoal = ((!down) ? yStart : 0f);
		while (timerDisplay.anchoredPosition.y != yGoal)
		{
			if (timerDisplay.gameObject.activeSelf)
			{
				Vector2 vector = Vector2.up * (Blocksworld.deltaTime * directionMod);
				Vector2 anchoredPosition = timerDisplay.anchoredPosition - vector;
				anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, 0f, yStart);
				timerDisplay.anchoredPosition = anchoredPosition;
				yield return null;
			}
		}
		if (!down)
		{
			timerDisplay.gameObject.SetActive(value: false);
		}
	}

	public IEnumerator FadeInGUI()
	{
		winFadeGroup.alpha = 0f;
		lbOverlay.color = new Color(0f, 0f, 0f, 0f);
		yield return new WaitForSeconds(1f);
		StopCoroutine("SlideInTimer");
		StartCoroutine("SlideInTimer", false);
		while (winFadeGroup.alpha < 1f)
		{
			float deltaTime = Blocksworld.deltaTime;
			winFadeGroup.alpha += deltaTime;
			lbOverlay.color = new Color(0f, 0f, 0f, winFadeGroup.alpha * 0.75f);
			yield return null;
		}
	}

	private void UpdateTabs(bool isWeek)
	{
		onWeekTab = isWeek;
		if (!tabRoot.activeSelf)
		{
			tabRoot.SetActive(value: true);
		}
		Color color = ((!isWeek) ? new Color(1f, 0.4f, 0f) : new Color(0.17f, 0.77f, 1f));
		leaderboardOutline.color = color;
	}

	public void ProcessButtonPress(string buttonCommand)
	{
		if (WorldSession.isProfileBuildSession())
		{
			return;
		}
		switch (buttonCommand)
		{
		case "AllTimeTab":
			if (onWeekTab)
			{
				UpdateTabs(isWeek: false);
				UpdateScrollableLeaderboard(storedAllTimeRecords);
			}
			break;
		case "WeeklyTab":
			if (!onWeekTab)
			{
				UpdateTabs(isWeek: true);
				UpdateScrollableLeaderboard(storedWeeklyRecords);
			}
			break;
		case "RealPlay":
			UIState = LBGUIState.Game;
			UpdateScreens();
			WorldSession.current.ExitLeaderboardStartScreen();
			break;
		case "Share":
		case "Facebook":
		case "SMS":
		case "Email":
			if (LoadState != LBGUILoad.Error)
			{
				string shareMessage = GetShareMessage();
				Canvas component = GetComponent<Canvas>();
				Rect shareRect = RectTransformUtility.PixelAdjustRect(shareButton, component);
				shareRect.center = shareButton.anchoredPosition;
				WorldSession.platformDelegate.ShowSharingPopupWithMessage(shareRect, shareMessage);
				BWLog.Info("Sending share popup message: " + shareMessage);
			}
			break;
		case "Retry":
			LoadState = LBGUILoad.Loading;
			UpdateNetworkLoading();
			if (Blocksworld.leaderboardData != null)
			{
				Blocksworld.leaderboardData.TryAgain(UIState == LBGUIState.Start || UIState == LBGUIState.Pause);
			}
			else
			{
				BWLog.Info("Couldn't find leaderboard data");
			}
			break;
		case "Leaderboard":
			UIState = LBGUIState.WinScreen;
			UpdateScreens();
			break;
		case "Restart":
			Blocksworld.bw.ButtonRestartTapped();
			break;
		default:
			BWLog.Info("No button functionality for: " + buttonCommand);
			break;
		}
	}

	private void UpdateScrollableLeaderboard(LeaderboardRecord[] leaderboardRecords)
	{
		WipeScoreText();
		firstTimeText.text = ((!isAuthor) ? firstTimeString : (firstTimeString + "\n" + authorFirstTimeString));
		if (leaderboardRecords == null || leaderboardRecords.Length == 0)
		{
			firstTimeText.gameObject.SetActive(value: true);
			winningTitle.text = "Awesome! Your time is <color=lightblue>" + timerTimecode.text + "</color>";
			return;
		}
		firstTimeText.gameObject.SetActive(leaderboardRecords.Length == 0);
		int currentUserRank = Blocksworld.leaderboardData.GetCurrentUserRank(leaderboardRecords);
		string text = ((currentUserRank != 1) ? "Awesome!" : "You got first place!");
		string text2 = timerTimecode.text;
		winningTitle.text = text + " Your time is <color=lightblue>" + text2 + "</color>";
		Color color = new Color(0.6f, 0.9f, 1f);
		Color color2 = new Color(0.8f, 0.8f, 0.8f);
		for (int i = 0; i < leaderboardRecords.Length; i++)
		{
			LeaderboardRecord leaderboardRecord = leaderboardRecords[i];
			bool flag = i == currentUserRank - 1;
			bool flag2 = i == leaderboardRecords.Length - 1;
			UILeaderboardEntry uILeaderboardEntry = UnityEngine.Object.Instantiate(leaderboardEntryTemplate);
			UIUserProfileLink userProfileLink = uILeaderboardEntry.userProfileLink;
			bool flag3 = i < 5 || flag;
			userProfileLink.SetupForUser(leaderboardRecord.username, leaderboardRecord.userId, flag);
			uILeaderboardEntry.gameObject.SetActive(value: true);
			RectTransform rectTransform = (RectTransform)uILeaderboardEntry.transform;
			rectTransform.SetParent(contentRoot, worldPositionStays: true);
			rectTransform.localScale = Vector3.one;
			if (flag3)
			{
				userProfileLink.LoadProfileImage(leaderboardRecord.userProfileImageUrl);
			}
			leaderboardEntries.Add(uILeaderboardEntry);
			uILeaderboardEntry.userRankText.color = ((!flag) ? color2 : Color.white);
			uILeaderboardEntry.userRankText.text = (i + 1).ToString();
			uILeaderboardEntry.userScoreText.color = ((!flag) ? color : Color.white);
			uILeaderboardEntry.userScoreText.text = Blocksworld.leaderboardData.FormatTime(leaderboardRecord.time);
			uILeaderboardEntry.playerBackground.gameObject.SetActive(value: false);
		}
		float num = Mathf.Max(1, leaderboardRecords.Length);
		float startScrollVal = 1f;
		float rankVal = -1f;
		if (currentUserRank > 0 && currentUserRank <= leaderboardEntries.Count)
		{
			leaderboardEntries[currentUserRank - 1].playerBackground.gameObject.SetActive(value: true);
			leaderboardEntries[currentUserRank - 1].playerBackground.color = leaderboardOutline.color;
			currentUserRank = Mathf.Max(0, currentUserRank - 1);
			startScrollVal = Mathf.Abs((float)(currentUserRank + 1) - num) / (num - 1f);
			float num2 = num + 0.625f;
			rankVal = (float)currentUserRank / num2;
		}
		StartCoroutine(UpdateScrollbar(startScrollVal, rankVal));
	}

	private IEnumerator UpdateScrollbar(float startScrollVal, float rankVal)
	{
		yield return new WaitForEndOfFrame();
		leaderboardScrollbar.value = startScrollVal;
	}

	public void UpdateTimer(string ourText)
	{
		if (!loadingSpinner.gameObject.activeSelf)
		{
			loadingSpinner.Rotate(0f, 0f, Time.deltaTime);
		}
		if (!timerDisplay.gameObject.activeSelf)
		{
			timerDisplay.gameObject.SetActive(value: true);
			timerDisplay.anchoredPosition = new Vector2(0f, yStart);
			StartCoroutine("SlideInTimer", true);
			Blocksworld.UI.Controls.SetupActiveControlsTimer();
		}
		timerTimecode.text = ourText;
	}

	public void PauseLeaderboard(bool isPaused)
	{
		if (UIState != LBGUIState.Reset)
		{
			if (isPaused)
			{
				StoreUIState = UIState;
				UIState = LBGUIState.Pause;
			}
			else
			{
				UIState = StoreUIState;
			}
			if (UIState == LBGUIState.WinScreen && !isPaused)
			{
				Blocksworld.UI.SetControlsCanvasVisible(visible: false);
			}
			UpdateScreens();
		}
	}

	public void Reset()
	{
		lastWorldID = WorldSession.current.worldId;
		UIState = LBGUIState.Reset;
		LoadState = LBGUILoad.Done;
		Vector3 vector = Vector3.up * yStart;
		StopAllCoroutines();
		timerDisplay.anchoredPosition = vector;
		timerDisplay.gameObject.SetActive(value: false);
		readyForTimer = false;
		WipeLeaderboard();
		UpdateScreens();
		isAuthor = false;
	}

	public void ClearWorldSessionName()
	{
		lastWorldID = string.Empty;
		UIState = LBGUIState.Reset;
		UpdateScreens();
	}

	private string GetFormattedPlace(int ourRank)
	{
		int num = ourRank % 10;
		string text = ((ourRank / 10 == 1) ? "th" : (num switch
		{
			1 => "st", 
			2 => "nd", 
			3 => "rd", 
			_ => "th", 
		}));
		return ourRank + text;
	}

	private string GetShareMessage()
	{
		string worldTitle = WorldSession.current.worldTitle;
		string empty = string.Empty;
		LeaderboardRecord[] array = ((!onWeekTab) ? storedAllTimeRecords : storedWeeklyRecords);
		int num = ((!(Blocksworld.leaderboardData != null) || array == null) ? (-1) : Blocksworld.leaderboardData.GetCurrentUserRank(array));
		if (array != null && num != -1)
		{
			if (array.Length > num && !string.IsNullOrEmpty(array[num].username))
			{
				return "I got " + GetFormattedPlace(num) + " place and beat " + array[num].username + " in " + worldTitle + "!";
			}
			return "I got " + GetFormattedPlace(num) + " place in " + worldTitle + "!";
		}
		return "I finished " + worldTitle + " in " + timerTimecode.text;
	}

	private void WipeLeaderboard()
	{
		firstTimeText.gameObject.SetActive(value: false);
		onWeekTab = true;
		tabRoot.SetActive(value: false);
		storedWeeklyRecords = null;
		storedAllTimeRecords = null;
		WipeScoreText();
		WipeBuilderText();
	}

	private void WipeScoreText()
	{
		foreach (UILeaderboardEntry leaderboardEntry in leaderboardEntries)
		{
			UnityEngine.Object.Destroy(leaderboardEntry.gameObject);
		}
		leaderboardEntries.Clear();
	}

	private void WipeBuilderText()
	{
		for (int i = 0; i < builderNameTexts.Length; i++)
		{
			builderNameTexts[i].text = string.Empty;
		}
	}

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
}
