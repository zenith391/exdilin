using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Exdilin;

// Settings screen
public class UISceneSettings : UISceneBase
{
	// Token: 0x06002EC0 RID: 11968 RVA: 0x0014C7E4 File Offset: 0x0014ABE4
	private void Awake()
	{
		this.RefreshResolutions();
		float @float = PlayerPrefs.GetFloat(UISceneSettings.inGameMusicVolumePrefLabel, 1f);
		float float2 = PlayerPrefs.GetFloat(UISceneSettings.inGameSFXVolumePrefLabel, 1f);
		float float3 = PlayerPrefs.GetFloat(UISceneSettings.uiSFXVolumePrefLabel, 1f);
        this._appliedSettings.gfxFullscreen = Screen.fullScreen;
		this._appliedSettings.gfxResolution = Screen.currentResolution.ToString();
		this._appliedSettings.gameMusicVolume = @float;
		this._appliedSettings.gameSFXVolume = float2;
		this._appliedSettings.uiSFXVolume = float3;

        this._appliedSettings.customSettings = new Dictionary<string, SettingEntry>();

        SettingEntry[] entries = SettingsRegistry.GetSettings();
        foreach (SettingEntry entry in entries)
        {
            string fullId = entry.mod.Id + '.' + entry.id;
            entry.value = PlayerPrefs.GetString(fullId, entry.defaultValue);
            entry._old = entry.value;
            entry._valedit = entry.value;
            this._appliedSettings.customSettings.Add(fullId, entry);
        }

        this.ApplySettingsToUI(this._appliedSettings);
	}

    private Rect textLabelRect = new Rect(300, 250, 275, 20);
    private Rect textFieldRect = new Rect(600, 250, 275, 20);
    private Rect restartToApplyLabelRect = new Rect(600, 340, 275, 20);
    private bool showRestartToApply;

    public void OnGUI()
    {
        Rect csLabelRect = new Rect(300, 320, 275, 20);
        Rect csFieldRect = new Rect(600, 320, 275, 20);

        SettingEntry[] entries = SettingsRegistry.GetSettings();
        foreach (SettingEntry entry in _currentSettings.customSettings.Values)
        {
            GUI.Label(csLabelRect, entry.label);
            entry._valedit = GUI.TextField(csFieldRect, entry._valedit);
            if (entry._valedit != entry._old)
            {
                string fullId = entry.mod.Id + '.' + entry.id;
                this.UpdateButtonState();
            }
            csLabelRect.y += 60;
            csFieldRect.y += 60;
        }
        if (showRestartToApply)
        {
            GUI.Label(restartToApplyLabelRect, "Restart to apply changes!");
        }
    }

    // Token: 0x06002EC1 RID: 11969 RVA: 0x0014C888 File Offset: 0x0014AC88
    private void ApplySettingsToUI(UISceneSettings.SettingsValues settings)
	{
		this._currentSettings = settings;
		this.fullscreenToggle.isOn = settings.gfxFullscreen;
		this.resolutionDropdown.value = 0;
		this.inGameMusicVolumeSlider.value = settings.gameMusicVolume;
		this.inGameMusicPercentText.text = this.WholePercentage(settings.gameMusicVolume);
		this.inGameSFXVolumeSlider.value = settings.gameSFXVolume;
		this.inGameSFXPercentText.text = this.WholePercentage(settings.gameSFXVolume);
		this.uiSFXVolumeSlider.value = settings.uiSFXVolume;
		this.uiSFXPercentText.text = this.WholePercentage(settings.uiSFXVolume);
        foreach (string key in settings.customSettings.Keys)
        {
            SettingEntry entry = settings.customSettings[key];
            entry._valedit = entry.value;
        }
        this.UpdateButtonState();
	}

	// Token: 0x06002EC2 RID: 11970 RVA: 0x0014C940 File Offset: 0x0014AD40
	private void ApplySettingsToGame(UISceneSettings.SettingsValues settings)
	{
		this._appliedSettings = settings;
		PlayerPrefs.SetFloat(UISceneSettings.inGameMusicVolumePrefLabel, this._appliedSettings.gameMusicVolume);
		if (Blocksworld.musicPlayer != null)
		{
			Blocksworld.musicPlayer.RefreshVolumeFromSettings();
		}
		PlayerPrefs.SetFloat(UISceneSettings.inGameSFXVolumePrefLabel, this._appliedSettings.gameSFXVolume);
		Sound.RefreshVolumeFromSettings();
		PlayerPrefs.SetFloat(UISceneSettings.uiSFXVolumePrefLabel, this._appliedSettings.uiSFXVolume);
		UISoundPlayer.Instance.RefreshVolumeFromSettings();
        foreach (string key in settings.customSettings.Keys)
        {
            SettingEntry entry = settings.customSettings[key];
            PlayerPrefs.SetString(key, entry._valedit);
            if (entry._valedit != entry.value)
            {
                showRestartToApply = true;
            }
            entry.value = entry._valedit;
        }
		this.UpdateButtonState();
	}

	// Token: 0x06002EC3 RID: 11971 RVA: 0x0014C9C4 File Offset: 0x0014ADC4
	public override void SceneDidLoad(UISceneInfo sceneInfo)
	{
		base.SceneDidLoad(sceneInfo);
		if (this.uiController != null)
		{
			this.uiController.menuBar.settingsMenuButton.interactable = false;
			this.uiController.menuBar.DeselectMenuButtons();
		}
		this.uiSFXVolumeSlider.onValueChanged.AddListener(delegate(float percent)
		{
			this.SetUISFXVolume(percent);
		});
		this.inGameSFXVolumeSlider.onValueChanged.AddListener(delegate(float percent)
		{
			this.SetInGameSFXVolume(percent);
		});
		this.inGameMusicVolumeSlider.onValueChanged.AddListener(delegate(float percent)
		{
			this.SetInGameMusicVolume(percent);
		});
		this.resetDefaultsButton.onClick.AddListener(delegate()
		{
			this.ResetDefaultSettingsPrompt();
		});
		this.applySettingsButton.onClick.AddListener(delegate()
		{
			this.ApplySettings();
		});
	}

	// Token: 0x06002EC4 RID: 11972 RVA: 0x0014CA9C File Offset: 0x0014AE9C
	private void GetWidthAndHeight(string resName, out int width, out int height)
	{
		string[] array = resName.Split(new char[]
		{
			'x'
		});
		int.TryParse(array[0], out width);
		int.TryParse(array[1], out height);
	}

	// Token: 0x06002EC5 RID: 11973 RVA: 0x0014CAD0 File Offset: 0x0014AED0
	private void RefreshResolutions()
	{
		this.resolutionDropdown.ClearOptions();
		this.resolutionDropdown.AddOptions(this.resolutionNames);
		for (int i = 0; i < this.resolutionNames.Count; i++)
		{
			int num;
			int num2;
			this.GetWidthAndHeight(this.resolutionNames[i], out num, out num2);
			if (Display.displays[0].renderingWidth == num && Display.displays[0].renderingHeight == num2)
			{
				this.resolutionDropdown.value = i;
			}
		}
	}

	// Token: 0x06002EC6 RID: 11974 RVA: 0x0014CB5B File Offset: 0x0014AF5B
	private void ApplySettings()
	{
		this.ApplySettingsToGame(this._currentSettings);
	}

	// Token: 0x06002EC7 RID: 11975 RVA: 0x0014CB69 File Offset: 0x0014AF69
	private void ResetDefaultSettingsPrompt()
	{
		if (BWStandalone.Instance != null)
		{
			BWStandalone.Overlays.ShowConfirmationDialog(BWMenuTextEnum.ResetToDefaultSettings, delegate()
			{
				this.ResetDefaultSettings();
			});
		}
		else
		{
			this.ResetDefaultSettings();
		}
	}

	// Token: 0x06002EC8 RID: 11976 RVA: 0x0014CB9E File Offset: 0x0014AF9E
	private void ResetDefaultSettings()
	{
		this.ApplySettingsToUI(UISceneSettings._defaultSettings);
		this.ApplySettings();
	}

	// Token: 0x06002EC9 RID: 11977 RVA: 0x0014CBB4 File Offset: 0x0014AFB4
	private string WholePercentage(float percent)
	{
		return ((int)(percent * 100f)).ToString() + "%";
	}

	// Token: 0x06002ECA RID: 11978 RVA: 0x0014CBE1 File Offset: 0x0014AFE1
	private void SetInGameMusicVolume(float percent)
	{
		this.inGameMusicPercentText.text = this.WholePercentage(percent);
		this._currentSettings.gameMusicVolume = percent;
		this.UpdateButtonState();
	}

	// Token: 0x06002ECB RID: 11979 RVA: 0x0014CC07 File Offset: 0x0014B007
	private void SetInGameSFXVolume(float percent)
	{
		this.inGameSFXPercentText.text = this.WholePercentage(percent);
		this._currentSettings.gameSFXVolume = percent;
		this.UpdateButtonState();
	}

	// Token: 0x06002ECC RID: 11980 RVA: 0x0014CC2D File Offset: 0x0014B02D
	private void SetUISFXVolume(float percent)
	{
		this.uiSFXPercentText.text = this.WholePercentage(percent);
		this._currentSettings.uiSFXVolume = percent;
		this.UpdateButtonState();
	}

	// Token: 0x06002ECD RID: 11981 RVA: 0x0014CC53 File Offset: 0x0014B053
	private void SetResolution(int dropdownOption)
	{
		this._currentSettings.gfxResolution = this.resolutionNames[dropdownOption];
		this.UpdateButtonState();
	}

	// Token: 0x06002ECE RID: 11982 RVA: 0x0014CC72 File Offset: 0x0014B072
	private void ToggleFullscreen(bool toggleOn)
	{
		this._currentSettings.gfxFullscreen = toggleOn;
		this.UpdateButtonState();
	}

	// Token: 0x06002ECF RID: 11983 RVA: 0x0014CC86 File Offset: 0x0014B086
	private void UpdateButtonState()
	{
        foreach (string key in this._currentSettings.customSettings.Keys)
        {
            SettingEntry entry = this._currentSettings.customSettings[key];
            if (entry._valedit != this._appliedSettings.customSettings[key]._old)
            {
                this.applySettingsButton.gameObject.SetActive(true);
                return;
            }
        }
        this.applySettingsButton.gameObject.SetActive(!this._appliedSettings.Equals(this._currentSettings));
	}

	// Token: 0x04002725 RID: 10021
	public Toggle fullscreenToggle;

	// Token: 0x04002726 RID: 10022
	public Dropdown resolutionDropdown;

	// Token: 0x04002727 RID: 10023
	public Slider uiSFXVolumeSlider;

	// Token: 0x04002728 RID: 10024
	public Slider inGameSFXVolumeSlider;

	// Token: 0x04002729 RID: 10025
	public Slider inGameMusicVolumeSlider;

	// Token: 0x0400272A RID: 10026
	public Text uiSFXPercentText;

	// Token: 0x0400272B RID: 10027
	public Text inGameSFXPercentText;

	// Token: 0x0400272C RID: 10028
	public Text inGameMusicPercentText;

	// Token: 0x0400272D RID: 10029
	public Button resetDefaultsButton;

	// Token: 0x0400272E RID: 10030
	public Button applySettingsButton;

    public string apiServerFieldText = "";

	// Token: 0x0400272F RID: 10031
	private List<string> resolutionNames = new List<string>
	{
		"1024x768",
		"1280x720",
		"1280x960",
		"1366x768",
		"1440x1080",
		"1600x900",
		"1600x1200",
		"1920x1080",
		"1920x1440",
		"2048x1536",
		"2560x1440"
	};

	// Token: 0x04002730 RID: 10032
	public static readonly string inGameMusicVolumePrefLabel = "Music Volume";

	// Token: 0x04002731 RID: 10033
	public static readonly string inGameSFXVolumePrefLabel = "Game SFX Volume";

	// Token: 0x04002732 RID: 10034
	public static readonly string uiSFXVolumePrefLabel = "UI SFX Volume";

    // Token: 0x04002733 RID: 10035
    private static UISceneSettings.SettingsValues _defaultSettings = new UISceneSettings.SettingsValues
    {
        gfxFullscreen = true,
        gfxResolution = "1600x1200",
        gameMusicVolume = 1f,
        gameSFXVolume = 1f,
        uiSFXVolume = 1f,
        customSettings = new Dictionary<string, SettingEntry>()
	};

	// Token: 0x04002734 RID: 10036
	private UISceneSettings.SettingsValues _appliedSettings = default(UISceneSettings.SettingsValues);

	// Token: 0x04002735 RID: 10037
	private UISceneSettings.SettingsValues _currentSettings = default(UISceneSettings.SettingsValues);

	// Token: 0x02000446 RID: 1094
	private struct SettingsValues
	{
		// Token: 0x04002736 RID: 10038
		public bool gfxFullscreen;

		// Token: 0x04002737 RID: 10039
		public string gfxResolution;

		// Token: 0x04002738 RID: 10040
		public float gameMusicVolume;

		// Token: 0x04002739 RID: 10041
		public float gameSFXVolume;

		// Token: 0x0400273A RID: 10042
		public float uiSFXVolume;

        public Dictionary<string, SettingEntry> customSettings;
	}
}
