using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x0200031F RID: 799
public class UITapedeck : MonoBehaviour
{
	// Token: 0x06002422 RID: 9250 RVA: 0x001094DC File Offset: 0x001078DC
	public void Init()
	{
		this._canvasGroup = base.GetComponent<CanvasGroup>();
		Button component = this.playButtonObj.GetComponent<Button>();
		component.onClick.AddListener(new UnityAction(this.ButtonPressed_Play));
		Button component2 = this.pauseButtonObj.GetComponent<Button>();
		component2.onClick.AddListener(new UnityAction(this.ButtonPressed_Pause));
		Button component3 = this.rewindButtonObj.GetComponent<Button>();
		component3.onClick.AddListener(new UnityAction(this.ButtonPressed_Rewind));
		Button component4 = this.stopButtonObj.GetComponent<Button>();
		component4.onClick.AddListener(new UnityAction(this.ButtonPressed_Stop));
		Button component5 = this.captureSetupButtonObj.GetComponent<Button>();
		component5.onClick.AddListener(new UnityAction(this.ButtonPressed_CaptureSetup));
		Button component6 = this.captureButtonObj.GetComponent<Button>();
		component6.onClick.AddListener(new UnityAction(this.ButtonPressed_Capture));
		Button component7 = this.menuButtonObj.GetComponent<Button>();
		component7.onClick.AddListener(new UnityAction(this.ButtonPressed_Menu));
		Button component8 = this.exitButtonObj.GetComponent<Button>();
		component8.onClick.AddListener(new UnityAction(this.ButtonPressed_Exit));
		Button component9 = this.toolsButtonObj.GetComponent<Button>();
		component9.onClick.AddListener(new UnityAction(this.ButtonPressed_Tools));
		Button component10 = this.profileSelectButtonObj.GetComponent<Button>();
		component10.onClick.AddListener(new UnityAction(this.ButtonPressed_ProfileSelect));
		this._startRecordingButton = this.recordButtonNeutralObj.GetComponent<Button>();
		this._startRecordingButton.onClick.AddListener(new UnityAction(this.ButtonPressed_RecordStart));
		this._stopRecordingButton = this.recordButtonActiveObj.GetComponent<Button>();
		this._stopRecordingButton.onClick.AddListener(new UnityAction(this.ButtonPressed_RecordStop));
		this._recordPopoutButton = this.recordPopoutHandle.GetComponent<Button>();
		this._recordPopoutButton.onClick.AddListener(new UnityAction(this.ButtonPressed_RecordDockToggle));
		this._recordGroupTransform = this.recordGroup.GetComponent<RectTransform>();
		this._recordPopoutHandleTransform = this.recordPopoutHandle.GetComponent<RectTransform>();
		this.SetRecordButtonStateInactive();
		this.DockRecordButton();
	}

	// Token: 0x06002423 RID: 9251 RVA: 0x00109714 File Offset: 0x00107B14
	public void ShowPlay(bool show)
	{
		this.playButtonObj.GetComponent<Image>().color = Color.white;
		this.playButtonObj.GetComponent<Button>().interactable = true;
		this.playButtonObj.SetActive(show);
	}

	// Token: 0x06002424 RID: 9252 RVA: 0x00109748 File Offset: 0x00107B48
	public void ShowPause(bool show)
	{
		this.pauseButtonObj.SetActive(show);
	}

	// Token: 0x06002425 RID: 9253 RVA: 0x00109756 File Offset: 0x00107B56
	public void ShowRewind(bool show)
	{
		this.rewindButtonObj.SetActive(show);
	}

	// Token: 0x06002426 RID: 9254 RVA: 0x00109764 File Offset: 0x00107B64
	public void ShowStop(bool show)
	{
		this.stopButtonObj.SetActive(show);
	}

	// Token: 0x06002427 RID: 9255 RVA: 0x00109772 File Offset: 0x00107B72
	public void ShowCaptureSetup(bool show)
	{
		this.captureSetupButtonObj.SetActive(show);
	}

	// Token: 0x06002428 RID: 9256 RVA: 0x00109780 File Offset: 0x00107B80
	public void ShowCapture(bool show)
	{
		this.captureButtonObj.SetActive(show);
	}

	// Token: 0x06002429 RID: 9257 RVA: 0x0010978E File Offset: 0x00107B8E
	public void ShowMenu(bool show)
	{
		this.menuButtonObj.SetActive(show);
	}

	// Token: 0x0600242A RID: 9258 RVA: 0x0010979C File Offset: 0x00107B9C
	public void ShowExit(bool show)
	{
		this.exitButtonObj.SetActive(show);
	}

	// Token: 0x0600242B RID: 9259 RVA: 0x001097AA File Offset: 0x00107BAA
	public void ShowTools(bool show)
	{
		this.toolsButtonObj.SetActive(show);
	}

	// Token: 0x0600242C RID: 9260 RVA: 0x001097B8 File Offset: 0x00107BB8
	public void ShowProfileSelect(bool show)
	{
		this.profileSelectButtonObj.SetActive(show);
	}

	// Token: 0x0600242D RID: 9261 RVA: 0x001097C6 File Offset: 0x00107BC6
	public void ShowRecord(bool show)
	{
		if (!this._screenRecordingEnabled)
		{
			return;
		}
		this.recordGroup.SetActive(show);
		if (show)
		{
			this.PositionRecordButton();
			this.RefreshRecordButtonState();
		}
	}

	// Token: 0x0600242E RID: 9262 RVA: 0x001097F4 File Offset: 0x00107BF4
	public void HideAllButtons()
	{
		this.playButtonObj.SetActive(false);
		this.pauseButtonObj.SetActive(false);
		this.rewindButtonObj.SetActive(false);
		this.stopButtonObj.SetActive(false);
		this.captureButtonObj.SetActive(false);
		this.captureSetupButtonObj.SetActive(false);
		this.menuButtonObj.SetActive(false);
		this.exitButtonObj.SetActive(false);
		this.toolsButtonObj.SetActive(false);
		this.profileSelectButtonObj.SetActive(false);
		this.recordGroup.SetActive(false);
	}

	// Token: 0x0600242F RID: 9263 RVA: 0x00109888 File Offset: 0x00107C88
	public void Ghost(bool ghost)
	{
		this.playButtonObj.GetComponent<Image>().color = ((!ghost) ? Color.white : new Color(1f, 1f, 1f, 0.5f));
		this.playButtonObj.GetComponent<Button>().interactable = !ghost;
	}

	// Token: 0x06002430 RID: 9264 RVA: 0x001098E4 File Offset: 0x00107CE4
	public void PositionRecordButton()
	{
		float num = 2f;
		if (this.playButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (this.pauseButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (this.rewindButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (this.stopButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (this.captureButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (this.captureSetupButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (this.menuButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (this.exitButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (this.toolsButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (this.profileSelectButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		this._recordGroupTransform.anchoredPosition = new Vector2(num, this._recordGroupTransform.anchoredPosition.y);
	}

	// Token: 0x06002431 RID: 9265 RVA: 0x00109A0B File Offset: 0x00107E0B
	public void Reset()
	{
		this.SetRecordButtonStateInactive();
		this.DockRecordButton();
		this.HideAllButtons();
	}

	// Token: 0x06002432 RID: 9266 RVA: 0x00109A20 File Offset: 0x00107E20
	public void GetUIObjects(List<GameObject> objectList)
	{
		objectList.Add(this.playButtonObj);
		objectList.Add(this.pauseButtonObj);
		objectList.Add(this.rewindButtonObj);
		objectList.Add(this.stopButtonObj);
		objectList.Add(this.captureSetupButtonObj);
		objectList.Add(this.captureButtonObj);
		objectList.Add(this.menuButtonObj);
		objectList.Add(this.exitButtonObj);
		objectList.Add(this.toolsButtonObj);
		objectList.Add(this.profileSelectButtonObj);
		objectList.Add(this.recordGroup);
	}

	// Token: 0x06002433 RID: 9267 RVA: 0x00109AB1 File Offset: 0x00107EB1
	public void SetScreenRecordingEnabled(bool status)
	{
		this._screenRecordingEnabled = status;
	}

	// Token: 0x06002434 RID: 9268 RVA: 0x00109ABA File Offset: 0x00107EBA
	public void RecordingWasStoppedExternally()
	{
		this.SetRecordButtonStateInactive();
	}

	// Token: 0x06002435 RID: 9269 RVA: 0x00109AC2 File Offset: 0x00107EC2
	public bool RecordButtonActive()
	{
		return this._isRecording;
	}

	// Token: 0x06002436 RID: 9270 RVA: 0x00109ACC File Offset: 0x00107ECC
	public void RefreshRecordButtonState()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		bool flag = WorldSession.platformDelegate.ScreenRecordingInProgress();
		if (flag)
		{
			this._isRecording = false;
			this.UndockRecordButton();
			this.SetRecordButtonStateActive();
		}
		else
		{
			this.SetRecordButtonStateInactive();
		}
	}

	// Token: 0x06002437 RID: 9271 RVA: 0x00109B19 File Offset: 0x00107F19
	private void DockRecordButtonSliding()
	{
		if (this._isRecording || this._recordButtonState != UITapedeck.RecordButtonState.Undocked)
		{
			return;
		}
		this._recordButtonState = UITapedeck.RecordButtonState.SlidingToDocked;
		this._sliderTimer = 0.25f;
	}

	// Token: 0x06002438 RID: 9272 RVA: 0x00109B45 File Offset: 0x00107F45
	private void UndockRecordButtonSliding()
	{
		if (this._isRecording || this._recordButtonState != UITapedeck.RecordButtonState.Docked)
		{
			return;
		}
		this._recordButtonState = UITapedeck.RecordButtonState.SlidingToUndocked;
		this._sliderTimer = 0.25f;
	}

	// Token: 0x06002439 RID: 9273 RVA: 0x00109B70 File Offset: 0x00107F70
	private void DockRecordButton()
	{
		if (this._isRecording)
		{
			return;
		}
		this._recordButtonState = UITapedeck.RecordButtonState.Docked;
		this.SlideRecordButtonTo(0f);
		this._startRecordingButton.enabled = false;
		this._stopRecordingButton.enabled = false;
	}

	// Token: 0x0600243A RID: 9274 RVA: 0x00109BA8 File Offset: 0x00107FA8
	private void UndockRecordButton()
	{
		if (this._isRecording)
		{
			return;
		}
		this._recordButtonState = UITapedeck.RecordButtonState.Undocked;
		this.SlideRecordButtonTo(1f);
		this._startRecordingButton.enabled = true;
		this._stopRecordingButton.enabled = true;
	}

	// Token: 0x0600243B RID: 9275 RVA: 0x00109BE0 File Offset: 0x00107FE0
	private void SlideRecordButtonTo(float slideFactor)
	{
		float num = 101f;
		float x = 61f + slideFactor * num;
		this._recordPopoutHandleTransform.anchoredPosition = new Vector2(slideFactor * num, this._recordPopoutHandleTransform.anchoredPosition.y);
		this.recordButtonSlider.sizeDelta = new Vector2(x, this.recordButtonSlider.sizeDelta.y);
	}

	// Token: 0x0600243C RID: 9276 RVA: 0x00109C47 File Offset: 0x00108047
	private void SetRecordButtonStateActive()
	{
		this._isRecording = true;
		this.recordButtonNeutralObj.SetActive(false);
		this.recordButtonActiveObj.SetActive(true);
	}

	// Token: 0x0600243D RID: 9277 RVA: 0x00109C68 File Offset: 0x00108068
	private void SetRecordButtonStateInactive()
	{
		this._isRecording = false;
		this._currentRecordingLength = 0f;
		this.recordButtonNeutralObj.SetActive(true);
		this.recordButtonActiveObj.SetActive(false);
	}

	// Token: 0x0600243E RID: 9278 RVA: 0x00109C94 File Offset: 0x00108094
	private void ButtonPressed_Play()
	{
		Blocksworld.bw.ButtonPlayTapped();
	}

	// Token: 0x0600243F RID: 9279 RVA: 0x00109CA0 File Offset: 0x001080A0
	private void ButtonPressed_Pause()
	{
		Blocksworld.bw.ButtonPauseTapped();
	}

	// Token: 0x06002440 RID: 9280 RVA: 0x00109CAC File Offset: 0x001080AC
	private void ButtonPressed_Rewind()
	{
		Blocksworld.bw.ButtonRestartTapped();
	}

	// Token: 0x06002441 RID: 9281 RVA: 0x00109CB8 File Offset: 0x001080B8
	private void ButtonPressed_Stop()
	{
		Blocksworld.bw.ButtonStopTapped();
	}

	// Token: 0x06002442 RID: 9282 RVA: 0x00109CC4 File Offset: 0x001080C4
	private void ButtonPressed_CaptureSetup()
	{
		Blocksworld.bw.ButtonCaptureSetupTapped();
	}

	// Token: 0x06002443 RID: 9283 RVA: 0x00109CD0 File Offset: 0x001080D0
	private void ButtonPressed_Capture()
	{
		Blocksworld.bw.ButtonCaptureTapped();
	}

	// Token: 0x06002444 RID: 9284 RVA: 0x00109CDC File Offset: 0x001080DC
	private void ButtonPressed_Menu()
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			CharacterEditor.Instance.Exit();
			return;
		}
		Blocksworld.bw.ButtonMenuTapped();
	}

	// Token: 0x06002445 RID: 9285 RVA: 0x00109D02 File Offset: 0x00108102
	private void ButtonPressed_Exit()
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			CharacterEditor.Instance.Exit();
			return;
		}
		Blocksworld.bw.ButtonExitWorldTapped();
	}

	// Token: 0x06002446 RID: 9286 RVA: 0x00109D28 File Offset: 0x00108128
	private void ButtonPressed_Tools()
	{
		Blocksworld.bw.ButtonStopTapped();
	}

	// Token: 0x06002447 RID: 9287 RVA: 0x00109D34 File Offset: 0x00108134
	private void ButtonPressed_ProfileSelect()
	{
		Blocksworld.UI.ShowProfileSelectionScreen();
	}

	// Token: 0x06002448 RID: 9288 RVA: 0x00109D40 File Offset: 0x00108140
	private void ButtonPressed_RecordStart()
	{
		if (!this._isRecording)
		{
			WorldSession.platformDelegate.StartRecordingScreen();
			this._currentRecordingLength = 0f;
			this.SetRecordButtonStateActive();
		}
	}

	// Token: 0x06002449 RID: 9289 RVA: 0x00109D68 File Offset: 0x00108168
	private void ButtonPressed_RecordStop()
	{
		if (this._isRecording && this._currentRecordingLength > 1f)
		{
			WorldSession.platformDelegate.StopRecordingScreen();
			this.SetRecordButtonStateInactive();
		}
	}

	// Token: 0x0600244A RID: 9290 RVA: 0x00109D95 File Offset: 0x00108195
	private void ButtonPressed_RecordDockToggle()
	{
		if (this._recordButtonState == UITapedeck.RecordButtonState.Docked)
		{
			this.UndockRecordButtonSliding();
		}
		else if (this._recordButtonState == UITapedeck.RecordButtonState.Undocked)
		{
			this.DockRecordButtonSliding();
		}
	}

	// Token: 0x0600244B RID: 9291 RVA: 0x00109DC0 File Offset: 0x001081C0
	private void Update()
	{
		if (this._isRecording)
		{
			this._currentRecordingLength += Time.deltaTime;
			if (this._currentRecordingLength > 1f && !WorldSession.platformDelegate.ScreenRecordingInProgress())
			{
				this._isRecording = false;
				this.SetRecordButtonStateInactive();
			}
			else if (this._currentRecordingLength > 300f)
			{
				this._isRecording = false;
				WorldSession.platformDelegate.StopRecordingScreen();
				this.SetRecordButtonStateInactive();
			}
		}
		else if (WorldSession.platformDelegate.ScreenRecordingInProgress())
		{
			this.SetRecordButtonStateActive();
		}
		if (this._recordButtonState == UITapedeck.RecordButtonState.SlidingToDocked)
		{
			this._sliderTimer -= Time.deltaTime;
			if (this._sliderTimer <= 0f)
			{
				this._sliderTimer = 0f;
				this.DockRecordButton();
			}
			else
			{
				this.SlideRecordButtonTo(this._sliderTimer / 0.25f);
			}
		}
		else if (this._recordButtonState == UITapedeck.RecordButtonState.SlidingToUndocked)
		{
			this._sliderTimer -= Time.deltaTime;
			if (this._sliderTimer <= 0f)
			{
				this._sliderTimer = 0f;
				this.UndockRecordButton();
			}
			else
			{
				this.SlideRecordButtonTo((0.25f - this._sliderTimer) / 0.25f);
			}
		}
	}

	// Token: 0x0600244C RID: 9292 RVA: 0x00109F13 File Offset: 0x00108313
	internal void BWApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus)
		{
			this.RefreshRecordButtonState();
		}
	}

	// Token: 0x04001F22 RID: 7970
	public GameObject playButtonObj;

	// Token: 0x04001F23 RID: 7971
	public GameObject pauseButtonObj;

	// Token: 0x04001F24 RID: 7972
	public GameObject rewindButtonObj;

	// Token: 0x04001F25 RID: 7973
	public GameObject stopButtonObj;

	// Token: 0x04001F26 RID: 7974
	public GameObject captureSetupButtonObj;

	// Token: 0x04001F27 RID: 7975
	public GameObject captureButtonObj;

	// Token: 0x04001F28 RID: 7976
	public GameObject menuButtonObj;

	// Token: 0x04001F29 RID: 7977
	public GameObject exitButtonObj;

	// Token: 0x04001F2A RID: 7978
	public GameObject toolsButtonObj;

	// Token: 0x04001F2B RID: 7979
	public GameObject profileSelectButtonObj;

	// Token: 0x04001F2C RID: 7980
	public GameObject recordGroup;

	// Token: 0x04001F2D RID: 7981
	public GameObject recordPopoutHandle;

	// Token: 0x04001F2E RID: 7982
	public GameObject recordButtonNeutralObj;

	// Token: 0x04001F2F RID: 7983
	public GameObject recordButtonActiveObj;

	// Token: 0x04001F30 RID: 7984
	public RectTransform recordButtonSlider;

	// Token: 0x04001F31 RID: 7985
	private CanvasGroup _canvasGroup;

	// Token: 0x04001F32 RID: 7986
	private Button _startRecordingButton;

	// Token: 0x04001F33 RID: 7987
	private Button _stopRecordingButton;

	// Token: 0x04001F34 RID: 7988
	private Button _recordPopoutButton;

	// Token: 0x04001F35 RID: 7989
	private RectTransform _recordGroupTransform;

	// Token: 0x04001F36 RID: 7990
	private RectTransform _recordPopoutHandleTransform;

	// Token: 0x04001F37 RID: 7991
	private bool _screenRecordingEnabled;

	// Token: 0x04001F38 RID: 7992
	private bool _isRecording;

	// Token: 0x04001F39 RID: 7993
	private float _currentRecordingLength;

	// Token: 0x04001F3A RID: 7994
	private float _sliderTimer;

	// Token: 0x04001F3B RID: 7995
	private const float _sliderDuration = 0.25f;

	// Token: 0x04001F3C RID: 7996
	private const float _minRecordingLength = 1f;

	// Token: 0x04001F3D RID: 7997
	private const float _maxRecordingLength = 300f;

	// Token: 0x04001F3E RID: 7998
	private const float _recordButtonDockedMaskWidth = 61f;

	// Token: 0x04001F3F RID: 7999
	private const float _recordButtonUndockedMaskWidth = 162f;

	// Token: 0x04001F40 RID: 8000
	private const float _buttonWidth = 160f;

	// Token: 0x04001F41 RID: 8001
	private UITapedeck.RecordButtonState _recordButtonState;

	// Token: 0x02000320 RID: 800
	private enum RecordButtonState
	{
		// Token: 0x04001F43 RID: 8003
		Docked,
		// Token: 0x04001F44 RID: 8004
		SlidingToUndocked,
		// Token: 0x04001F45 RID: 8005
		Undocked,
		// Token: 0x04001F46 RID: 8006
		SlidingToDocked
	}
}
