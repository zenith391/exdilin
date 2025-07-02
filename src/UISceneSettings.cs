using System.Collections.Generic;
using Exdilin;
using UnityEngine;
using UnityEngine.UI;

public class UISceneSettings : UISceneBase
{
	private struct SettingsValues
	{
		public bool gfxFullscreen;

		public string gfxResolution;

		public float gameMusicVolume;

		public float gameSFXVolume;

		public float uiSFXVolume;

		public Dictionary<string, SettingEntry> customSettings;
	}

	private Rect textLabelRect = new Rect(300f, 250f, 275f, 20f);

	private Rect textFieldRect = new Rect(600f, 250f, 275f, 20f);

	private Rect restartToApplyLabelRect = new Rect(600f, 340f, 275f, 20f);

	private bool showRestartToApply;

	public Toggle fullscreenToggle;

	public Dropdown resolutionDropdown;

	public Slider uiSFXVolumeSlider;

	public Slider inGameSFXVolumeSlider;

	public Slider inGameMusicVolumeSlider;

	public Text uiSFXPercentText;

	public Text inGameSFXPercentText;

	public Text inGameMusicPercentText;

	public Button resetDefaultsButton;

	public Button applySettingsButton;

	public string apiServerFieldText = "";

	private List<string> resolutionNames = new List<string>
	{
		"1024x768", "1280x720", "1280x960", "1366x768", "1440x1080", "1600x900", "1600x1200", "1920x1080", "1920x1440", "2048x1536",
		"2560x1440"
	};

	public static readonly string inGameMusicVolumePrefLabel = "Music Volume";

	public static readonly string inGameSFXVolumePrefLabel = "Game SFX Volume";

	public static readonly string uiSFXVolumePrefLabel = "UI SFX Volume";

	private static SettingsValues _defaultSettings = new SettingsValues
	{
		gfxFullscreen = true,
		gfxResolution = "1600x1200",
		gameMusicVolume = 1f,
		gameSFXVolume = 1f,
		uiSFXVolume = 1f,
		customSettings = new Dictionary<string, SettingEntry>()
	};

	private SettingsValues _appliedSettings;

	private SettingsValues _currentSettings;

	private void Awake()
	{
		RefreshResolutions();
		float gameMusicVolume = PlayerPrefs.GetFloat(inGameMusicVolumePrefLabel, 1f);
		float gameSFXVolume = PlayerPrefs.GetFloat(inGameSFXVolumePrefLabel, 1f);
		float uiSFXVolume = PlayerPrefs.GetFloat(uiSFXVolumePrefLabel, 1f);
		_appliedSettings.gfxFullscreen = Screen.fullScreen;
		_appliedSettings.gfxResolution = Screen.currentResolution.ToString();
		_appliedSettings.gameMusicVolume = gameMusicVolume;
		_appliedSettings.gameSFXVolume = gameSFXVolume;
		_appliedSettings.uiSFXVolume = uiSFXVolume;
		_appliedSettings.customSettings = new Dictionary<string, SettingEntry>();
		SettingEntry[] settings = SettingsRegistry.GetSettings();
		SettingEntry[] array = settings;
		foreach (SettingEntry settingEntry in array)
		{
			string key = settingEntry.mod.Id + "." + settingEntry.id;
			settingEntry.value = PlayerPrefs.GetString(key, settingEntry.defaultValue);
			settingEntry._old = settingEntry.value;
			settingEntry._valedit = settingEntry.value;
			_appliedSettings.customSettings.Add(key, settingEntry);
		}
		ApplySettingsToUI(_appliedSettings);
	}

	public void OnGUI()
	{
		Rect position = new Rect(300f, 320f, 275f, 20f);
		Rect position2 = new Rect(600f, 320f, 275f, 20f);
		SettingEntry[] settings = SettingsRegistry.GetSettings();
		foreach (SettingEntry value in _currentSettings.customSettings.Values)
		{
			GUI.Label(position, value.label);
			value._valedit = GUI.TextField(position2, value._valedit);
			if (value._valedit != value._old)
			{
				string text = value.mod.Id + "." + value.id;
				UpdateButtonState();
			}
			position.y += 60f;
			position2.y += 60f;
		}
		if (showRestartToApply)
		{
			GUI.Label(restartToApplyLabelRect, "Restart to apply changes!");
		}
	}

	private void ApplySettingsToUI(SettingsValues settings)
	{
		_currentSettings = settings;
		fullscreenToggle.isOn = settings.gfxFullscreen;
		resolutionDropdown.value = 0;
		inGameMusicVolumeSlider.value = settings.gameMusicVolume;
		inGameMusicPercentText.text = WholePercentage(settings.gameMusicVolume);
		inGameSFXVolumeSlider.value = settings.gameSFXVolume;
		inGameSFXPercentText.text = WholePercentage(settings.gameSFXVolume);
		uiSFXVolumeSlider.value = settings.uiSFXVolume;
		uiSFXPercentText.text = WholePercentage(settings.uiSFXVolume);
		foreach (string key in settings.customSettings.Keys)
		{
			SettingEntry settingEntry = settings.customSettings[key];
			settingEntry._valedit = settingEntry.value;
		}
		UpdateButtonState();
	}

	private void ApplySettingsToGame(SettingsValues settings)
	{
		_appliedSettings = settings;
		PlayerPrefs.SetFloat(inGameMusicVolumePrefLabel, _appliedSettings.gameMusicVolume);
		if (Blocksworld.musicPlayer != null)
		{
			Blocksworld.musicPlayer.RefreshVolumeFromSettings();
		}
		PlayerPrefs.SetFloat(inGameSFXVolumePrefLabel, _appliedSettings.gameSFXVolume);
		Sound.RefreshVolumeFromSettings();
		PlayerPrefs.SetFloat(uiSFXVolumePrefLabel, _appliedSettings.uiSFXVolume);
		UISoundPlayer.Instance.RefreshVolumeFromSettings();
		foreach (string key in settings.customSettings.Keys)
		{
			SettingEntry settingEntry = settings.customSettings[key];
			PlayerPrefs.SetString(key, settingEntry._valedit);
			if (settingEntry._valedit != settingEntry.value)
			{
				showRestartToApply = true;
			}
			settingEntry.value = settingEntry._valedit;
		}
		UpdateButtonState();
	}

	public override void SceneDidLoad(UISceneInfo sceneInfo)
	{
		base.SceneDidLoad(sceneInfo);
		if (uiController != null)
		{
			uiController.menuBar.settingsMenuButton.interactable = false;
			uiController.menuBar.DeselectMenuButtons();
		}
		uiSFXVolumeSlider.onValueChanged.AddListener(delegate(float percent)
		{
			SetUISFXVolume(percent);
		});
		inGameSFXVolumeSlider.onValueChanged.AddListener(delegate(float percent)
		{
			SetInGameSFXVolume(percent);
		});
		inGameMusicVolumeSlider.onValueChanged.AddListener(delegate(float percent)
		{
			SetInGameMusicVolume(percent);
		});
		resetDefaultsButton.onClick.AddListener(delegate
		{
			ResetDefaultSettingsPrompt();
		});
		applySettingsButton.onClick.AddListener(delegate
		{
			ApplySettings();
		});
	}

	private void GetWidthAndHeight(string resName, out int width, out int height)
	{
		string[] array = resName.Split('x');
		int.TryParse(array[0], out width);
		int.TryParse(array[1], out height);
	}

	private void RefreshResolutions()
	{
		resolutionDropdown.ClearOptions();
		resolutionDropdown.AddOptions(resolutionNames);
		for (int i = 0; i < resolutionNames.Count; i++)
		{
			GetWidthAndHeight(resolutionNames[i], out var width, out var height);
			if (Display.displays[0].renderingWidth == width && Display.displays[0].renderingHeight == height)
			{
				resolutionDropdown.value = i;
			}
		}
	}

	private void ApplySettings()
	{
		ApplySettingsToGame(_currentSettings);
	}

	private void ResetDefaultSettingsPrompt()
	{
		if (BWStandalone.Instance != null)
		{
			BWStandalone.Overlays.ShowConfirmationDialog(BWMenuTextEnum.ResetToDefaultSettings, delegate
			{
				ResetDefaultSettings();
			});
		}
		else
		{
			ResetDefaultSettings();
		}
	}

	private void ResetDefaultSettings()
	{
		ApplySettingsToUI(_defaultSettings);
		ApplySettings();
	}

	private string WholePercentage(float percent)
	{
		return (int)(percent * 100f) + "%";
	}

	private void SetInGameMusicVolume(float percent)
	{
		inGameMusicPercentText.text = WholePercentage(percent);
		_currentSettings.gameMusicVolume = percent;
		UpdateButtonState();
	}

	private void SetInGameSFXVolume(float percent)
	{
		inGameSFXPercentText.text = WholePercentage(percent);
		_currentSettings.gameSFXVolume = percent;
		UpdateButtonState();
	}

	private void SetUISFXVolume(float percent)
	{
		uiSFXPercentText.text = WholePercentage(percent);
		_currentSettings.uiSFXVolume = percent;
		UpdateButtonState();
	}

	private void SetResolution(int dropdownOption)
	{
		_currentSettings.gfxResolution = resolutionNames[dropdownOption];
		UpdateButtonState();
	}

	private void ToggleFullscreen(bool toggleOn)
	{
		_currentSettings.gfxFullscreen = toggleOn;
		UpdateButtonState();
	}

	private void UpdateButtonState()
	{
		foreach (string key in _currentSettings.customSettings.Keys)
		{
			SettingEntry settingEntry = _currentSettings.customSettings[key];
			if (settingEntry._valedit != _appliedSettings.customSettings[key]._old)
			{
				applySettingsButton.gameObject.SetActive(value: true);
				return;
			}
		}
		applySettingsButton.gameObject.SetActive(!_appliedSettings.Equals(_currentSettings));
	}
}
