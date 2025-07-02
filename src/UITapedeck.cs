using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITapedeck : MonoBehaviour
{
	private enum RecordButtonState
	{
		Docked,
		SlidingToUndocked,
		Undocked,
		SlidingToDocked
	}

	public GameObject playButtonObj;

	public GameObject pauseButtonObj;

	public GameObject rewindButtonObj;

	public GameObject stopButtonObj;

	public GameObject captureSetupButtonObj;

	public GameObject captureButtonObj;

	public GameObject menuButtonObj;

	public GameObject exitButtonObj;

	public GameObject toolsButtonObj;

	public GameObject profileSelectButtonObj;

	public GameObject recordGroup;

	public GameObject recordPopoutHandle;

	public GameObject recordButtonNeutralObj;

	public GameObject recordButtonActiveObj;

	public RectTransform recordButtonSlider;

	private CanvasGroup _canvasGroup;

	private Button _startRecordingButton;

	private Button _stopRecordingButton;

	private Button _recordPopoutButton;

	private RectTransform _recordGroupTransform;

	private RectTransform _recordPopoutHandleTransform;

	private bool _screenRecordingEnabled;

	private bool _isRecording;

	private float _currentRecordingLength;

	private float _sliderTimer;

	private const float _sliderDuration = 0.25f;

	private const float _minRecordingLength = 1f;

	private const float _maxRecordingLength = 300f;

	private const float _recordButtonDockedMaskWidth = 61f;

	private const float _recordButtonUndockedMaskWidth = 162f;

	private const float _buttonWidth = 160f;

	private RecordButtonState _recordButtonState;

	public void Init()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
		Button component = playButtonObj.GetComponent<Button>();
		component.onClick.AddListener(ButtonPressed_Play);
		Button component2 = pauseButtonObj.GetComponent<Button>();
		component2.onClick.AddListener(ButtonPressed_Pause);
		Button component3 = rewindButtonObj.GetComponent<Button>();
		component3.onClick.AddListener(ButtonPressed_Rewind);
		Button component4 = stopButtonObj.GetComponent<Button>();
		component4.onClick.AddListener(ButtonPressed_Stop);
		Button component5 = captureSetupButtonObj.GetComponent<Button>();
		component5.onClick.AddListener(ButtonPressed_CaptureSetup);
		Button component6 = captureButtonObj.GetComponent<Button>();
		component6.onClick.AddListener(ButtonPressed_Capture);
		Button component7 = menuButtonObj.GetComponent<Button>();
		component7.onClick.AddListener(ButtonPressed_Menu);
		Button component8 = exitButtonObj.GetComponent<Button>();
		component8.onClick.AddListener(ButtonPressed_Exit);
		Button component9 = toolsButtonObj.GetComponent<Button>();
		component9.onClick.AddListener(ButtonPressed_Tools);
		Button component10 = profileSelectButtonObj.GetComponent<Button>();
		component10.onClick.AddListener(ButtonPressed_ProfileSelect);
		_startRecordingButton = recordButtonNeutralObj.GetComponent<Button>();
		_startRecordingButton.onClick.AddListener(ButtonPressed_RecordStart);
		_stopRecordingButton = recordButtonActiveObj.GetComponent<Button>();
		_stopRecordingButton.onClick.AddListener(ButtonPressed_RecordStop);
		_recordPopoutButton = recordPopoutHandle.GetComponent<Button>();
		_recordPopoutButton.onClick.AddListener(ButtonPressed_RecordDockToggle);
		_recordGroupTransform = recordGroup.GetComponent<RectTransform>();
		_recordPopoutHandleTransform = recordPopoutHandle.GetComponent<RectTransform>();
		SetRecordButtonStateInactive();
		DockRecordButton();
	}

	public void ShowPlay(bool show)
	{
		playButtonObj.GetComponent<Image>().color = Color.white;
		playButtonObj.GetComponent<Button>().interactable = true;
		playButtonObj.SetActive(show);
	}

	public void ShowPause(bool show)
	{
		pauseButtonObj.SetActive(show);
	}

	public void ShowRewind(bool show)
	{
		rewindButtonObj.SetActive(show);
	}

	public void ShowStop(bool show)
	{
		stopButtonObj.SetActive(show);
	}

	public void ShowCaptureSetup(bool show)
	{
		captureSetupButtonObj.SetActive(show);
	}

	public void ShowCapture(bool show)
	{
		captureButtonObj.SetActive(show);
	}

	public void ShowMenu(bool show)
	{
		menuButtonObj.SetActive(show);
	}

	public void ShowExit(bool show)
	{
		exitButtonObj.SetActive(show);
	}

	public void ShowTools(bool show)
	{
		toolsButtonObj.SetActive(show);
	}

	public void ShowProfileSelect(bool show)
	{
		profileSelectButtonObj.SetActive(show);
	}

	public void ShowRecord(bool show)
	{
		if (_screenRecordingEnabled)
		{
			recordGroup.SetActive(show);
			if (show)
			{
				PositionRecordButton();
				RefreshRecordButtonState();
			}
		}
	}

	public void HideAllButtons()
	{
		playButtonObj.SetActive(value: false);
		pauseButtonObj.SetActive(value: false);
		rewindButtonObj.SetActive(value: false);
		stopButtonObj.SetActive(value: false);
		captureButtonObj.SetActive(value: false);
		captureSetupButtonObj.SetActive(value: false);
		menuButtonObj.SetActive(value: false);
		exitButtonObj.SetActive(value: false);
		toolsButtonObj.SetActive(value: false);
		profileSelectButtonObj.SetActive(value: false);
		recordGroup.SetActive(value: false);
	}

	public void Ghost(bool ghost)
	{
		playButtonObj.GetComponent<Image>().color = ((!ghost) ? Color.white : new Color(1f, 1f, 1f, 0.5f));
		playButtonObj.GetComponent<Button>().interactable = !ghost;
	}

	public void PositionRecordButton()
	{
		float num = 2f;
		if (playButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (pauseButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (rewindButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (stopButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (captureButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (captureSetupButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (menuButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (exitButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (toolsButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		if (profileSelectButtonObj.activeInHierarchy)
		{
			num += 158f;
		}
		_recordGroupTransform.anchoredPosition = new Vector2(num, _recordGroupTransform.anchoredPosition.y);
	}

	public void Reset()
	{
		SetRecordButtonStateInactive();
		DockRecordButton();
		HideAllButtons();
	}

	public void GetUIObjects(List<GameObject> objectList)
	{
		objectList.Add(playButtonObj);
		objectList.Add(pauseButtonObj);
		objectList.Add(rewindButtonObj);
		objectList.Add(stopButtonObj);
		objectList.Add(captureSetupButtonObj);
		objectList.Add(captureButtonObj);
		objectList.Add(menuButtonObj);
		objectList.Add(exitButtonObj);
		objectList.Add(toolsButtonObj);
		objectList.Add(profileSelectButtonObj);
		objectList.Add(recordGroup);
	}

	public void SetScreenRecordingEnabled(bool status)
	{
		_screenRecordingEnabled = status;
	}

	public void RecordingWasStoppedExternally()
	{
		SetRecordButtonStateInactive();
	}

	public bool RecordButtonActive()
	{
		return _isRecording;
	}

	public void RefreshRecordButtonState()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (WorldSession.platformDelegate.ScreenRecordingInProgress())
			{
				_isRecording = false;
				UndockRecordButton();
				SetRecordButtonStateActive();
			}
			else
			{
				SetRecordButtonStateInactive();
			}
		}
	}

	private void DockRecordButtonSliding()
	{
		if (!_isRecording && _recordButtonState == RecordButtonState.Undocked)
		{
			_recordButtonState = RecordButtonState.SlidingToDocked;
			_sliderTimer = 0.25f;
		}
	}

	private void UndockRecordButtonSliding()
	{
		if (!_isRecording && _recordButtonState == RecordButtonState.Docked)
		{
			_recordButtonState = RecordButtonState.SlidingToUndocked;
			_sliderTimer = 0.25f;
		}
	}

	private void DockRecordButton()
	{
		if (!_isRecording)
		{
			_recordButtonState = RecordButtonState.Docked;
			SlideRecordButtonTo(0f);
			_startRecordingButton.enabled = false;
			_stopRecordingButton.enabled = false;
		}
	}

	private void UndockRecordButton()
	{
		if (!_isRecording)
		{
			_recordButtonState = RecordButtonState.Undocked;
			SlideRecordButtonTo(1f);
			_startRecordingButton.enabled = true;
			_stopRecordingButton.enabled = true;
		}
	}

	private void SlideRecordButtonTo(float slideFactor)
	{
		float num = 101f;
		float x = 61f + slideFactor * num;
		_recordPopoutHandleTransform.anchoredPosition = new Vector2(slideFactor * num, _recordPopoutHandleTransform.anchoredPosition.y);
		recordButtonSlider.sizeDelta = new Vector2(x, recordButtonSlider.sizeDelta.y);
	}

	private void SetRecordButtonStateActive()
	{
		_isRecording = true;
		recordButtonNeutralObj.SetActive(value: false);
		recordButtonActiveObj.SetActive(value: true);
	}

	private void SetRecordButtonStateInactive()
	{
		_isRecording = false;
		_currentRecordingLength = 0f;
		recordButtonNeutralObj.SetActive(value: true);
		recordButtonActiveObj.SetActive(value: false);
	}

	private void ButtonPressed_Play()
	{
		Blocksworld.bw.ButtonPlayTapped();
	}

	private void ButtonPressed_Pause()
	{
		Blocksworld.bw.ButtonPauseTapped();
	}

	private void ButtonPressed_Rewind()
	{
		Blocksworld.bw.ButtonRestartTapped();
	}

	private void ButtonPressed_Stop()
	{
		Blocksworld.bw.ButtonStopTapped();
	}

	private void ButtonPressed_CaptureSetup()
	{
		Blocksworld.bw.ButtonCaptureSetupTapped();
	}

	private void ButtonPressed_Capture()
	{
		Blocksworld.bw.ButtonCaptureTapped();
	}

	private void ButtonPressed_Menu()
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			CharacterEditor.Instance.Exit();
		}
		else
		{
			Blocksworld.bw.ButtonMenuTapped();
		}
	}

	private void ButtonPressed_Exit()
	{
		if (CharacterEditor.Instance.InEditMode())
		{
			CharacterEditor.Instance.Exit();
		}
		else
		{
			Blocksworld.bw.ButtonExitWorldTapped();
		}
	}

	private void ButtonPressed_Tools()
	{
		Blocksworld.bw.ButtonStopTapped();
	}

	private void ButtonPressed_ProfileSelect()
	{
		Blocksworld.UI.ShowProfileSelectionScreen();
	}

	private void ButtonPressed_RecordStart()
	{
		if (!_isRecording)
		{
			WorldSession.platformDelegate.StartRecordingScreen();
			_currentRecordingLength = 0f;
			SetRecordButtonStateActive();
		}
	}

	private void ButtonPressed_RecordStop()
	{
		if (_isRecording && _currentRecordingLength > 1f)
		{
			WorldSession.platformDelegate.StopRecordingScreen();
			SetRecordButtonStateInactive();
		}
	}

	private void ButtonPressed_RecordDockToggle()
	{
		if (_recordButtonState == RecordButtonState.Docked)
		{
			UndockRecordButtonSliding();
		}
		else if (_recordButtonState == RecordButtonState.Undocked)
		{
			DockRecordButtonSliding();
		}
	}

	private void Update()
	{
		if (_isRecording)
		{
			_currentRecordingLength += Time.deltaTime;
			if (_currentRecordingLength > 1f && !WorldSession.platformDelegate.ScreenRecordingInProgress())
			{
				_isRecording = false;
				SetRecordButtonStateInactive();
			}
			else if (_currentRecordingLength > 300f)
			{
				_isRecording = false;
				WorldSession.platformDelegate.StopRecordingScreen();
				SetRecordButtonStateInactive();
			}
		}
		else if (WorldSession.platformDelegate.ScreenRecordingInProgress())
		{
			SetRecordButtonStateActive();
		}
		if (_recordButtonState == RecordButtonState.SlidingToDocked)
		{
			_sliderTimer -= Time.deltaTime;
			if (_sliderTimer <= 0f)
			{
				_sliderTimer = 0f;
				DockRecordButton();
			}
			else
			{
				SlideRecordButtonTo(_sliderTimer / 0.25f);
			}
		}
		else if (_recordButtonState == RecordButtonState.SlidingToUndocked)
		{
			_sliderTimer -= Time.deltaTime;
			if (_sliderTimer <= 0f)
			{
				_sliderTimer = 0f;
				UndockRecordButton();
			}
			else
			{
				SlideRecordButtonTo((0.25f - _sliderTimer) / 0.25f);
			}
		}
	}

	internal void BWApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus)
		{
			RefreshRecordButtonState();
		}
	}
}
