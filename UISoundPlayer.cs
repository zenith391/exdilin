using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200045D RID: 1117
public class UISoundPlayer : MonoBehaviour
{
	// Token: 0x1700024E RID: 590
	// (get) Token: 0x06002F44 RID: 12100 RVA: 0x0014ED6A File Offset: 0x0014D16A
	public static UISoundPlayer Instance
	{
		get
		{
			if (UISoundPlayer.instance == null)
			{
				UISoundPlayer.instance = UISoundPlayer.CreateInstance();
			}
			return UISoundPlayer.instance;
		}
	}

	// Token: 0x06002F45 RID: 12101 RVA: 0x0014ED8C File Offset: 0x0014D18C
	private static UISoundPlayer CreateInstance()
	{
		GameObject gameObject = new GameObject("UISoundPlayer");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		UISoundPlayer uisoundPlayer = gameObject.AddComponent<UISoundPlayer>();
		uisoundPlayer.audioSources = new AudioSource[UISoundPlayer.sourceCount];
		for (int i = 0; i < UISoundPlayer.sourceCount; i++)
		{
			uisoundPlayer.audioSources[i] = gameObject.AddComponent<AudioSource>();
		}
		uisoundPlayer.RefreshVolumeFromSettings();
		return uisoundPlayer;
	}

	// Token: 0x06002F46 RID: 12102 RVA: 0x0014EDEC File Offset: 0x0014D1EC
	public void PlayClip(string clipName, float volume = 1f)
	{
		AudioClip clip = this.GetClip(clipName);
		if (clip != null)
		{
			this.audioSources[this.sourceIndex].PlayOneShot(clip, volume);
			this.sourceIndex = (this.sourceIndex + 1) % UISoundPlayer.sourceCount;
		}
	}

	// Token: 0x06002F47 RID: 12103 RVA: 0x0014EE38 File Offset: 0x0014D238
	private AudioClip GetClip(string clipName)
	{
		if (this.clipLibrary == null)
		{
			this.clipLibrary = new Dictionary<string, AudioClip>();
		}
		AudioClip audioClip = null;
		if (!this.clipLibrary.TryGetValue(clipName, out audioClip))
		{
			audioClip = Resources.Load<AudioClip>("UISounds/" + clipName);
			this.clipLibrary.Add(clipName, audioClip);
		}
		return audioClip;
	}

	// Token: 0x06002F48 RID: 12104 RVA: 0x0014EE90 File Offset: 0x0014D290
	public void RefreshVolumeFromSettings()
	{
		float @float = PlayerPrefs.GetFloat(UISceneSettings.uiSFXVolumePrefLabel, 1f);
		for (int i = 0; i < UISoundPlayer.sourceCount; i++)
		{
			this.audioSources[i].volume = @float;
		}
	}

	// Token: 0x040027AB RID: 10155
	private static UISoundPlayer instance;

	// Token: 0x040027AC RID: 10156
	private static int sourceCount = 4;

	// Token: 0x040027AD RID: 10157
	private AudioSource[] audioSources;

	// Token: 0x040027AE RID: 10158
	private Dictionary<string, AudioClip> clipLibrary;

	// Token: 0x040027AF RID: 10159
	private int sourceIndex;
}
